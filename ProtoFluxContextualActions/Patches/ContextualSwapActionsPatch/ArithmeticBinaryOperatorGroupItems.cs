using Elements.Core;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using ProtoFlux.Runtimes.Execution.Nodes.Math;
using ProtoFlux.Runtimes.Execution.Nodes.Operators;
using ProtoFluxContextualActions.Extensions;
using ProtoFluxContextualActions.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProtoFluxContextualActions.Patches;

static partial class ContextualSwapActionsPatch
{
	static readonly HashSet<Type> ArithmeticBinaryOperatorGroup = [
		typeof(ValueAdd<>),
		typeof(ValueSub<>),
		typeof(ValueMul<>),
		typeof(ValueDiv<>),
		typeof(ValueMod<>),
		typeof(ValueSquare<>),
	];

	internal static IEnumerable<MenuItem> ArithmeticBinaryOperatorGroupItems(ContextualContext context)
	{
		if (TypeUtils.TryGetGenericTypeDefinition(context.NodeType, out var genericType))
		{
			bool inGroup = ArithmeticBinaryOperatorGroup.Contains(genericType);
			var psuedoGenerics = context.World.GetPsuedoGenericTypesForWorld();
			var SqrtNodes = psuedoGenerics.Sqrt.ToBiDictionary(a => a.Node, a => a.Types.First());
			var SqrtGroup = SqrtNodes.ToDictionary(a => a.First, a => a.Second);

			bool isSqrt = false;
			if (SqrtGroup.TryGetValue(context.NodeType, out var typeArgument))
			{
				isSqrt = true;
			}
			if (inGroup || isSqrt)
			{
				var opType = context.NodeType.GenericTypeArguments[0];
				var coder = Traverse.Create(typeof(Coder<>).MakeGenericType(opType));

				if (coder.Property<bool>("SupportsAddSub").Value)
				{
					yield return new MenuItem(typeof(ValueAdd<>).MakeGenericType(opType));
					yield return new MenuItem(typeof(ValueSub<>).MakeGenericType(opType));
				}

				if (coder.Property<bool>("SupportsMul").Value)
				{
					yield return new MenuItem(typeof(ValueMul<>).MakeGenericType(opType));
				}

				if (coder.Property<bool>("SupportsDiv").Value)
				{
					yield return new MenuItem(typeof(ValueDiv<>).MakeGenericType(opType));
				}

				if (coder.Property<bool>("SupportsMod").Value)
				{
					yield return new MenuItem(typeof(ValueMod<>).MakeGenericType(opType));
				}

				if (coder.Property<bool>("SupportsMul").Value)
				{
					yield return new MenuItem(typeof(ValueSquare<>).MakeGenericType(opType));
				}

				if (isSqrt)
				{
					var matchingNodes = SqrtGroup.Where(a => a.Value == typeArgument).Select(a => a.Key);
					foreach (var match in matchingNodes)
					{
						yield return new MenuItem(match);
					}
				}
			}
		}
	}
}