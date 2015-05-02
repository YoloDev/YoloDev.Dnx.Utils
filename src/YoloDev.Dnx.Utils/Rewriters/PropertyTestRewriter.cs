using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace YoloDev.Dnx.Utils.Rewriters
{
	class PropertyTestRewriter : CSharpSyntaxRewriter
	{
		readonly IEnumerable<StatementSyntax> _tests;
		
		private PropertyTestRewriter(IEnumerable<StatementSyntax> tests)
		{
			_tests = tests;
		}
		
		public static PropertyDeclarationSyntax Rewrite(PropertyDeclarationSyntax node, IEnumerable<StatementSyntax> tests)
		{
			return (PropertyDeclarationSyntax)new PropertyTestRewriter(tests).VisitPropertyDeclaration(node);
		}
		
		public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
		{
			if(node.IsKind(SyntaxKind.SetAccessorDeclaration))
			{
				node = node.WithBody(
					node.Body.WithStatements(
						node.Body.Statements.InsertRange(0, _tests)
					)
				);
			}
			
			return node;
		}
	}
}