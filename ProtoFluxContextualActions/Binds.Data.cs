using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProtoFluxContextualActions.BindGetters;

namespace ProtoFluxContextualActions;


public enum Target
{
	Invalid,
	None,
	Select,
	Swap,
	Reference
}

public struct BindData
{
	public DateTime? lastPressedTime;
	public bool currentlyPressed;
	public bool pressedThisUpdate;
	public bool releasedThisUpdate;
	public bool isDoublePress;

	public readonly bool IsHeld => currentlyPressed && TimeSincePressed > 0.5f;
	public readonly double TimeSincePressed => (DateTime.Now - lastPressedTime.GetValueOrDefault()).TotalSeconds;

	public void Update(bool state)
	{
		if (TimeSincePressed < 0.5f && state) isDoublePress = true;

		pressedThisUpdate = false;
		releasedThisUpdate = false;

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
public struct SidedBinds
{
	public BindData Primary;
	public BindData Opposite;

	public void Update(bool VRLeft, bool VRRight, bool DesktopLeft, bool DesktopRight)
	{
		Primary.Update(VRRight || DesktopRight);
		Opposite.Update(VRLeft || DesktopLeft);
	}
}
public class ProtoFluxToolData
{
	public SidedBinds Secondary;
	public SidedBinds Menu;
	public SidedBinds Primary;
	public SidedBinds Grip;
	public SidedBinds Touch;

	public bool UserInVR;
	public InputInterface? UserInput;

	public IWorldElement? GrabbedReference;

	public void UpdateBinds(ProtoFluxTool tool)
	{
		Chirality side = tool.ActiveHandler.Side;
		Chirality opposite = side.NextValue();
		User user = tool.LocalUser;

		GrabbedReference = tool.GetGrabbedReference();

		UserInVR = user.VR_Active;

		InputInterface input = user.InputInterface;
		IStandardController PrimaryController = input.GetControllerNode(side);
		IStandardController OppositeController = input.GetControllerNode(opposite);
		TouchController PrimaryTouchController = (TouchController)PrimaryController;
		TouchController OppositeTouchController = (TouchController)OppositeController;

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

		bool VRTouchLeft = OppositeTouchController.ThumbRestTouch.Held;
		bool VRTouchRight = PrimaryTouchController.ThumbRestTouch.Held;

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
		Primary.Update(VRTriggerLeft, VRTriggerRight, DesktopTriggerLeft, DesktopTriggerRight);
		Grip.Update(VRGripLeft, VRGripRight, DesktopGripLeft, DesktopGripRight);
		Touch.Update(VRTouchLeft, VRTouchRight, false, false);
	}

	public void ResetHolds()
	{

	}
}

public class BindGetters
{
	public static bool ShouldUseDesktopBinds(ProtoFluxToolData data)
	{
		return !GetDesktopShouldUseVR() && !data.UserInVR;
	}

	public static bool GetDesktopShouldUseVR()
	{
		return ProtoFluxContextualActions.GetDesktopShouldUseVR();
	}
	public static Key GetSecondaryKey()
	{

		return ProtoFluxContextualActions.GetSecondaryKey();
	}
	public static Key GetMenuKey()
	{
		return ProtoFluxContextualActions.GetMenuKey();
	}
}
public enum ConditionState
{
	Invalid,
	Pressed,
	Held,
	Press,
	Release,
	DoubleTap,
}
public enum ControlBind
{
	Invalid,
	Secondary,
	Menu,
	Primary,
	Grip,
	Touch
}
public struct Condition
{
	[JsonConverter(typeof(StringEnumConverter))]
	public ConditionState State;
	public bool Invert;
}
public struct Control
{
	[JsonConverter(typeof(StringEnumConverter))]
	public ControlBind Bind;
	public bool IsPrimary;
	public Condition FireCondition;
}
public struct Bind
{
	[JsonConverter(typeof(StringEnumConverter))]
	public Target Action;
	public List<Control> Inputs;
	public bool IsDesktopBind;
}