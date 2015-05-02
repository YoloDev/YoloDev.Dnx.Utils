using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace YoloDev.Dnx.Utils
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class NotNullAttribute : Attribute, IParameterValidator
    {
        public StatementSyntax ValidateParameter(ExpressionSyntax value, string name)
            => MethodValidator.ValidateWithFunction(
                typeof(Check),
                nameof(Check.NotNull),
                value,
                name);
    }
}
