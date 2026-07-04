#r "C:\\Program Files\\dotnet\\sdk\\10.0.201\\Roslyn\\bincore\\Microsoft.CodeAnalysis.dll"
#r "C:\\Program Files\\dotnet\\sdk\\10.0.201\\Roslyn\\bincore\\Microsoft.CodeAnalysis.CSharp.dll"
using System;
using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
var text = File.ReadAllText("Assets\\Scripts\\Features\\Home\\HomeUIScript.cs");
var tree = CSharpSyntaxTree.ParseText(text);
var root = tree.GetRoot();
foreach (var node in root.DescendantNodes())
{
    if (node is MethodDeclarationSyntax m)
    {
        var line = m.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        Console.WriteLine($"{line}\t{m.Identifier.ValueText}\t{m.ReturnType}");
    }
}
