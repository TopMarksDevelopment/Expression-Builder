using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

// Prints the public API of the Expression Builder assemblies in a folder, one member per line and
// sorted, so two builds can be compared with a plain diff:
//
//   dotnet run --project tools/ExpressionBuilder.ApiSnapshot -- <folder> [assembly-name-regex] > api.txt
//
// The folder is any build output that holds the assemblies (such as tests/ExpressionBuilder.Tests/bin/
// Debug/net10.0). It reads them without loading them, so it works on assemblies that fail to run.
// Assembly names are matched with a regular expression; the default is every Expression Builder package.
if (args.Length < 1)
{
    Console.Error.WriteLine(
        "Usage: ApiSnapshot <folder> [assembly-name-regex]"
    );

    return 1;
}

var folder = args[0];
var pattern = new Regex(
    args.Length > 1 ? args[1] : @"^TopMarksDevelopment\.ExpressionBuilder"
);

var resolver = new PathAssemblyResolver(
    Directory
        .GetFiles(folder, "*.dll")
        .Concat(
            Directory.GetFiles(
                RuntimeEnvironment.GetRuntimeDirectory(),
                "*.dll"
            )
        )
);

using var context = new MetadataLoadContext(resolver);
var lines = new SortedSet<string>(StringComparer.Ordinal);

foreach (var file in Directory.GetFiles(folder, "*.dll"))
{
    if (!pattern.IsMatch(Path.GetFileNameWithoutExtension(file)))
        continue;

    foreach (var type in context.LoadFromAssemblyPath(file).GetExportedTypes())
    {
        var interfaces = type.GetInterfaces()
            .Select(i => i.FullName ?? i.Name)
            .Order(StringComparer.Ordinal);

        lines.Add(
            $"T {type.FullName} : {type.BaseType?.FullName} [{string.Join(",", interfaces)}]"
        );

        var members = type.GetMembers(
            BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.DeclaredOnly
        );

        foreach (var member in members)
            switch (member)
            {
                case MethodBase method when method.IsPublic || method.IsFamily:
                    var returns = method is MethodInfo info
                        ? info.ReturnType.ToString()
                        : "ctor";
                    var generics = method.IsGenericMethod
                        ? "<"
                            + string.Join(
                                ",",
                                method.GetGenericArguments().Select(g => g.Name)
                            )
                            + ">"
                        : "";
                    var parameters = string.Join(
                        ", ",
                        method
                            .GetParameters()
                            .Select(p => $"{p.ParameterType} {p.Name}")
                    );

                    lines.Add(
                        $"M {type.FullName}::{method.Name}{generics}({parameters}) : {returns}"
                    );
                    break;

                case PropertyInfo
                or FieldInfo
                or EventInfo:
                    lines.Add($"P {type.FullName}::{member.Name}");
                    break;
            }
    }
}

foreach (var line in lines)
    Console.WriteLine(line);

return 0;
