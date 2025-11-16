using Elements.Core;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Math;
using HarmonyLib;
using ProtoFluxContextualActions.Utils;
using System;
using System.Collections.Generic;

namespace ProtoFluxContextualActions.Patches;

static partial class ContextualSwapActionsPatch
{
	static readonly HashSet<Type> LerpGroup = [
	  typeof(ValueSmoothLerp<>),
	typeof(ValueConstantLerp<>),
	typeof(ValueLerp<>),
	typeof(ValueLerpUnclamped<>),
	typeof(ValueMultiLerp<>),
	typeof(ValueInverseLerp<>),
  ];

	internal static IEnumerable<MenuItem> LerpGroupItems(ContextualContext context)
	{
		if (TypeUtils.TryGetGenericTypeDefinition(context.NodeType, out var genericType) && LerpGroup.Contains(genericType))
		{
			var opCount = context.NodeType.GenericTypeArguments.Length;
			var opType = context.NodeType.GenericTypeArguments[opCount - 1];
			var coder = Traverse.Create(typeof(Coder<>).MakeGenericType(opType));

			// in theory, this check shouldn't be needed
			// in practice, https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/3319
			if (coder.Property<bool>("SupportsAddSub").Value)
			{
				yield return new(typeof(ValueSmoothLerp<>).MakeGenericType(opType));
				yield return new(typeof(ValueConstantLerp<>).MakeGenericType(opType));
				yield return new(typeof(ValueLerp<>).MakeGenericType(opType));
				yield return new(typeof(ValueLerpUnclamped<>).MakeGenericType(opType));
				yield return new(typeof(ValueMultiLerp<>).MakeGenericType(opType));
				yield return new(typeof(ValueInverseLerp<>).MakeGenericType(opType));
			}
		}
	}
}