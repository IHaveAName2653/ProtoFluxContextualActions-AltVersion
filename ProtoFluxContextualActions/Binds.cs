using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.Undo;
using HarmonyLib;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFluxContextualActions.Attributes;
using ProtoFluxContextualActions.Extensions;
using ProtoFluxContextualActions.Patches;
using ProtoFluxContextualActions.Utils.ProtoFlux;
using Renderite.Shared;
using ResoniteModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProtoFluxContextualActions;

[HarmonyPatch(typeof(ProtoFluxTool), nameof(ProtoFluxTool.Update))]
internal class Binds
{
	[AutoRegisterConfigKey]
	public static readonly ModConfigurationKey<bool> DesktopUsesVRBinds = new ModConfigurationKey<bool>("Desktop Uses VR Binds", "If desktop should use the same binds as VR", () => false);

	[AutoRegisterConfigKey]
	public static readonly ModConfigurationKey<Key> SecondaryKey = new ModConfigurationKey<Key>("Secondary Key", "What key to use for 'opposite' secondary", () => Key.BackQuote);
	[AutoRegisterConfigKey]
	public static readonly ModConfigurationKey<Key> MenuKey = new ModConfigurationKey<Key>("Menu Key", "What key to use for 'opposite' menu", () => Key.LeftShift);

	internal static Target GetBind(ProtoFluxToolData data)
	{
		// Desktop specific binds
		if (ShouldUseDesktopBinds(data))
		{
			if (data.GrabbedReference != null && data.Menu.Opposite.pressedThisUpdate) return Target.Reference;
			if (data.Secondary.Opposite.pressedThisUpdate && data.Menu.Opposite.currentlyPressed) return Target.Swap;
			if (data.Menu.Opposite.pressedThisUpdate && !data.Menu.Opposite.currentlyPressed) return Target.Selection;
		}
		else
		{
			// VR specific binds (or desktop sometimes)
			if (data.Secondary.Opposite.currentlyPressed && data.Menu.Primary.pressedThisUpdate) return Target.Selection;
			if (data.Menu.Primary.IsHeld && data.Secondary.Opposite.currentlyPressed) return Target.Swap;
			if (data.Menu.Primary.IsHeld && !data.Secondary.Opposite.currentlyPressed) return Target.Selection;
			if (data.Secondary.Opposite.IsHeld && data.Secondary.Primary.pressedThisUpdate) return Target.Swap;
		}

		return Target.None;
	}

	internal static bool Prefix(ProtoFluxTool __instance, SyncRef<ProtoFluxElementProxy> ____currentProxy)
	{
		// Get Bind information
		var data = additionalData.GetOrCreateValue(__instance);
		// Update bind variables for this update loop
		data.UpdateBinds(__instance);

		Target targetFunction = GetBind(data);

		// Call the functions
		return targetFunction switch
		{
			// Select with dragged wire
			Target.Selection => ContextualSelectionActionsPatch.Prefix(__instance, ____currentProxy),
			// Swap highlighted node
			Target.Swap => ContextualSwapActionsPatch.Prefix(__instance, ____currentProxy),
			// nodes from held reference type
			Target.Reference => ContextualReferenceActionsPatch.Prefix(__instance),
			// No function
			_ => true,
		};
	}

	internal enum Target
	{
		None,
		Selection,
		Swap,
		Reference
	}

	internal struct BindData
	{
		internal DateTime? lastPressedTime;
		internal bool currentlyPressed;
		internal bool pressedThisUpdate;
		internal bool releasedThisUpdate;
		internal bool isDoublePress;

		internal readonly bool IsHeld => currentlyPressed && TimeSincePressed > 0.5f;
		internal readonly double TimeSincePressed => (DateTime.Now - lastPressedTime.GetValueOrDefault()).TotalSeconds;

		internal void Update(bool state)
		{
			if (TimeSincePressed < 0.5f && state) isDoublePress = true;

			bool changed = currentlyPressed != state;
			if (changed)
			{
				pressedThisUpdate = state;
				releasedThisUpdate = !state;
				currentlyPressed = state;
			}

			if (pressedThisUpdate) lastPressedTime = DateTime.Now;
		}
	}
	internal struct SidedBinds
	{
		internal BindData Primary;
		internal BindData Opposite;

		internal void Update(bool VRLeft, bool VRRight, bool DesktopLeft, bool DesktopRight)
		{
			Primary.Update(VRRight || DesktopRight);
			Opposite.Update(VRLeft || DesktopLeft);
		}
	}
	internal class ProtoFluxToolData
	{
		internal SidedBinds Secondary;
		internal SidedBinds Menu;
		internal SidedBinds Trigger;
		internal SidedBinds Grip;

		internal bool UserInVR;
		internal InputInterface? UserInput;

		internal IWorldElement? GrabbedReference;

		internal void UpdateBinds(ProtoFluxTool tool)
		{
			Chirality side = tool.ActiveHandler.Side;
			Chirality opposite = side.NextValue();
			User user = tool.LocalUser;

			GrabbedReference = tool.GetGrabbedReference();

			UserInVR = user.VR_Active;

			InputInterface input = user.InputInterface;
			IStandardController PrimaryController = input.GetControllerNode(side);
			IStandardController OppositeController = input.GetControllerNode(opposite);

			UserInput = input;

			// VR BINDS
			bool VRMenuLeft = OppositeController.ActionMenu.Held;
			bool VRMenuRight = PrimaryController.ActionMenu.Held;
			bool VRSecondaryLeft = OppositeController.ActionSecondary.Held;
			bool VRSecondaryRight = PrimaryController.ActionSecondary.Held;
			bool VRTriggerLeft = OppositeController.ActionPrimary.Held;
			bool VRTriggerRight = PrimaryController.ActionPrimary.Held;
			bool VRGripLeft = OppositeController.ActionGrab.Held;
			bool VRGripRight = PrimaryController.ActionGrab.Held;

			// DESKTOP BINDS
			bool DesktopMenuLeft = input.GetKey(GetMenuKey());
			bool DesktopMenuRight = input.GetKey(Key.T);
			bool DesktopSecondaryLeft = input.GetKey(GetSecondaryKey());
			bool DesktopSecondaryRight = input.GetKey(Key.R);
			bool DesktopTriggerLeft = input.Mouse.MouseButton5.Held;
			bool DesktopTriggerRight = input.Mouse.LeftButton.Held;
			bool DesktopGripLeft = input.Mouse.MouseButton4.Held;
			bool DesktopGripRight = input.Mouse.RightButton.Held;

			Secondary.Update(VRSecondaryLeft, VRSecondaryRight, DesktopSecondaryLeft, DesktopSecondaryRight);
			Menu.Update(VRMenuLeft, VRMenuRight, DesktopMenuLeft, DesktopMenuRight);
			Trigger.Update(VRTriggerLeft, VRTriggerRight, DesktopTriggerLeft, DesktopTriggerRight);
			Grip.Update(VRGripLeft, VRGripRight, DesktopGripLeft, DesktopGripRight);
		}
	}

	private static readonly ConditionalWeakTable<ProtoFluxTool, ProtoFluxToolData> additionalData = [];
	public static T GetConfig<T>(ModConfigurationKey<T> key, T Default)
	{
		ModConfiguration? config = ProtoFluxContextualActions.Config;
		if (config != null) return config.GetValue(key) ?? Default;
		return Default;
	}
	public static bool ShouldUseDesktopBinds(ProtoFluxToolData data)
	{
		return !GetDesktopShouldUseVR() && data.UserInVR;
	}
	public static bool GetDesktopShouldUseVR()
	{
		return GetConfig(DesktopUsesVRBinds, false);
	}
	public static Key GetSecondaryKey()
	{

		return GetConfig(SecondaryKey, Key.BackQuote);
	}
	public static Key GetMenuKey()
	{
		return GetConfig(MenuKey, Key.LeftShift);
	}

}
