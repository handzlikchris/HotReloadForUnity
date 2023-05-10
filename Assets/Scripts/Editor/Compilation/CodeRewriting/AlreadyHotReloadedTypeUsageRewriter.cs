using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;

namespace FastScriptReload.Editor.Compilation.CodeRewriting
{
    class AlreadyHotReloadedTypeUsageRewriter: ThisRewriterBase {
        private HashSet<string> _alreadyHotReloadedTypes;
        public HashSet<string> AlreadyHotReloadedTypes => _alreadyHotReloadedTypes;

        public AlreadyHotReloadedTypeUsageRewriter(HashSet<string> alreadyHotReloadedTypes, bool writeRewriteReasonAsComment, bool visitIntoStructuredTrivia = false) 
            : base(writeRewriteReasonAsComment, visitIntoStructuredTrivia)
        {
            _alreadyHotReloadedTypes = alreadyHotReloadedTypes;
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            return base.VisitPropertyDeclaration(
                VisitTypeEnabledDeclaration(node, () => node.Type.ToString(), (newName) => node.WithType(newName), 
                nameof(VisitPropertyDeclaration))
            );
        }
        
        public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            return base.VisitVariableDeclaration(
                VisitTypeEnabledDeclaration(node, () => node.Type.ToString(), (newName) => node.WithType(newName), 
                    nameof(VisitVariableDeclaration))
            );        
        }
        
        public override SyntaxNode VisitCastExpression(CastExpressionSyntax node)
        {
            return base.VisitCastExpression(
                VisitTypeEnabledDeclaration(node, () => node.Type.ToString(), (newName) => node.WithType(newName), 
                    nameof(VisitCastExpression))
            );    
        }

        public override SyntaxNode VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            return base.VisitTypeOfExpression(
                VisitTypeEnabledDeclaration(node, () => node.Type.ToString(), (newName) => node.WithType(newName), 
                    nameof(VisitTypeOfExpression))
            );    
        }
        
        public override SyntaxNode VisitTypeArgumentList(TypeArgumentListSyntax node)
        {
            if(node.Arguments.Any(a => _alreadyHotReloadedTypes.Contains(a.ToString())))
            {
                return node.WithArguments(SyntaxFactory.SeparatedList(node.Arguments.Select(typeSyntax =>
                {
                    var matchingAlreadyHotReloadedType = _alreadyHotReloadedTypes.FirstOrDefault(t => t == typeSyntax.ToString());
                    return AddRewriteCommentIfNeeded(
                        !string.IsNullOrEmpty(matchingAlreadyHotReloadedType) ? CreateAndCapturePatchedPostfixTypeSyntax(matchingAlreadyHotReloadedType) : typeSyntax,
                        $"{nameof(AlreadyHotReloadedTypeUsageRewriter)}.{nameof(VisitTypeArgumentList)}"
                    );
                })));
            }

            return base.VisitTypeArgumentList(node);
        }
        
        private T VisitTypeEnabledDeclaration<T>(T node, Func<string> getIdentifierName, Func<TypeSyntax, T> replaceTypeNodeWithNewName, string rewriteComment)
            where T: SyntaxNode
        {
            var identifierName = getIdentifierName();
            var matchingAlreadyHotReloadedType = _alreadyHotReloadedTypes.FirstOrDefault(t => t == identifierName);
            if (!string.IsNullOrEmpty(matchingAlreadyHotReloadedType))
            {
                var newNode = replaceTypeNodeWithNewName(CreateAndCapturePatchedPostfixTypeSyntax(matchingAlreadyHotReloadedType));
                return AddRewriteCommentIfNeeded(newNode, $"{nameof(AlreadyHotReloadedTypeUsageRewriter)}.{rewriteComment}");
            }

            return node;
        }

        private TypeSyntax CreateAndCapturePatchedPostfixTypeSyntax(string matchingAlreadyHotReloadedType)
        {
            _alreadyHotReloadedTypes.Add(matchingAlreadyHotReloadedType);
            return SyntaxFactory.ParseTypeName($"{matchingAlreadyHotReloadedType}{Runtime.AssemblyChangesLoader.ClassnamePatchedPostfix} ");
        }
    }
}