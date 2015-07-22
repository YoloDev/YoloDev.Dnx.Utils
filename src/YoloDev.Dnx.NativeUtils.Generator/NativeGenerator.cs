using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.InteropServices;

namespace YoloDev.Dnx.NativeUtils.Generator
{
    static class NativeGenerator
    {
        internal static SyntaxTree Generate(NativeModel api, Compilation compilation)
        {
            var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("YoloDev.Dnx.NativeUtils.Generated"));
            var dec = SyntaxFactory.ClassDeclaration($"NativeMethods${api.Symbol.MetadataName}");
            dec = dec.WithBaseList(SyntaxFactory.BaseList().AddTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(api.Symbol.ToDisplayString()))));

            foreach (var method in api.Methods)
            {
                var name = method.Name;
                var returnType = method.ReturnsVoid ?
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)) :
                    SyntaxFactory.ParseTypeName(method.ReturnType.ToDisplayString());

                var attr = method.GetAttributes().Single(a => a.AttributeClass.Equals(compilation.GetType<NativeMethodAttribute>()));
                var ctorArgs = attr.ConstructorArguments;
                var namedArgs = attr.NamedArguments;

                var attrSyntax = SyntaxFactory.Attribute(SyntaxFactory.ParseName(compilation.GetTypeName<UnmanagedFunctionPointerAttribute>()));
                var library = (string)ctorArgs.First().Value;

                attrSyntax = attrSyntax.AddArgumentListArguments(ctorArgs.Skip(1).Select(val => SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression(val.ToCSharpString()))).ToArray());
                attrSyntax = attrSyntax.AddArgumentListArguments(namedArgs.Select(val => SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression(val.Value.ToCSharpString())).WithNameEquals(SyntaxFactory.NameEquals(val.Key))).ToArray());

                var dlgName = $"dlg${library}${name}";
                var dlg = SyntaxFactory.DelegateDeclaration(
                    returnType,
                    dlgName)
                    .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(attrSyntax))
                    .NormalizeWhitespace();

                if (method.Parameters.Length > 0)
                {
                    var parameters = method.Parameters.Select(
                        p => SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                            .WithType(SyntaxFactory.ParseTypeName(p.Type.ToDisplayString())));

                    var list = new SeparatedSyntaxList<ParameterSyntax>()
                        .AddRange(parameters);

                    dlg = dlg.WithParameterList(SyntaxFactory.ParameterList(list));
                }

                var fieldName = $"_${library}${name}";
                var field = CreateField(SyntaxFactory.IdentifierName(dlg.Identifier), fieldName);

                var metaFieldName = $"_meta${library}${name}";
                var newExpr = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(compilation.GetTypeName<NativeMethodAttribute>()));
                newExpr = newExpr.AddArgumentListArguments(ctorArgs.Select(val => SyntaxFactory.Argument(SyntaxFactory.ParseExpression(val.ToCSharpString()))).ToArray());
                if (namedArgs.Length > 0)
                {
                    var newInit = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression);
                    foreach (var arg in namedArgs)
                    {
                        var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(metaFieldName), SyntaxFactory.IdentifierName(arg.Key));
                        var valueExpr = SyntaxFactory.ParseExpression(arg.Value.ToCSharpString());
                        var assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(arg.Key), valueExpr);
                        newInit = newInit.AddExpressions(assignment);
                    }

                    newExpr = newExpr.WithInitializer(newInit);
                }

                var metaField = CreateField(SyntaxFactory.ParseTypeName(compilation.GetTypeName<NativeMethodAttribute>()), metaFieldName, newExpr);

                var methodImpl = CreateMethod(method, dlg, SyntaxFactory.IdentifierName(dlg.Identifier), SyntaxFactory.IdentifierName(fieldName), SyntaxFactory.IdentifierName(metaFieldName));

                dec = dec.AddMembers(field, metaField, methodImpl, dlg);
            }

            return SyntaxFactory.SyntaxTree(
                SyntaxFactory.CompilationUnit()
                    .WithMembers(
                        new SyntaxList<MemberDeclarationSyntax>()
                            .Add(
                                ns.WithMembers(
                                    new SyntaxList<MemberDeclarationSyntax>()
                                        .Add(dec)
                                ).NormalizeWhitespace()))
            );
        }

        static FieldDeclarationSyntax CreateField(TypeSyntax type, string identifier, ExpressionSyntax initializer = null)
        {
            var declarator = SyntaxFactory.VariableDeclarator(identifier);
            if (initializer != null)
                declarator = declarator.WithInitializer(SyntaxFactory.EqualsValueClause(initializer));

            var varDecl = SyntaxFactory.VariableDeclaration(type)
                .AddVariables(declarator);

            return SyntaxFactory.FieldDeclaration(varDecl);
        }

        static MethodDeclarationSyntax CreateMethod(IMethodSymbol baseSymbol, DelegateDeclarationSyntax dlg, NameSyntax dlgType, NameSyntax dlgField, NameSyntax metaField)
        {
            var method = SyntaxFactory.MethodDeclaration(dlg.ReturnType, baseSymbol.Name).WithParameterList(dlg.ParameterList);
            var declaration = (MethodDeclarationSyntax)baseSymbol.OriginalDefinition.DeclaringSyntaxReferences[0].GetSyntax();
            var modifiers = declaration.Modifiers;
            modifiers = modifiers.Replace(modifiers.Single(t => t.IsKind(SyntaxKind.AbstractKeyword)), SyntaxFactory.Token(SyntaxKind.OverrideKeyword));
            method = method.WithModifiers(modifiers);
            
            // Get delegate
            var dlgExpr = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression($"base.{nameof(NativeMethods.GetNativeMethodDelegate)}()");
            dlgExpr = dlgExpr.AddArgumentListArguments(
                SyntaxFactory.Argument(dlgField).WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.RefKeyword)),
                SyntaxFactory.Argument(metaField),
                SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(baseSymbol.Name))));
            var dlgIdentifier = SyntaxFactory.Identifier("dlg");
            var dlgInitializer = SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"));
            dlgInitializer = dlgInitializer.AddVariables(SyntaxFactory.VariableDeclarator(dlgIdentifier).WithInitializer(SyntaxFactory.EqualsValueClause(dlgExpr)));
            var localVar = SyntaxFactory.LocalDeclarationStatement(dlgInitializer);

            // Run delegate
            var expr = SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(dlgIdentifier));
            foreach(var arg in method.ParameterList.Parameters)
            {
                expr = expr.AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(arg.Identifier)));
            }

            List<StatementSyntax> statements = new List<StatementSyntax>();
            statements.Add(localVar);
            if (!baseSymbol.ReturnsVoid)
            {
                statements.Add(SyntaxFactory.ReturnStatement(expr));
            }
            else
            {
                statements.Add(SyntaxFactory.ExpressionStatement(expr));
            }

            method = method.AddBodyStatements(statements.ToArray());
            return method;
        }
    }
}
