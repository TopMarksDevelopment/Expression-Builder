// Expression Builder filter builder. Generated from client/src by `npm run build`; do not edit.

// src/model.ts
var VERSION = 1;
function emptyDocument() {
  return { version: VERSION, root: newGroup() };
}
function newGroup(collection) {
  const group = { type: "group", connector: "And", items: [] };
  if (collection) group.collection = collection;
  return group;
}
function newCondition() {
  return {
    type: "condition",
    connector: "And",
    field: "",
    operation: "",
    values: []
  };
}
function parseDocument(json, schema) {
  if (!json || !json.trim()) return emptyDocument();
  try {
    const parsed = JSON.parse(json);
    if (!isObject(parsed) || parsed.version !== VERSION) return emptyDocument();
    const root = readNode(parsed.root, schema);
    return root?.type === "group" ? { version: VERSION, root } : emptyDocument();
  } catch {
    return emptyDocument();
  }
}
function serialise(document2) {
  return JSON.stringify(document2);
}
function isObject(value) {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}
function asString(value) {
  return typeof value === "string" ? value : "";
}
function readConnector(value) {
  return typeof value === "string" && value.toLowerCase() === "or" ? "Or" : "And";
}
function readNode(value, schema) {
  if (!isObject(value)) return null;
  if (value.type === "group") {
    const group = newGroup(asString(value.collection) || void 0);
    group.connector = readConnector(value.connector);
    group.items = (Array.isArray(value.items) ? value.items : []).map((item) => readNode(item, schema)).filter((n) => n !== null);
    return group;
  }
  if (value.type === "condition") {
    const condition = newCondition();
    condition.connector = readConnector(value.connector);
    condition.field = asString(value.field);
    condition.operation = asString(value.operation);
    condition.values = (Array.isArray(value.values) ? value.values : []).map(
      (v) => v === null || v === void 0 ? "" : String(v)
    );
    if (value.match === "Any" || value.match === "All") condition.match = value.match;
    const manipulators = (Array.isArray(value.manipulators) ? value.manipulators : []).map((m) => readManipulator(m, schema)).filter((m) => m !== null);
    if (manipulators.length) condition.manipulators = manipulators;
    return condition;
  }
  return null;
}
function readManipulator(value, schema) {
  if (!isObject(value)) return null;
  const name = asString(value.name);
  if (!name) return null;
  if (schema && !findManipulator(schema, name)) return null;
  const manipulator = { name };
  if (Array.isArray(value.args) && value.args.length > 0) manipulator.args = value.args.map(asString);
  return manipulator;
}
function typeOf(schema, typeId) {
  const type = schema.types[typeId];
  if (!type) throw new Error(`The schema has no type "${typeId}"`);
  return type;
}
function findField(schema, typeId, path) {
  return schema.types[typeId]?.fields.find((f) => f.path === path);
}
function findOperation(schema, name) {
  return schema.operations.find((o) => o.name === name);
}
function findManipulator(schema, name) {
  return schema.manipulators.find((m) => m.name === name);
}
function manipulatorsFor(schema, field) {
  if (!field) return [];
  return schema.manipulators.filter((m) => m.kinds.includes(field.kind));
}
function operationsFor(schema, field) {
  if (!field) return [];
  return schema.operations.filter((o) => field.operations.includes(o.name));
}
function typeIdOf(schema, parentTypeId, group) {
  if (!group.collection) return parentTypeId;
  return findField(schema, parentTypeId, group.collection)?.itemType ?? parentTypeId;
}
function minimumValues(operation) {
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
function chooseField(schema, typeId, condition, field) {
  const previousKind = findField(schema, typeId, condition.field)?.kind;
  condition.field = field?.path ?? "";
  const operations = operationsFor(schema, field);
  const current = operations.find((o) => o.name === condition.operation);
  if (!current) condition.operation = operations[0]?.name ?? "";
  if (previousKind !== field?.kind || !current) condition.values = [];
  if (previousKind !== field?.kind) {
    delete condition.manipulators;
    delete condition.match;
  }
  fitValues(condition, findOperation(schema, condition.operation));
}
function chooseOperation(schema, condition, operation) {
  condition.operation = operation?.name ?? "";
  if (operation && !operation.supportsMatch) delete condition.match;
  fitValues(condition, operation);
}
function fitValues(condition, operation) {
  switch (operation?.arity) {
    case "none":
    case void 0:
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
function toggleManipulator(condition, manipulator, on) {
  const rest = (condition.manipulators ?? []).filter((m) => m.name !== manipulator.name);
  condition.manipulators = on ? [...rest, manipulator] : rest;
  if (condition.manipulators.length === 0) delete condition.manipulators;
}
function hasManipulator(condition, name) {
  return condition.manipulators?.find((m) => m.name === name);
}
function hasAdvancedOptions(condition, schema) {
  const operation = findOperation(schema, condition.operation);
  return operation?.supportsMatch === true && condition.match !== void 0 || (condition.manipulators?.length ?? 0) > 0;
}
function findIssues(schema, group, typeId, issues = []) {
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
function missingArgument(schema, condition) {
  for (const manipulator of condition.manipulators ?? []) {
    const parameters = findManipulator(schema, manipulator.name)?.parameters ?? [];
    const missing = parameters.find((p, index) => p.required && !manipulator.args?.[index]);
    if (missing) return missing.label;
  }
  return void 0;
}
function describe(schema, document2) {
  const text = describeItems(schema, document2.root, schema.root);
  return text || schema.labels.summaryEmpty;
}
function describeItems(schema, group, typeId) {
  const labels = schema.labels;
  const parts = [];
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
function describeGroup(schema, group, typeId) {
  const inner = describeItems(schema, group, typeIdOf(schema, typeId, group));
  if (!inner) return "";
  if (!group.collection) return `(${inner})`;
  const field = findField(schema, typeId, group.collection);
  const heading = schema.labels.collectionGroup.replace("{0}", field?.label ?? group.collection);
  return `${heading.toLowerCase()} (${inner})`;
}
function describeCondition(schema, condition, typeId) {
  const field = findField(schema, typeId, condition.field);
  const operation = findOperation(schema, condition.operation);
  if (!field || !operation) return "";
  if (condition.values.length < minimumValues(operation) || condition.values.some((v) => v === "")) return "";
  const values = condition.values.map((v) => `"${describeValue(field, v, schema.labels)}"`);
  let joined = "";
  if (operation.arity === "two") joined = values.join(` ${schema.labels.and.toLowerCase()} `);
  else if (operation.arity !== "none") joined = values.join(", ");
  return [field.label, operation.label.toLowerCase(), joined].filter(Boolean).join(" ");
}
function describeValue(field, value, labels) {
  if (field.kind === "boolean") return value === "true" ? labels.yes : value === "false" ? labels.no : value;
  if (field.kind === "enum") return field.options?.find((o) => o.value === value)?.label ?? value;
  return value.replace("T", " ");
}
function countConditions(group) {
  return group.items.reduce((total, item) => total + (item.type === "group" ? countConditions(item) : 1), 0);
}

// src/builder.ts
var COLLECTION_PREFIX = "collection:";
var CHANGE_EVENT = "eb:change";
var DEFAULT_INVALID_CLASS = "eb-invalid";
var REQUIRED_PARTS = {
  value: ['[data-eb-control="input"]', '[data-eb-control="select"]', "[data-eb-remove-value]"],
  "manipulator-group": ["[data-eb-manipulator-group-title]", "[data-eb-manipulator-group-items]"],
  manipulator: ["[data-eb-manipulator]", "[data-eb-manipulator-label]", "[data-eb-manipulator-args]"]
};
var instances = /* @__PURE__ */ new WeakMap();
var FilterBuilder = class _FilterBuilder {
  /** What can be filtered, as sent by the server. */
  schema;
  root;
  #document;
  #input;
  /** What the server rendered. A hidden input reflects its value into its attribute, so it can't be asked later. */
  #renderedJson;
  #templates;
  /** Whether the options panel of a condition is open. This is only about the display, so it is not posted. */
  #optionsOpen = /* @__PURE__ */ new WeakSet();
  #elements = /* @__PURE__ */ new Map();
  #showErrors = false;
  /** Where focus goes when the next render is done: this item, or failing that the add button of this group. */
  #focusAfterRender;
  #focusFallback;
  #addButtons = /* @__PURE__ */ new Map();
  #lastId = 0;
  #issues = [];
  constructor(root) {
    this.root = root;
    this.#input = query(root, "[data-eb-input]");
    this.#renderedJson = this.#input.value;
    this.schema = readSchema(root);
    this.#templates = {
      group: template(root, "group"),
      item: template(root, "item"),
      condition: template(root, "condition"),
      value: template(root, "value"),
      connector: template(root, "connector"),
      manipulatorGroup: template(root, "manipulator-group"),
      manipulator: template(root, "manipulator"),
      manipulatorArg: template(root, "manipulator-arg")
    };
    for (const [name, selectors] of Object.entries(REQUIRED_PARTS))
      for (const selector of selectors) query(template(root, name).content, selector);
    this.#document = parseDocument(this.#input.value, this.schema);
    this.#bindForm();
    this.#render();
  }
  /**
   * Starts the filter builder on an element rendered by the view. Safe to call more than once for the
   * same element.
   */
  static attach(root) {
    let instance = instances.get(root);
    if (!instance) {
      instance = new _FilterBuilder(root);
      instances.set(root, instance);
    }
    return instance;
  }
  /** The filter as it will be posted. */
  get json() {
    return serialise(this.#document);
  }
  get document() {
    return this.#document;
  }
  /** What is incomplete, if anything. */
  get issues() {
    return this.#issues;
  }
  /** Replaces the filter, such as with one loaded from somewhere else. */
  load(json) {
    this.#document = parseDocument(json, this.schema);
    this.#showErrors = false;
    this.#render();
    this.#sync(false);
  }
  /**
   * Checks that the filter is complete, showing what is missing if it is not.
   * @returns true if it is complete
   */
  validate() {
    this.#showErrors = true;
    this.#sync(false);
    return this.#issues.length === 0;
  }
  // -- Rendering ----------------------------------------------------------------------------
  #render() {
    this.#elements.clear();
    this.#addButtons.clear();
    const host = query(this.root, "[data-eb-root]");
    host.replaceChildren(this.#renderGroup(this.#document.root, this.schema.root, void 0));
    this.#sync(false);
    const focus = this.#focusAfterRender && this.#elements.get(this.#focusAfterRender);
    const fallback = this.#focusFallback && this.#addButtons.get(this.#focusFallback);
    this.#focusAfterRender = void 0;
    this.#focusFallback = void 0;
    (focus?.querySelector("select, input, button") ?? fallback)?.focus();
  }
  /**
   * Something was removed from a group, which takes focus with it. Put it on whatever now sits in that place
   * or, failing that, before it, or on the group's "add condition" button when the group is empty.
   */
  #focusNear(group, index) {
    this.#focusAfterRender = group.items[index] ?? group.items[index - 1];
    this.#focusFallback = group;
  }
  /** An id that is unique on the page, for pointing one part of a builder at another. */
  #id(kind) {
    return `${this.root.id || "eb"}-${kind}-${++this.#lastId}`;
  }
  /**
   * @param typeId the type of the items in the group (the type its conditions are about)
   * @param parent the group this one is in, if any
   */
  #renderGroup(group, typeId, parent) {
    const labels = this.schema.labels;
    const element = clone(this.#templates.group);
    const title = query(element, "[data-eb-title]");
    const remove = query(element, "[data-eb-remove-group]");
    const items = query(element, "[data-eb-items]");
    const empty = query(element, "[data-eb-empty]");
    const addCondition = query(element, "[data-eb-add-condition]");
    const addGroup = query(element, "[data-eb-add-group]");
    this.#elements.set(group, element);
    this.#addButtons.set(group, addCondition);
    title.id = this.#id("title");
    element.setAttribute("aria-labelledby", title.id);
    const collection = parent && group.collection ? findField(this.schema, parent.typeId, group.collection) : void 0;
    title.textContent = !parent ? labels.conditions : group.collection ? labels.collectionGroup.replace("{0}", collection?.label ?? group.collection) : labels.group;
    remove.hidden = !parent;
    remove.addEventListener("click", () => {
      if (!parent) return;
      const index = parent.group.items.indexOf(group);
      parent.group.items.splice(index, 1);
      this.#focusNear(parent.group, index);
      this.#render();
      this.#changed();
    });
    group.items.forEach((item, index) => {
      const li = clone(this.#templates.item);
      li.append(
        item.type === "group" ? this.#renderGroup(item, typeIdOf(this.schema, typeId, item), { group, typeId }) : this.#renderCondition(item, typeId, group)
      );
      if (index < group.items.length - 1) li.append(this.#renderConnector(item));
      items.append(li);
    });
    empty.hidden = group.items.length > 0;
    addCondition.addEventListener("click", () => {
      const condition = newCondition();
      group.items.push(condition);
      this.#focusAfterRender = condition;
      this.#render();
      this.#changed();
    });
    addGroup.addEventListener("click", () => {
      const added = newGroup();
      const first = newCondition();
      added.items.push(first);
      group.items.push(added);
      this.#focusAfterRender = first;
      this.#render();
      this.#changed();
    });
    return element;
  }
  #renderConnector(item) {
    const element = clone(this.#templates.connector);
    const select = query(element, "[data-eb-connector]");
    const labels = this.schema.labels;
    select.options[0].text = labels.and;
    select.options[1].text = labels.or;
    select.value = item.connector ?? "And";
    select.addEventListener("change", () => {
      item.connector = select.value === "Or" ? "Or" : "And";
      this.#changed();
    });
    return element;
  }
  #renderCondition(node, typeId, parent) {
    const element = clone(this.#templates.condition);
    const fieldSelect = query(element, "[data-eb-field]");
    const labels = this.schema.labels;
    const number = (parent.items.indexOf(node) + 1).toString();
    this.#elements.set(node, element);
    this.#fillFields(fieldSelect, typeId, node);
    const fields = typeOf(this.schema, typeId).fields;
    const implicitField = fields.length === 1 && fields[0].path === "" ? fields[0] : void 0;
    if (implicitField) {
      fieldSelect.hidden = true;
      if (!node.operation) chooseField(this.schema, typeId, node, implicitField);
    }
    fieldSelect.setAttribute("aria-label", `${labels.field} ${number}`);
    fieldSelect.addEventListener("change", () => {
      const value = fieldSelect.value;
      if (value.startsWith(COLLECTION_PREFIX)) {
        const group = newGroup(value.slice(COLLECTION_PREFIX.length));
        const first = newCondition();
        group.connector = node.connector ?? "And";
        group.items.push(first);
        parent.items[parent.items.indexOf(node)] = group;
        this.#focusAfterRender = first;
        this.#render();
      } else {
        chooseField(this.schema, typeId, node, findField(this.schema, typeId, value));
        this.#fillCondition(element, node, typeId);
      }
      this.#changed();
    });
    const operationSelect = query(element, "[data-eb-operation]");
    operationSelect.setAttribute("aria-label", `${labels.operation} ${number}`);
    operationSelect.addEventListener("change", () => {
      chooseOperation(this.schema, node, findOperation(this.schema, operationSelect.value));
      this.#fillCondition(element, node, typeId);
      this.#changed();
    });
    const removeCondition = query(element, "[data-eb-remove]");
    removeCondition.setAttribute("aria-label", `${labels.removeCondition} ${number}`);
    removeCondition.title = `${labels.removeCondition} ${number}`;
    removeCondition.addEventListener("click", () => {
      const index = parent.items.indexOf(node);
      parent.items.splice(index, 1);
      this.#focusNear(parent, index);
      this.#render();
      this.#changed();
    });
    query(element, "[data-eb-add-value]").addEventListener("click", () => {
      node.values.push("");
      this.#fillValues(
        element,
        node,
        typeId,
        findField(this.schema, typeId, node.field),
        findOperation(this.schema, node.operation)
      );
      [...element.querySelectorAll("[data-eb-value] [data-eb-control]")].at(-1)?.focus();
      this.#changed();
    });
    this.#bindOptions(element, node);
    this.#fillCondition(element, node, typeId);
    return element;
  }
  #fillFields(select, typeId, node) {
    const labels = this.schema.labels;
    const fields = typeOf(this.schema, typeId).fields;
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
    select.value = findField(this.schema, typeId, node.field) ? node.field : "";
  }
  /** Brings the operation, values and options of a condition in line with its property and operation. */
  #fillCondition(element, node, typeId) {
    const labels = this.schema.labels;
    const field = findField(this.schema, typeId, node.field);
    const operations = operationsFor(this.schema, field);
    const operationSelect = query(element, "[data-eb-operation]");
    operationSelect.replaceChildren(option("", labels.operationPlaceholder));
    this.#appendOperations(operationSelect, operations);
    operationSelect.disabled = !field;
    operationSelect.value = operations.some((o) => o.name === node.operation) ? node.operation : "";
    const operation = findOperation(this.schema, operationSelect.value);
    this.#fillValues(element, node, typeId, field, operation);
    this.#fillOptions(element, node, field, operation);
  }
  /**
   * Adds the operations to the list. When some are custom they are listed under headings, the built-in
   * ones first.
   */
  #appendOperations(select, operations) {
    if (operations.every((o) => !o.group)) {
      for (const operation of operations) select.append(option(operation.name, operation.label));
      return;
    }
    const groups = /* @__PURE__ */ new Map([[void 0, []]]);
    for (const operation of operations) {
      const members = groups.get(operation.group) ?? [];
      members.push(operation);
      groups.set(operation.group, members);
    }
    for (const [name, members] of groups) {
      if (members.length === 0) continue;
      const group = document.createElement("optgroup");
      group.label = name ?? this.schema.labels.standardGroup;
      for (const operation of members) group.append(option(operation.name, operation.label));
      select.append(group);
    }
  }
  #fillValues(element, node, typeId, field, operation) {
    const labels = this.schema.labels;
    const container = query(element, "[data-eb-values]");
    const add = query(element, "[data-eb-add-value]");
    container.replaceChildren();
    add.hidden = true;
    if (!field || !operation || operation.arity === "none") return;
    node.values.forEach((value, index) => {
      const label = operation.arity === "two" ? index === 0 ? labels.valueFrom : labels.valueTo : labels.value;
      const { row, control } = this.#createValue(
        field,
        value,
        node.values.length > 1 && operation.arity === "many" ? `${label} ${index + 1}` : label
      );
      if (control.value !== value) node.values[index] = control.value;
      const update = () => {
        node.values[index] = control.value;
        this.#changed();
      };
      control.addEventListener("input", update);
      control.addEventListener("change", update);
      const joiner = row.querySelector("[data-eb-value-joiner]");
      if (joiner && operation.arity === "two" && index === 0) {
        joiner.textContent = labels.and.toLowerCase();
        joiner.hidden = false;
      }
      const remove = query(row, "[data-eb-remove-value]");
      if (operation.arity === "many" && node.values.length > 1) {
        remove.hidden = false;
        remove.setAttribute("aria-label", `${labels.removeValue} ${index + 1}`);
        remove.title = `${labels.removeValue} ${index + 1}`;
        remove.addEventListener("click", () => {
          node.values.splice(index, 1);
          this.#fillValues(element, node, typeId, field, operation);
          const controls = [...container.querySelectorAll("[data-eb-control]")];
          controls[Math.min(index, controls.length - 1)]?.focus();
          this.#changed();
        });
      }
      container.append(row);
    });
    add.hidden = operation.arity !== "many";
  }
  /** One value of a condition, made from the view's template: a box, or a list of choices. */
  #createValue(field, value, label) {
    const labels = this.schema.labels;
    const row = clone(this.#templates.value);
    const input = query(row, '[data-eb-control="input"]');
    const select = query(row, '[data-eb-control="select"]');
    let control;
    if (field.input === "select" || field.input === "boolean") {
      const choices = field.input === "boolean" ? [
        { value: "true", label: labels.yes },
        { value: "false", label: labels.no }
      ] : field.options ?? [];
      input.remove();
      select.append(option("", ""));
      for (const choice of choices) select.append(option(choice.value, choice.label));
      select.value = choices.some((c) => c.value === value) ? value : "";
      control = select;
    } else {
      select.remove();
      input.type = ["number", "date", "time", "datetime-local"].includes(field.input) ? field.input : "text";
      if (field.input === "number") {
        input.step = field.step ?? "any";
        input.inputMode = field.step === "1" ? "numeric" : "decimal";
      }
      input.value = tidyForInput(field.input, value);
      control = input;
    }
    control.setAttribute("aria-label", label);
    const caption = row.querySelector("[data-eb-value-label]");
    if (caption) caption.textContent = label;
    return { row, control };
  }
  #bindOptions(element, node) {
    const toggle = query(element, "[data-eb-toggle-options]");
    const panel = query(element, "[data-eb-options]");
    panel.id = this.#id("options");
    toggle.setAttribute("aria-controls", panel.id);
    toggle.addEventListener("click", () => {
      const open = !this.#optionsOpen.has(node);
      if (open) this.#optionsOpen.add(node);
      else this.#optionsOpen.delete(node);
      toggle.setAttribute("aria-expanded", String(open));
      panel.hidden = !open;
    });
    const match = query(element, "[data-eb-match]");
    match.addEventListener("change", () => {
      node.match = match.value === "All" ? "All" : "Any";
      this.#changed();
    });
    this.#buildManipulators(element, node);
  }
  /**
   * Lists the adjustments the schema offers, from the view's templates: a checkbox for each, with a box for
   * each thing it asks for, under a heading for the group (built-in or custom) it belongs to.
   */
  #buildManipulators(element, node) {
    const labels = this.schema.labels;
    const host = query(element, "[data-eb-manipulators]");
    const groups = /* @__PURE__ */ new Map();
    const headings = new Set(this.schema.manipulators.map((m) => m.group)).size > 1;
    host.replaceChildren();
    for (const manipulator of this.schema.manipulators) {
      let items = groups.get(manipulator.group);
      if (!items) {
        const group = clone(this.#templates.manipulatorGroup);
        const title = query(group, "[data-eb-manipulator-group-title]");
        title.textContent = manipulator.group ?? labels.standardGroup;
        title.hidden = !headings;
        items = query(group, "[data-eb-manipulator-group-items]");
        groups.set(manipulator.group, items);
        host.append(group);
      }
      items.append(this.#createManipulator(manipulator, node));
    }
  }
  #createManipulator(manipulator, node) {
    const item = clone(this.#templates.manipulator);
    const box = query(item, "[data-eb-manipulator]");
    const args = query(item, "[data-eb-manipulator-args]");
    item.setAttribute("data-eb-manipulator-item", manipulator.name);
    box.setAttribute("data-eb-manipulator", manipulator.name);
    query(item, "[data-eb-manipulator-label]").textContent = manipulator.label;
    const inputs = manipulator.parameters.map((parameter) => {
      const input = clone(this.#templates.manipulatorArg);
      input.placeholder = parameter.label;
      input.setAttribute("aria-label", `${manipulator.label}: ${parameter.label}`);
      args.append(input);
      return input;
    });
    const current = () => inputs.length > 0 ? { name: manipulator.name, args: inputs.map((input) => input.value) } : { name: manipulator.name };
    box.addEventListener("change", () => {
      args.hidden = !box.checked || inputs.length === 0;
      toggleManipulator(node, current(), box.checked);
      this.#changed();
    });
    for (const input of inputs)
      input.addEventListener("input", () => {
        toggleManipulator(node, current(), true);
        this.#changed();
      });
    return item;
  }
  #fillOptions(element, node, field, operation) {
    const toggle = query(element, "[data-eb-toggle-options]");
    const panel = query(element, "[data-eb-options]");
    const matchWrap = query(element, "[data-eb-match-wrap]");
    const manipulatorsWrap = query(element, "[data-eb-manipulators-wrap]");
    matchWrap.hidden = !(operation?.supportsMatch && operation.arity === "many");
    const suited = new Set(manipulatorsFor(this.schema, field).map((m) => m.name));
    const takesValues = operation !== void 0 && operation.arity !== "none";
    for (const item of element.querySelectorAll("[data-eb-manipulator-item]")) {
      const name = item.getAttribute("data-eb-manipulator-item") ?? "";
      const applied = hasManipulator(node, name);
      const inputs = [...item.querySelectorAll("[data-eb-manipulator-arg]")];
      item.hidden = !(takesValues && suited.has(name));
      query(item, "[data-eb-manipulator]").checked = applied !== void 0;
      inputs.forEach((input, index) => input.value = applied?.args?.[index] ?? "");
      query(item, "[data-eb-manipulator-args]").hidden = applied === void 0 || inputs.length === 0;
    }
    for (const group of element.querySelectorAll("[data-eb-manipulator-group]"))
      group.hidden = ![...group.querySelectorAll("[data-eb-manipulator-item]")].some((i) => !i.hidden);
    manipulatorsWrap.hidden = [...element.querySelectorAll("[data-eb-manipulator-item]")].every((i) => i.hidden);
    const available = !matchWrap.hidden || !manipulatorsWrap.hidden;
    toggle.hidden = !available;
    if (!available) this.#optionsOpen.delete(node);
    const open = available && this.#optionsOpen.has(node);
    toggle.setAttribute("aria-expanded", String(open));
    toggle.toggleAttribute("data-eb-active", hasAdvancedOptions(node, this.schema));
    panel.hidden = !open;
    const match = query(element, "[data-eb-match]");
    match.value = node.match ?? operation?.defaultMatch ?? "Any";
  }
  // -- Keeping the page and the posted value in step --------------------------------------
  /** Something was changed by the user. */
  #changed() {
    this.#sync(true);
  }
  #sync(announce) {
    this.#input.value = serialise(this.#document);
    this.#issues = findIssues(this.schema, this.#document.root, this.schema.root);
    const labels = this.schema.labels;
    const invalid = (this.root.dataset.ebInvalidClass ?? DEFAULT_INVALID_CLASS).split(/\s+/).filter(Boolean);
    for (const [node, element] of this.#elements) {
      const issue = this.#issues.find((i) => i.node === node);
      const show = this.#showErrors && issue !== void 0;
      for (const name of invalid) element.classList.toggle(name, show);
      if (node.type === "condition") {
        query(element, "[data-eb-toggle-options]").toggleAttribute(
          "data-eb-active",
          hasAdvancedOptions(node, this.schema)
        );
        const error = query(element, "[data-eb-error]");
        error.hidden = !show;
        error.textContent = show ? issue?.message ?? "" : "";
      }
    }
    const summary = this.root.querySelector("[data-eb-summary]");
    if (summary) {
      const text = describe(this.schema, this.#document);
      summary.textContent = this.#showErrors && this.#issues.length > 0 ? `${text} ${labels.incomplete}` : text;
    }
    if (announce)
      this.root.dispatchEvent(
        new CustomEvent(CHANGE_EVENT, {
          bubbles: true,
          detail: { json: this.json, document: this.#document }
        })
      );
  }
  #bindForm() {
    const form = this.root.closest("form");
    if (!form) return;
    form.addEventListener("submit", (event) => {
      if (this.root.hasAttribute("data-eb-novalidate")) return;
      if (!this.validate()) {
        event.preventDefault();
        const first = this.#issues[0];
        const element = first && this.#elements.get(first.node);
        element?.querySelector("select, input, button")?.focus();
      }
    });
    form.addEventListener("reset", () => {
      setTimeout(() => this.load(this.#renderedJson), 0);
    });
  }
};
function query(scope, selector) {
  const found = scope.querySelector(selector);
  if (!found)
    throw new Error(
      `The filter builder could not find "${selector}". If the view was customised, keep its data-eb-* attributes.`
    );
  return found;
}
function template(root, name) {
  return query(root, `template[data-eb-template="${name}"]`);
}
function clone(source) {
  const element = source.content.firstElementChild?.cloneNode(true);
  if (!(element instanceof HTMLElement)) throw new Error("A filter builder template is empty.");
  return element;
}
function readSchema(root) {
  const script = query(root, "script[data-eb-schema]");
  return JSON.parse(script.textContent ?? "");
}
function option(value, text) {
  const element = document.createElement("option");
  element.value = value;
  element.textContent = text;
  return element;
}
function tidyForInput(input, value) {
  if (input === "date") return /^\d{4}-\d{2}-\d{2}/.test(value) ? value.slice(0, 10) : value;
  if (input === "datetime-local") {
    const text = value.replace(" ", "T");
    return /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:00$/.test(text) ? text.slice(0, 16) : text;
  }
  return value;
}

// src/index.ts
function initialise(scope = document) {
  return [...scope.querySelectorAll("[data-eb-filter-builder]")].map(
    (element) => FilterBuilder.attach(element)
  );
}
if (typeof document !== "undefined") {
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", () => initialise());
  else initialise();
}
export {
  FilterBuilder,
  VERSION,
  chooseField,
  chooseOperation,
  countConditions,
  describe,
  emptyDocument,
  findField,
  findIssues,
  findManipulator,
  findOperation,
  fitValues,
  hasAdvancedOptions,
  hasManipulator,
  initialise,
  manipulatorsFor,
  minimumValues,
  newCondition,
  newGroup,
  operationsFor,
  parseDocument,
  serialise,
  toggleManipulator,
  typeIdOf,
  typeOf
};
