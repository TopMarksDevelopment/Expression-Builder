/**
 * What the script posts has to be what the server accepts. Each scenario is driven through the interface like a
 * user would, and the document it posts is compared with a fixture. The .NET tests (PostedFixtureTests) read the same
 * files, build them into filters and check they find the right people - so a change that breaks the agreement fails
 * on whichever side moved.
 *
 * To update the fixtures after a deliberate change, run these tests with UPDATE_FIXTURES=1 and commit the result.
 */

import assert from "node:assert/strict";
import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { describe, it } from "node:test";
import { addCondition, all, choose, click, conditions, fill, one, render, type } from "./helpers.js";
import type { Page } from "./helpers.js";

const folder = new URL("../../test/fixtures/posted/", import.meta.url);

function check(name: string, page: Page): void {
    const file = new URL(`${name}.json`, folder);
    const actual = JSON.stringify(JSON.parse(page.input.value), null, 2) + "\n";

    if (process.env.UPDATE_FIXTURES === "1") {
        mkdirSync(folder, { recursive: true });
        writeFileSync(file, actual);
    }

    assert.ok(existsSync(file), `Missing fixture ${name}.json. Run with UPDATE_FIXTURES=1.`);
    assert.equal(actual, readFileSync(file, "utf8").replace(/\r\n/g, "\n"));
}

function checkbox(row: HTMLElement, name: string, page: Page): void {
    const box = one<HTMLInputElement>(row, `[data-eb-manipulator="${name}"]`);

    box.checked = true;
    box.dispatchEvent(new page.dom.window.Event("change", { bubbles: true }));
}

describe("what the script posts", () => {
    it("text: several terms that must all match, adjusted before comparing", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Name", "Contains", "turing");
        click(one(row, ".eb-btn-add-value"));
        type(all(row, ".eb-value input")[1]!, "alan");
        click(one(row, "[data-eb-toggle-options]"));
        choose(one(row, "[data-eb-match]"), "All");
        checkbox(row, "ToLower", page);
        checkbox(row, "Trim", page);

        check("text-all-terms", page);
    });

    it("values of every kind, joined with and/or", () => {
        const page = render();

        fill(addCondition(page), "Age", "Between", "20", "45");
        fill(addCondition(page), "Born", "LessThan", "1915-01-01");
        fill(addCondition(page), "Favourite", "In", "Red");
        click(one(conditions(page)[2]!, ".eb-btn-add-value"));
        choose(all(conditions(page)[2]!, ".eb-value select")[1]!, "Blue");
        fill(addCondition(page), "Active", "Equal", "true");
        fill(addCondition(page), "Reference", "NotEqual", "22222222-2222-2222-2222-222222222222");

        // Between the second and third conditions, "or"
        choose(all(page.root, "[data-eb-connector]")[1]!, "Or");

        check("kinds-and-connectors", page);
    });

    it("a collection of objects, a related object and a nested group", () => {
        const page = render();

        choose(one(addCondition(page), "[data-eb-field]"), "collection:Contacts");
        fill(conditions(page)[0]!, "Type", "Equal", "Email");

        fill(addCondition(page), "Home.Postcode", "StartsWith", "M1");

        click(one(page.root, "[data-eb-root] > [data-eb-group] > .eb-group-actions [data-eb-add-group]"));
        const inner = conditions(page).at(-1)!;

        fill(inner, "Email", "IsNotNull");

        check("collection-related-and-group", page);
    });

    it("a collection of plain values, and a value that must be set", () => {
        const page = render();

        choose(one(addCondition(page), "[data-eb-field]"), "collection:Tags");
        fill(conditions(page)[0]!, "", "Equal", "maths");

        fill(addCondition(page), "Verified", "IsNotNull");

        check("plain-values-and-null", page);
    });
});
