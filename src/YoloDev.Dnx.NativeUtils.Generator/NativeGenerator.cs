using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace YoloDev.Dnx.NativeUtils.Generator
{
    class NativeGenerator
    {
        static readonly NamespaceDeclarationSyntax _ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("YoloDev.Dnx.NativeUtils.Generated"));

        readonly NativeModel _api;
        readonly Compilation _compilation;

        readonly List<MemberDeclarationSyntax> _members = new List<MemberDeclarationSyntax>();
        readonly ClassDeclarationSyntax _class;

        public NativeGenerator(NativeModel api, Compilation compilation)
        {
            _api = api;
            _compilation = compilation;
            _class = SyntaxFactory.ClassDeclaration($"NativeMethods${api.Symbol.MetadataName}")
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(api.Symbol.ToDisplayString())))
                .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(CreateAttribute<CompilerGeneratedAttribute>()));
        }

        SyntaxTree Generate()
        {
            foreach (var ctor in _api.Symbol.Constructors)
            {
                GenerateCtor(ctor);
            }

            foreach (var method in _api.Methods)
            {
                GenerateMethod(method);
            }

            return SyntaxFactory.SyntaxTree(SyntaxFactory.CompilationUnit()
                .WithMembers(new SyntaxList<MemberDeclarationSyntax>().Add(_ns.AddMembers(_class.AddMembers(_members.ToArray())))).NormalizeWhitespace());
        }

        void GenerateCtor(IMethodSymbol ctor)
        {
            var syntax = SyntaxFactory.ConstructorDeclaration(_class.Identifier)
                .WithModifiers(GetModifiers(ctor.DeclaredAccessibility))
                .WithBody(SyntaxFactory.Block())
                .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(ctor.GetAttributes().Select(RecreateAttribute).ToArray()));

            var parameters = new List<ParameterSyntax> ();
            var arguments = new List<ArgumentSyntax> ();

            foreach (var p in ctor.Parameters)
            {
                var identifier = SyntaxFactory.Identifier(p.Name);
                var type = SyntaxFactory.ParseTypeName(p.Type.ToDisplayString());
                var parameter = SyntaxFactory.Parameter(identifier).WithType(type);
                var argument = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(identifier));

                parameters.Add(parameter);
                arguments.Add(argument);
            }

            syntax = syntax
                .AddParameterListParameters(parameters.ToArray())
                .WithInitializer(
                    SyntaxFactory.ConstructorInitializer(
                        SyntaxKind.BaseConstructorInitializer,
                        SyntaxFactory.ArgumentList().AddArguments(arguments.ToArray())
                    )
                );

            _members.Add(syntax);
        }

        static SyntaxTokenList GetModifiers(Accessibility accessibility)
        {
            var tokens = new List<SyntaxToken> ();
            switch (accessibility)
            {
                case Accessibility.Public:
                    tokens.Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword)); break;
                case Accessibility.Private:
                    tokens.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)); break;
                case Accessibility.Protected:
                    tokens.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword)); break;
                case Accessibility.Internal:
                    tokens.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword)); break;
                case Accessibility.ProtectedAndInternal:
                    tokens.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                    tokens.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword)); break;
                default:
                    // CommonAccessibility.ProtectedOrInternal can't be expressed in C#
                    // but is legal in metadata.
                    throw new NotSupportedException();
            }

            return SyntaxFactory.TokenList(tokens);
        }

        void GenerateMethod(IMethodSymbol method)
        {
            var name = method.Name;
            var returnType = method.ReturnsVoid ?
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)) :
                SyntaxFactory.ParseTypeName(method.ReturnType.ToDisplayString());

            var attr = method.GetAttribute<NativeMethodAttribute>(_compilation);
            if (attr == null)
                throw new InvalidOperationException("Native method attribute not found");

            var library = (string)attr.ConstructorArguments.First().Value;

            var attrSyntax = SyntaxFactory.Attribute(SyntaxFactory.ParseName(_compilation.GetTypeName<UnmanagedFunctionPointerAttribute>()))
                .AddArgumentListArguments(attr.ConstructorArguments.Skip(1).Select(CreateAttributeArgument).ToArray())
                .AddArgumentListArguments(attr.NamedArguments.Select(CreateAttributeArgument).ToArray());

            var dlgName = $"dlg${library}${name}";
            var dlg = SyntaxFactory.DelegateDeclaration(
                returnType,
                dlgName)
                .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(attrSyntax));

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
            var newExpr = InstansiateAttribute(attr);

            var metaField = CreateField(SyntaxFactory.ParseTypeName(_compilation.GetTypeName<NativeMethodAttribute>()), metaFieldName, newExpr);
            var methodImpl = CreateMethod(method, dlg, SyntaxFactory.IdentifierName(dlg.Identifier), SyntaxFactory.IdentifierName(fieldName), SyntaxFactory.IdentifierName(metaFieldName));

            _members.AddRange(field, metaField, methodImpl, dlg);
        }

        AttributeSyntax CreateAttribute<TAttribute>()
            where TAttribute : Attribute
        {
            var type = _compilation.GetTypeName<TAttribute>();
            return SyntaxFactory.Attribute(SyntaxFactory.ParseName(type));
        }

        static AttributeArgumentSyntax CreateAttributeArgument(TypedConstant constant)
        {
            return SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression(constant.ToCSharpString()));
        }

        static AttributeArgumentSyntax CreateAttributeArgument(KeyValuePair<string, TypedConstant> namedConstant)
        {
            return SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression(namedConstant.Value.ToCSharpString()))
                .WithNameEquals((SyntaxFactory.NameEquals(namedConstant.Key)));
        }

        internal static SyntaxTree Generate(NativeModel api, Compilation compilation)
        {
            var generator = new NativeGenerator(api, compilation);
            return generator.Generate();
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

        static AttributeSyntax RecreateAttribute(AttributeData attr)
        {
            var args = new List<AttributeArgumentSyntax>();

            foreach (var ctorArg in attr.ConstructorArguments)
            {
                var arg = SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression(ctorArg.ToCSharpString()));
                args.Add(arg);
            }

            foreach (var namedArg in attr.NamedArguments)
            {
                var arg = SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression(namedArg.Value.ToCSharpString()))
                    .WithNameEquals(SyntaxFactory.NameEquals(namedArg.Key));
                args.Add(arg);
            }

            var syntax = SyntaxFactory.Attribute(SyntaxFactory.ParseName(attr.AttributeClass.ToDisplayString()))
                .AddArgumentListArguments(args.ToArray());

            return syntax;
        }

        static ExpressionSyntax InstansiateAttribute(AttributeData attr)
        {
            var expr = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(attr.AttributeClass.ToDisplayString()));
            var args = attr.ConstructorArguments.Select(c => SyntaxFactory.Argument(SyntaxFactory.ParseExpression(c.ToCSharpString())));
            expr = expr.AddArgumentListArguments(args.ToArray());

            if(attr.NamedArguments.Length > 0)
            {
                var initializer = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression);
                foreach (var arg in attr.NamedArguments)
                {
                    var valueExpr = SyntaxFactory.ParseExpression(arg.Value.ToCSharpString());
                    var assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(arg.Key), valueExpr);
                    initializer = initializer.AddExpressions(assignment);
                }

                expr = expr.WithInitializer(initializer);
            }

            return expr;
        }
    }
}
