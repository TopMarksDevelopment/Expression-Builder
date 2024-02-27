namespace TopMarksDevelopment.ExpressionBuilder.Factories
{
    using System;
    using TopMarksDevelopment.ExpressionBuilder;
    using TopMarksDevelopment.ExpressionBuilder.Api;

    public class FilterFactory
    {   
        /// <summary>
        /// Creates a Filter{TClass} by passing the 'TClass' type.
        /// </summary>
        /// <param name="type"></param>
        /// <typeparam name="TClass"></typeparam>
        /// <returns></returns>
        public static IFilter Create(Type type)
        {
            var instance = 
                (IFilter?)Activator.CreateInstance(
                    typeof(Filter<>).MakeGenericType(type)
                );

            if (instance == null)
                throw new NullReferenceException(nameof(type));
            
            return instance;
        }
    }
}