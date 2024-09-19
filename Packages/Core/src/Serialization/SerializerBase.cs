namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

using System.Linq.Expressions;
using System.Text.Json;
using TopMarksDevelopment.ExpressionBuilder.Api;

internal static class SerializerBase
{
    internal static IOperation FindOperationByName(
        string? operationName,
        ref ICollection<Type> _foundTypes
    )
    {
        ArgumentNullException.ThrowIfNull(operationName);

        if (_foundTypes.Count == 0)
            _foundTypes = AppDomain
                .CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                    a.GetTypes()
                        .Where(asyType =>
                            asyType.GetInterfaces().Contains(typeof(IOperation))
                        )
                )
                .Where(t => t != null)
                .ToList()!;

        var operationType = _foundTypes.FirstOrDefault(t =>
            t.Name == operationName
        );

        if (operationType == null)
            throw new TypeLoadException(nameof(operationType));

        if (Activator.CreateInstance(operationType) is not IOperation operation)
            throw new TypeLoadException(nameof(operation));

        return operation;
    }

    internal static IEntityManipulator FindManipulator(
        ManipulatorInfo info,
        ref ICollection<Type> _foundTypes
    )
    {
        IEntityManipulator manipulator;

        if (info.Type != null && info.ArgTypes != null)
            manipulator = FindMethodManipulator(
                info.Type,
                info.ArgTypes,
                info.Name,
                info.Arguments
            );
        else
        {
            manipulator = FindManipulatorByName(info.Name, ref _foundTypes);

            if (manipulator.ExpectedTypes.Length != info.Arguments.Length)
                throw new OverflowException(
                    $"Argument count is incorrect, {info.Name} requires {manipulator.ExpectedTypes.Length}"
                );

            manipulator.Arguments =
            [
                .. ConvertArguments(info.Arguments, manipulator.ExpectedTypes),
            ];
        }

        manipulator.Validate();

        return manipulator;

        static IEntityManipulator FindManipulatorByName(
            string? manipulatorName,
            ref ICollection<Type> _foundTypes
        )
        {
            ArgumentNullException.ThrowIfNull(manipulatorName);

            if (_foundTypes.Count == 0)
                _foundTypes = AppDomain
                    .CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                        a.GetTypes()
                            .Where(asyType =>
                                asyType
                                    .GetInterfaces()
                                    .Contains(typeof(IEntityManipulator))
                            )
                    )
                    .Where(t => t != null)
                    .ToList()!;

            var manipulatorType =
                _foundTypes.FirstOrDefault(t => t.Name == manipulatorName)
                ?? throw new TypeLoadException(
                    "Could not find the manipulator: " + manipulatorName
                );

            if (
                Activator.CreateInstance(manipulatorType)
                is not IEntityManipulator manipulator
            )
                throw new Exception(nameof(manipulator));

            return manipulator;
        }

        static IEntityManipulator FindMethodManipulator(
            string typeName,
            IEnumerable<string> argTypeNames,
            string methodName,
            object?[] arguments
        )
        {
            var type =
                Type.GetType(typeName, true)
                ?? throw new TypeLoadException("Type could not be found");

            if (arguments.Length != argTypeNames.Count())
                throw new OverflowException(
                    $"Argument count is incorrect. The {methodName} method expects {argTypeNames.Count()}"
                );

            // Parse the argument types
            var types = argTypeNames
                .Select(x =>
                    Type.GetType(x, true)
                    ?? throw new TypeLoadException("Type could not be found")
                )
                .ToArray();

            var methodInfo =
                type.GetMethod(methodName, types)
                ?? throw new MissingMethodException(
                    "Could not find method: " + methodName
                );

            return new ExpressionMethodManipulator(
                methodInfo,
                ConvertArguments(arguments, types).Select(Expression.Constant)
            );
        }

        static List<object?> ConvertArguments(object?[] inArray, Type[] types)
        {
            List<object?> outArray = [];

            for (var i = 0; i < Math.Min(inArray.Length, types.Length); i++)
            {
                var inType = types.ElementAt(i);
                var workingArg = inArray[i];

                outArray.Add(
                    workingArg is JsonElement jArg
                        ? jArg.Deserialize(inType)
                        : Convert.ChangeType(workingArg, inType)
                );
            }

            return outArray;
        }
    }
}
