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
function parseDocument(json) {
  if (!json || !json.trim()) return emptyDocument();
  try {
    const parsed = JSON.parse(json);
    if (!isObject(parsed) || parsed.version !== VERSION) return emptyDocument();
    const root = readNode(parsed.root);
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
function readNode(value) {
  if (!isObject(value)) return null;
  if (value.type === "group") {
    const group = newGroup(asString(value.collection) || void 0);
    group.connector = readConnector(value.connector);
    group.items = (Array.isArray(value.items) ? value.items : []).map(readNode).filter((n) => n !== null);
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
    const manipulators = (Array.isArray(value.manipulators) ? value.manipulators : []).map(readManipulator).filter((m) => m !== null);
    if (manipulators.length) condition.manipulators = manipulators;
    return condition;
  }
  return null;
}
function readManipulator(value) {
  if (!isObject(value)) return null;
  switch (value.name) {
    case "Trim":
    case "ToLower":
    case "ToUpper":
      return { name: value.name };
    case "Replace":
      return { name: "Replace", find: asString(value.find), with: asString(value.with) };
    default:
      return null;
  }
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
    } else if (item.manipulators?.some((m) => m.name === "Replace" && !m.find)) {
      issues.push({ node: item, message: labels.replaceFind });
    }
  }
  return issues;
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
  #focusAfterRender;
  #issues = [];
  constructor(root) {
    this.root = root;
    this.#input = query(root, "[data-eb-input]");
    this.#renderedJson = this.#input.value;
    this.schema = readSchema(root);
    this.#templates = {
      group: template(root, "group"),
      condition: template(root, "condition"),
      connector: template(root, "connector")
    };
    this.#document = parseDocument(this.#input.value);
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
    this.#document = parseDocument(json);
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
    const host = query(this.root, "[data-eb-root]");
    host.replaceChildren(this.#renderGroup(this.#document.root, this.schema.root, void 0));
    this.#sync(false);
    const focus = this.#focusAfterRender && this.#elements.get(this.#focusAfterRender);
    this.#focusAfterRender = void 0;
    focus?.querySelector("select, input, button")?.focus();
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
    const collection = parent && group.collection ? findField(this.schema, parent.typeId, group.collection) : void 0;
    title.textContent = !parent ? labels.conditions : group.collection ? labels.collectionGroup.replace("{0}", collection?.label ?? group.collection) : labels.group;
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
        item.type === "group" ? this.#renderGroup(item, typeIdOf(this.schema, typeId, item), { group, typeId }) : this.#renderCondition(item, typeId, group)
      );
      items.append(li);
      if (index < group.items.length - 1) items.append(this.#renderConnector(item));
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
    const li = document.createElement("li");
    const element = clone(this.#templates.connector);
    const select = query(element, "[data-eb-connector]");
    const labels = this.schema.labels;
    li.className = "eb-item eb-item-connector";
    li.setAttribute("role", "presentation");
    select.options[0].text = labels.and;
    select.options[1].text = labels.or;
    select.value = item.connector ?? "And";
    select.addEventListener("change", () => {
      item.connector = select.value === "Or" ? "Or" : "And";
      this.#changed();
    });
    li.append(element);
    return li;
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
    query(element, "[data-eb-remove]").addEventListener("click", () => {
      parent.items.splice(parent.items.indexOf(node), 1);
      this.#render();
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
    for (const operation2 of operations) operationSelect.append(option(operation2.name, operation2.label));
    operationSelect.disabled = !field;
    operationSelect.value = operations.some((o) => o.name === node.operation) ? node.operation : "";
    const operation = findOperation(this.schema, operationSelect.value);
    this.#fillValues(query(element, "[data-eb-values]"), node, field, operation);
    this.#fillOptions(element, node, field, operation);
  }
  #fillValues(container, node, field, operation) {
    const labels = this.schema.labels;
    container.replaceChildren();
    if (!field || !operation || operation.arity === "none") return;
    node.values.forEach((value, index) => {
      const wrapper = document.createElement("span");
      const label = operation.arity === "two" ? index === 0 ? labels.valueFrom : labels.valueTo : labels.value;
      const input = this.#createInput(field, value, node.values.length > 1 && operation.arity === "many" ? `${label} ${index + 1}` : label);
      wrapper.className = "eb-value";
      wrapper.append(input);
      if (input.value !== value) node.values[index] = input.value;
      const update = () => {
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
        const remove = button("eb-btn eb-btn-icon eb-btn-remove-value", "\u2212", labels.removeValue);
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
        [...container.querySelectorAll(".eb-value input, .eb-value select")].at(-1)?.focus();
        this.#changed();
      });
      container.append(add);
    }
  }
  #createInput(field, value, label) {
    const labels = this.schema.labels;
    let control;
    if (field.input === "select" || field.input === "boolean") {
      const select = document.createElement("select");
      const choices = field.input === "boolean" ? [
        { value: "true", label: labels.yes },
        { value: "false", label: labels.no }
      ] : field.options ?? [];
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
  #bindOptions(element, node) {
    const toggle = query(element, "[data-eb-toggle-options]");
    const panel = query(element, "[data-eb-options]");
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
    const replaceFind = query(element, "[data-eb-replace-find]");
    const replaceWith = query(element, "[data-eb-replace-with]");
    const replacePanel = query(element, "[data-eb-replace]");
    const updateReplace = () => {
      toggleManipulator(node, { name: "Replace", find: replaceFind.value, with: replaceWith.value }, true);
      this.#changed();
    };
    replaceFind.addEventListener("input", updateReplace);
    replaceWith.addEventListener("input", updateReplace);
    for (const box of element.querySelectorAll("[data-eb-manipulator]")) {
      box.addEventListener("change", () => {
        const name = box.dataset.ebManipulator;
        if (name === "Replace") {
          replacePanel.hidden = !box.checked;
          if (box.checked) updateReplace();
          else toggleManipulator(node, { name: "Replace" }, false);
        } else {
          toggleManipulator(node, { name }, box.checked);
        }
        this.#changed();
      });
    }
  }
  #fillOptions(element, node, field, operation) {
    const toggle = query(element, "[data-eb-toggle-options]");
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
    toggle.toggleAttribute("data-eb-active", hasAdvancedOptions(node, this.schema));
    panel.hidden = !open;
    const match = query(element, "[data-eb-match]");
    match.value = node.match ?? operation?.defaultMatch ?? "Any";
    for (const box of element.querySelectorAll("[data-eb-manipulator]"))
      box.checked = hasManipulator(node, box.dataset.ebManipulator ?? "") !== void 0;
    const replace = hasManipulator(node, "Replace");
    query(element, "[data-eb-replace]").hidden = !replace;
    query(element, "[data-eb-replace-find]").value = replace?.find ?? "";
    query(element, "[data-eb-replace-with]").value = replace?.with ?? "";
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
    for (const [node, element] of this.#elements) {
      const issue = this.#issues.find((i) => i.node === node);
      const show = this.#showErrors && issue !== void 0;
      element.classList.toggle("eb-invalid", show);
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
function button(className, text, label) {
  const element = document.createElement("button");
  element.type = "button";
  element.className = className;
  element.textContent = text;
  element.setAttribute("aria-label", label);
  element.title = label;
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
  findOperation,
  fitValues,
  hasAdvancedOptions,
  hasManipulator,
  initialise,
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
