using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace YoloDev.Dnx.Utils
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class NotEmptyAttribute : Attribute, IParameterValidator, IPropertyValidator
    {
        public StatementSyntax ValidateParameter(ExpressionSyntax value, string name)
            => MethodValidator.ValidateWithFunction(
                typeof(Check),
                nameof(Check.NotEmpty),
                value,
                name);
        
        public StatementSyntax ValidateProperty(ExpressionSyntax value, string propertyName, string argumentName)
            => MethodValidator.ValidateWithFunction(
                typeof(Check),
                nameof(Check.NotEmpty),
                value,
                propertyName,
                argumentName);
    }
}
