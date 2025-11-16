using ProtoFluxContextualActions.Tagging;
using System.Collections.Generic;

namespace ProtoFluxContextualActions.Patches;

static partial class ContextualSwapActionsPatch
{
	internal static IEnumerable<MenuItem> EasingOfSameKindDoubleItems(ContextualContext context)
	{
		if (EasingGroups.ContainsNodeDouble(context.NodeType))
		{
			foreach (var match in EasingGroups.GetEasingOfSameKindDouble(context.NodeType))
			{
				yield return new MenuItem(match);
			}
		}
	}
}