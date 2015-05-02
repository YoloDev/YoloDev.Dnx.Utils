using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace YoloDev.Dnx.Utils
{
    public interface IPropertyValidator
    {
        StatementSyntax ValidateProperty(ExpressionSyntax value, string propertyName, string argumentName);
    }
}
