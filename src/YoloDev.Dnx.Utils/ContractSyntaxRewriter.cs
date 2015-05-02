using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Common.DependencyInjection;

namespace YoloDev.Dnx.Utils
{
	class ContractSyntaxRewriter : CSharpSyntaxRewriter 
	{
		readonly SemanticModel _model;
		readonly IAssemblyLoadContext _loadContext;
		readonly IServiceProvider _services;
		
		private ContractSyntaxRewriter(SemanticModel model, IAssemblyLoadContext loadContext, IServiceProvider services)
		{
			_model = model;
			_loadContext = loadContext;
		}
		
		public static SyntaxNode Rewrite(SyntaxNode node, SemanticModel model, IAssemblyLoadContext loadContext, IServiceProvider services)
		{
			var rewriter = new ContractSyntaxRewriter(model, loadContext, services);
			return rewriter.Visit(node);
		}
		
		public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);
			var name = node.Identifier.ToString();
			var tests = node.ParameterList.Parameters.SelectMany(GetParameterTests).ToList();
			if (tests.Count > 0)
			{
				var body = node.ExpressionBody != null ?
					SyntaxFactory.Block(SyntaxFactory.ReturnStatement(node.ExpressionBody.Expression)) :
					node.Body;
				
				body = body.WithStatements(body.Statements.InsertRange(0, tests));
				node = node.WithExpressionBody(null).WithBody(body);
			}
			return node;
		}
		
		public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
		{
			node = (ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node);
			var tests = node.ParameterList.Parameters.SelectMany(GetParameterTests).ToList();
			if (tests.Count > 0)
			{
				var body = node.Body;
				body = body.WithStatements(body.Statements.InsertRange(0, tests));
				node = node.WithBody(body);
			}
			return node;
		}
		
		private IImmutableList<StatementSyntax> GetParameterTests(ParameterSyntax syntax)
		{
			var value = SyntaxFactory.IdentifierName(syntax.Identifier);
			var name = syntax.Identifier.ToString();
			return syntax.AttributeLists
				.SelectMany(l => l.Attributes)
				.Select(GetParameterValidator)
				.Where(NotNull)
				.Select(pv => pv.ValidateParameter(value, name))
				.ToImmutableList();
		}
		
		private IParameterValidator GetParameterValidator(AttributeSyntax attribute)
		{
			//if (!System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();
			if (attribute.ArgumentList?.Arguments.Count > 0)
				return null;
			
			var si = _model.GetSymbolInfo(attribute);
			if (si.Symbol == null)
				return null;
			
			var symbol = si.Symbol;
			if (symbol.ContainingAssembly == _model.Compilation.Assembly)
				return null;
				
			if (!symbol.ContainingType.AllInterfaces.Any(iface => iface.ToString() == "YoloDev.Dnx.Utils.IParameterValidator"))
				return null;
			
			var assemblyName = symbol.ContainingAssembly?.Name;
			if (assemblyName == null)
				return null;
			
			//var asm = _loadContext.Load(symbol.ContainingAssembly.Name + "!preprocess");
			var asm = Assembly.Load(new AssemblyName(symbol.ContainingAssembly.Name));
            if (asm == null)
				return null;
			
			var type = asm.GetType(symbol.ContainingType.ToString());
			if (type == null)
				return null;
			
			if (!typeof(IParameterValidator).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
				return null;
			
			//if (!System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();
			var instance = (IParameterValidator)ActivatorUtilities.CreateInstance(_services, type);
			return instance;
		}
		
		private static bool NotNull<T>(T obj) where T : class
		{
			return obj != null;
		}
	}
}