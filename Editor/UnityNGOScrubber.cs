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

public static class UnityNGOScrubber
{
	private static readonly string[] StripConditions = new string[6] { "__rpc_exec_stage != __RpcExecStage.Client || (!networkManager.IsClient && !networkManager.IsHost)", "__rpc_exec_stage != __RpcExecStage.Client || (!networkManager.IsClient && !networkManager.IsHost) || NetworkManager.Singleton == null", "__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost)", "__rpc_exec_stage != __RpcExecStage.Server || (!networkManager.IsServer && !networkManager.IsHost)", "__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost)", "__rpc_exec_stage == __RpcExecStage.Client && !networkManager.IsClient && networkManager.IsHost" };

	public static void Scrub(string[] files, Action<string> log)
	{
		foreach (string path in files)
		{
			if (!File.Exists(path))
			{
				continue;
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			if (fileNameWithoutExtension == "UnitySourceGeneratedAssemblyMonoScriptTypes_v1")
			{
				File.Delete(path);
				continue;
			}
			string text = File.ReadAllText(path);
			if (fileNameWithoutExtension == "NfgoClient")
			{
				int startIndex = text.IndexOf("public NfgoClient([NotNull] NfgoCommsNetwork network)", StringComparison.Ordinal);
				if (text.IndexOf("base(network)", startIndex, StringComparison.Ordinal) == -1)
				{
					int num = text.IndexOf(')', startIndex);
					text = text.Insert(num + 1, " : base(network)");
				}
			}
			SyntaxNode root = CSharpSyntaxTree.ParseText(text, (CSharpParseOptions)null, "", (Encoding)null, default(CancellationToken)).GetRoot(default(CancellationToken));
			IEnumerable<MethodDeclarationSyntax> enumerable = root.DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<MethodDeclarationSyntax>().Where(delegate(MethodDeclarationSyntax m)
			{
				SyntaxToken identifier = m.Identifier;
				if (!identifier.Text.StartsWith("__getTypeName"))
				{
					identifier = m.Identifier;
					if (!identifier.Text.StartsWith("__initializeVariables"))
					{
						identifier = m.Identifier;
						if (!identifier.Text.StartsWith("InitializeRPCS_"))
						{
							identifier = m.Identifier;
							return identifier.Text.StartsWith("__rpc_handler_");
						}
					}
				}
				return true;
			});
			SyntaxNode val = SyntaxNodeExtensions.RemoveNodes<SyntaxNode>(root, (IEnumerable<SyntaxNode>)enumerable, (SyntaxRemoveOptions)0);
			foreach (ClassDeclarationSyntax item in val.DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<ClassDeclarationSyntax>())
			{
				IEnumerable<string> source = ((SyntaxNode)item).DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<IdentifierNameSyntax>().Select(delegate(IdentifierNameSyntax x)
				{
					SyntaxToken identifier = ((SimpleNameSyntax)x).Identifier;
					return identifier.Text;
				});
				ClassDeclarationSyntax val2 = item;
				if (source.Contains("INetworkSerializable") && !((SyntaxNode)item).DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<MethodDeclarationSyntax>().Any(delegate(MethodDeclarationSyntax x)
				{
					SyntaxToken identifier = x.Identifier;
					return identifier.Text == "NetworkSerialize";
				}))
				{
					MethodDeclarationSyntax val3 = SyntaxFactory.MethodDeclaration((TypeSyntax)(object)SyntaxFactory.PredefinedType(SyntaxFactory.Token((SyntaxKind)8318)), "NetworkSerialize").WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token((SyntaxKind)8343))).WithTypeParameterList(SyntaxFactory.TypeParameterList(SyntaxFactory.SingletonSeparatedList<TypeParameterSyntax>(SyntaxFactory.TypeParameter("T"))))
						.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(SyntaxFactory.Parameter(SyntaxFactory.Identifier("serializer")).WithType((TypeSyntax)(object)SyntaxFactory.IdentifierName("BufferSerializer<T>")))))
						.WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>((StatementSyntax)(object)SyntaxFactory.ThrowStatement((ExpressionSyntax)(object)SyntaxFactory.ObjectCreationExpression((TypeSyntax)(object)SyntaxFactory.IdentifierName("System.NotImplementedException")).WithArgumentList(SyntaxFactory.ArgumentList(default(SeparatedSyntaxList<ArgumentSyntax>)))))))
						.WithSemicolonToken(SyntaxFactory.Token((SyntaxKind)8212));
					val3 = val3.AddConstraintClauses((TypeParameterConstraintClauseSyntax[])(object)new TypeParameterConstraintClauseSyntax[1] { SyntaxFactory.TypeParameterConstraintClause("T").WithConstraints(SyntaxFactory.SingletonSeparatedList<TypeParameterConstraintSyntax>((TypeParameterConstraintSyntax)(object)SyntaxFactory.TypeConstraint((TypeSyntax)(object)SyntaxFactory.IdentifierName("IReaderWriter")))) });
					val2 = val2.AddMembers((MemberDeclarationSyntax[])(object)new MemberDeclarationSyntax[1] { (MemberDeclarationSyntax)val3 });
				}
				if (source.Contains("IAsyncStateMachine"))
				{
					if (!((SyntaxNode)item).DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<MethodDeclarationSyntax>().Any(delegate(MethodDeclarationSyntax x)
					{
						SyntaxToken identifier = x.Identifier;
						return identifier.Text == "MoveNext";
					}))
					{
						MethodDeclarationSyntax val4 = SyntaxFactory.MethodDeclaration((TypeSyntax)(object)SyntaxFactory.PredefinedType(SyntaxFactory.Token((SyntaxKind)8318)), "MoveNext").WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token((SyntaxKind)8343))).WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>((StatementSyntax)(object)SyntaxFactory.ThrowStatement((ExpressionSyntax)(object)SyntaxFactory.ObjectCreationExpression((TypeSyntax)(object)SyntaxFactory.IdentifierName("System.NotImplementedException")).WithArgumentList(SyntaxFactory.ArgumentList(default(SeparatedSyntaxList<ArgumentSyntax>)))))))
							.WithSemicolonToken(SyntaxFactory.Token((SyntaxKind)8212));
						val2 = val2.AddMembers((MemberDeclarationSyntax[])(object)new MemberDeclarationSyntax[1] { (MemberDeclarationSyntax)val4 });
					}
					if (!((SyntaxNode)item).DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<MethodDeclarationSyntax>().Any(delegate(MethodDeclarationSyntax x)
					{
						SyntaxToken identifier = x.Identifier;
						return identifier.Text == "SetStateMachine";
					}))
					{
						MethodDeclarationSyntax val5 = SyntaxFactory.MethodDeclaration((TypeSyntax)(object)SyntaxFactory.PredefinedType(SyntaxFactory.Token((SyntaxKind)8318)), "SetStateMachine").WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token((SyntaxKind)8343))).WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(SyntaxFactory.Parameter(SyntaxFactory.Identifier("stateMachine")).WithType((TypeSyntax)(object)SyntaxFactory.IdentifierName("IAsyncStateMachine")))))
							.WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>((StatementSyntax)(object)SyntaxFactory.ThrowStatement((ExpressionSyntax)(object)SyntaxFactory.ObjectCreationExpression((TypeSyntax)(object)SyntaxFactory.IdentifierName("System.NotImplementedException")).WithArgumentList(SyntaxFactory.ArgumentList(default(SeparatedSyntaxList<ArgumentSyntax>)))))))
							.WithSemicolonToken(SyntaxFactory.Token((SyntaxKind)8212));
						val2 = val2.AddMembers((MemberDeclarationSyntax[])(object)new MemberDeclarationSyntax[1] { (MemberDeclarationSyntax)val5 });
					}
				}
				val = SyntaxNodeExtensions.ReplaceNode<SyntaxNode>(val, (SyntaxNode)(object)item, (SyntaxNode)(object)val2);
			}
			if (val != null)
			{
				val = ((CSharpSyntaxVisitor<SyntaxNode>)(object)new RemoveCtorMethodCalls()).Visit(val);
			}
			List<StructDeclarationSyntax> list = val.DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<StructDeclarationSyntax>().ToList();
			while (list.Any((StructDeclarationSyntax x) => ((SyntaxNode)x).DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<ConstructorDeclarationSyntax>().ToList()
				.Count > 0))
			{
				foreach (StructDeclarationSyntax item2 in list)
				{
					List<ConstructorDeclarationSyntax> list2 = ((SyntaxNode)item2).DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<ConstructorDeclarationSyntax>().ToList();
					StructDeclarationSyntax val6 = item2;
					foreach (ConstructorDeclarationSyntax item3 in list2)
					{
						StructDeclarationSyntax val7 = SyntaxNodeExtensions.RemoveNode<StructDeclarationSyntax>(val6, (SyntaxNode)(object)item3, (SyntaxRemoveOptions)0);
						val = SyntaxNodeExtensions.ReplaceNode<SyntaxNode>(val, (SyntaxNode)(object)val6, (SyntaxNode)(object)val7);
						val6 = val7;
					}
				}
				list = val.DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<StructDeclarationSyntax>().ToList();
			}
			string contents = val.ToFullString();
			File.WriteAllText(path, contents);
		}
	}

	private static SyntaxNode Scrub__rpcCalls(SyntaxNode root)
	{
		IEnumerable<MethodDeclarationSyntax> enumerable = root.DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<MethodDeclarationSyntax>().Where(delegate(MethodDeclarationSyntax m)
		{
			SyntaxToken identifier = m.Identifier;
			if (!identifier.Text.StartsWith("__getTypeName"))
			{
				identifier = m.Identifier;
				if (!identifier.Text.StartsWith("__initializeVariables"))
				{
					identifier = m.Identifier;
					if (!identifier.Text.StartsWith("InitializeRPCS_"))
					{
						identifier = m.Identifier;
						return identifier.Text.StartsWith("__rpc_handler_");
					}
				}
			}
			return true;
		});
		return SyntaxNodeExtensions.RemoveNodes<SyntaxNode>(root, (IEnumerable<SyntaxNode>)enumerable, (SyntaxRemoveOptions)0);
	}

	public static void ScrubDecompiledScript(string[] files, bool outputCopy, Action<string> log)
	{
		foreach (string text in files)
		{
			try
			{
				string text2 = text.Replace(".cs", ".copy.cs");
				if (File.Exists(text2))
				{
					File.Delete(text2);
				}
				if (!File.Exists(text))
				{
					continue;
				}
				if (Path.GetFileNameWithoutExtension(text) == "UnitySourceGeneratedAssemblyMonoScriptTypes_v1")
				{
					File.Delete(text);
					continue;
				}
				SyntaxNode root = CSharpSyntaxTree.ParseText(File.ReadAllText(text), (CSharpParseOptions)null, "", (Encoding)null, default(CancellationToken)).GetRoot(default(CancellationToken));
				root = Scrub__rpcCalls(root);
				MemberDeclarationSyntax[] array = root.DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<MemberDeclarationSyntax>().ToArray();
				List<(MethodDeclarationSyntax, MethodDeclarationSyntax)> methodsToReplace = new List<(MethodDeclarationSyntax, MethodDeclarationSyntax)>();
				List<SyntaxNode> list = new List<SyntaxNode>();
				string[] bannedAttributes = new string[1] { "MonoPInvokeCallback" };
				MemberDeclarationSyntax[] array2 = array;
				foreach (MemberDeclarationSyntax obj in array2)
				{
					MethodDeclarationSyntax val = (MethodDeclarationSyntax)(object)((obj is MethodDeclarationSyntax) ? obj : null);
					if (val == null)
					{
						continue;
					}
					AttributeSyntax[] array3 = ((IEnumerable<AttributeListSyntax>)(object)((MemberDeclarationSyntax)val).AttributeLists).SelectMany((AttributeListSyntax x) => (IEnumerable<AttributeSyntax>)(object)x.Attributes).ToArray();
					if (array3.Any((AttributeSyntax x) => Enumerable.Contains<string>(bannedAttributes, ((object)x.Name).ToString())))
					{
						list.AddRange((IEnumerable<SyntaxNode>)(object)array3);
						list.Add((SyntaxNode)(object)val);
						continue;
					}
					AttributeSyntax? obj2 = array3.FirstOrDefault((AttributeSyntax x) => ((object)x.Name).ToString() == "ServerRpc");
					AttributeSyntax val2 = array3.FirstOrDefault((AttributeSyntax x) => ((object)x.Name).ToString() == "ClientRpc");
					MethodDeclarationSyntax val3 = null;
					if (obj2 != null)
					{
						val3 = HandleRpcFunction(val, log);
					}
					else
					{
						if (val2 == null)
						{
							continue;
						}
						val3 = HandleRpcFunction(val, log);
					}
					if (val3 != null)
					{
						methodsToReplace.Add((val, val3));
					}
				}
				root = SyntaxNodeExtensions.ReplaceNodes<SyntaxNode, MethodDeclarationSyntax>(root, methodsToReplace.Select<(MethodDeclarationSyntax, MethodDeclarationSyntax), MethodDeclarationSyntax>(((MethodDeclarationSyntax, MethodDeclarationSyntax) x) => x.Item1), (Func<MethodDeclarationSyntax, MethodDeclarationSyntax, SyntaxNode>)((MethodDeclarationSyntax x, MethodDeclarationSyntax y) => (SyntaxNode)(object)methodsToReplace.First<(MethodDeclarationSyntax, MethodDeclarationSyntax)>(((MethodDeclarationSyntax, MethodDeclarationSyntax) z) => z.Item1 == x).Item2));
				root = SyntaxNodeExtensions.RemoveNodes<SyntaxNode>(root, (IEnumerable<SyntaxNode>)list, (SyntaxRemoveOptions)0);
				string text3 = root.ToFullString();
				if (root.DescendantNodes((Func<SyntaxNode, bool>)null, false).OfType<ClassDeclarationSyntax>().FirstOrDefault(delegate(ClassDeclarationSyntax x)
				{
					SyntaxToken identifier = ((BaseTypeDeclarationSyntax)x).Identifier;
					return identifier.Text == "StartOfRound";
				}) != null)
				{
					text3 = text3.Replace("voiceChatModule.IsMuted = !IngamePlayerSettings.Instance.playerInput.actions.FindAction(\"VoiceButton\").IsPressed() && !GameNetworkManager.Instance.localPlayerController.speakingToWalkieTalkie;", "// voiceChatModule.IsMuted = !IngamePlayerSettings.Instance.playerInput.actions.FindAction(\"VoiceButton\").IsPressed() && !GameNetworkManager.Instance.localPlayerController.speakingToWalkieTalkie;");
					text3 = text3.Replace("voiceChatModule.IsMuted = !IngamePlayerSettings.Instance.settings.micEnabled;", "// voiceChatModule.IsMuted = !IngamePlayerSettings.Instance.settings.micEnabled;");
					text3 = text3.Replace("if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null || GameNetworkManager.Instance.localPlayerController.isPlayerDead || voiceChatModule.IsMuted || !voiceChatModule.enabled || voiceChatModule == null)", "if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null || GameNetworkManager.Instance.localPlayerController.isPlayerDead || voiceChatModule == null)");
					text3 = text3.Replace("allPlayerScripts[i].gameObject.GetComponentInChildren<NfgoPlayer>().VoiceChatTrackingStart();", "// allPlayerScripts[i].gameObject.GetComponentInChildren<NfgoPlayer>().VoiceChatTrackingStart();");
					text3 = text3.Replace("playerControllerB.gameObject.GetComponentInChildren<NfgoPlayer>().VoiceChatTrackingStart();", "// playerControllerB.gameObject.GetComponentInChildren<NfgoPlayer>().VoiceChatTrackingStart();");
				}
				File.WriteAllText(outputCopy ? text2 : text, text3);
			}
			catch (Exception arg)
			{
				log($"[error] {arg}");
			}
		}
	}

	private static MethodDeclarationSyntax? HandleRpcFunction(MethodDeclarationSyntax methodDeclaration, Action<string> log)
	{
		BlockSyntax body = ((BaseMethodDeclarationSyntax)methodDeclaration).Body;
		SyntaxList<StatementSyntax>? val = ((body != null) ? new SyntaxList<StatementSyntax>?(body.Statements) : ((SyntaxList<StatementSyntax>?)null));
		if (val.HasValue)
		{
			SyntaxList<StatementSyntax> valueOrDefault = val.GetValueOrDefault();
			if (valueOrDefault.Count == 2)
			{
				StatementSyntax obj = valueOrDefault[1];
				IfStatementSyntax val2 = (IfStatementSyntax)(object)((obj is IfStatementSyntax) ? obj : null);
				if (val2 == null)
				{
					log("[error] no secondStatementIf");
					return null;
				}
				SyntaxNode[] array = ((SyntaxNode)val2).ChildNodes().ToArray();
				if (array.Length > 1)
				{
					array = array[1].ChildNodes().ToArray();
					SyntaxNode obj2 = ((array.Length == 1) ? array[0] : array[1]);
					IfStatementSyntax val3 = (IfStatementSyntax)(object)((obj2 is IfStatementSyntax) ? obj2 : null);
					if (val3 == null)
					{
						log("[error] no nestedNodeIf");
						return null;
					}
					IfStatementSyntax val4 = StripIfStatement(val3, log);
					if (val4 != null)
					{
						return SyntaxFactory.MethodDeclaration(methodDeclaration.ReturnType, methodDeclaration.Identifier).WithModifiers(((MemberDeclarationSyntax)methodDeclaration).Modifiers).WithParameterList(((BaseMethodDeclarationSyntax)methodDeclaration).ParameterList)
							.WithAttributeLists(((MemberDeclarationSyntax)methodDeclaration).AttributeLists)
							.WithBody(SyntaxFactory.Block((StatementSyntax[])(object)new StatementSyntax[1] { (StatementSyntax)val4 }));
					}
					MethodDeclarationSyntax obj3 = SyntaxFactory.MethodDeclaration(methodDeclaration.ReturnType, methodDeclaration.Identifier).WithModifiers(((MemberDeclarationSyntax)methodDeclaration).Modifiers).WithParameterList(((BaseMethodDeclarationSyntax)methodDeclaration).ParameterList)
						.WithAttributeLists(((MemberDeclarationSyntax)methodDeclaration).AttributeLists);
					StatementSyntax statement = val3.Statement;
					return obj3.WithBody((BlockSyntax)(object)((statement is BlockSyntax) ? statement : null));
				}
				StatementSyntax obj4 = valueOrDefault[3];
				IfStatementSyntax val5 = (IfStatementSyntax)(object)((obj4 is IfStatementSyntax) ? obj4 : null);
				if (val5 == null)
				{
					log("[error] no thirdStatementIf");
					return null;
				}
				StripIfStatement(val5, log);
				log("<color=red>[error] not handled yet</color>");
				return null;
			}
			StatementSyntax obj5 = valueOrDefault[3];
			IfStatementSyntax val6 = (IfStatementSyntax)(object)((obj5 is IfStatementSyntax) ? obj5 : null);
			if (val6 == null)
			{
				log("[error] no fourthStatementIf");
				return null;
			}
			IfStatementSyntax val7 = StripIfStatement(val6, log);
			if (val7 != null)
			{
				IEnumerable<StatementSyntax> source = ((IEnumerable<StatementSyntax>)(object)valueOrDefault).Skip(4);
				return SyntaxFactory.MethodDeclaration(methodDeclaration.ReturnType, methodDeclaration.Identifier).WithModifiers(((MemberDeclarationSyntax)methodDeclaration).Modifiers).WithParameterList(((BaseMethodDeclarationSyntax)methodDeclaration).ParameterList)
					.WithAttributeLists(((MemberDeclarationSyntax)methodDeclaration).AttributeLists)
					.WithBody(SyntaxFactory.Block(SyntaxFactory.List<StatementSyntax>(source.Prepend((StatementSyntax)(object)val7))));
			}
			StatementSyntax[] array2 = ((IEnumerable<StatementSyntax>)(object)valueOrDefault).Skip(4).ToArray();
			if (((SyntaxNode)val6.Statement).ChildNodes().FirstOrDefault() is ReturnStatementSyntax)
			{
				return SyntaxFactory.MethodDeclaration(methodDeclaration.ReturnType, methodDeclaration.Identifier).WithModifiers(((MemberDeclarationSyntax)methodDeclaration).Modifiers).WithParameterList(((BaseMethodDeclarationSyntax)methodDeclaration).ParameterList)
					.WithAttributeLists(((MemberDeclarationSyntax)methodDeclaration).AttributeLists)
					.WithBody(SyntaxFactory.Block(SyntaxFactory.List<StatementSyntax>((IEnumerable<StatementSyntax>)array2)));
			}
			return SyntaxFactory.MethodDeclaration(methodDeclaration.ReturnType, methodDeclaration.Identifier).WithModifiers(((MemberDeclarationSyntax)methodDeclaration).Modifiers).WithParameterList(((BaseMethodDeclarationSyntax)methodDeclaration).ParameterList)
				.WithAttributeLists(((MemberDeclarationSyntax)methodDeclaration).AttributeLists)
				.WithBody(SyntaxFactory.Block(((IEnumerable<StatementSyntax>)(object)SyntaxFactory.List<StatementSyntax>((IEnumerable<StatementSyntax>)array2)).Prepend(val6.Statement)));
		}
		log("[error] has no statements");
		return null;
	}

	private static IfStatementSyntax? StripIfStatement(IfStatementSyntax ifStatementSyntax, Action<string> log)
	{
		string text = ((object)ifStatementSyntax.Condition).ToString();
		string[] stripConditions = StripConditions;
		foreach (string oldValue in stripConditions)
		{
			text = text.Replace(oldValue, string.Empty).TrimStart();
		}
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		if (text.StartsWith("||") || text.StartsWith("&&"))
		{
			string text2 = text;
			text = text2.Substring(2, text2.Length - 2).TrimStart();
			ifStatementSyntax = SyntaxFactory.IfStatement(SyntaxFactory.ParseExpression(text, 0, (ParseOptions)null, true), ifStatementSyntax.Statement);
		}
		return ifStatementSyntax;
	}

	private static MethodDeclarationSyntax? HandleServerRpc(string file, MethodDeclarationSyntax methodDeclaration, Action<string> log)
	{
		BlockSyntax body = ((BaseMethodDeclarationSyntax)methodDeclaration).Body;
		SyntaxList<StatementSyntax>? val = ((body != null) ? new SyntaxList<StatementSyntax>?(body.Statements) : ((SyntaxList<StatementSyntax>?)null));
		if (val.HasValue)
		{
			val.GetValueOrDefault();
			return HandleBranchedRpc(file, methodDeclaration, isClientRpc: false, log);
		}
		string[] obj = new string[5] { "[error] ServerRpc method ", null, null, null, null };
		SyntaxToken identifier = methodDeclaration.Identifier;
		obj[1] = identifier.Text;
		obj[2] = " in ";
		obj[3] = file;
		obj[4] = " has no statements";
		log(string.Concat(obj));
		return null;
	}

	private static MethodDeclarationSyntax HandleServerRpc_1(string file, MethodDeclarationSyntax methodDeclaration, IfStatementSyntax ifStatementSyntax, Action<string> log)
	{
		log("[info] [ServerRpc1] found in " + file);
		StatementSyntax statement = ifStatementSyntax.Statement;
		SyntaxList<StatementSyntax> val = SyntaxFactory.List<StatementSyntax>((IEnumerable<StatementSyntax>)(object)new StatementSyntax[1] { statement });
		MethodDeclarationSyntax val2 = SyntaxFactory.MethodDeclaration(methodDeclaration.ReturnType, methodDeclaration.Identifier).WithModifiers(((MemberDeclarationSyntax)methodDeclaration).Modifiers).WithParameterList(((BaseMethodDeclarationSyntax)methodDeclaration).ParameterList)
			.WithAttributeLists(((MemberDeclarationSyntax)methodDeclaration).AttributeLists)
			.WithBody(SyntaxFactory.Block(val));
		log($"[info] new method: {val2}");
		return val2;
	}

	private static MethodDeclarationSyntax HandleServerRpc_2(string file, SyntaxList<StatementSyntax> validStatements, MethodDeclarationSyntax methodDeclaration, IfStatementSyntax ifStatementSyntax, Action<string> log)
	{
		log("[info] [ServerRpc2] found in " + file);
		List<StatementSyntax> source = ((IEnumerable<StatementSyntax>)(object)validStatements).Skip(4).ToList();
		IfStatementSyntax element = StripIfStatementCondition(ifStatementSyntax, keepContent: false, log);
		StatementSyntax[] array = (from x in source.Prepend((StatementSyntax)(object)element)
			where x != null
			select x).Cast<StatementSyntax>().ToArray();
		MethodDeclarationSyntax val = SyntaxFactory.MethodDeclaration(methodDeclaration.ReturnType, methodDeclaration.Identifier).WithModifiers(((MemberDeclarationSyntax)methodDeclaration).Modifiers).WithParameterList(((BaseMethodDeclarationSyntax)methodDeclaration).ParameterList)
			.WithAttributeLists(((MemberDeclarationSyntax)methodDeclaration).AttributeLists)
			.WithBody(SyntaxFactory.Block(array));
		log($"[info] new method: {val}");
		return val;
	}

	private static MethodDeclarationSyntax? HandleBranchedRpc(string file, MethodDeclarationSyntax methodDeclaration, bool isClientRpc, Action<string> log)
	{
		BlockSyntax body = ((BaseMethodDeclarationSyntax)methodDeclaration).Body;
		SyntaxList<StatementSyntax>? val = ((body != null) ? new SyntaxList<StatementSyntax>?(body.Statements) : ((SyntaxList<StatementSyntax>?)null));
		if (val.HasValue)
		{
			SyntaxList<StatementSyntax> valueOrDefault = val.GetValueOrDefault();
			Enumerator<StatementSyntax> enumerator = valueOrDefault.GetEnumerator();
			while (enumerator.MoveNext())
			{
				StatementSyntax current = enumerator.Current;
				log("[info] Rpc statement is " + ((object)current).GetType().FullName + ": " + ((SyntaxNode)current).ToFullString());
			}
			StatementSyntax[] array;
			if (valueOrDefault.Count > 2)
			{
				array = ((IEnumerable<StatementSyntax>)(object)valueOrDefault).Skip(3).ToArray();
				IfStatementSyntax val2 = (IfStatementSyntax)array[0];
				IfStatementSyntax val3 = StripIfStatementCondition(val2, isClientRpc, log);
				if (val3 == null)
				{
					if (val3 != null && ((SyntaxNode)val3.Statement).ChildNodes().Count() == 1 && ((SyntaxNode)val3.Statement).ChildNodes().First() is ReturnStatementSyntax)
					{
						array = array[1..];
					}
					else
					{
						array[0] = val2.Statement;
					}
				}
				else
				{
					array[0] = (StatementSyntax)(object)val3;
				}
			}
			else
			{
				log("nested");
				IfStatementSyntax val4 = (IfStatementSyntax)valueOrDefault[1];
				IfStatementSyntax val5 = StripIfStatementCondition(val4, isClientRpc, log);
				if (val5 == null)
				{
					val4 = (IfStatementSyntax)((SyntaxNode)val4).ChildNodes().ElementAt(1);
					array = (StatementSyntax[])(object)new StatementSyntax[1] { val4.Statement };
				}
				else
				{
					StatementSyntax[] array2 = (StatementSyntax[])(object)new IfStatementSyntax[1] { val5 };
					array = array2;
				}
			}
			MethodDeclarationSyntax val6 = SyntaxFactory.MethodDeclaration(methodDeclaration.ReturnType, methodDeclaration.Identifier).WithModifiers(((MemberDeclarationSyntax)methodDeclaration).Modifiers).WithParameterList(((BaseMethodDeclarationSyntax)methodDeclaration).ParameterList)
				.WithAttributeLists(((MemberDeclarationSyntax)methodDeclaration).AttributeLists)
				.WithBody(SyntaxFactory.Block(SyntaxFactory.List<StatementSyntax>((IEnumerable<StatementSyntax>)array)));
			log($"[info] new method: {val6}");
			return val6;
		}
		string[] obj = new string[5] { "[error] Rpc method ", null, null, null, null };
		SyntaxToken identifier = methodDeclaration.Identifier;
		obj[1] = identifier.Text;
		obj[2] = " in ";
		obj[3] = file;
		obj[4] = " has no statements";
		log(string.Concat(obj));
		return null;
	}

	private static MethodDeclarationSyntax? HandleClientRpc(string file, MethodDeclarationSyntax methodDeclaration, Action<string> log)
	{
		BlockSyntax body = ((BaseMethodDeclarationSyntax)methodDeclaration).Body;
		SyntaxList<StatementSyntax>? val = ((body != null) ? new SyntaxList<StatementSyntax>?(body.Statements) : ((SyntaxList<StatementSyntax>?)null));
		SyntaxToken identifier;
		if (!val.HasValue)
		{
			string[] obj = new string[5] { "[error] ClientRpc method ", null, null, null, null };
			identifier = methodDeclaration.Identifier;
			obj[1] = identifier.Text;
			obj[2] = " in ";
			obj[3] = file;
			obj[4] = " has no statements";
			log(string.Concat(obj));
			return null;
		}
		if (val.GetValueOrDefault().Count < 2)
		{
			string[] obj2 = new string[5] { "[error] ClientRpc method ", null, null, null, null };
			identifier = methodDeclaration.Identifier;
			obj2[1] = identifier.Text;
			obj2[2] = " in ";
			obj2[3] = file;
			obj2[4] = " has less than 2 statements";
			log(string.Concat(obj2));
			return null;
		}
		return HandleBranchedRpc(file, methodDeclaration, isClientRpc: true, log);
	}

	private static IfStatementSyntax? StripIfStatementCondition(IfStatementSyntax ifStatementSyntax, bool keepContent, Action<string> log)
	{
		string text = ((object)ifStatementSyntax.Condition).ToString();
		string[] stripConditions = StripConditions;
		foreach (string oldValue in stripConditions)
		{
			text = text.Replace(oldValue, string.Empty).TrimStart();
		}
		log("[new condition1] \"" + text + "\"");
		if (!string.IsNullOrEmpty(text) && (text.StartsWith("||") || text.StartsWith("&&")))
		{
			string text2 = text;
			text = text2.Substring(2, text2.Length - 2).TrimStart();
		}
		log("[new condition2] \"" + text + "\"");
		IfStatementSyntax val = SyntaxFactory.IfStatement(SyntaxFactory.ParseExpression(text, 0, (ParseOptions)null, true), (StatementSyntax)(keepContent ? ((object)ifStatementSyntax.Statement) : ((object)SyntaxFactory.Block((StatementSyntax[])(object)new StatementSyntax[1] { (StatementSyntax)SyntaxFactory.ReturnStatement((ExpressionSyntax)null) }))));
		if (string.IsNullOrEmpty(((object)val.Condition).ToString()))
		{
			return null;
		}
		return val;
	}
}
}