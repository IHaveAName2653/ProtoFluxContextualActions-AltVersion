using ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Slots;
using System;
using System.Collections.Generic;

namespace ProtoFluxContextualActions.Patches;

static partial class ContextualSwapActionsPatch
{
	static readonly HashSet<Type> SlotMetaGroup = [
		typeof(GetSlotName),
		typeof(SetSlotName),
		typeof(GetTag),
	];

	internal static IEnumerable<MenuItem> SlotMetaGroupItems(ContextualContext context)
	{
		if (SlotMetaGroup.Contains(context.NodeType))
		{
			foreach (var match in SlotMetaGroup)
			{
				yield return new MenuItem(match);
			}
		}
	}
}