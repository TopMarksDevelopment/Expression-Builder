import assert from "node:assert/strict";
import { describe as suite, it } from "node:test";
import * as M from "../src/model.js";
import type { ConditionNode, GroupNode } from "../src/types.js";
import { personSchema } from "./helpers.js";

const schema = personSchema();
const root = schema.root;

function field(path: string, typeId = root) {
    const found = M.findField(schema, typeId, path);

    assert.ok(found, `The schema has no ${path}`);

    return found;
}

function operation(name: string) {
    const found = M.findOperation(schema, name);

    assert.ok(found, `The schema has no ${name}`);

    return found;
}

function condition(fieldPath: string, operationName: string, values: string[]): ConditionNode {
    return { type: "condition", connector: "And", field: fieldPath, operation: operationName, values };
}

suite("parsing and writing", () => {
    it("starts with an empty group", () => {
        const document = M.emptyDocument();

        assert.deepEqual(document, { version: 1, root: { type: "group", connector: "And", items: [] } });
    });

    it("reads nothing, or rubbish, as an empty filter", () => {
        for (const text of [undefined, null, "", "   ", "{", "[]", "null", '"text"', '{"version":2}', '{"version":1}', '{"version":1,"root":{"type":"condition"}}'])
            assert.deepEqual(M.parseDocument(text), M.emptyDocument(), String(text));
    });

    it("round trips a filter", () => {
        const group: GroupNode = {
            type: "group",
            connector: "And",
            items: [
                { ...condition("Name", "Contains", ["a", "b"]), connector: "Or", match: "All", manipulators: [{ name: "ToLower" }, { name: "Replace", find: " ", with: "" }] },
                { type: "group", connector: "And", collection: "Contacts", items: [condition("Value", "StartsWith", ["0"])] },
            ],
        };
        const text = M.serialise({ version: 1, root: group });

        assert.deepEqual(M.parseDocument(text).root, group);
    });

    it("ignores what it doesn't understand inside a filter", () => {
        const parsed = M.parseDocument(
            JSON.stringify({
                version: 1,
                root: {
                    type: "group",
                    items: [
                        { type: "other" },
                        null,
                        "text",
                        { type: "condition", field: "Name", operation: "Equal", values: [1, null, "x"], match: "Some", connector: "or", manipulators: [{ name: "Nope" }, { name: "Trim" }, 4] },
                    ],
                },
            }),
        );

        assert.equal(parsed.root.items.length, 1);

        const read = parsed.root.items[0] as ConditionNode;

        assert.deepEqual(read.values, ["1", "", "x"]);
        assert.equal(read.connector, "Or");
        assert.equal(read.match, undefined);
        assert.deepEqual(read.manipulators, [{ name: "Trim" }]);
    });
});

suite("looking things up", () => {
    it("finds fields and operations", () => {
        assert.equal(field("Name").kind, "string");
        assert.equal(M.findField(schema, root, "Nope"), undefined);
        assert.equal(M.findField(schema, "t99", "Name"), undefined);
        assert.equal(operation("Between").arity, "two");
        assert.equal(M.findOperation(schema, "Nope"), undefined);
    });

    it("lists the operations of a field in the schema's order", () => {
        const names = M.operationsFor(schema, field("Age")).map((o) => o.name);

        assert.deepEqual(names.slice(0, 3), ["Equal", "NotEqual", "GreaterThan"]);
        assert.deepEqual(M.operationsFor(schema, undefined), []);
    });

    it("complains about a type it doesn't have", () => {
        assert.throws(() => M.typeOf(schema, "t99"), /no type/);
    });

    it("works out the type a group is about", () => {
        const contacts = field("Contacts");
        const group: GroupNode = M.newGroup("Contacts");

        assert.equal(M.typeIdOf(schema, root, group), contacts.itemType);
        assert.equal(M.typeIdOf(schema, root, M.newGroup()), root);
        assert.equal(M.typeIdOf(schema, root, M.newGroup("Nope")), root);
    });
});

suite("changing conditions", () => {
    it("picks the first suitable operation when a property is chosen", () => {
        const c = M.newCondition();

        M.chooseField(schema, root, c, field("Name"));

        assert.equal(c.field, "Name");
        assert.equal(c.operation, "Equal");
        assert.deepEqual(c.values, [""]);
    });

    it("keeps the operation and values when the new property suits them", () => {
        const c = condition("Name", "Contains", ["bike"]);

        M.chooseField(schema, root, c, field("Email"));

        assert.equal(c.operation, "Contains");
        assert.deepEqual(c.values, ["bike"]);
    });

    it("drops the values when the kind of property changes", () => {
        const c = condition("Name", "Equal", ["bike"]);

        M.chooseField(schema, root, c, field("Age"));

        assert.equal(c.operation, "Equal");
        assert.deepEqual(c.values, [""]);
    });

    it("drops the adjustments when the kind of property changes", () => {
        const c: ConditionNode = { ...condition("Name", "Contains", ["x"]), match: "All", manipulators: [{ name: "Trim" }] };

        M.chooseField(schema, root, c, field("Age"));

        assert.equal(c.match, undefined);
        assert.equal(c.manipulators, undefined);
    });

    it("picks another operation when the old one doesn't suit the new property", () => {
        const c = condition("Name", "Contains", ["bike"]);

        M.chooseField(schema, root, c, field("Active"));

        assert.equal(c.operation, "Equal");
    });

    it("clears everything when no property is chosen", () => {
        const c = condition("Name", "Contains", ["bike"]);

        M.chooseField(schema, root, c, undefined);

        assert.equal(c.field, "");
        assert.equal(c.operation, "");
        assert.deepEqual(c.values, []);
    });

    it("makes the values fit the operation", () => {
        const c = condition("Age", "Equal", ["5", "6"]);

        M.chooseOperation(schema, c, operation("Equal"));
        assert.deepEqual(c.values, ["5"]);

        M.chooseOperation(schema, c, operation("Between"));
        assert.deepEqual(c.values, ["5", ""]);

        M.chooseOperation(schema, c, operation("In"));
        assert.deepEqual(c.values, ["5", ""]);

        M.chooseOperation(schema, c, operation("IsNull"));
        assert.deepEqual(c.values, []);

        M.chooseOperation(schema, c, operation("In"));
        assert.deepEqual(c.values, [""]);

        M.chooseOperation(schema, c, undefined);
        assert.equal(c.operation, "");
        assert.deepEqual(c.values, []);
    });

    it("forgets the match when the operation has no use for it", () => {
        const c: ConditionNode = { ...condition("Name", "Contains", ["a", "b"]), match: "All" };

        M.chooseOperation(schema, c, operation("Equal"));

        assert.equal(c.match, undefined);

        const keep: ConditionNode = { ...condition("Name", "Contains", ["a", "b"]), match: "All" };

        M.chooseOperation(schema, keep, operation("DoesNotContain"));
        assert.equal(keep.match, "All");
    });

    it("adds and removes adjustments, once each", () => {
        const c = condition("Name", "Equal", ["a"]);

        M.toggleManipulator(c, { name: "Trim" }, true);
        M.toggleManipulator(c, { name: "Trim" }, true);
        M.toggleManipulator(c, { name: "ToLower" }, true);

        assert.deepEqual(c.manipulators?.map((m) => m.name), ["Trim", "ToLower"]);
        assert.ok(M.hasManipulator(c, "Trim"));

        M.toggleManipulator(c, { name: "Trim" }, false);
        M.toggleManipulator(c, { name: "ToLower" }, false);

        assert.equal(c.manipulators, undefined);
        assert.equal(M.hasManipulator(c, "Trim"), undefined);
    });

    it("knows when advanced options are set", () => {
        const c = condition("Name", "Contains", ["a"]);

        assert.equal(M.hasAdvancedOptions(c, schema), false);

        c.match = "All";
        assert.equal(M.hasAdvancedOptions(c, schema), true);

        // The match means nothing to an operation that doesn't use it
        const equal = { ...condition("Name", "Equal", ["a"]), match: "All" as const };

        assert.equal(M.hasAdvancedOptions(equal, schema), false);

        M.toggleManipulator(equal, { name: "Trim" }, true);
        assert.equal(M.hasAdvancedOptions(equal, schema), true);
    });

    it("says how many values an operation needs to start with", () => {
        assert.equal(M.minimumValues(operation("IsNull")), 0);
        assert.equal(M.minimumValues(operation("Equal")), 1);
        assert.equal(M.minimumValues(operation("Between")), 2);
        assert.equal(M.minimumValues(operation("In")), 1);
        assert.equal(M.minimumValues(undefined), 0);
    });
});

suite("finding what is incomplete", () => {
    const issues = (items: GroupNode["items"]) => M.findIssues(schema, { type: "group", items }, root);

    it("is happy with nothing, and with a complete filter", () => {
        assert.deepEqual(issues([]), []);
        assert.deepEqual(issues([condition("Name", "Equal", ["a"]), condition("Age", "Between", ["1", "2"]), condition("Email", "IsNull", [])]), []);
    });

    it("wants a property, an operation and every value", () => {
        const noField = M.newCondition();
        const noOperation = condition("Name", "", []);
        const noValue = condition("Name", "Equal", [""]);
        const missingTo = condition("Age", "Between", ["1", ""]);
        const noValues = condition("Name", "Contains", []);

        const found = issues([noField, noOperation, noValue, missingTo, noValues]);

        assert.deepEqual(found.map((i) => i.node), [noField, noOperation, noValue, missingTo, noValues]);
        assert.equal(found[0]?.message, schema.labels.fieldPlaceholder);
        assert.equal(found[1]?.message, schema.labels.operationPlaceholder);
        assert.equal(found[2]?.message, schema.labels.required);
    });

    it("rejects an operation that doesn't suit the property", () => {
        assert.equal(issues([condition("Salary", "Contains", ["a"])]).length, 1);
    });

    it("wants the text to find when replacing", () => {
        const c = condition("Name", "Equal", ["a"]);

        M.toggleManipulator(c, { name: "Replace", find: "", with: "x" }, true);

        assert.equal(issues([c])[0]?.message, schema.labels.replaceFind);
    });

    it("looks inside groups, using the type of a collection's items", () => {
        const empty = M.newGroup();
        const inner = condition("Value", "StartsWith", ["0"]);
        const wrong = condition("Name", "Equal", ["x"]);
        const group: GroupNode = { type: "group", collection: "Contacts", items: [inner, wrong] };

        const found = issues([empty, group]);

        // The empty group, and Name which a contact doesn't have
        assert.deepEqual(found.map((i) => i.node), [empty, wrong]);
    });
});

suite("describing a filter", () => {
    const describe = (items: GroupNode["items"]) => M.describe(schema, { version: 1, root: { type: "group", items } });

    it("says so when there is nothing", () => {
        assert.equal(describe([]), schema.labels.summaryEmpty);
        assert.equal(describe([M.newCondition()]), schema.labels.summaryEmpty);
    });

    it("writes conditions in words", () => {
        const text = describe([
            { ...condition("Name", "Contains", ["bike", "car"]), connector: "Or" },
            condition("Age", "Between", ["20", "30"]),
        ]);

        assert.equal(text, 'Name contains "bike", "car" or Age is between "20" and "30"');
    });

    it("writes groups in brackets and collections with their heading", () => {
        const text = describe([
            { type: "group", items: [condition("Active", "Equal", ["true"])], connector: "And" },
            { type: "group", collection: "Contacts", items: [condition("Type", "Equal", ["Email"])] },
        ]);

        assert.equal(text, '(Active equals "Yes") and any contacts where (Type equals "Email")');
    });

    it("skips what is unfinished, and doesn't leave a dangling connector", () => {
        const text = describe([
            M.newCondition(),
            condition("Name", "Equal", [""]),
            condition("Age", "Between", ["1", ""]),
            condition("Name", "IsNull", []),
            { type: "group", items: [] },
        ]);

        assert.equal(text, "Name is not set");
    });

    it("shows dates tidily, and enums by their labels", () => {
        const text = describe([condition("LastSeen", "Equal", ["2024-01-10T09:30"]), condition("Favourite", "Equal", ["Blue"])]);

        assert.equal(text, 'Last seen equals "2024-01-10 09:30" and Favourite equals "Deep blue"');
    });

    it("counts conditions in groups", () => {
        const group: GroupNode = {
            type: "group",
            items: [condition("Name", "IsNull", []), { type: "group", items: [condition("Name", "IsNull", []), M.newCondition()] }],
        };

        assert.equal(M.countConditions(group), 3);
    });
});
