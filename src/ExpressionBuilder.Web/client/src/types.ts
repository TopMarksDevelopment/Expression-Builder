/**
 * The shapes the filter builder works with.
 *
 * The schema is written by the server (`FilterSchema.ToJson`) and says what can be filtered. The document
 * is what the script builds and posts back (`FilterBuilderModel.Json`); the server checks it against the
 * same schema, so nothing here is trusted.
 */

export type FieldKind =
    | "string"
    | "number"
    | "date"
    | "dateTime"
    | "time"
    | "boolean"
    | "enum"
    | "guid"
    | "object"
    | "collection";

export type ValueArity = "none" | "one" | "two" | "many";

export type Connector = "And" | "Or";

export type Matches = "Any" | "All";

export interface EnumOption {
    value: string;
    label: string;
}

export interface FieldSchema {
    /** `Name`, `Birth.Country`; empty for the item of a collection of plain values */
    path: string;
    label: string;
    kind: FieldKind;
    nullable: boolean;
    /** `text`, `number`, `date`, `datetime-local`, `time`, `boolean`, `select` or empty */
    input: string;
    step?: string;
    options?: EnumOption[];
    /** For collections, the id of the type of the items */
    itemType?: string;
    /** The names of the operations that suit the field */
    operations: string[];
}

export interface TypeSchema {
    id: string;
    label: string;
    fields: FieldSchema[];
}

export interface OperationSchema {
    name: string;
    label: string;
    arity: ValueArity;
    supportsMatch: boolean;
    defaultMatch: Matches;
    /** The heading it is listed under; absent for the built-in operations */
    group?: string;
}

export interface ManipulatorParameter {
    label: string;
    required: boolean;
}

/** An adjustment of the text before it is compared. */
export interface ManipulatorSchema {
    name: string;
    label: string;
    /** What the user is asked for, in the order it is posted */
    parameters: ManipulatorParameter[];
    /** The kinds of property it suits */
    kinds: FieldKind[];
    /** The heading it is listed under; absent for the built-in manipulators */
    group?: string;
}

export interface Labels {
    conditions: string;
    group: string;
    collectionGroup: string;
    emptyGroup: string;
    addCondition: string;
    addGroup: string;
    collections: string;
    removeCondition: string;
    removeGroup: string;
    and: string;
    or: string;
    connector: string;
    field: string;
    fieldPlaceholder: string;
    operation: string;
    operationPlaceholder: string;
    value: string;
    valueFrom: string;
    valueTo: string;
    addValue: string;
    removeValue: string;
    options: string;
    match: string;
    matchAny: string;
    matchAll: string;
    manipulators: string;
    standardGroup: string;
    customGroup: string;
    trim: string;
    toLower: string;
    toUpper: string;
    replace: string;
    replaceFind: string;
    replaceWith: string;
    yes: string;
    no: string;
    summaryEmpty: string;
    incomplete: string;
    required: string;
}

export interface Schema {
    root: string;
    types: Record<string, TypeSchema>;
    operations: OperationSchema[];
    manipulators: ManipulatorSchema[];
    labels: Labels;
}

export interface ManipulatorNode {
    name: string;
    /** The text it was given, one item for each of its parameters */
    args?: string[];
}

export interface ConditionNode {
    type: "condition";
    connector?: Connector;
    field: string;
    operation: string;
    values: string[];
    match?: Matches;
    manipulators?: ManipulatorNode[];
}

export interface GroupNode {
    type: "group";
    connector?: Connector;
    /** The collection whose items the group filters, if it filters one */
    collection?: string;
    items: FilterNode[];
}

export type FilterNode = ConditionNode | GroupNode;

export interface FilterDocument {
    version: 1;
    root: GroupNode;
}

/** Something wrong with a condition or group, found before posting. */
export interface Issue {
    node: FilterNode;
    message: string;
}
