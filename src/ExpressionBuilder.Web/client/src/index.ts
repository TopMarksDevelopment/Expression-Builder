/**
 * Entry point of the filter builder script.
 *
 * Loading it starts every filter builder on the page (every `[data-eb-filter-builder]` the Razor view
 * rendered). Use `initialise` for ones added later, and `FilterBuilder.attach` to work with one.
 */

import { FilterBuilder } from "./builder.js";

export { FilterBuilder } from "./builder.js";
export * from "./model.js";
export type * from "./types.js";

/** Starts every filter builder inside `scope` that is not started yet. */
export function initialise(scope: ParentNode = document): FilterBuilder[] {
    return [...scope.querySelectorAll<HTMLElement>("[data-eb-filter-builder]")].map((element) =>
        FilterBuilder.attach(element),
    );
}

if (typeof document !== "undefined") {
    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", () => initialise());
    else initialise();
}
