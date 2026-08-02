using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Rogue.CodeGenUtils
{
public static class ScriptCloner
{
	public static string[] Clone(string assetRipperPath, string dataPath, string[] files, string[] supportedTypes, string fileTemplate, Action<string> log, string[] assemblies)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (string text in files)
		{
			try
			{
				SyntaxTree val = CSharpSyntaxTree.ParseText(File.ReadAllText(text), (CSharpParseOptions)null, "", (Encoding)null, default(CancellationToken));
				SyntaxNode root = val.GetRoot(default(CancellationToken));
				CSharpCompilation val2 = CSharpCompilation.Create("UnityGame", (IEnumerable<SyntaxTree>)null, (IEnumerable<MetadataReference>)null, (CSharpCompilationOptions)null).AddReferences((MetadataReference[])(object)new MetadataReference[1] { (MetadataReference)MetadataReference.CreateFromFile(typeof(object).Assembly.Location, default(MetadataReferenceProperties), (DocumentationProvider)null) });
				foreach (string text2 in assemblies)
				{
					val2 = val2.AddReferences((MetadataReference[])(object)new MetadataReference[1] { (MetadataReference)MetadataReference.CreateFromFile(text2, default(MetadataReferenceProperties), (DocumentationProvider)null) });
				}
				val2 = val2.AddSyntaxTrees((SyntaxTree[])(object)new SyntaxTree[1] { val });
				SemanticModel semanticModel = ((Compilation)val2).GetSemanticModel(val, false);
				ClassDeclarationSyntax[] array = root.DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<ClassDeclarationSyntax>().ToArray();
				bool flag = false;
				ClassDeclarationSyntax val3 = array.FirstOrDefault();
				if (val3 == null)
				{
					list2.Add(text);
					continue;
				}
				SyntaxTokenList modifiers = ((MemberDeclarationSyntax)val3).Modifiers;
				bool isPublic = modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword));
				bool isNotStatic = !modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword));
				bool isNotPartial = !modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword));
				if (!isPublic || !isNotStatic || !isNotPartial)
				{
					list2.Add(text);
					continue;
				}
				SyntaxNode parent = ((SyntaxNode)val3).Parent;
				NamespaceDeclarationSyntax val4 = (NamespaceDeclarationSyntax)(object)((parent is NamespaceDeclarationSyntax) ? parent : null);
				string text3 = null;
				if (val4 == null)
				{
					goto IL_0216;
				}
				text3 = ((object)val4.Name).ToString();
				if (!(text3 != "GameNetcodeStuff") || !(text3 != "Dissonance.Integrations.Unity_NFGO"))
				{
					goto IL_0216;
				}
				list2.Add(text);
				goto end_IL_001a;
				IL_0216:
				ClassDeclarationSyntax[] array2 = array;
				foreach (ClassDeclarationSyntax val5 in array2)
				{
					INamedTypeSymbol? symbol = semanticModel.GetDeclaredSymbol(val5);
					if (symbol != null && supportedTypes.Where((string x) => !x.StartsWith("I")).Any((string x) => InheritsFromType((ITypeSymbol?)(object)symbol, x)))
					{
						log(((object)((BaseTypeDeclarationSyntax)val5).Identifier/*cast due to constrained. prefix*/).ToString());
						string text4 = text.Replace(Path.Combine(assetRipperPath, "Assets"), dataPath);
						log("Creating " + text4 + " at " + Path.GetDirectoryName(text4));
						string directoryName = Path.GetDirectoryName(text4);
						if (directoryName != null)
						{
							Directory.CreateDirectory(directoryName);
							string? fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
							string path = text4;
							string text5 = fileNameWithoutExtension;
							string contents = fileTemplate.Replace("$CLASS_NAME$", text5).Replace("$BASE_CLASS$", (string.IsNullOrEmpty(text3) ? "global::" : (text3 + ".")) + text5);
							File.WriteAllText(path, contents);
							list.Add(text);
							flag = true;
							break;
						}
						list2.Add(text);
					}
				}
				if (!flag)
				{
					list2.Add(text);
				}
				end_IL_001a:;
			}
			catch (Exception arg)
			{
				log($"Error: {arg}");
				list2.Add(text);
			}
		}
		foreach (string item in list2)
		{
			if (File.Exists(item))
			{
				File.Delete(item);
			}
		}
		return list.ToArray();
	}

	private static bool InheritsFromType(ITypeSymbol? typeSymbol, string type)
	{
		if (typeSymbol == null)
		{
			return false;
		}
		if (((ISymbol)typeSymbol).Name == type)
		{
			return true;
		}
		return InheritsFromType((ITypeSymbol?)(object)typeSymbol.BaseType, type);
	}
}
}
