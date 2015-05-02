using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Framework.Runtime;

namespace YoloDev.Dnx.Utils.Rewriters
{
	class ContractSyntaxRewriter : CSharpSyntaxRewriter
	{
		readonly SemanticModel _model;
		readonly IAssemblyLoadContext _loadContext;
		readonly IServiceProvider _services;
		readonly Stack<List<MemberDeclarationSyntax>> _newFields;

		private ContractSyntaxRewriter(SemanticModel model, IAssemblyLoadContext loadContext, IServiceProvider services)
		{
			_model = model;
			_loadContext = loadContext;
			_newFields = new Stack<List<MemberDeclarationSyntax>>();
		}

		public static SyntaxNode Rewrite(SyntaxNode node, SemanticModel model, IAssemblyLoadContext loadContext, IServiceProvider services)
		{
			var rewriter = new ContractSyntaxRewriter(model, loadContext, services);
			return rewriter.Visit(node);
		}
		
		public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			_newFields.Push(new List<MemberDeclarationSyntax>());
			node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);
			var newFields = _newFields.Pop();
			
			if (newFields.Count > 0) 
			{
				node = node.AddMembers(newFields.ToArray());
			}
			
			return node;
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

		public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
		{
			//if (!System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch(); else System.Diagnostics.Debugger.Break();
			node = (PropertyDeclarationSyntax)base.VisitPropertyDeclaration(node);
			var propertySymbol = _model.GetDeclaredSymbol(node);
			var tests = GetPropertyTests(propertySymbol);
			if (tests.Count > 0)
			{
				// First, if auto-property, rewrite to read and write backing field
				var backingField = propertySymbol.ContainingType.GetMembers()
					.OfType<IFieldSymbol>()
					.SingleOrDefault(m => m.AssociatedSymbol == propertySymbol);
				
				if (backingField != null)
				{
					MemberDeclarationSyntax newField;
					node = AutoPropertyRewriter.Rewrite(node, backingField, out newField);
					_newFields.Peek().Add(newField);
				}
				
				// Then, add the tests
				node = PropertyTestRewriter.Rewrite(node, tests);
			}
			return node;
		}

		private IImmutableList<StatementSyntax> GetParameterTests(ParameterSyntax syntax)
		{
			var symbol = (IParameterSymbol)_model.GetDeclaredSymbol(syntax);
			var value = SyntaxFactory.IdentifierName(syntax.Identifier);
			var name = symbol.Name;

			return symbol.GetAttributes()
				.Select(GetParameterValidator<IParameterValidator>)
				.Where(NotNull)
				.Select(pv => pv.ValidateParameter(value, name))
				.ToImmutableList();
		}

		private IImmutableList<StatementSyntax> GetPropertyTests(IPropertySymbol prop)
		{
			if (prop.SetMethod == null)
				return ImmutableList.Create<StatementSyntax>();

			var setMethod = prop.SetMethod;
			var valueParam = setMethod.Parameters.SingleOrDefault(p => p.Name == "value");
			if (valueParam == null)
				return null;

			var value = SyntaxFactory.IdentifierName(valueParam.Name);
			var propName = prop.Name;
			var valueName = valueParam.Name;

			return prop.GetAttributes()
				.Concat(setMethod.GetAttributes())
				.Select(GetParameterValidator<IPropertyValidator>)
				.Where(NotNull)
				.Select(pv => pv.ValidateProperty(value, propName, valueName))
				.ToImmutableList();
		}

		private TValidator GetParameterValidator<TValidator>(AttributeData attribute)
			where TValidator : class
		{
			//if (!System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();
			if (attribute.ConstructorArguments.Length > 0
				|| attribute.NamedArguments.Length > 0)
				return null;

			if (attribute.AttributeClass == null)
				return null;

			if (attribute.AttributeClass.ContainingAssembly == _model.Compilation.Assembly)
				return null;

			if (!attribute.AttributeClass.AllInterfaces.Any(iface => iface.ToString() == "YoloDev.Dnx.Utils.IParameterValidator"))
				return null;

			var assemblyName = attribute.AttributeClass.ContainingAssembly?.Name;
			if (assemblyName == null)
				return null;

			//var asm = _loadContext.Load(symbol.ContainingAssembly.Name + "!preprocess");
			var asm = Assembly.Load(new AssemblyName(attribute.AttributeClass.ContainingAssembly.Name));
            if (asm == null)
				return null;

			var type = asm.GetType(attribute.AttributeClass.ToString());
			if (type == null)
				return null;

			if (!typeof(TValidator).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
				return null;

			//if (!System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();
			var instance = (TValidator)Activator.CreateInstance(type);
			return instance;
		}

		private static bool NotNull<T>(T obj) where T : class
		{
			return obj != null;
		}
	}
}
