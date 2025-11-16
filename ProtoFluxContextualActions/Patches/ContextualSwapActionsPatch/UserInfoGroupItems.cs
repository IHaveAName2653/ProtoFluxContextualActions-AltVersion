using ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Users;
using System;
using System.Collections.Generic;

namespace ProtoFluxContextualActions.Patches;

static partial class ContextualSwapActionsPatch
{
	static readonly HashSet<Type> UserInfoGroup = [
	  typeof(UserVR_Active),
	typeof(UserFPS),
	typeof(UserTime),
	typeof(UserVoiceMode),
	typeof(UserHeadOutputDevice),
	typeof(UserActiveViewTargettingController),
	typeof(UserPrimaryHand),
	typeof(UserUserID),
	typeof(UserUsername),
	typeof(UserRootSlot),
  ];

	internal static IEnumerable<MenuItem> UserInfoGroupItems(ContextualContext context)
	{
		if (UserInfoGroup.Contains(context.NodeType))
		{
			foreach (var match in UserInfoGroup)
			{
				yield return new MenuItem(match);
			}
		}
	}
}