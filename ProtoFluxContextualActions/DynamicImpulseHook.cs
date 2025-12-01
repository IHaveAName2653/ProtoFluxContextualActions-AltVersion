using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elements.Core;
using FrooxEngine.Undo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes;
using ProtoFlux.Runtimes.Execution.Nodes.Actions;
using ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Slots;
using ProtoFluxContextualActions.Attributes;
using ProtoFluxContextualActions.Utils;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Reflection.Metadata;

namespace ProtoFluxContextualActions;



public abstract class DynOverride
{
	public abstract bool InvokeOverride(string FunctionName, Slot target, bool excludeDisabled, FrooxEngineContext context);
}




[HarmonyPatch(typeof(DynamicImpulseTrigger), "Trigger")]
public static class DynamicImpulseHook
{
	public static Dictionary<string, DynOverride> Overrides = new Dictionary<string, DynOverride>()
	{
		{"RecipeConfig", new RecipeConfigDynOverride() },
		{"RecipeData", new RecipeDataDynOverride() },
		{"RecipeMaker", new RecipeMakerDynOverride() },


		{"Binds", new BindDynOverride() }
	};
	public static Dictionary<string, DynOverride> LegacyOverrides = new Dictionary<string, DynOverride>()
	{
		{"FluxRecipe", new LegacyRecipeStringInterface() },
	};


	public static bool Prefix(Slot hierarchy, string tag, bool excludeDisabled, FrooxEngineContext context)
	{
		// ALWAYS return true, unless this impulse should not continue to the world.

		// If there isnt a tag, skip it!!!
		if (tag == null) return true;
		if (string.IsNullOrEmpty(tag.Trim() ?? "")) return true;
		// Determine if this is a legacy or new function.

		string[] parts = tag.Split("_");
		if (parts.Length > 1)
		{
			// This is a legacy function.
			// What "space" is this?
			if (LegacyOverrides.TryGetValue(parts[0], out DynOverride? targetImpulse))
			{
				return targetImpulse.InvokeOverride(parts[1], hierarchy, excludeDisabled, context);
			}
		}

		return true;
	}
}