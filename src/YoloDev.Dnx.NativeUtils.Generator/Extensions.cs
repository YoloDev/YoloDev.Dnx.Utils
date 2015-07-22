using Microsoft.CodeAnalysis;
using System.Reflection;

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
    }
}
