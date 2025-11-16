using ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Audio;
using System;
using System.Collections.Generic;

namespace ProtoFluxContextualActions.Patches;

static partial class ContextualSwapActionsPatch
{
	static readonly HashSet<Type> PlayOneShotGroup = [
	  typeof(PlayOneShot),
	typeof(PlayOneShotAndWait),
  ];

	internal static IEnumerable<MenuItem> PlayOneShotGroupItems(ContextualContext context)
	{
		if (PlayOneShotGroup.Contains(context.NodeType))
		{
			foreach (var match in PlayOneShotGroup)
			{
				yield return new MenuItem(match);
			}
		}
	}
}