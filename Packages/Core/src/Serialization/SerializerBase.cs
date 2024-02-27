namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

internal static class SerializerBase
{
    internal static IOperation FindOperationByName(
        string? operationName,
        ref ICollection<Type>? _foundTypes
    )
    {
        ArgumentNullException.ThrowIfNull(operationName);

        _foundTypes ??= AppDomain
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
        ref ICollection<Type>? _foundTypes
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
            manipulator.Arguments = info.Arguments;
        }

        manipulator.Validate();

        return manipulator;

        static IEntityManipulator FindManipulatorByName(
            string? manipulatorName,
            ref ICollection<Type>? _foundTypes
        )
        {
            ArgumentNullException.ThrowIfNull(manipulatorName);

            _foundTypes ??= AppDomain
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
                Type.GetType(typeName)
                ?? throw new TypeLoadException("Type could not be found");

            if (arguments.Length != argTypeNames.Count())
                throw new OverflowException("Argument count doesn't match types count");

            var methodInfo =
                type.GetMethod(
                    methodName,
                    // Parse the argument types
                    argTypeNames
                        .Select(x =>
                            Type.GetType(x)
                            ?? throw new TypeLoadException(
                                "Type could not be found"
                            )
                        )
                        .ToArray()
                )
                ?? throw new MissingMethodException(
                    "Could not find method: " + methodName
                );

            return new ExpressionMethodManipulator(
                methodInfo,
                arguments.Select(Expression.Constant)
            );
        }
    }
}
