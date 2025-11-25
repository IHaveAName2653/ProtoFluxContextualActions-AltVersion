using Elements.Core;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Runtimes.Execution.Nodes.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProtoFluxContextualActions.Patches;

static partial class ContextualSwapActionsPatch
{
	static readonly HashSet<Type> DynamicImpulseGroup = [
		typeof(DynamicImpulseReceiver),
		typeof(DynamicImpulseTrigger),
		typeof(DynamicImpulseReceiverWithValue<>),
		typeof(DynamicImpulseReceiverWithObject<>),
		typeof(DynamicImpulseTriggerWithValue<>),
		typeof(DynamicImpulseTriggerWithObject<>),
	];

	internal static IEnumerable<MenuItem> DynamicImpulseGroupItems(ContextualContext context)
	{
		if (DynamicImpulseGroup.Any(t => context.NodeType.IsGenericType ? t == context.NodeType.GetGenericTypeDefinition() : t == context.NodeType))
		{

			yield return new MenuItem(typeof(DynamicImpulseReceiver), connectionTransferType: ConnectionTransferType.ByIndexLossy);
			yield return new MenuItem(typeof(DynamicImpulseTrigger), connectionTransferType: ConnectionTransferType.ByIndexLossy);

			Type? target = null;
			if (context.proxy is ProtoFluxInputProxy)
			{
				ProtoFluxInputProxy inputType = (ProtoFluxInputProxy)(context.proxy);
				Type targetType = inputType.InputType;
				target = targetType;
			}
			if (context.proxy is ProtoFluxOutputProxy)
			{
				ProtoFluxOutputProxy outputType = (ProtoFluxOutputProxy)(context.proxy);
				Type targetType = outputType.OutputType;
				target = targetType;
			}
			if (context.NodeType.IsGenericType && target == null)
			{
				var opCount = context.NodeType.GenericTypeArguments.Length;
				var opType = context.NodeType.GenericTypeArguments[opCount - 1];
				target = opType;
			}

			if (target == null) yield break;

			var variableInput = GetNodeForType(target, [
				new NodeTypeRecord(typeof(DynamicImpulseReceiverWithValue<>), null, null),
				new NodeTypeRecord(typeof(DynamicImpulseReceiverWithObject<>), null, null),
			]);
			yield return new(variableInput, connectionTransferType: ConnectionTransferType.ByIndexLossy);

			var variableInput2 = GetNodeForType(target, [
				new NodeTypeRecord(typeof(DynamicImpulseTriggerWithValue<>), null, null),
				new NodeTypeRecord(typeof(DynamicImpulseTriggerWithObject<>), null, null),
			]);
			yield return new(variableInput2, connectionTransferType: ConnectionTransferType.ByIndexLossy);
		}
	}
}