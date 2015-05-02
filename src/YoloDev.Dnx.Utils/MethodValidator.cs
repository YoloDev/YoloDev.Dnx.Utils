using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace YoloDev.Dnx.Utils
{
    public static class MethodValidator
    {
        public static StatementSyntax ValidateWithFunction(Type type, string methodName, ExpressionSyntax value, string name)
        {
            var fullName = $"{type.Namespace}.{type.Name}.{methodName}";
            var valueArgument = SyntaxFactory.Argument(value);
            var nameArgument = SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(name)));

            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.ParseName(fullName),
                    SyntaxFactory.ArgumentList(
                        new SeparatedSyntaxList<ArgumentSyntax>()
                            .Add(valueArgument)
                            .Add(nameArgument))));
        }
    }
}
