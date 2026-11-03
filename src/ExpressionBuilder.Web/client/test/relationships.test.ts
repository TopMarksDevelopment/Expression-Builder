import assert from "node:assert/strict";
import { describe, it } from "node:test";
import * as M from "../src/model.js";
import { addCondition, all, choose, click, conditions, fill, one, optionTexts, personSchema, posted, render } from "./helpers.js";

const schema = personSchema();

function doc(...items: unknown[]) {
    return { version: 1, root: { type: "group", connector: "And", items } };
}

function condition(field: string, operation: string, values: string[], extra: object = {}) {
    return { type: "condition", connector: "And", field, operation, values, ...extra };
}

/** The steps of the path above the list of properties, as the user reads them. */
function trail(row: HTMLElement): string[] {
    const path = one(row, "[data-eb-path]");

    return path.hidden ? [] : all(path, "[data-eb-path-step]").map((s) => s.textContent ?? "");
}

function choices(row: HTMLElement): string[] {
    return all(one(row, "[data-eb-field]"), "option").map((o) => (o as HTMLOptionElement).value);
}

describe("following relationships in the list of properties", () => {
    it("moves a relationship into the path above the list, which then offers what it leads to", () => {
        const page = render();
        const row = addCondition(page);

        assert.deepEqual(trail(row), []);

        choose(one(row, "[data-eb-field]"), "relationship:Home");

        assert.deepEqual(trail(row), ["Person", "Home address"]);
        assert.ok(choices(row).includes("Postcode"));
        assert.ok(!choices(row).includes("Name"), "the list is about the address now, not the person");
        assert.ok(!choices(row).includes("relationship:Home"));
        assert.equal(one<HTMLSelectElement>(row, "[data-eb-field]").value, "", "nothing is chosen yet");
        assert.equal(one<HTMLSelectElement>(row, "[data-eb-operation]").disabled, true);
        assert.equal(posted(page).root.items[0].field, "", "a path that leads nowhere is not posted as a property");
    });

    it("keeps following, and posts the whole path once a property is chosen", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Home.Country.Name", "Equal", "UK");

        assert.deepEqual(trail(row), ["Person", "Home address", "Country"]);
        assert.equal(one<HTMLSelectElement>(row, "[data-eb-field]").value, "Name");
        assert.deepEqual(posted(page).root.items[0], condition("Home.Country.Name", "Equal", ["UK"]));
        assert.equal(one(page.root, "[data-eb-summary]").textContent, 'Home address › Country › Name equals "UK"');
    });

    it("makes the earlier steps a way back, and the last one just where the list is looking", () => {
        const page = render();
        const row = addCondition(page);

        fill(row, "Home.Country.Name", "Equal", "UK");

        const steps = all(one(row, "[data-eb-path]"), "[data-eb-path-step]") as HTMLButtonElement[];

        assert.deepEqual(
            steps.map((s) => s.disabled),
            [false, false, true],
        );
        assert.equal(steps[0]!.getAttribute("aria-label"), "Back to Person");
        assert.equal(steps[2]!.getAttribute("aria-current"), "step");

        click(steps[1]!);

        assert.deepEqual(trail(row), ["Person", "Home address"]);
        assert.ok(choices(row).includes("Postcode"));
        assert.equal(one<HTMLSelectElement>(row, "[data-eb-field]").value, "");
        assert.equal(posted(page).root.items[0].field, "", "going back leaves the property to choose again");
        assert.equal(page.document.activeElement, one(row, "[data-eb-field]"));

        click(all(one(row, "[data-eb-path]"), "[data-eb-path-step]")[0]!);

        assert.deepEqual(trail(row), []);
        assert.ok(choices(row).includes("Name"));
    });

    it("shows the path of a filter that was loaded", () => {
        const page = render(doc(condition("Home.Country.Name", "Equal", ["UK"])));
        const row = conditions(page)[0]!;

        assert.deepEqual(trail(row), ["Person", "Home address", "Country"]);
        assert.equal(one<HTMLSelectElement>(row, "[data-eb-field]").value, "Name");
        assert.equal(one<HTMLSelectElement>(row, "[data-eb-operation]").value, "Equal");
    });

    it("lets a relationship itself be asked about", () => {
        const page = render();
        const row = addCondition(page);

        choose(one(row, "[data-eb-field]"), "relationship:Home");

        assert.ok(optionTexts(one(row, "[data-eb-field]")).includes("Home address itself"));

        choose(one(row, "[data-eb-field]"), "@itself");

        assert.equal(posted(page).root.items[0].field, "Home");
        assert.deepEqual(trail(row), ["Person", "Home address"], "the path stays where it was");

        choose(one(row, "[data-eb-operation]"), "IsNotNull");

        assert.deepEqual(posted(page).root.items[0], condition("Home", "IsNotNull", []));

        // A saved filter that asks about the relationship opens at the same place
        const again = render(doc(condition("Home", "IsNotNull", [])));
        const loaded = conditions(again)[0]!;

        assert.deepEqual(trail(loaded), ["Person", "Home address"]);
        assert.equal(one<HTMLSelectElement>(loaded, "[data-eb-field]").value, "@itself");
    });

    it("turns a collection behind relationships into a group, and shows the way to it", () => {
        const page = render();
        const row = addCondition(page);

        choose(one(row, "[data-eb-field]"), "relationship:Employer");
        choose(one(row, "[data-eb-field]"), "collection:Branches");

        assert.equal(posted(page).root.items[0].collection, "Employer.Branches");
        assert.deepEqual(
            all(page.root, "[data-eb-title]").map((t) => t.textContent),
            ["Conditions", "of Employer › Branches where"],
        );

        // The conditions in it are about branches
        assert.ok(choices(conditions(page)[0]!).includes("Town"));

        fill(conditions(page)[0]!, "Staff", "GreaterThan", "100");

        assert.equal(one(page.root, "[data-eb-summary]").textContent, 'any item of employer › branches where (Staff is greater than "100")');
    });

    it("stops offering relationships where the schema's limit is reached", () => {
        const limited = { ...schema, maxNavigationDepth: 2 };
        const page = render(undefined, { schema: limited });
        const row = addCondition(page);

        choose(one(row, "[data-eb-field]"), "relationship:Home");

        // Home.Postcode and Home.Country are two names; Home.Country.Name would be three
        assert.ok(choices(row).includes("Postcode"));
        assert.ok(choices(row).includes("relationship:Country"));

        choose(one(row, "[data-eb-field]"), "relationship:Country");

        assert.deepEqual(trail(row), ["Person", "Home address", "Country"]);
        assert.deepEqual(choices(row), ["", "@itself"], "only the relationship itself, as nothing more would fit");
    });

    it("has nothing to follow when the limit is one name", () => {
        const page = render(undefined, { schema: { ...schema, maxNavigationDepth: 1 } });
        const row = addCondition(page);

        assert.ok(choices(row).includes("Name"));
        assert.ok(choices(row).includes("relationship:Home"), "it can still be asked whether it is set");

        choose(one(row, "[data-eb-field]"), "relationship:Home");

        assert.deepEqual(choices(row), ["", "@itself"]);
    });
});

describe("a group on a collection matches any item, or every item", () => {
    it("is offered on a collection group only, and says any item to start with", () => {
        const page = render(
            doc(
                { type: "group", connector: "And", items: [condition("Name", "Equal", ["a"])] },
                { type: "group", connector: "And", collection: "Contacts", items: [condition("Value", "Equal", ["x"])] },
            ),
        );
        const [plain, collection] = all(page.root, "[data-eb-group]").slice(1) as HTMLElement[];

        assert.equal(one(plain!, "[data-eb-group-match]").hidden, true);

        const match = one<HTMLSelectElement>(collection!, "[data-eb-group-match]");

        assert.equal(match.hidden, false);
        assert.equal(match.value, "Any");
        assert.deepEqual(optionTexts(match), ["Any item", "Every item"]);
    });

    it("posts all, and leaves it out again for any", () => {
        const page = render();

        choose(one(addCondition(page), "[data-eb-field]"), "collection:Contacts");
        fill(conditions(page)[0]!, "Type", "Equal", "Email");

        const match = one<HTMLSelectElement>(page.root, "[data-eb-group] [data-eb-group] [data-eb-group-match]");

        choose(match, "All");

        assert.equal(posted(page).root.items[0].match, "All");
        assert.equal(one(page.root, "[data-eb-summary]").textContent, 'every item of contacts where (Type equals "Email")');

        choose(match, "Any");

        assert.equal("match" in posted(page).root.items[0], false);
        assert.equal(one(page.root, "[data-eb-summary]").textContent, 'any item of contacts where (Type equals "Email")');
    });

    it("loads a group that asks for every item", () => {
        const page = render(
            doc({ type: "group", connector: "And", collection: "Contacts", match: "All", items: [condition("Value", "Equal", ["x"])] }),
        );

        assert.equal(one<HTMLSelectElement>(page.root, "[data-eb-group] [data-eb-group] [data-eb-group-match]").value, "All");
        assert.deepEqual(posted(page).root.items[0].match, "All");
    });

    it("ignores a match on a group that is not for a collection", () => {
        const page = render(doc({ type: "group", connector: "And", match: "All", items: [condition("Name", "Equal", ["a"])] }));

        assert.equal("match" in posted(page).root.items[0], false);
    });

    it("keeps the group's own connector to the next item", () => {
        const page = render(
            doc(
                { type: "group", connector: "Or", collection: "Contacts", match: "All", items: [condition("Value", "Equal", ["x"])] },
                condition("Name", "Equal", ["a"]),
            ),
        );

        assert.equal(posted(page).root.items[0].connector, "Or");
        assert.equal(one<HTMLSelectElement>(page.root, "[data-eb-connector]").value, "Or");
    });
});

describe("finding properties through relationships", () => {
    it("follows relationships one name at a time, and labels the way", () => {
        const found = M.findField(schema, schema.root, "Home.Country.Name");

        assert.equal(found?.path, "Home.Country.Name");
        assert.equal(found?.label, "Home address › Country › Name");
        assert.equal(found?.kind, "string");
    });

    it("finds nothing the schema doesn't offer", () => {
        for (const path of ["Home.Nope", "Name.Length", "Home..City", "Home.", ".Home", "Contacts.Value", "Nope.Name"])
            assert.equal(M.findField(schema, schema.root, path), undefined, path);
    });

    it("refuses a path that is longer than the schema allows", () => {
        assert.ok(M.findField({ ...schema, maxNavigationDepth: 3 }, schema.root, "Home.Country.Name"));
        assert.equal(M.findField({ ...schema, maxNavigationDepth: 2 }, schema.root, "Home.Country.Name"), undefined);
    });

    it("offers a class's own properties, collections and relationships at each step", () => {
        const atRoot = M.choicesAt(schema, schema.root, "")!;

        assert.ok(atRoot.properties.some((f) => f.path === "Name"));
        assert.ok(atRoot.collections.some((f) => f.path === "Contacts"));
        assert.ok(atRoot.relationships.some((f) => f.path === "Home"));
        assert.equal(atRoot.itself, undefined);
        assert.ok(!atRoot.properties.some((f) => f.kind === "object" || f.kind === "collection"));

        const atHome = M.choicesAt(schema, schema.root, "Home")!;

        assert.ok(atHome.properties.some((f) => f.path === "Postcode"));
        assert.equal(atHome.itself?.path, "Home");
        assert.equal(M.choicesAt(schema, schema.root, "Name"), undefined, "a property is not a step");
    });

    it("says which relationships a condition's property is behind", () => {
        assert.equal(M.pathOf(schema, schema.root, "Name"), "");
        assert.equal(M.pathOf(schema, schema.root, "Home.Country.Name"), "Home.Country");
        assert.equal(M.pathOf(schema, schema.root, "Home.Country"), "Home.Country", "a relationship asked about is the step");
        assert.equal(M.pathOf(schema, schema.root, ""), "");
    });
});
