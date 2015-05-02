using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace YoloDev.Dnx.Utils
{
    public interface IParameterValidator
    {
        StatementSyntax ValidateParameter(ExpressionSyntax value, string name);
    }
}
