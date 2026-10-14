/**
 * The state of the filter and the rules about it. Nothing here touches the page, so it can be tested
 * without a browser.
 */

import type {
    ConditionNode,
    Connector,
    FieldSchema,
    FilterDocument,
    FilterNode,
    GroupNode,
    Issue,
    Labels,
    ManipulatorNode,
    ManipulatorSchema,
    OperationSchema,
    Schema,
    TypeSchema,
} from "./types.js";

export const VERSION = 1 as const;

export function emptyDocument(): FilterDocument {
    return { version: VERSION, root: newGroup() };
}

export function newGroup(collection?: string): GroupNode {
    const group: GroupNode = { type: "group", connector: "And", items: [] };

    if (collection) group.collection = collection;

    return group;
}

/** A condition that has not been filled in yet. */
export function newCondition(): ConditionNode {
    return {
        type: "condition",
        connector: "And",
        field: "",
        operation: "",
        values: [],
    };
}

/**
 * Reads a posted document. Anything that is not understood becomes an empty filter, rather than an error:
 * the page must still load, and the server is the one that rejects bad input.
 */
export function parseDocument(json: string | null | undefined, schema?: Schema): FilterDocument {
    if (!json || !json.trim()) return emptyDocument();

    try {
        const parsed: unknown = JSON.parse(json);

        if (!isObject(parsed) || parsed.version !== VERSION) return emptyDocument();

        const root = readNode(parsed.root, schema);

        return root?.type === "group" ? { version: VERSION, root } : emptyDocument();
    } catch {
        return emptyDocument();
    }
}

export function serialise(document: FilterDocument): string {
    return JSON.stringify(document);
}

function isObject(value: unknown): value is Record<string, unknown> {
    return typeof value === "object" && value !== null && !Array.isArray(value);
}

function asString(value: unknown): string {
    return typeof value === "string" ? value : "";
}

function readConnector(value: unknown): Connector {
    return typeof value === "string" && value.toLowerCase() === "or" ? "Or" : "And";
}

function readNode(value: unknown, schema: Schema | undefined): FilterNode | null {
    if (!isObject(value)) return null;

    if (value.type === "group") {
        const group = newGroup(asString(value.collection) || undefined);

        group.connector = readConnector(value.connector);
        group.items = (Array.isArray(value.items) ? value.items : [])
            .map((item) => readNode(item, schema))
            .filter((n): n is FilterNode => n !== null);

        return group;
    }

    if (value.type === "condition") {
        const condition = newCondition();

        condition.connector = readConnector(value.connector);
        condition.field = asString(value.field);
        condition.operation = asString(value.operation);
        condition.values = (Array.isArray(value.values) ? value.values : []).map((v) =>
            v === null || v === undefined ? "" : String(v),
        );

        if (value.match === "Any" || value.match === "All") condition.match = value.match;

        const manipulators = (Array.isArray(value.manipulators) ? value.manipulators : [])
            .map((m) => readManipulator(m, schema))
            .filter((m): m is ManipulatorNode => m !== null);

        if (manipulators.length) condition.manipulators = manipulators;

        return condition;
    }

    return null;
}

function readManipulator(value: unknown, schema: Schema | undefined): ManipulatorNode | null {
    if (!isObject(value)) return null;

    const name = asString(value.name);

    if (!name) return null;

    // Something this builder does not offer is dropped, rather than posted for the server to refuse
    if (schema && !findManipulator(schema, name)) return null;

    const manipulator: ManipulatorNode = { name };

    if (Array.isArray(value.args) && value.args.length > 0) manipulator.args = value.args.map(asString);

    return manipulator;
}

// -- Looking things up in the schema --------------------------------------------------------

export function typeOf(schema: Schema, typeId: string): TypeSchema {
    const type = schema.types[typeId];

    if (!type) throw new Error(`The schema has no type "${typeId}"`);

    return type;
}

export function findField(schema: Schema, typeId: string, path: string): FieldSchema | undefined {
    return schema.types[typeId]?.fields.find((f) => f.path === path);
}

export function findOperation(schema: Schema, name: string): OperationSchema | undefined {
    return schema.operations.find((o) => o.name === name);
}

export function findManipulator(schema: Schema, name: string): ManipulatorSchema | undefined {
    return schema.manipulators.find((m) => m.name === name);
}

/** The adjustments that suit a field, in the order the schema lists them. */
export function manipulatorsFor(schema: Schema, field: FieldSchema | undefined): ManipulatorSchema[] {
    if (!field) return [];

    return schema.manipulators.filter((m) => m.kinds.includes(field.kind));
}

/** The operations that can be used on a field, in the order the schema lists them. */
export function operationsFor(schema: Schema, field: FieldSchema | undefined): OperationSchema[] {
    if (!field) return [];

    return schema.operations.filter((o) => field.operations.includes(o.name));
}

/** The type that a collection group filters, or the type it is inside if it is not for a collection. */
export function typeIdOf(schema: Schema, parentTypeId: string, group: GroupNode): string {
    if (!group.collection) return parentTypeId;

    return findField(schema, parentTypeId, group.collection)?.itemType ?? parentTypeId;
}

// -- Changing conditions --------------------------------------------------------------------

/** How many values an operation needs to start with. */
export function minimumValues(operation: OperationSchema | undefined): number {
    switch (operation?.arity) {
        case "one":
            return 1;
        case "two":
            return 2;
        case "many":
            return 1;
        default:
            return 0;
    }
}

/**
 * Picks a property for a condition. If the operation it already has still suits the property it is kept,
 * and so are the values that still make sense; otherwise the first suitable operation is chosen.
 */
export function chooseField(
    schema: Schema,
    typeId: string,
    condition: ConditionNode,
    field: FieldSchema | undefined,
): void {
    const previousKind = findField(schema, typeId, condition.field)?.kind;

    condition.field = field?.path ?? "";

    const operations = operationsFor(schema, field);
    const current = operations.find((o) => o.name === condition.operation);

    if (!current) condition.operation = operations[0]?.name ?? "";

    // A value for one kind of property means nothing for another
    if (previousKind !== field?.kind || !current) condition.values = [];

    if (previousKind !== field?.kind) {
        delete condition.manipulators;
        delete condition.match;
    }

    fitValues(condition, findOperation(schema, condition.operation));
}

/** Picks an operation, keeping the values that still fit. */
export function chooseOperation(schema: Schema, condition: ConditionNode, operation: OperationSchema | undefined): void {
    condition.operation = operation?.name ?? "";

    if (operation && !operation.supportsMatch) delete condition.match;

    fitValues(condition, operation);
}

/** Makes the number of values match what the operation takes. */
export function fitValues(condition: ConditionNode, operation: OperationSchema | undefined): void {
    switch (operation?.arity) {
        case "none":
        case undefined:
            condition.values = [];
            break;
        case "one":
            condition.values = [condition.values[0] ?? ""];
            break;
        case "two":
            condition.values = [condition.values[0] ?? "", condition.values[1] ?? ""];
            break;
        case "many":
            if (condition.values.length === 0) condition.values = [""];
            break;
    }
}

export function toggleManipulator(condition: ConditionNode, manipulator: ManipulatorNode, on: boolean): void {
    const rest = (condition.manipulators ?? []).filter((m) => m.name !== manipulator.name);

    condition.manipulators = on ? [...rest, manipulator] : rest;

    if (condition.manipulators.length === 0) delete condition.manipulators;
}

export function hasManipulator(condition: ConditionNode, name: string): ManipulatorNode | undefined {
    return condition.manipulators?.find((m) => m.name === name);
}

/** Whether a condition has advanced options set, so they should be signposted when hidden. */
export function hasAdvancedOptions(condition: ConditionNode, schema: Schema): boolean {
    const operation = findOperation(schema, condition.operation);

    return (
        (operation?.supportsMatch === true && condition.match !== undefined) ||
        (condition.manipulators?.length ?? 0) > 0
    );
}

// -- Checking a filter before it is posted --------------------------------------------------

/** What is incomplete in a group (and the groups and conditions in it). */
export function findIssues(schema: Schema, group: GroupNode, typeId: string, issues: Issue[] = []): Issue[] {
    const labels = schema.labels;

    for (const item of group.items) {
        if (item.type === "group") {
            const childType = typeIdOf(schema, typeId, item);

            if (item.items.length === 0)
                issues.push({ node: item, message: labels.emptyGroup });
            else findIssues(schema, item, childType, issues);

            continue;
        }

        const field = findField(schema, typeId, item.field);
        const operation = findOperation(schema, item.operation);

        if (!field) {
            issues.push({ node: item, message: `${labels.fieldPlaceholder}` });
        } else if (!operation || !field.operations.includes(operation.name)) {
            issues.push({ node: item, message: labels.operationPlaceholder });
        } else if (item.values.length < minimumValues(operation) || item.values.some((v) => v === "")) {
            issues.push({ node: item, message: labels.required });
        } else {
            const missing = missingArgument(schema, item);

            if (missing) issues.push({ node: item, message: missing });
        }
    }

    return issues;
}

/** The label of the first thing an adjustment still needs, if there is one. */
function missingArgument(schema: Schema, condition: ConditionNode): string | undefined {
    for (const manipulator of condition.manipulators ?? []) {
        const parameters = findManipulator(schema, manipulator.name)?.parameters ?? [];
        const missing = parameters.find((p, index) => p.required && !manipulator.args?.[index]);

        if (missing) return missing.label;
    }

    return undefined;
}

// -- Describing a filter in words -----------------------------------------------------------

/** A readable version of the filter, such as `Name contains "bike" and Price is greater than 5`. */
export function describe(schema: Schema, document: FilterDocument): string {
    const text = describeItems(schema, document.root, schema.root);

    return text || schema.labels.summaryEmpty;
}

function describeItems(schema: Schema, group: GroupNode, typeId: string): string {
    const labels = schema.labels;
    const parts: string[] = [];

    group.items.forEach((item, index) => {
        const text = item.type === "group" ? describeGroup(schema, item, typeId) : describeCondition(schema, item, typeId);

        if (!text) return;

        if (parts.length > 0) {
            const previous = group.items[index - 1];
            const connector = (previous?.connector ?? "And") === "Or" ? labels.or : labels.and;

            parts.push(connector.toLowerCase());
        }

        parts.push(text);
    });

    return parts.join(" ");
}

function describeGroup(schema: Schema, group: GroupNode, typeId: string): string {
    const inner = describeItems(schema, group, typeIdOf(schema, typeId, group));

    if (!inner) return "";

    if (!group.collection) return `(${inner})`;

    const field = findField(schema, typeId, group.collection);
    const heading = schema.labels.collectionGroup.replace("{0}", field?.label ?? group.collection);

    return `${heading.toLowerCase()} (${inner})`;
}

function describeCondition(schema: Schema, condition: ConditionNode, typeId: string): string {
    const field = findField(schema, typeId, condition.field);
    const operation = findOperation(schema, condition.operation);

    if (!field || !operation) return "";

    // A value that hasn't been filled in yet would only read as nonsense
    if (condition.values.length < minimumValues(operation) || condition.values.some((v) => v === "")) return "";

    const values = condition.values.map((v) => `"${describeValue(field, v, schema.labels)}"`);
    let joined = "";

    if (operation.arity === "two") joined = values.join(` ${schema.labels.and.toLowerCase()} `);
    else if (operation.arity !== "none") joined = values.join(", ");

    return [field.label, operation.label.toLowerCase(), joined].filter(Boolean).join(" ");
}

function describeValue(field: FieldSchema, value: string, labels: Labels): string {
    if (field.kind === "boolean") return value === "true" ? labels.yes : value === "false" ? labels.no : value;

    if (field.kind === "enum") return field.options?.find((o) => o.value === value)?.label ?? value;

    return value.replace("T", " ");
}

/** Counts every condition, however deeply it is nested. */
export function countConditions(group: GroupNode): number {
    return group.items.reduce((total, item) => total + (item.type === "group" ? countConditions(item) : 1), 0);
}
