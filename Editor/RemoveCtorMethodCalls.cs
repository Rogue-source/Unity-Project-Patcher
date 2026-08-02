using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Rogue.CodeGenUtils
{
internal sealed class RemoveCtorMethodCalls : CSharpSyntaxRewriter
{
    public RemoveCtorMethodCalls()
        : base(visitIntoStructuredTrivia: false)
    {
    }

    public override SyntaxNode? VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        var invocation = node.Expression as InvocationExpressionSyntax;

        if (invocation != null &&
            invocation.Expression.ToString().Contains("ctor"))
        {
            return null;
        }

        return base.VisitExpressionStatement(node);
    }
}
}