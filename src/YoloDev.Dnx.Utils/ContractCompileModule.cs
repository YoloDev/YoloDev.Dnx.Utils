using System;
using Microsoft.Framework.Runtime.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace YoloDev.Dnx.Utils
{
    public class ContractCompileModule : ICompileModule
    {
        public void AfterCompile(AfterCompileContext context)
        {

        }

        public void BeforeCompile(BeforeCompileContext context)
        {
            Console.WriteLine("Hello world");
            var compilation = context.Compilation;
            foreach (var st in compilation.SyntaxTrees)
            {
                var model = compilation.GetSemanticModel(st);
                var methods = st.GetRoot()
                    .DescendantNodesAndSelf()
                    .OfType<MethodDeclarationSyntax>();

                foreach (var method in methods)
                {
                    var tests = new List<StatementSyntax>();
                    foreach (var parameter in method.ParameterList.Parameters)
                    {
                        var attributes = parameter.AttributeLists.Select(a => model.GetSymbolInfo(a))
                            .Where(a => a.Symbol != null).Select(a => a.Symbol).Where(s => s.IsExtern);
                        foreach (var attribute in attributes)
                        {
                            Console.WriteLine($"Looking up assembly {attribute.ContainingAssembly.Name} and type {attribute.ContainingNamespace.Name}.{attribute.Name}");
                            var assembly = Assembly.Load(new AssemblyName(attribute.ContainingAssembly.Name));
                            var type = assembly.GetType(attribute.ContainingNamespace.Name + "." + attribute.Name);
                            if (typeof(IParameterValidator).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                            {
                                var inst = (IParameterValidator)Activator.CreateInstance(type);
                                var test = inst.ValidateParameter(SyntaxFactory.IdentifierName(parameter.Identifier.ValueText), parameter.Identifier.ValueText);
                                Console.WriteLine(test.ToString());
                            }
                        }
                    }
                }
            }
        }
    }
}
