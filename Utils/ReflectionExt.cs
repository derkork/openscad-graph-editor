using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenScadGraphEditor.Utils
{
    public static class ReflectionExt
    {
        public static IEnumerable<Type> GetImplementors(this Type baseType, params Type[] constructorArguments)
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(it => baseType.IsAssignableFrom(it) && !it.IsAbstract)
                .Where(it => it.GetConstructor(constructorArguments) != null);
        }

        public static IEnumerable<T> CreateInstances<T>(this IEnumerable<Type> types,
            params object[] constructorArguments)
        {
            return types.Select(it => Activator.CreateInstance(it, constructorArguments))
                .Cast<T>();
        }
    }
}