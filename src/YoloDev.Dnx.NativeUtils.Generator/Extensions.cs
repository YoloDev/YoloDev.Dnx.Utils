using Microsoft.CodeAnalysis;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace YoloDev.Dnx.NativeUtils.Generator
{
    static class Extensions
    {
        public static INamedTypeSymbol GetType<T>(this Compilation compilation)
        {
            var typeInfo = typeof(T).GetTypeInfo();
            var metadataName = typeInfo.FullName;
            var symbol = compilation.GetTypeByMetadataName(metadataName);
            return symbol;
        }

        public static string GetTypeName<T>(this Compilation compilation)
        {
            return GetType<T>(compilation).ToDisplayString();
        }

        public static AttributeData GetAttribute<TAttribute>(this ISymbol symbol, Compilation compilation, bool inherited = false)
            where TAttribute : Attribute
        {
            var type = compilation.GetType<TAttribute>();
            var attributes = symbol.GetAttributes();
            foreach (var attr in attributes)
            {
                var attrType = attr.AttributeClass;
                if (attrType.Equals(type) || (inherited && attrType.Inherits(type)))
                {
                    return attr;
                }
            }

            return null;
        }

        public static bool Inherits(this ITypeSymbol child, ITypeSymbol baseType)
        {
            if (child.Equals(baseType))
                return true;

            if (child.AllInterfaces.Any(iface => iface.Inherits(baseType)))
                return true;

            return child.BaseType?.Inherits(baseType) ?? false;
        }

        public static void AddRange<T>(this List<T> list, params T[] items)
        {
            list.AddRange(items);
        }
    }
}
