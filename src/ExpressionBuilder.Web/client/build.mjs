// Bundles src/index.ts into ../wwwroot/js/filter-builder.js, as one file so the browser caches (and the
// package versions) it as a unit. The output is committed: building the .NET projects does not need node.
import { build } from "esbuild";

await build({
    entryPoints: ["src/index.ts"],
    outfile: "../wwwroot/js/filter-builder.js",
    bundle: true,
    format: "esm",
    target: "es2022",
    sourcemap: false,
    minify: false,
    legalComments: "none",
    banner: {
        js: "// Expression Builder filter builder. Generated from client/src by `npm run build`; do not edit.",
    },
    logLevel: "info",
});
