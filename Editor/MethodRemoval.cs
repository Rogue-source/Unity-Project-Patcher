using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Rogue.CodeGenUtils;

public static class MethodRemoval
{
    public readonly struct NodeInfo
    {
        public readonly string File;
        public readonly string Type;
        public readonly string Identifier;
        public readonly string Text;

        public NodeInfo(string file, MethodDeclarationSyntax node)
        {
            File = file;
            Type = node.GetType().Name;
            Identifier = node.Identifier.Text;
            Text = node.ToFullString();
        }
    }

    public static void Scrub(
        string[] files,
        Func<NodeInfo, bool> canRemoveFunction,
        Action<string> log)
    {
        foreach (var file in files)
        {
            if (!File.Exists(file))
                continue;

            var root = CSharpSyntaxTree
                .ParseText(File.ReadAllText(file))
                .GetRoot();

            var methodsToRemove = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => canRemoveFunction(new NodeInfo(file, method)));

            var newRoot = root.RemoveNodes(methodsToRemove, SyntaxRemoveOptions.KeepNoTrivia);

            newRoot = new RemoveCtorMethodCalls().Visit(newRoot);

            File.WriteAllText(file, newRoot.ToFullString());
        }
    }
}