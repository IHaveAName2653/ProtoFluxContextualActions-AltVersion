using Elements.Core;
using HarmonyLib;
using ProtoFlux.Runtimes.Execution.Nodes;
using ProtoFluxContextualActions.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProtoFluxContextualActions.Patches;

static partial class ContextualSwapActionsPatch
{
	static readonly HashSet<Type> NullGroup = [
		typeof(IsNull<>),
		typeof(NotNull<>),
	];

	internal static IEnumerable<MenuItem> NullGroupItems(ContextualContext context)
	{
		if (TypeUtils.TryGetGenericTypeDefinition(context.NodeType, out var genericType) && NullGroup.Contains(genericType))
		{
			var opCount = context.NodeType.GenericTypeArguments.Length;
			var opType = context.NodeType.GenericTypeArguments[opCount - 1];

			List<Type> validTypes = NullGroup.ToList();
			validTypes.Remove(context.NodeType);
			foreach (var type in validTypes)
			{
				yield return new MenuItem(type);
			}
		}
	}
}