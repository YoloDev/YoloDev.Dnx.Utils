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
            var fullName = $"{type.FullName}.{methodName}";
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
        
        public static StatementSyntax ValidateWithFunction(Type type, string methodName, ExpressionSyntax value, string propertyName, string argumentName)
        {
            var fullName = $"{type.FullName}.{methodName}";
            var valueArgument = SyntaxFactory.Argument(value);
            var propertyNameArgument = SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(propertyName)));
            var argumentNameArgument = SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(argumentName)));
            
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.ParseName(fullName),
                    SyntaxFactory.ArgumentList(
                        new SeparatedSyntaxList<ArgumentSyntax>()
                            .Add(valueArgument)
                            .Add(propertyNameArgument)
                            .Add(argumentNameArgument))));
        }
    }
}
