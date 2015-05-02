using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace YoloDev.Dnx.Utils.Rewriters
{
	class AutoPropertyRewriter : CSharpSyntaxRewriter
	{
		readonly IFieldSymbol _backingField;
		
		private AutoPropertyRewriter(IFieldSymbol backingField)
		{
			_backingField = backingField;
		}
		
		public static PropertyDeclarationSyntax Rewrite(PropertyDeclarationSyntax node, IFieldSymbol backingField, out MemberDeclarationSyntax newField)
		{
			var rewriter = new AutoPropertyRewriter(backingField);
			newField = rewriter.GenerateNewField();
			return (PropertyDeclarationSyntax)rewriter.VisitPropertyDeclaration(node);
		}
		
		private MemberDeclarationSyntax GenerateNewField()
		{
			var attributes = _backingField.GetAttributes()
				.Select(attribute => {
					if (attribute.ConstructorArguments.Length > 0)
						throw new NotImplementedException();
					
					if (attribute.NamedArguments.Length > 0)
						throw new NotImplementedException();
					
					return SyntaxFactory.Attribute(
						SyntaxFactory.ParseName(attribute.AttributeClass.ToString())
					);
				});

			return SyntaxFactory.FieldDeclaration(
				SyntaxFactory.VariableDeclaration(
					SyntaxFactory.ParseTypeName(_backingField.Type.ToString()),
					new SeparatedSyntaxList<VariableDeclaratorSyntax>()
						.Add(SyntaxFactory.VariableDeclarator(
							SyntaxFactory.Identifier(_backingField.Name)
						))
				)
			).WithAttributeLists(
				new SyntaxList<AttributeListSyntax>()
					.Add(SyntaxFactory.AttributeList(
						new SeparatedSyntaxList<AttributeSyntax>()
							.AddRange(attributes)
					))
			);
		}
		
		public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
		{
			if (node.IsKind(SyntaxKind.GetAccessorDeclaration))
			{
				if (node.Body == null)
				{
					node = node.WithBody(
						SyntaxFactory.Block(
							SyntaxFactory.ReturnStatement(
								SyntaxFactory.MemberAccessExpression(
									SyntaxKind.SimpleMemberAccessExpression,
									SyntaxFactory.ThisExpression(),
									SyntaxFactory.IdentifierName(_backingField.Name)
								)
							)
						)
					);
				}
			}
			else if(node.IsKind(SyntaxKind.SetAccessorDeclaration))
			{
				if (node.Body == null)
				{
					node = node.WithBody(
						SyntaxFactory.Block(
							SyntaxFactory.ExpressionStatement(
								SyntaxFactory.AssignmentExpression(
									SyntaxKind.SimpleAssignmentExpression,
									SyntaxFactory.MemberAccessExpression(
										SyntaxKind.SimpleMemberAccessExpression,
										SyntaxFactory.ThisExpression(),
										SyntaxFactory.IdentifierName(_backingField.Name)
									),
									SyntaxFactory.IdentifierName("value")
								)
							)
						)
					);
				}
			}
			
			return node;
		}
	}
}