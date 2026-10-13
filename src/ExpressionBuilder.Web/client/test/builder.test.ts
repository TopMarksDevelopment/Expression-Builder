import assert from "node:assert/strict";
import { describe, it } from "node:test";
import { FilterBuilder, tidyForInput } from "../src/builder.js";
import { addCondition, all, choose, click, conditions, fill, one, optionTexts, personSchema, posted, render, type, viewHtml } from "./helpers.js";

const schema = personSchema();

function doc(...items: unknown[]) {
    return { version: 1, root: { type: "group", connector: "And", items } };
}

function condition(field: string, operation: string, values: string[], extra: object = {}) {
    return { type: "condition", connector: "And", field, operation, values, ...extra };
}

describe("starting", () => {
    it("shows an empty filter, its heading and what it means", () => {
        const page = render();

        assert.equal(one(page.root, "[data-eb-title]").textContent, "Conditions");
        assert.equal(conditions(page).length, 0);
        assert.equal(one(page.root, "[data-eb-empty]").hidden, false);
        assert.equal(one(page.root, "[data-eb-remove-group]").hidden, true, "the top level group can't be removed");
        assert.equal(one(page.root, "[data-eb-summary]").textContent, schema.labels.summaryEmpty);
    });

    it("posts a valid empty document", () => {
        const page = render();

        assert.deepEqual(posted(page), { version: 1, root: { type: "group", connector: "And", items: [] } });
    });

    it("is only started once for an element", () => {
        const page = render();

        assert.equal(FilterBuilder.attach(page.root), page.builder);
    });

    it("starts every builder on the page", async () => {
        const { initialise } = await import("../src/index.js");
        const page = render();

        assert.equal(initialise(page.document).length, 1);
    });

    it("explains what's wrong if the view has lost a part", () => {
        const page = render();
        const broken = page.document.createElement("div");

        broken.innerHTML = viewHtml(schema, "").replace("data-eb-add-group", "data-eb-nope");
        page.document.body.append(broken);

        assert.throws(() => FilterBuilder.attach(broken.querySelector<HTMLElement>("[data-eb-filter-builder]")!), /keep its data-eb-\* attributes/);
    });

    it("loads a filter that was posted earlier", () => {
        const page = render(
            doc(
                condition("Name", "Contains", ["bike"], { connector: "Or" }),
                { type: "group", connector: "And", items: [condition("Age", "Between", ["20", "30"])] },
                { type: "group", connector: "And", collection: "Contacts", items: [condition("Value", "StartsWith", ["0"])] },
            ),
        );

        assert.equal(conditions(page).length, 3);
        assert.equal(all(page.root, "[data-eb-group]").length, 3);
        assert.deepEqual(
            all(page.root, "[data-eb-title]").map((t) => t.textContent),
            ["Conditions", "Group", "Any Contacts where"],
        );
        assert.equal(one(page.root, "[data-eb-summary]").textContent, 'Name contains "bike" or (Age is between "20" and "30") and any contacts where (Value starts with "0")');
        assert.deepEqual(
            posted(page),
            doc(
                condition("Name", "Contains", ["bike"], { connector: "Or" }),
                { type: "group", connector: "And", items: [condition("Age", "Between", ["20", "30"])] },
                { type: "group", connector: "And", collection: "Contacts", items: [condition("Value", "StartsWith", ["0"])] },
            ),
        );
    });

    it("starts empty if what was posted can't be read", () => {
        const page = render("this is not json");

        assert.equal(conditions(page).length, 0);
    });
});

describe("choosing a property, a condition and a value", () => {
    it("offers the properties, and the collections in a group of their own", () => {
        const page = render();
        const select = one(addCondition(page), "[data-eb-field]");
        const texts = optionTexts(select);

        assert.equal(texts[0], schema.labels.fieldPlaceholder);
        assert.ok(texts.includes("Name"));
        assert.ok(texts.includes("Home address › Postcode"));
        assert.ok(!select.querySelector(":scope > option[value='collection:Contacts']"), "collections are not among the plain properties");
        assert.ok(select.querySelector("optgroup > option[value='collection:Contacts']"));
        assert.equal(select.querySelector("optgroup")?.label, schema.labels.collections);
        assert.ok(!texts.includes("Secret"));
    });

    it("selects the first suitable condition, so only a value is left to type", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Name");

        const operation = one<HTMLSelectElement>(row, "[data-eb-operation]");

        assert.equal(operation.value, "Equal");
        assert.equal(operation.disabled, false);
        assert.ok(optionTexts(operation).includes("Contains"));
        assert.ok(!optionTexts(operation).includes("Is greater than"));
    });

    it("keeps the condition disabled until a property is chosen", () => {
        const page = render();
        const row = addCondition(page);

        assert.equal(one<HTMLSelectElement>(row, "[data-eb-operation]").disabled, true);
        assert.equal(all(row, "[data-eb-values] *").length, 0);
    });

    it("posts what was chosen and typed", () => {
        const page = render();

        fill(addCondition(page), "Name", "Contains", "bike");

        assert.deepEqual(posted(page).root.items, [condition("Name", "Contains", ["bike"])]);
        assert.equal(one(page.root, "[data-eb-summary]").textContent, 'Name contains "bike"');
    });

    it("lets a number be typed as a number", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Age", "Equal");

        const input = one<HTMLInputElement>(row, "[data-eb-values] input");

        assert.equal(input.type, "number");
        assert.equal(input.step, "1");

        fill(row, "Salary");
        assert.equal(one<HTMLInputElement>(row, "[data-eb-values] input").step, "any");
    });

    it("gives a range two boxes", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Age", "Between", "20", "30");

        assert.equal(all(row, "[data-eb-values] input").length, 2);
        assert.equal(one(row, ".eb-value-joiner").textContent, "and");
        assert.deepEqual(posted(page).root.items[0].values, ["20", "30"]);
    });

    it("lets several values be added and removed", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Id", "In", "1");

        assert.equal(all(row, ".eb-value").length, 1);
        assert.equal(all(row, ".eb-btn-remove-value").length, 0, "there is no point removing the only value");

        click(one(row, ".eb-btn-add-value"));
        click(one(row, ".eb-btn-add-value"));
        all(row, ".eb-value input").forEach((input, i) => type(input, String(i + 1)));

        assert.deepEqual(posted(page).root.items[0].values, ["1", "2", "3"]);

        click(all(row, ".eb-btn-remove-value")[1]!);

        assert.deepEqual(posted(page).root.items[0].values, ["1", "3"]);
        assert.deepEqual(
            all(row, ".eb-value input").map((i) => (i as HTMLInputElement).value),
            ["1", "3"],
        );
    });

    it("shows nothing to type for a condition with no value", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Email", "IsNull");

        assert.equal(all(row, "[data-eb-values] *").length, 0);
        assert.deepEqual(posted(page).root.items[0].values, []);
    });

    it("offers an enum's members, and posts the member's name", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Favourite", "Equal");

        const select = one<HTMLSelectElement>(row, "[data-eb-values] select");

        assert.deepEqual(optionTexts(select), ["", "Red", "Green", "Deep blue"]);

        choose(select, "Blue");

        assert.deepEqual(posted(page).root.items[0].values, ["Blue"]);
    });

    it("offers yes and no for a boolean", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Active", "Equal");
        choose(one(row, "[data-eb-values] select"), "true");

        assert.deepEqual(optionTexts(one(row, "[data-eb-values] select")), ["", "Yes", "No"]);
        assert.deepEqual(posted(page).root.items[0].values, ["true"]);
    });

    it("uses the right kind of box for dates and times", () => {
        const page = render();
        const row = addCondition(page);
        const kind = (field: string) => (fill(row, field, "Equal"), one<HTMLInputElement>(row, "[data-eb-values] input").type);

        assert.equal(kind("Born"), "date");
        assert.equal(kind("LastSeen"), "datetime-local");
        assert.equal(kind("Joined"), "date");
        assert.equal(kind("StartsAt"), "time");
        assert.equal(kind("Name"), "text");
        assert.equal(kind("Reference"), "text");
    });

    it("tidies a loaded date to what the box can show, and posts that", () => {
        const page = render(doc(condition("Born", "Equal", ["1912-06-23T00:00:00"]), condition("LastSeen", "Equal", ["2024-01-10T09:30:00"])));

        assert.deepEqual(
            all(page.root, "[data-eb-values] input").map((i) => (i as HTMLInputElement).value),
            ["1912-06-23", "2024-01-10T09:30"],
        );
        assert.deepEqual(posted(page).root.items.map((i: { values: string[] }) => i.values[0]), ["1912-06-23", "2024-01-10T09:30"]);
    });

    it("keeps the condition when the property changes to one that suits it", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Name", "Contains", "x");
        choose(one(row, "[data-eb-field]"), "Email");

        assert.deepEqual(posted(page).root.items, [condition("Email", "Contains", ["x"])]);
        assert.equal(one<HTMLInputElement>(row, "[data-eb-values] input").value, "x");
    });

    it("clears the value when the property changes to a different kind", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Name", "Equal", "x");
        choose(one(row, "[data-eb-field]"), "Age");

        assert.deepEqual(posted(page).root.items, [condition("Age", "Equal", [""])]);
    });

    it("can go back to no property", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Name", "Equal", "x");
        choose(one(row, "[data-eb-field]"), "");

        assert.deepEqual(posted(page).root.items, [condition("", "", [])]);
        assert.equal(one<HTMLSelectElement>(row, "[data-eb-operation]").disabled, true);
    });
});

describe("joining, grouping and removing", () => {
    it("puts a connector between conditions and joins to the next", () => {
        const page = render();

        fill(addCondition(page), "Name", "Equal", "a");
        fill(addCondition(page), "Name", "Equal", "b");

        const connectors = all(page.root, "[data-eb-connector]") as HTMLSelectElement[];

        assert.equal(connectors.length, 1);
        assert.deepEqual(optionTexts(connectors[0]!), ["And", "Or"]);

        choose(connectors[0]!, "Or");

        assert.equal(posted(page).root.items[0].connector, "Or");
        assert.equal(one(page.root, "[data-eb-summary]").textContent, 'Name equals "a" or Name equals "b"');
    });

    it("adds a group with a condition ready to fill in, and can remove it", () => {
        const page = render();

        click(one(page.root, "[data-eb-add-group]"));

        assert.equal(all(page.root, "[data-eb-group]").length, 2);
        assert.equal(conditions(page).length, 1);
        assert.equal(page.document.activeElement?.hasAttribute("data-eb-field"), true, "focus moves to the new condition");

        fill(conditions(page)[0]!, "Name", "Equal", "a");
        assert.equal(posted(page).root.items[0].type, "group");

        const group = all(page.root, "[data-eb-group]")[1]!;

        click(one(group, "[data-eb-remove-group]"));

        assert.equal(all(page.root, "[data-eb-group]").length, 1);
        assert.deepEqual(posted(page).root.items, []);
    });

    it("adds a condition inside a group", () => {
        const page = render();

        click(one(page.root, "[data-eb-add-group]"));
        click(one(all(page.root, "[data-eb-group]")[1]!, "[data-eb-add-condition]"));

        assert.equal(posted(page).root.items[0].items.length, 2);
    });

    it("gives every group its own buttons, however deeply it is nested", () => {
        const page = render(doc({ type: "group", connector: "And", items: [condition("Name", "Equal", ["a"])] }));
        const [outer, inner] = all(page.root, "[data-eb-group]") as [HTMLElement, HTMLElement];

        // The outer group has a nested one, so its own parts must not be confused with the inner group's
        assert.equal(one(outer, "[data-eb-empty]").hidden, true);
        assert.equal(one(outer, ":scope > .eb-group-actions [data-eb-add-condition]") !== one(inner, "[data-eb-add-condition]"), true);

        click(one(outer, ":scope > .eb-group-actions [data-eb-add-condition]"));

        const posted1 = posted(page).root;

        assert.equal(posted1.items.length, 2, "the condition went in the outer group");
        assert.equal(posted1.items[0].items.length, 1, "and not in the inner one");
    });

    it("shows the empty message only for groups that are empty", () => {
        const page = render(doc({ type: "group", connector: "And", items: [condition("Name", "Equal", ["a"])] }));

        click(one(page.root, "[data-eb-root] > [data-eb-group] > .eb-group-actions [data-eb-add-group]"));
        click(all(page.root, "[data-eb-remove]")[1]!);

        const messages = all(page.root, "[data-eb-empty]").map((m) => m.hidden);

        // In document order a group's message comes after the groups inside it: the first nested group (has
        // items), the second nested group (now empty), then the top level group (has items)
        assert.deepEqual(messages, [true, false, true]);
    });

    it("removes a condition", () => {
        const page = render();

        fill(addCondition(page), "Name", "Equal", "a");
        fill(addCondition(page), "Name", "Equal", "b");

        click(one(conditions(page)[0]!, "[data-eb-remove]"));

        assert.equal(conditions(page).length, 1);
        assert.deepEqual(posted(page).root.items.map((i: { values: string[] }) => i.values[0]), ["b"]);
        assert.equal(all(page.root, "[data-eb-connector]").length, 0);
    });

    it("turns a condition into a group when a collection is chosen", () => {
        const page = render();
        const row = addCondition(page);

        choose(one(row, "[data-eb-field]"), "collection:Contacts");

        assert.equal(conditions(page).length, 1);
        assert.deepEqual(
            all(page.root, "[data-eb-title]").map((t) => t.textContent),
            ["Conditions", "Any Contacts where"],
        );
        assert.equal(posted(page).root.items[0].collection, "Contacts");

        // Its conditions are about contacts
        const field = one<HTMLSelectElement>(conditions(page)[0]!, "[data-eb-field]");

        assert.ok(optionTexts(field).includes("Value"));
        assert.ok(!optionTexts(field).includes("Salary"));

        fill(conditions(page)[0]!, "Type", "Equal", "Email");

        assert.deepEqual(posted(page).root.items[0].items, [condition("Type", "Equal", ["Email"])]);
        assert.equal(one(page.root, "[data-eb-summary]").textContent, 'any contacts where (Type equals "Email")');
    });

    it("keeps the connector when a condition becomes a collection group", () => {
        const page = render(doc(condition("Name", "Equal", ["a"], { connector: "Or" }), condition("", "", [])));

        choose(one(conditions(page)[0]!, "[data-eb-field]"), "collection:Contacts");

        assert.equal(posted(page).root.items[0].connector, "Or");
    });

    it("filters a collection of plain values by the items themselves", () => {
        const page = render();

        choose(one(addCondition(page), "[data-eb-field]"), "collection:Tags");
        fill(conditions(page)[0]!, "", "Equal", "maths");

        assert.deepEqual(posted(page).root.items[0], { type: "group", connector: "And", collection: "Tags", items: [condition("", "Equal", ["maths"])] });
    });

    it("doesn't ask which property for the items of a collection of plain values", () => {
        const page = render();

        choose(one(addCondition(page), "[data-eb-field]"), "collection:Tags");

        const row = conditions(page)[0]!;

        assert.equal(one(row, "[data-eb-field]").hidden, true);
        assert.equal(one<HTMLSelectElement>(row, "[data-eb-operation]").value, "Equal", "a condition is already chosen");
        assert.equal(page.builder.validate(), false, "but there is still a value to type");

        type(one(row, "[data-eb-values] input"), "maths");

        assert.equal(page.builder.validate(), true);
    });

    it("can nest a collection inside a collection", () => {
        const page = render();

        choose(one(addCondition(page), "[data-eb-field]"), "collection:Contacts");
        choose(one(conditions(page)[0]!, "[data-eb-field]"), "collection:Owner.Contacts");

        const [outer, inner] = posted(page).root.items[0].items;

        assert.equal(posted(page).root.items[0].collection, "Contacts");
        assert.equal(outer.collection, "Owner.Contacts");
        assert.equal(inner, undefined);
        assert.deepEqual(
            all(page.root, "[data-eb-title]").map((t) => t.textContent),
            ["Conditions", "Any Contacts where", "Any Owner › Contacts where"],
        );
    });
});

describe("advanced options, a click away", () => {
    function open(row: HTMLElement) {
        click(one(row, "[data-eb-toggle-options]"));
    }

    it("hides the options until asked, and offers them only where they apply", () => {
        const page = render();
        const row = addCondition(page);

        assert.equal(one(row, "[data-eb-toggle-options]").hidden, true, "nothing to adjust yet");

        fill(row, "Age", "Equal", "1");
        assert.equal(one(row, "[data-eb-toggle-options]").hidden, true, "numbers have no options");

        fill(row, "Name", "Contains", "a");

        const toggle = one(row, "[data-eb-toggle-options]");

        assert.equal(toggle.hidden, false);
        assert.equal(toggle.getAttribute("aria-expanded"), "false");
        assert.equal(one(row, "[data-eb-options]").hidden, true);

        open(row);

        assert.equal(toggle.getAttribute("aria-expanded"), "true");
        assert.equal(one(row, "[data-eb-options]").hidden, false);

        open(row);
        assert.equal(one(row, "[data-eb-options]").hidden, true);
    });

    it("lets any or all of several values be chosen where it applies", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Name", "Contains", "a");
        open(row);

        const match = one<HTMLSelectElement>(row, "[data-eb-match]");

        assert.equal(one(row, "[data-eb-match-wrap]").hidden, false);
        assert.equal(match.value, "Any", "Contains matches any by default");

        choose(match, "All");
        assert.equal(posted(page).root.items[0].match, "All");

        choose(one(row, "[data-eb-operation]"), "DoesNotContain");
        assert.equal(match.value, "All", "the choice made is kept");

        choose(one(row, "[data-eb-operation]"), "StartsWith");
        assert.equal(one(row, "[data-eb-match-wrap]").hidden, true, "StartsWith can only match any");
        assert.equal(posted(page).root.items[0].match, undefined);
    });

    it("shows the default match of the operation", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Name", "DoesNotContain", "a");
        open(row);

        assert.equal(one<HTMLSelectElement>(row, "[data-eb-match]").value, "All");
    });

    it("adjusts text before comparing", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Name", "Equal", "a");
        open(row);

        const box = (name: string) => one<HTMLInputElement>(row, `[data-eb-manipulator="${name}"]`);

        box("Trim").checked = true;
        box("Trim").dispatchEvent(new page.dom.window.Event("change", { bubbles: true }));
        box("ToLower").checked = true;
        box("ToLower").dispatchEvent(new page.dom.window.Event("change", { bubbles: true }));

        assert.deepEqual(posted(page).root.items[0].manipulators, [{ name: "Trim" }, { name: "ToLower" }]);
        assert.equal(one(row, "[data-eb-toggle-options]").hasAttribute("data-eb-active"), true, "options that are set are signposted");

        box("Trim").checked = false;
        box("Trim").dispatchEvent(new page.dom.window.Event("change", { bubbles: true }));

        assert.deepEqual(posted(page).root.items[0].manipulators, [{ name: "ToLower" }]);
    });

    it("replaces text", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Name", "Equal", "a");
        open(row);

        assert.equal(one(row, "[data-eb-replace]").hidden, true);

        const box = one<HTMLInputElement>(row, '[data-eb-manipulator="Replace"]');

        box.checked = true;
        box.dispatchEvent(new page.dom.window.Event("change", { bubbles: true }));

        assert.equal(one(row, "[data-eb-replace]").hidden, false);

        type(one(row, "[data-eb-replace-find]"), " ");
        type(one(row, "[data-eb-replace-with]"), "_");

        assert.deepEqual(posted(page).root.items[0].manipulators, [{ name: "Replace", find: " ", with: "_" }]);

        box.checked = false;
        box.dispatchEvent(new page.dom.window.Event("change", { bubbles: true }));

        assert.equal(posted(page).root.items[0].manipulators, undefined);
    });

    it("shows loaded options, and keeps them open across other changes", () => {
        const page = render(doc(condition("Name", "Contains", ["a", "b"], { match: "All", manipulators: [{ name: "ToUpper" }, { name: "Replace", find: "x", with: "y" }] })));
        const row = conditions(page)[0]!;

        open(row);

        assert.equal(one<HTMLSelectElement>(row, "[data-eb-match]").value, "All");
        assert.equal(one<HTMLInputElement>(row, '[data-eb-manipulator="ToUpper"]').checked, true);
        assert.equal(one<HTMLInputElement>(row, '[data-eb-manipulator="Trim"]').checked, false);
        assert.equal(one<HTMLInputElement>(row, "[data-eb-replace-find]").value, "x");
        assert.equal(one<HTMLInputElement>(row, "[data-eb-replace-with]").value, "y");

        click(one(page.root, "[data-eb-add-condition]"));

        assert.equal(one(conditions(page)[0]!, "[data-eb-options]").hidden, false, "adding a condition does not close it");
    });

    it("drops the adjustments when the property is no longer text", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Name", "Equal", "a");
        open(row);
        click(one(row, '[data-eb-manipulator="Trim"]'));
        choose(one(row, "[data-eb-field]"), "Id");

        assert.equal(posted(page).root.items[0].manipulators, undefined);
        assert.equal(one(row, "[data-eb-toggle-options]").hidden, true);
    });
});

describe("checking before posting", () => {
    it("stops an incomplete filter being posted, and says what is missing", () => {
        const page = render();
        let prevented = false;

        addCondition(page);
        page.form.addEventListener("submit", (e) => {
            prevented = e.defaultPrevented;
            e.preventDefault();
        });

        page.form.dispatchEvent(new page.dom.window.Event("submit", { cancelable: true, bubbles: true }));

        assert.equal(prevented, true);
        assert.equal(one(conditions(page)[0]!, "[data-eb-error]").hidden, false);
        assert.equal(one(conditions(page)[0]!, "[data-eb-error]").textContent, schema.labels.fieldPlaceholder);
        assert.match(one(page.root, "[data-eb-summary]").textContent ?? "", new RegExp(schema.labels.incomplete));
        assert.equal(conditions(page)[0]!.classList.contains("eb-invalid"), true);
        assert.equal(page.document.activeElement, one(conditions(page)[0]!, "[data-eb-field]"));
    });

    it("lets a complete filter through", () => {
        const page = render();
        let prevented = true;

        fill(addCondition(page), "Name", "Equal", "a");
        page.form.addEventListener("submit", (e) => {
            prevented = e.defaultPrevented;
            e.preventDefault();
        });

        page.form.dispatchEvent(new page.dom.window.Event("submit", { cancelable: true, bubbles: true }));

        assert.equal(prevented, false);
        assert.equal(page.builder.issues.length, 0);
    });

    it("lets the server decide when asked to", () => {
        const page = render(undefined, { rootAttributes: { "data-eb-novalidate": "" } });
        let prevented = true;

        addCondition(page);
        page.form.addEventListener("submit", (e) => {
            prevented = e.defaultPrevented;
            e.preventDefault();
        });
        page.form.dispatchEvent(new page.dom.window.Event("submit", { cancelable: true, bubbles: true }));

        assert.equal(prevented, false);
    });

    it("hides the errors again once they are fixed", () => {
        const page = render();
        const row = addCondition(page);

        assert.equal(page.builder.validate(), false);
        assert.equal(one(row, "[data-eb-error]").hidden, false);

        fill(row, "Name", "Equal", "a");

        assert.equal(one(row, "[data-eb-error]").hidden, true);
        assert.equal(row.classList.contains("eb-invalid"), false);
        assert.equal(page.builder.validate(), true);
    });

    it("reports an empty group", () => {
        const page = render(doc({ type: "group", connector: "And", items: [] }));

        assert.equal(page.builder.validate(), false);
        assert.equal(all(page.root, "[data-eb-group]")[1]!.classList.contains("eb-invalid"), true);
    });

    it("doesn't nag before the first attempt", () => {
        const page = render();

        addCondition(page);

        assert.equal(one(conditions(page)[0]!, "[data-eb-error]").hidden, true);
        assert.equal(page.builder.issues.length, 1);
    });
});

describe("talking to the rest of the page", () => {
    it("announces changes, with the filter", () => {
        const page = render();
        const heard: string[] = [];

        page.root.addEventListener("eb:change", (e) => heard.push((e as CustomEvent<{ json: string }>).detail.json));

        fill(addCondition(page), "Name", "Equal", "a");

        assert.ok(heard.length >= 3);
        assert.equal(heard.at(-1), page.input.value);
    });

    it("can be given a different filter", () => {
        const page = render();

        page.builder.load(JSON.stringify(doc(condition("Name", "Equal", ["loaded"]))));

        assert.equal(one<HTMLInputElement>(conditions(page)[0]!, "[data-eb-values] input").value, "loaded");
        assert.equal(page.builder.document.root.items.length, 1);
        assert.deepEqual(JSON.parse(page.builder.json), posted(page));
    });

    it("goes back to the filter the server rendered when the form is reset", async () => {
        const original = doc(condition("Name", "Equal", ["server"]));
        const page = render(original);

        type(one(conditions(page)[0]!, "[data-eb-values] input"), "changed");
        page.form.dispatchEvent(new page.dom.window.Event("reset"));
        await new Promise((resolve) => setTimeout(resolve, 5));

        assert.equal(one<HTMLInputElement>(conditions(page)[0]!, "[data-eb-values] input").value, "server");
    });

    it("names the posted input as the view says", () => {
        const page = render();

        assert.equal(page.input.name, "Filter.Json");
        assert.equal(page.input.type, "hidden");
    });
});

describe("tidying values for an input", () => {
    it("keeps the date, drops a zero seconds, and leaves the rest", () => {
        assert.equal(tidyForInput("date", "2024-01-10T00:00:00"), "2024-01-10");
        assert.equal(tidyForInput("date", "soon"), "soon");
        assert.equal(tidyForInput("datetime-local", "2024-01-10 09:30:00"), "2024-01-10T09:30");
        assert.equal(tidyForInput("datetime-local", "2024-01-10T09:30:15"), "2024-01-10T09:30:15");
        assert.equal(tidyForInput("time", "09:30:00"), "09:30:00");
        assert.equal(tidyForInput("text", "x"), "x");
    });
});
