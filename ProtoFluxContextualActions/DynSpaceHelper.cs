using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

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
	public static bool VariableExists<T>(DynamicVariableSpace space, string variableName)
	{
		ValueManager<T> manager = space.GetManager<T>(variableName, false);
		if (manager == null || manager.ReadableValueCount == 0) return false;
		return true;
	}

	public static bool EnsureVariableExists<T>(DynamicVariableSpace space, string variableName, T valueIfNotExist)
	{
		if (!VariableExists<T>(space, variableName))
		{
			TryWrite(space, variableName, valueIfNotExist, true);
			return false;
		}
		return true;
	}
	public static bool EnsureVariableExists<T>(DynamicVariableSpace space, string variableName)
	{
		if (!VariableExists<T>(space, variableName))
		{
			var coder = Traverse.Create(typeof(Coder<T>));
			TryWrite(space, variableName, coder.Property<T>("Default").Value, true);
			return false;
		}
		return true;
	}
}
