using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using static FrooxEngine.DynamicVariableSpace;

namespace ProtoFluxContextualActions;

public static class DynSpaceHelper
{
	public static DynamicVariableSpace TryGetSpace(Slot root, string SpaceName, bool createIfNotExist = false)
	{
		DynamicVariableSpace space = root.FindSpace(SpaceName);
		if (space != null) return space;
		if (!createIfNotExist) return null;
		space = root.AttachComponent<DynamicVariableSpace>();
		space.SpaceName.Value = SpaceName;
		return space;
	}

	public static bool TryWrite<T>(DynamicVariableSpace space, string variableName, T value, bool createIfNotExist = false)
	{
		ValueManager<T> manager = space.GetManager<T>(variableName, false);
		if (manager == null || manager.ReadableValueCount == 0)
		{
			if (!createIfNotExist)
			{
				return false;
			}
			else
			{
				Type dynVar = (typeof(T).IsValueType || typeof(T) == typeof(string)) ? typeof(DynamicValueVariable<>) : typeof(DynamicReferenceVariable<>);
				var newVar = (DynamicVariableBase<T>)space.Slot.AttachComponent(dynVar.MakeGenericType(typeof(T)));
				newVar.VariableName.Value = space.SpaceName + "/" + variableName;
				newVar.DynamicValue = value;
				return true;
			}
		}

		if (!manager.IsValidValue(value))
		{
			return false;
		}

		manager.SetValue(value);

		return true;
	}
	public static bool TryRead<T>(DynamicVariableSpace space, string variableName, out T value, bool createIfNotExist = false)
	{
		ValueManager<T> manager = space.GetManager<T>(variableName, false);
		if (manager == null || manager.ReadableValueCount == 0)
		{
			if (!createIfNotExist)
			{
				value = Coder<T>.Default;
				return false;
			}
			else
			{
				Type dynVar = (typeof(T).IsValueType || typeof(T) == typeof(string)) ? typeof(DynamicValueVariable<>) : typeof(DynamicReferenceVariable<>);
				var newVar = (DynamicVariableBase<T>)space.Slot.AttachComponent(dynVar.MakeGenericType(typeof(T)));
				newVar.VariableName.Value = space.SpaceName + "/" + variableName;
				value = Coder<T>.Default;
				return false;
			}
		}

		value = manager.Value;
		return true;
	}
}
