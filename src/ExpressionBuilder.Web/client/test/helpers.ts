import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { JSDOM } from "jsdom";
import { FilterBuilder } from "../src/builder.js";
import type { Schema } from "../src/types.js";

const viewUrl = new URL("../../../Views/Shared/Components/FilterBuilder/Default.cshtml", import.meta.url);
const fixtureUrl = new URL("../../test/fixtures/person-schema.json", import.meta.url);

/** The schema the server produces for the Person class used by the .NET tests. */
export function personSchema(): Schema {
    return JSON.parse(readFileSync(fixtureUrl, "utf8")) as Schema;
}

function camel(name: string): string {
    return name.charAt(0).toLowerCase() + name.slice(1);
}

function escapeAttribute(text: string): string {
    return text.replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;");
}

/**
 * Turns the real Razor view into HTML, so these tests run against the markup that is shipped rather than a
 * copy of it. The view only uses a handful of Razor features; anything else makes this throw, so the helper
 * is extended with the view instead of silently going out of date.
 */
export function viewHtml(schema: Schema, json: string, name = "Filter.Json"): string {
    const labels = schema.labels as unknown as Record<string, string>;

    const html = readFileSync(viewUrl, "utf8")
        .replace(/@\*[\s\S]*?\*@/g, "")
        .replace(/^@using .*$/gm, "")
        .replace(/^@model .*$/gm, "")
        .replace(/@\{[\s\S]*?\n\}/g, "")
        .replace(/@Html\.Raw\(Model\.SchemaJson\)/g, () => JSON.stringify(schema))
        .replace(/@Model\.Id/g, "eb-test")
        .replace(/@Model\.Name/g, name)
        .replace(/@Model\.Json/g, () => escapeAttribute(json))
        .replace(/@l\.(\w+)/g, (_, key: string) => {
            const label = labels[camel(key)];

            if (label === undefined) throw new Error(`The view uses a label the schema doesn't have: ${key}`);

            return escapeAttribute(label);
        });

    const leftover = /@[A-Za-z{(]/.exec(html);

    if (leftover) throw new Error(`The view uses Razor this helper can't turn into HTML: ${html.slice(leftover.index, leftover.index + 40)}`);

    return html;
}

export interface Page {
    dom: JSDOM;
    document: Document;
    form: HTMLFormElement;
    root: HTMLElement;
    input: HTMLInputElement;
    builder: FilterBuilder;
}

/** Puts the view in a page and starts the filter builder on it. */
export function render(
    initial?: unknown,
    options: { schema?: Schema; rootAttributes?: Record<string, string>; html?: (html: string) => string } = {},
): Page {
    const schema = options.schema ?? personSchema();
    const json = initial === undefined ? "" : typeof initial === "string" ? initial : JSON.stringify(initial);
    const view = options.html ? options.html(viewHtml(schema, json)) : viewHtml(schema, json);
    const dom = new JSDOM(`<!doctype html><body><form>${view}<button id="submit">Go</button></form></body>`);

    const { window } = dom;

    Object.assign(globalThis, {
        document: window.document,
        HTMLElement: window.HTMLElement,
        CustomEvent: window.CustomEvent,
        Event: window.Event,
    });

    const root = window.document.querySelector<HTMLElement>("[data-eb-filter-builder]")!;

    for (const [key, value] of Object.entries(options.rootAttributes ?? {})) root.setAttribute(key, value);

    return {
        dom,
        document: window.document,
        form: window.document.querySelector("form")!,
        root,
        input: root.querySelector<HTMLInputElement>("[data-eb-input]")!,
        builder: FilterBuilder.attach(root),
    };
}

// -- Acting like a user ---------------------------------------------------------------------

export function choose(element: Element, value: string): void {
    const select = element as HTMLSelectElement;

    select.value = value;
    assert.equal(select.value, value, `"${value}" is not one of the choices`);
    select.dispatchEvent(new element.ownerDocument.defaultView!.Event("change", { bubbles: true }));
}

export function type(element: Element, text: string): void {
    const input = element as HTMLInputElement;

    input.value = text;
    input.dispatchEvent(new element.ownerDocument.defaultView!.Event("input", { bubbles: true }));
}

export function click(element: Element): void {
    (element as HTMLElement).click();
}

export function tick(): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, 5));
}

// -- Finding things -------------------------------------------------------------------------

export function all(scope: ParentNode, selector: string): HTMLElement[] {
    return [...scope.querySelectorAll<HTMLElement>(selector)];
}

export function one<T extends HTMLElement = HTMLElement>(scope: ParentNode, selector: string): T {
    const found = scope.querySelector<T>(selector);

    assert.ok(found, `Nothing matches ${selector}`);

    return found;
}

export function conditions(page: Page): HTMLElement[] {
    return all(page.root, "[data-eb-condition]");
}

export function optionTexts(select: Element): string[] {
    return [...(select as HTMLSelectElement).options].map((o) => o.text);
}

export function posted(page: Page): ReturnType<typeof JSON.parse> {
    return JSON.parse(page.input.value);
}

// -- Filling in a filter the way a user would ------------------------------------------------

/** Adds a condition to the top level group and returns its row. */
export function addCondition(page: Page): HTMLElement {
    const before = conditions(page).length;

    click(one(page.root, "[data-eb-root] > [data-eb-group] > .eb-group-actions [data-eb-add-condition]"));

    const rows = conditions(page);

    assert.equal(rows.length, before + 1);

    return rows.at(-1)!;
}

/** Chooses a property, then (optionally) an operation, then types or chooses the values. */
export function fill(row: HTMLElement, field: string, operation?: string, ...values: string[]): void {
    choose(one(row, "[data-eb-field]"), field);

    if (operation) choose(one(row, "[data-eb-operation]"), operation);

    all(row, "[data-eb-values] [data-eb-value] [data-eb-control]").forEach((control, i) => {
        const value = values[i];

        if (value !== undefined) {
            if (control.tagName === "SELECT") choose(control, value);
            else type(control, value);
        }
    });
}
