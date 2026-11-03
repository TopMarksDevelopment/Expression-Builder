import assert from "node:assert/strict";
import { describe, it } from "node:test";
import * as M from "../src/model.js";
import type { Schema } from "../src/types.js";
import { addCondition, all, choose, click, conditions, fill, one, personSchema, posted, render, type } from "./helpers.js";

function doc(...items: unknown[]) {
    return { version: 1, root: { type: "group", connector: "And", items } };
}

function condition(field: string, operation: string, values: string[], extra: object = {}) {
    return { type: "condition", connector: "And", field, operation, values, ...extra };
}

/** The Person schema with an operation and a manipulator of the app's own, as the server would send them. */
function withCustom(): Schema {
    const schema = structuredClone(personSchema());

    schema.operations.push({
        name: "IsShortName",
        label: "Has a short name",
        arity: "none",
        supportsMatch: false,
        defaultMatch: "Any",
        group: "Custom",
    });
    schema.operations.push({
        name: "SoundsLike",
        label: "Sounds like",
        arity: "one",
        supportsMatch: false,
        defaultMatch: "Any",
        group: "Phonetics",
    });

    for (const field of schema.types[schema.root]!.fields.filter((f) => f.kind === "string")) field.operations.push("IsShortName", "SoundsLike");

    schema.manipulators.push({
        name: "Left",
        label: "First letters",
        parameters: [{ label: "How many", required: true }],
        kinds: ["string"],
        group: "Custom",
    });

    return schema;
}

function open(row: HTMLElement): void {
    click(one(row, "[data-eb-toggle-options]"));
}

describe("operations of your own", () => {
    it("lists only the built-in operations in a plain list", () => {
        const page = render();
        const row = addCondition(page);

        choose(one(row, "[data-eb-field]"), "Name");

        assert.equal(all(row, "[data-eb-operation] optgroup").length, 0);
    });

    it("lists custom operations under headings, after the built-in ones", () => {
        const page = render(undefined, { schema: withCustom() });
        const row = addCondition(page);

        choose(one(row, "[data-eb-field]"), "Name");

        const groups = all(row, "[data-eb-operation] optgroup") as HTMLOptGroupElement[];

        assert.deepEqual(
            groups.map((g) => g.label),
            ["Standard", "Custom", "Phonetics"],
        );
        const texts = (group: HTMLOptGroupElement) => [...group.children].map((o) => o.textContent);

        assert.ok(texts(groups[0]!).includes("Contains"));
        assert.deepEqual(texts(groups[1]!), ["Has a short name"]);
        assert.deepEqual(texts(groups[2]!), ["Sounds like"]);
    });

    it("posts a custom operation like any other", () => {
        const page = render(undefined, { schema: withCustom() });
        const row = addCondition(page);

        fill(row, "Name", "SoundsLike", "smith");

        assert.deepEqual(posted(page).root.items[0], condition("Name", "SoundsLike", ["smith"]));

        choose(one(row, "[data-eb-operation]"), "IsShortName");

        assert.deepEqual(posted(page).root.items[0], condition("Name", "IsShortName", []));
    });

    it("doesn't offer a custom operation for a property it wasn't described for", () => {
        const page = render(undefined, { schema: withCustom() });
        const row = addCondition(page);

        choose(one(row, "[data-eb-field]"), "Age");

        assert.equal(all(row, "[data-eb-operation] option").some((o) => o.textContent === "Has a short name"), false);
    });
});

describe("manipulators of your own", () => {
    function box(row: HTMLElement, name: string): HTMLInputElement {
        return one<HTMLInputElement>(row, `[data-eb-manipulator="${name}"]`);
    }

    it("lists only the built-in manipulators, with no heading", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Name", "Equal", "a");
        open(row);

        assert.equal(all(row, "[data-eb-manipulator-item]").length, 4);
        assert.equal(all(row, "[data-eb-manipulator-group-title]").filter((t) => !t.hidden).length, 0);
    });

    it("lists custom manipulators under a heading of their own", () => {
        const page = render(undefined, { schema: withCustom() });
        const row = addCondition(page);

        fill(row, "Name", "Equal", "a");
        open(row);

        const titles = all(row, "[data-eb-manipulator-group-title]");

        assert.deepEqual(
            titles.map((t) => [t.textContent, t.hidden]),
            [
                ["Standard", false],
                ["Custom", false],
            ],
        );
        assert.deepEqual(
            all(row, "[data-eb-manipulator-group]").map((g) => all(g, "[data-eb-manipulator-item]").length),
            [4, 1],
        );
    });

    it("asks for what the manipulator needs, and posts it", () => {
        const page = render(undefined, { schema: withCustom() });
        const row = addCondition(page);

        fill(row, "Name", "Equal", "a");
        open(row);

        const input = one<HTMLInputElement>(row, '[data-eb-manipulator-item="Left"] [data-eb-manipulator-arg]');

        assert.equal(one(row, '[data-eb-manipulator-item="Left"] [data-eb-manipulator-args]').hidden, true);

        box(row, "Left").checked = true;
        box(row, "Left").dispatchEvent(new page.dom.window.Event("change", { bubbles: true }));

        assert.equal(one(row, '[data-eb-manipulator-item="Left"] [data-eb-manipulator-args]').hidden, false);
        assert.equal(input.placeholder, "How many");

        type(input, "2");

        assert.deepEqual(posted(page).root.items[0].manipulators, [{ name: "Left", args: ["2"] }]);
    });

    it("says what a manipulator still needs before the filter is posted", () => {
        const schema = withCustom();
        const page = render(doc(condition("Name", "Equal", ["a"], { manipulators: [{ name: "Left", args: [""] }] })), { schema });

        assert.equal(page.builder.validate(), false);
        assert.equal(page.builder.issues[0]?.message, "How many");
    });

    it("shows a loaded custom manipulator with what it was given", () => {
        const page = render(doc(condition("Name", "Equal", ["a"], { manipulators: [{ name: "Left", args: ["3"] }] })), { schema: withCustom() });
        const row = conditions(page)[0]!;

        open(row);

        assert.equal(box(row, "Left").checked, true);
        assert.equal(one<HTMLInputElement>(row, '[data-eb-manipulator-item="Left"] [data-eb-manipulator-arg]').value, "3");
    });

    it("drops a loaded manipulator that is not offered", () => {
        const page = render(doc(condition("Name", "Equal", ["a"], { manipulators: [{ name: "Left", args: ["3"] }, { name: "Trim" }] })));

        assert.deepEqual(posted(page).root.items[0].manipulators, [{ name: "Trim" }]);
    });

    it("hides a manipulator on a property it doesn't suit", () => {
        const schema = withCustom();

        schema.manipulators.find((m) => m.name === "Left")!.kinds = ["number"];

        const page = render(undefined, { schema });
        const row = addCondition(page);

        fill(row, "Name", "Equal", "a");
        open(row);

        assert.equal(one(row, '[data-eb-manipulator-item="Left"]').hidden, true);
        assert.equal(one(row, '[data-eb-manipulator-item="Trim"]').hidden, false);
        assert.equal(one(row, '[data-eb-manipulator-group]:has([data-eb-manipulator-item="Left"])').hidden, true, "an empty group is hidden");

        fill(row, "Age", "Equal", "1");

        assert.equal(one(row, "[data-eb-toggle-options]").hidden, false, "a number has a manipulator now");
        assert.equal(one(row, '[data-eb-manipulator-item="Left"]').hidden, false);
        assert.equal(one(row, '[data-eb-manipulator-item="Trim"]').hidden, true);
    });

    it("finds the manipulators that suit a property", () => {
        const schema = withCustom();
        const name = M.findField(schema, schema.root, "Name");
        const age = M.findField(schema, schema.root, "Age");

        assert.deepEqual(
            M.manipulatorsFor(schema, name).map((m) => m.name),
            ["Trim", "ToLower", "ToUpper", "Replace", "Left"],
        );
        assert.deepEqual(M.manipulatorsFor(schema, age), []);
        assert.deepEqual(M.manipulatorsFor(schema, undefined), []);
    });
});

describe("restyling the view", () => {
    const bootstrap = (html: string): string =>
        html
            // Every box and list the script makes comes from the view's templates, so classes are the view's to choose
            .replaceAll('class="eb-input"', 'class="form-control"')
            .replace('class="eb-btn eb-btn-text eb-btn-add-value"', 'class="btn btn-link"')
            .replace('class="eb-btn eb-btn-icon eb-btn-remove-value"', 'class="btn btn-outline-danger"')
            .replace('class="eb-item" data-eb-item', 'class="list-group-item" data-eb-item')
            .replace('data-eb-invalid-class="eb-invalid"', 'data-eb-invalid-class="is-invalid border-danger"')
            .replace("<span class=\"eb-value-joiner\"", "<label class=\"form-label\" data-eb-value-label></label><span class=\"eb-value-joiner\"");

    it("puts the classes of the view on everything the script makes", () => {
        const page = render(undefined, { html: bootstrap });
        const row = addCondition(page);

        fill(row, "Name", "In", "a");
        click(one(row, "[data-eb-add-value]"));

        assert.ok(all(row, "[data-eb-values] [data-eb-control]").every((c) => c.className === "form-control"));
        assert.equal(one(row, "[data-eb-add-value]").className, "btn btn-link");
        assert.ok(all(row, "[data-eb-remove-value]").every((b) => b.className === "btn btn-outline-danger"));
        assert.ok(all(page.root, "[data-eb-items] > li").every((li) => li.className === "list-group-item"));
        assert.equal(all(page.root, ".eb-input").length, 0, "nothing falls back to the default classes");
    });

    it("fills in the label of a value where the view has a place for it", () => {
        const page = render(undefined, { html: bootstrap });
        const row = addCondition(page);

        fill(row, "Name", "In", "a");
        click(one(row, "[data-eb-add-value]"));

        assert.deepEqual(
            all(row, "[data-eb-value-label]").map((l) => l.textContent),
            ["Value 1", "Value 2"],
        );
    });

    it("marks an incomplete condition with the classes the view asks for", () => {
        const page = render(undefined, { html: bootstrap });
        const row = addCondition(page);

        fill(row, "Name", "Equal");

        assert.equal(page.builder.validate(), false);
        assert.ok(row.classList.contains("is-invalid"));
        assert.ok(row.classList.contains("border-danger"));
        assert.equal(row.classList.contains("eb-invalid"), false);
    });

    it("uses the default class for an incomplete condition if the view doesn't say", () => {
        const page = render(undefined, { html: (html) => html.replace(' data-eb-invalid-class="eb-invalid"', "") });
        const row = addCondition(page);

        fill(row, "Name", "Equal");
        page.builder.validate();

        assert.ok(row.classList.contains("eb-invalid"));
    });

    it("says which part of the view is missing", () => {
        assert.throws(
            () => render(undefined, { html: (html) => html.replace('data-eb-template="value"', 'data-eb-template="nope"') }),
            /keep its data-eb-\* attributes/,
        );

        // The parts a template needs are checked when the page loads, not when a condition first needs them
        assert.throws(
            () => render(undefined, { html: (html) => html.replace('data-eb-control="select"', 'data-eb-control="nope"') }),
            /keep its data-eb-\* attributes/,
        );
        assert.throws(
            () => render(undefined, { html: (html) => html.replace("data-eb-manipulator-args", "data-eb-nope") }),
            /keep its data-eb-\* attributes/,
        );
    });
});
