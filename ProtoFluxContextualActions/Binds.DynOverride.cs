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

public class BindDynOverride : DynOverride
{
	public override bool InvokeOverride(string FunctionName, Slot hierarchy, bool excludeDisabled, FrooxEngineContext context)
	{
		string[] parts = FunctionName.Split("/");
		string target = parts[0];
		string variable = parts.Length >= 2 ? parts[1] : "";

		if (target == "EnsureVars_Data")
		{
			var recipeSpace = DynSpaceHelper.TryGetSpace(hierarchy, "FluxRecipeData", true);
			DynSpaceHelper.TryRead(recipeSpace, "ThisRecipe", out string _, true);
			DynSpaceHelper.TryRead(recipeSpace, "AllRecipes", out string _, true);
			DynSpaceHelper.TryRead(recipeSpace, "AllNames", out string _, true);
			DynSpaceHelper.TryRead(recipeSpace, "AllRecipeString", out string _, true);
			DynSpaceHelper.TryRead(recipeSpace, "Recipe", out string _, true);
		}
		if (target == "EnsureVars_Maker")
		{
			var makerSpace = DynSpaceHelper.TryGetSpace(hierarchy, "FluxRecipeMaker", true);
			DynSpaceHelper.TryRead(makerSpace, "RecipeIsOutput", out bool _, true);
			DynSpaceHelper.TryRead(makerSpace, "RecipeRootNode", out Slot _, true);
			DynSpaceHelper.TryRead(makerSpace, "RecipeType", out Type _, true);
			DynSpaceHelper.TryRead(makerSpace, "RecipeName", out string _, true);
		}
		if (target == "EnsureVars_Construct")
		{
			var constructSpace = DynSpaceHelper.TryGetSpace(hierarchy, "FluxConstructData", true);
			DynSpaceHelper.TryRead(constructSpace, "Tool", out ProtoFluxTool _, true);
		}

		if (target == "AddRecipe")
		{
			if (hierarchy == null) return true;
			var makerSpace = DynSpaceHelper.TryGetSpace(hierarchy, "FluxRecipeMaker", true);
			FluxRecipeConfig.RecipeFromSlot(hierarchy, makerSpace);
			return false;
		}
		if (target == "DetermineRoot")
		{
			if (hierarchy == null) return true;
			var makerSpace = DynSpaceHelper.TryGetSpace(hierarchy, "FluxRecipeMaker");
			if (makerSpace == null) return true;
			ProtoFluxNode node = hierarchy.GetComponent<ProtoFluxNode>();
			if (node == null) return true;
			Type nodeType = node.NodeType;
			if (!nodeType.IsGenericType) return true;
			Type genericTypeDef = nodeType.GetGenericTypeDefinition();
			if (genericTypeDef == null) return true;
			if (!(genericTypeDef == typeof(ValueRelay<>) || genericTypeDef == typeof(ObjectRelay<>))) return true;
			Type genericType = nodeType.GenericTypeArguments[^1];
			bool isOutput = false;
			Slot objectSlot = ((Component)node.GetInput(0).Target).Slot;
			if (objectSlot != null)
			{
				ProtoFluxNode connectedNode = objectSlot.GetComponent<ProtoFluxNode>();
				Type inputNodeType = connectedNode.NodeType;
				if (inputNodeType.IsGenericType)
				{
					Type? inputType = inputNodeType.GetGenericTypeDefinition();
					if (inputType == typeof(ExternalValueInput<,>) || inputType == typeof(ExternalObjectInput<,>)) isOutput = true;
				}
			}
			else isOutput = true;
			DynSpaceHelper.TryWrite(makerSpace, "RecipeIsOutput", isOutput, true);
			DynSpaceHelper.TryWrite(makerSpace, "RecipeRootNode", hierarchy, true);
			DynSpaceHelper.TryWrite(makerSpace, "RecipeType", genericType, true);
		}
		if (target == "BuildToSlot")
		{
			if (string.IsNullOrEmpty(variable)) return true;
			if (hierarchy == null) return true;
			var constructSpace = DynSpaceHelper.TryGetSpace(hierarchy, "FluxConstructData", true);
			if (DynSpaceHelper.TryRead(constructSpace, "Tool", out ProtoFluxTool fluxTool, true))
			{
				if (fluxTool == null) return true;
				FluxRecipe? targetRecipe = FluxRecipeConfig.FluxRecipes.Find(recipe => recipe.RecipeName == variable);
				if (targetRecipe == null) return true;
				FluxRecipeConfig.ConstructFluxRecipe(
					fluxTool,
					null,
					targetRecipe.Value, true, hierarchy
				);
				return false;
			}
		}
		if (target == "RemoveRecipe")
		{
			if (string.IsNullOrEmpty(variable)) return true;
			FluxRecipeConfig.OnRemoveItem(variable);
			return false;
		}
		if (target == "Reload")
		{
			FluxRecipeConfig.ReadFromConfig();
			return false;
		}
		if (target == "Get")
		{
			if (string.IsNullOrEmpty(variable)) return true;
			var recipeSpace = DynSpaceHelper.TryGetSpace(hierarchy, "FluxRecipeData", true);
			DynSpaceHelper.TryWrite(recipeSpace, "ThisRecipe", FluxRecipeConfig.GetStringFor(variable), true);
			return false;
		}
		if (target == "GetAll")
		{
			var recipeSpace = DynSpaceHelper.TryGetSpace(hierarchy, "FluxRecipeData", true);
			DynSpaceHelper.TryWrite(recipeSpace, "AllRecipes", FluxRecipeConfig.StringFromData(), true);
			return false;
		}
		if (target == "GetAllNames")
		{
			var recipeSpace = DynSpaceHelper.TryGetSpace(hierarchy, "FluxRecipeData", true);
			DynSpaceHelper.TryWrite(recipeSpace, "AllNames", string.Join(",", FluxRecipeConfig.FluxRecipes.Select(recipe => recipe.RecipeName)), true);
			return false;
		}
		if (target == "SetAll")
		{
			if (!string.IsNullOrEmpty(variable))
			{
				FluxRecipeConfig.LoadFromString(variable);
				return false;
			}
			else
			{
				var recipeSpace = DynSpaceHelper.TryGetSpace(hierarchy, "FluxRecipeData", true);
				if (DynSpaceHelper.TryRead(recipeSpace, "AllRecipeString", out string content, true))
				{
					if (string.IsNullOrEmpty(content)) return true;
					FluxRecipeConfig.LoadFromString(content);
					return false;
				}
			}
		}
		if (target == "AddRecipeString")
		{
			if (!string.IsNullOrEmpty(variable))
			{
				FluxRecipeConfig.LoadSingleString(variable);
				return false;
			}
			else
			{
				var recipeSpace = DynSpaceHelper.TryGetSpace(hierarchy, "FluxRecipeData", true);
				if (DynSpaceHelper.TryRead(recipeSpace, "Recipe", out string content, true))
				{
					if (string.IsNullOrEmpty(content)) return true;
					FluxRecipeConfig.LoadSingleString(content);
					return false;
				}
			}
		}
		if (target == "AddMultiple")
		{
			if (!string.IsNullOrEmpty(variable))
			{
				FluxRecipeConfig.LoadMultiString(variable);
				return false;
			}
			else
			{
				var recipeSpace = DynSpaceHelper.TryGetSpace(hierarchy, "FluxRecipeData", true);
				if (DynSpaceHelper.TryRead(recipeSpace, "Recipes", out string content, true))
				{
					if (string.IsNullOrEmpty(content)) return true;
					FluxRecipeConfig.LoadMultiString(content);
					return false;
				}
			}
		}
		return true;
	}


}