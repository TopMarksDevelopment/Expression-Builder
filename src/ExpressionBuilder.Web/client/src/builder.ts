/**
 * The filter builder: binds the model to the markup that the Razor view renders.
 *
 * The view supplies the structure (as <template>s) and the text; this file supplies the behaviour. It
 * finds each part by its `data-eb-*` attribute, so the markup can be restyled or rearranged freely as
 * long as those attributes stay.
 */

import * as M from "./model.js";
import type {
    ConditionNode,
    FieldSchema,
    FilterDocument,
    FilterNode,
    GroupNode,
    Issue,
    OperationSchema,
    Schema,
} from "./types.js";

/** Prefix of the value of a field choice that adds a collection group instead of choosing a property. */
const COLLECTION_PREFIX = "collection:";

const CHANGE_EVENT = "eb:change";

interface Templates {
    group: HTMLTemplateElement;
    condition: HTMLTemplateElement;
    connector: HTMLTemplateElement;
}

interface Parent {
    group: GroupNode;
    typeId: string;
}

const instances = new WeakMap<HTMLElement, FilterBuilder>();

export class FilterBuilder {
    /** What can be filtered, as sent by the server. */
    readonly schema: Schema;

    readonly root: HTMLElement;

    #document: FilterDocument;
    readonly #input: HTMLInputElement;
    /** What the server rendered. A hidden input reflects its value into its attribute, so it can't be asked later. */
    readonly #renderedJson: string;
    readonly #templates: Templates;
    /** Whether the options panel of a condition is open. This is only about the display, so it is not posted. */
    readonly #optionsOpen = new WeakSet<ConditionNode>();
    readonly #elements = new Map<FilterNode, HTMLElement>();
    #showErrors = false;
    #focusAfterRender: FilterNode | undefined;
    #issues: Issue[] = [];

    private constructor(root: HTMLElement) {
        this.root = root;
        this.#input = query<HTMLInputElement>(root, "[data-eb-input]");
        this.#renderedJson = this.#input.value;
        this.schema = readSchema(root);
        this.#templates = {
            group: template(root, "group"),
            condition: template(root, "condition"),
            connector: template(root, "connector"),
        };
        this.#document = M.parseDocument(this.#input.value);

        this.#bindForm();
        this.#render();
    }

    /**
     * Starts the filter builder on an element rendered by the view. Safe to call more than once for the
     * same element.
     */
    static attach(root: HTMLElement): FilterBuilder {
        let instance = instances.get(root);

        if (!instance) {
            instance = new FilterBuilder(root);
            instances.set(root, instance);
        }

        return instance;
    }

    /** The filter as it will be posted. */
    get json(): string {
        return M.serialise(this.#document);
    }

    get document(): FilterDocument {
        return this.#document;
    }

    /** What is incomplete, if anything. */
    get issues(): readonly Issue[] {
        return this.#issues;
    }

    /** Replaces the filter, such as with one loaded from somewhere else. */
    load(json: string | null | undefined): void {
        this.#document = M.parseDocument(json);
        this.#showErrors = false;
        this.#render();
        this.#sync(false);
    }

    /**
     * Checks that the filter is complete, showing what is missing if it is not.
     * @returns true if it is complete
     */
    validate(): boolean {
        this.#showErrors = true;
        this.#sync(false);

        return this.#issues.length === 0;
    }

    // -- Rendering ----------------------------------------------------------------------------

    #render(): void {
        this.#elements.clear();

        const host = query(this.root, "[data-eb-root]");

        host.replaceChildren(this.#renderGroup(this.#document.root, this.schema.root, undefined));
        this.#sync(false);

        const focus = this.#focusAfterRender && this.#elements.get(this.#focusAfterRender);

        this.#focusAfterRender = undefined;
        focus?.querySelector<HTMLElement>("select, input, button")?.focus();
    }

    /**
     * @param typeId the type of the items in the group (the type its conditions are about)
     * @param parent the group this one is in, if any
     */
    #renderGroup(group: GroupNode, typeId: string, parent: Parent | undefined): HTMLElement {
        const labels = this.schema.labels;
        const element = clone(this.#templates.group);

        // Find this group's own parts before its items are added: an item can be a group too, with
        // parts of its own that would otherwise be found first.
        const title = query(element, "[data-eb-title]");
        const remove = query<HTMLButtonElement>(element, "[data-eb-remove-group]");
        const items = query(element, "[data-eb-items]");
        const empty = query(element, "[data-eb-empty]");
        const addCondition = query<HTMLButtonElement>(element, "[data-eb-add-condition]");
        const addGroup = query<HTMLButtonElement>(element, "[data-eb-add-group]");

        this.#elements.set(group, element);

        const collection =
            parent && group.collection ? M.findField(this.schema, parent.typeId, group.collection) : undefined;

        title.textContent = !parent
            ? labels.conditions
            : group.collection
              ? labels.collectionGroup.replace("{0}", collection?.label ?? group.collection)
              : labels.group;

        remove.hidden = !parent;
        remove.addEventListener("click", () => {
            if (!parent) return;

            parent.group.items.splice(parent.group.items.indexOf(group), 1);
            this.#render();
            this.#changed();
        });

        group.items.forEach((item, index) => {
            const li = document.createElement("li");

            li.className = "eb-item";
            li.append(
                item.type === "group"
                    ? this.#renderGroup(item, M.typeIdOf(this.schema, typeId, item), { group, typeId })
                    : this.#renderCondition(item, typeId, group),
            );
            items.append(li);

            if (index < group.items.length - 1) items.append(this.#renderConnector(item));
        });

        empty.hidden = group.items.length > 0;

        addCondition.addEventListener("click", () => {
            const condition = M.newCondition();

            group.items.push(condition);
            this.#focusAfterRender = condition;
            this.#render();
            this.#changed();
        });

        addGroup.addEventListener("click", () => {
            const added = M.newGroup();
            const first = M.newCondition();

            added.items.push(first);
            group.items.push(added);
            this.#focusAfterRender = first;
            this.#render();
            this.#changed();
        });

        return element;
    }

    #renderConnector(item: FilterNode): HTMLElement {
        const li = document.createElement("li");
        const element = clone(this.#templates.connector);
        const select = query<HTMLSelectElement>(element, "[data-eb-connector]");
        const labels = this.schema.labels;

        li.className = "eb-item eb-item-connector";
        li.setAttribute("role", "presentation");

        select.options[0]!.text = labels.and;
        select.options[1]!.text = labels.or;
        select.value = item.connector ?? "And";
        select.addEventListener("change", () => {
            item.connector = select.value === "Or" ? "Or" : "And";
            this.#changed();
        });

        li.append(element);

        return li;
    }

    #renderCondition(node: ConditionNode, typeId: string, parent: GroupNode): HTMLElement {
        const element = clone(this.#templates.condition);
        const fieldSelect = query<HTMLSelectElement>(element, "[data-eb-field]");
        const labels = this.schema.labels;
        const number = (parent.items.indexOf(node) + 1).toString();

        this.#elements.set(node, element);

        this.#fillFields(fieldSelect, typeId, node);

        // The items of a collection of plain values have nothing to choose between: the "property" is
        // the item itself, so there is nothing to ask
        const fields = M.typeOf(this.schema, typeId).fields;
        const implicitField = fields.length === 1 && fields[0]!.path === "" ? fields[0] : undefined;

        if (implicitField) {
            fieldSelect.hidden = true;

            if (!node.operation) M.chooseField(this.schema, typeId, node, implicitField);
        }

        fieldSelect.setAttribute("aria-label", `${labels.field} ${number}`);
        fieldSelect.addEventListener("change", () => {
            const value = fieldSelect.value;

            if (value.startsWith(COLLECTION_PREFIX)) {
                // A collection is filtered in a group of its own, so this condition becomes one
                const group = M.newGroup(value.slice(COLLECTION_PREFIX.length));
                const first = M.newCondition();

                group.connector = node.connector ?? "And";
                group.items.push(first);
                parent.items[parent.items.indexOf(node)] = group;
                this.#focusAfterRender = first;
                this.#render();
            } else {
                M.chooseField(this.schema, typeId, node, M.findField(this.schema, typeId, value));
                this.#fillCondition(element, node, typeId);
            }

            this.#changed();
        });

        const operationSelect = query<HTMLSelectElement>(element, "[data-eb-operation]");

        operationSelect.setAttribute("aria-label", `${labels.operation} ${number}`);
        operationSelect.addEventListener("change", () => {
            M.chooseOperation(this.schema, node, M.findOperation(this.schema, operationSelect.value));
            this.#fillCondition(element, node, typeId);
            this.#changed();
        });

        query(element, "[data-eb-remove]").addEventListener("click", () => {
            parent.items.splice(parent.items.indexOf(node), 1);
            this.#render();
            this.#changed();
        });

        this.#bindOptions(element, node);
        this.#fillCondition(element, node, typeId);

        return element;
    }

    #fillFields(select: HTMLSelectElement, typeId: string, node: ConditionNode): void {
        const labels = this.schema.labels;
        const fields = M.typeOf(this.schema, typeId).fields;

        select.replaceChildren(option("", labels.fieldPlaceholder));

        for (const field of fields.filter((f) => f.kind !== "collection" && f.operations.length > 0))
            select.append(option(field.path, field.label));

        const collections = fields.filter((f) => f.kind === "collection" && f.itemType);

        if (collections.length > 0) {
            const group = document.createElement("optgroup");

            group.label = labels.collections;

            for (const field of collections) group.append(option(COLLECTION_PREFIX + field.path, field.label));

            select.append(group);
        }

        select.value = M.findField(this.schema, typeId, node.field) ? node.field : "";
    }

    /** Brings the operation, values and options of a condition in line with its property and operation. */
    #fillCondition(element: HTMLElement, node: ConditionNode, typeId: string): void {
        const labels = this.schema.labels;
        const field = M.findField(this.schema, typeId, node.field);
        const operations = M.operationsFor(this.schema, field);
        const operationSelect = query<HTMLSelectElement>(element, "[data-eb-operation]");

        operationSelect.replaceChildren(option("", labels.operationPlaceholder));

        for (const operation of operations) operationSelect.append(option(operation.name, operation.label));

        operationSelect.disabled = !field;
        operationSelect.value = operations.some((o) => o.name === node.operation) ? node.operation : "";

        const operation = M.findOperation(this.schema, operationSelect.value);

        this.#fillValues(query(element, "[data-eb-values]"), node, field, operation);
        this.#fillOptions(element, node, field, operation);
    }

    #fillValues(
        container: HTMLElement,
        node: ConditionNode,
        field: FieldSchema | undefined,
        operation: OperationSchema | undefined,
    ): void {
        const labels = this.schema.labels;

        container.replaceChildren();

        if (!field || !operation || operation.arity === "none") return;

        node.values.forEach((value, index) => {
            const wrapper = document.createElement("span");
            const label =
                operation.arity === "two" ? (index === 0 ? labels.valueFrom : labels.valueTo) : labels.value;
            const input = this.#createInput(field, value, node.values.length > 1 && operation.arity === "many" ? `${label} ${index + 1}` : label);

            wrapper.className = "eb-value";
            wrapper.append(input);

            // What the input shows may be tidier than what was loaded (for example, a date without its time)
            if (input.value !== value) node.values[index] = input.value;

            const update = (): void => {
                node.values[index] = input.value;
                this.#changed();
            };

            input.addEventListener("input", update);
            input.addEventListener("change", update);

            if (operation.arity === "two" && index === 0) {
                const joiner = document.createElement("span");

                joiner.className = "eb-value-joiner";
                joiner.textContent = labels.and.toLowerCase();
                wrapper.append(joiner);
            }

            if (operation.arity === "many" && node.values.length > 1) {
                const remove = button("eb-btn eb-btn-icon eb-btn-remove-value", "−", labels.removeValue);

                remove.addEventListener("click", () => {
                    node.values.splice(index, 1);
                    this.#fillValues(container, node, field, operation);
                    this.#changed();
                });
                wrapper.append(remove);
            }

            container.append(wrapper);
        });

        if (operation.arity === "many") {
            const add = button("eb-btn eb-btn-text eb-btn-add-value", labels.addValue, labels.addValue);

            add.addEventListener("click", () => {
                node.values.push("");
                this.#fillValues(container, node, field, operation);
                [...container.querySelectorAll<HTMLElement>(".eb-value input, .eb-value select")].at(-1)?.focus();
                this.#changed();
            });
            container.append(add);
        }
    }

    #createInput(field: FieldSchema, value: string, label: string): HTMLInputElement | HTMLSelectElement {
        const labels = this.schema.labels;
        let control: HTMLInputElement | HTMLSelectElement;

        if (field.input === "select" || field.input === "boolean") {
            const select = document.createElement("select");
            const choices =
                field.input === "boolean"
                    ? [
                          { value: "true", label: labels.yes },
                          { value: "false", label: labels.no },
                      ]
                    : (field.options ?? []);

            select.append(option("", ""));

            for (const choice of choices) select.append(option(choice.value, choice.label));

            select.value = choices.some((c) => c.value === value) ? value : "";
            control = select;
        } else {
            const input = document.createElement("input");

            input.type = ["number", "date", "time", "datetime-local"].includes(field.input) ? field.input : "text";

            if (field.input === "number") {
                input.step = field.step ?? "any";
                input.inputMode = field.step === "1" ? "numeric" : "decimal";
            }

            input.value = tidyForInput(field.input, value);
            control = input;
        }

        control.className = "eb-input";
        control.setAttribute("aria-label", label);

        return control;
    }

    #bindOptions(element: HTMLElement, node: ConditionNode): void {
        const toggle = query<HTMLButtonElement>(element, "[data-eb-toggle-options]");
        const panel = query(element, "[data-eb-options]");

        toggle.addEventListener("click", () => {
            const open = !this.#optionsOpen.has(node);

            if (open) this.#optionsOpen.add(node);
            else this.#optionsOpen.delete(node);

            toggle.setAttribute("aria-expanded", String(open));
            panel.hidden = !open;
        });

        const match = query<HTMLSelectElement>(element, "[data-eb-match]");

        match.addEventListener("change", () => {
            node.match = match.value === "All" ? "All" : "Any";
            this.#changed();
        });

        const replaceFind = query<HTMLInputElement>(element, "[data-eb-replace-find]");
        const replaceWith = query<HTMLInputElement>(element, "[data-eb-replace-with]");
        const replacePanel = query(element, "[data-eb-replace]");

        const updateReplace = (): void => {
            M.toggleManipulator(node, { name: "Replace", find: replaceFind.value, with: replaceWith.value }, true);
            this.#changed();
        };

        replaceFind.addEventListener("input", updateReplace);
        replaceWith.addEventListener("input", updateReplace);

        for (const box of element.querySelectorAll<HTMLInputElement>("[data-eb-manipulator]")) {
            box.addEventListener("change", () => {
                const name = box.dataset.ebManipulator as "Trim" | "ToLower" | "ToUpper" | "Replace";

                if (name === "Replace") {
                    replacePanel.hidden = !box.checked;

                    if (box.checked) updateReplace();
                    else M.toggleManipulator(node, { name: "Replace" }, false);
                } else {
                    M.toggleManipulator(node, { name }, box.checked);
                }

                this.#changed();
            });
        }
    }

    #fillOptions(
        element: HTMLElement,
        node: ConditionNode,
        field: FieldSchema | undefined,
        operation: OperationSchema | undefined,
    ): void {
        const toggle = query<HTMLButtonElement>(element, "[data-eb-toggle-options]");
        const panel = query(element, "[data-eb-options]");
        const matchWrap = query(element, "[data-eb-match-wrap]");
        const manipulatorsWrap = query(element, "[data-eb-manipulators-wrap]");

        matchWrap.hidden = !(operation?.supportsMatch && operation.arity === "many");
        manipulatorsWrap.hidden = !(field?.kind === "string" && operation && operation.arity !== "none");

        const available = !matchWrap.hidden || !manipulatorsWrap.hidden;

        toggle.hidden = !available;

        if (!available) this.#optionsOpen.delete(node);

        const open = available && this.#optionsOpen.has(node);

        toggle.setAttribute("aria-expanded", String(open));
        toggle.toggleAttribute("data-eb-active", M.hasAdvancedOptions(node, this.schema));
        panel.hidden = !open;

        const match = query<HTMLSelectElement>(element, "[data-eb-match]");

        match.value = node.match ?? operation?.defaultMatch ?? "Any";

        for (const box of element.querySelectorAll<HTMLInputElement>("[data-eb-manipulator]"))
            box.checked = M.hasManipulator(node, box.dataset.ebManipulator ?? "") !== undefined;

        const replace = M.hasManipulator(node, "Replace");

        query(element, "[data-eb-replace]").hidden = !replace;
        query<HTMLInputElement>(element, "[data-eb-replace-find]").value = replace?.find ?? "";
        query<HTMLInputElement>(element, "[data-eb-replace-with]").value = replace?.with ?? "";
    }

    // -- Keeping the page and the posted value in step --------------------------------------

    /** Something was changed by the user. */
    #changed(): void {
        this.#sync(true);
    }

    #sync(announce: boolean): void {
        this.#input.value = M.serialise(this.#document);
        this.#issues = M.findIssues(this.schema, this.#document.root, this.schema.root);

        const labels = this.schema.labels;

        for (const [node, element] of this.#elements) {
            const issue = this.#issues.find((i) => i.node === node);
            const show = this.#showErrors && issue !== undefined;

            element.classList.toggle("eb-invalid", show);

            if (node.type === "condition") {
                query(element, "[data-eb-toggle-options]").toggleAttribute(
                    "data-eb-active",
                    M.hasAdvancedOptions(node, this.schema),
                );

                const error = query(element, "[data-eb-error]");

                error.hidden = !show;
                error.textContent = show ? (issue?.message ?? "") : "";
            }
        }

        const summary = this.root.querySelector<HTMLElement>("[data-eb-summary]");

        if (summary) {
            const text = M.describe(this.schema, this.#document);

            summary.textContent = this.#showErrors && this.#issues.length > 0 ? `${text} ${labels.incomplete}` : text;
        }

        if (announce)
            this.root.dispatchEvent(
                new CustomEvent(CHANGE_EVENT, {
                    bubbles: true,
                    detail: { json: this.json, document: this.#document },
                }),
            );
    }

    #bindForm(): void {
        const form = this.root.closest("form");

        if (!form) return;

        form.addEventListener("submit", (event) => {
            if (this.root.hasAttribute("data-eb-novalidate")) return;

            if (!this.validate()) {
                event.preventDefault();

                const first = this.#issues[0];
                const element = first && this.#elements.get(first.node);

                element?.querySelector<HTMLElement>("select, input, button")?.focus();
            }
        });

        // A reset puts back what the server rendered
        form.addEventListener("reset", () => {
            setTimeout(() => this.load(this.#renderedJson), 0);
        });
    }
}

// -- Small helpers ------------------------------------------------------------------------

function query<T extends HTMLElement = HTMLElement>(scope: ParentNode, selector: string): T {
    const found = scope.querySelector<T>(selector);

    if (!found)
        throw new Error(
            `The filter builder could not find "${selector}". If the view was customised, keep its data-eb-* attributes.`,
        );

    return found;
}

function template(root: HTMLElement, name: string): HTMLTemplateElement {
    return query<HTMLTemplateElement>(root, `template[data-eb-template="${name}"]`);
}

function clone(source: HTMLTemplateElement): HTMLElement {
    const element = source.content.firstElementChild?.cloneNode(true);

    if (!(element instanceof HTMLElement)) throw new Error("A filter builder template is empty.");

    return element;
}

function readSchema(root: HTMLElement): Schema {
    const script = query(root, "script[data-eb-schema]");

    return JSON.parse(script.textContent ?? "") as Schema;
}

function option(value: string, text: string): HTMLOptionElement {
    const element = document.createElement("option");

    element.value = value;
    element.textContent = text;

    return element;
}

function button(className: string, text: string, label: string): HTMLButtonElement {
    const element = document.createElement("button");

    element.type = "button";
    element.className = className;
    element.textContent = text;
    element.setAttribute("aria-label", label);
    element.title = label;

    return element;
}

/** Puts a value in the form that a date or time input accepts. */
export function tidyForInput(input: string, value: string): string {
    if (input === "date") return /^\d{4}-\d{2}-\d{2}/.test(value) ? value.slice(0, 10) : value;

    if (input === "datetime-local") {
        const text = value.replace(" ", "T");

        // Seconds that are zero only get in the way of a picker that works in minutes
        return /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:00$/.test(text) ? text.slice(0, 16) : text;
    }

    return value;
}
