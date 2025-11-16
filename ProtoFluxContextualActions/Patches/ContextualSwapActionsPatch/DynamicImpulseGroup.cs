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
			yield return new MenuItem(typeof(DynamicImpulseReceiver));
			yield return new MenuItem(typeof(DynamicImpulseTrigger));

			if (context.proxy is ProtoFluxInputProxy)
			{
				ProtoFluxInputProxy inputType = (ProtoFluxInputProxy)(context.proxy);
				Type targetType = inputType.InputType;

				var variableInput = GetNodeForType(targetType, [
				  new NodeTypeRecord(typeof(DynamicImpulseReceiverWithValue<>), null, null),
		  new NodeTypeRecord(typeof(DynamicImpulseReceiverWithObject<>), null, null),
		]);
				var variableInput2 = GetNodeForType(targetType, [
				  new NodeTypeRecord(typeof(DynamicImpulseTriggerWithValue<>), null, null),
		  new NodeTypeRecord(typeof(DynamicImpulseTriggerWithObject<>), null, null),
		]);

				yield return new(variableInput);
				yield return new(variableInput2);
			}
			if (context.proxy is ProtoFluxOutputProxy)
			{
				ProtoFluxOutputProxy outputType = (ProtoFluxOutputProxy)(context.proxy);
				Type targetType = outputType.OutputType;

				var variableInput = GetNodeForType(targetType, [
				  new NodeTypeRecord(typeof(DynamicImpulseReceiverWithValue<>), null, null),
		  new NodeTypeRecord(typeof(DynamicImpulseReceiverWithObject<>), null, null),
		]);
				var variableInput2 = GetNodeForType(targetType, [
				  new NodeTypeRecord(typeof(DynamicImpulseTriggerWithValue<>), null, null),
		  new NodeTypeRecord(typeof(DynamicImpulseTriggerWithObject<>), null, null),
		]);

				yield return new(variableInput);
				yield return new(variableInput2);
			}
		}
	}
}