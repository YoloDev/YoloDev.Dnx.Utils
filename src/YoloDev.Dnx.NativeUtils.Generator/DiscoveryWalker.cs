using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;

namespace YoloDev.Dnx.NativeUtils.Generator
{
    class DiscoveryWalker : CSharpSyntaxWalker
    {
        readonly SemanticModel _model;
        readonly ImmutableList<NativeModel>.Builder _apiClasses = ImmutableList.CreateBuilder<NativeModel>();

        public DiscoveryWalker (SemanticModel model)
        {
            _model = model;

            Debug.Assert(model.Compilation.GetTypeByMetadataName(typeof(NativeMethods).FullName) != null);
            Debug.Assert(model.Compilation.GetTypeByMetadataName(typeof(NativeMethodAttribute).FullName) != null);
        }

        internal static IEnumerable<NativeModel> FindNativeApis(SyntaxTree st, SemanticModel sm)
        {
            var walker = new DiscoveryWalker(sm);
            walker.Visit(st.GetRoot());
            return walker._apiClasses.ToImmutable();
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var symbol = _model.GetDeclaredSymbol(node);
            var model = NativeModel.From(symbol, _model.Compilation);

            if (model != null)
                _apiClasses.Add(model);

            base.VisitClassDeclaration(node);
        }
    }
}
