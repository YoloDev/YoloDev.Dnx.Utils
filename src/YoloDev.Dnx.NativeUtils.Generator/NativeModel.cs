using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace YoloDev.Dnx.NativeUtils.Generator
{
    public class NativeModel
    {
        readonly INamedTypeSymbol _symbol;
        readonly ImmutableList<IMethodSymbol> _methods;

        public INamedTypeSymbol Symbol => _symbol;
        public ImmutableList<IMethodSymbol> Methods => _methods;

        public NativeModel(INamedTypeSymbol symbol, ImmutableList<IMethodSymbol> methods)
        {
            _symbol = symbol;
            _methods = methods;
        }

        public static NativeModel From(INamedTypeSymbol symbol, Compilation compilation)
        {
            if (!symbol.IsAbstract)
                return null;

            var baseClass = symbol.BaseType;
            if (!baseClass.Equals(compilation.GetTypeByMetadataName(typeof(NativeMethods).FullName)))
                return null;

            bool isNativeClass = false;
            var methods = symbol.GetMembers().OfType<IMethodSymbol>();

            var nativeMethods = ImmutableList.CreateBuilder<IMethodSymbol>();

            foreach (var method in methods)
            {
                if (!method.IsAbstract)
                    continue;

                var attributes = method.GetAttributes();
                bool isNative = false;
                foreach (var attribute in attributes)
                {
                    if (!attribute.AttributeClass.Equals(compilation.GetTypeByMetadataName(typeof(NativeMethodAttribute).FullName)))
                        continue;

                    isNative = true;
                    break;
                }

                if (!isNative)
                    return null;

                isNativeClass = true;
                nativeMethods.Add(method);
            }

            if (!isNativeClass)
                return null;

            return new NativeModel(symbol, nativeMethods.ToImmutable());
        }
    }
}
