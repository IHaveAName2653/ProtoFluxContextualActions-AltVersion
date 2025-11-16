using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.Undo;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes;
using ProtoFlux.Runtimes.Execution.Nodes.Actions;
using ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Slots;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace ProtoFluxContextualActions;

public static class FluxRecipeConfig
{
	public static List<FluxRecipe> FluxRecipes = [];

	static readonly string configPath = Path.Combine(Directory.GetCurrentDirectory(), "rml_config", "FluxContextActions_Recipe.json");

	public static void OnInit()
	{
		FluxRecipes = [];
		ReadFromConfig();
	}

	public static void OnAddItem(FluxRecipe recipe)
	{
		int possibleIndex = FluxRecipes.FindIndex(el => el.RecipeName == recipe.RecipeName);
		if (possibleIndex != -1) FluxRecipes[possibleIndex] = recipe;
		else FluxRecipes.Add(recipe);
		WriteIntoConfig();
	}

	public static void OnRemoveItem(string ItemName)
	{
		FluxRecipes.RemoveAll(item => item.RecipeName == ItemName);
		WriteIntoConfig();
	}
	public static void OnRemoveItem(FluxRecipe recipe)
	{
		FluxRecipes.Remove(recipe);
		WriteIntoConfig();
	}

	public static void LoadFromString(string content, bool WriteFile = true)
	{
		FluxRecipes = JsonConvert.DeserializeObject<List<FluxRecipe>>(content) ?? [];
		if (WriteFile) WriteIntoConfig();
	}
	public static void LoadSingleString(string content)
	{
		FluxRecipe obj = JsonConvert.DeserializeObject<FluxRecipe>(content);
		OnAddItem(obj);
	}
	public static void LoadMultiString(string content)
	{
		List<FluxRecipe>? objs = JsonConvert.DeserializeObject<List<FluxRecipe>>(content);
		if (objs == null) return;
		objs.ForEach(OnAddItem);
	}
	public static string StringFromData()
	{
		return JsonConvert.SerializeObject(FluxRecipes);
	}
	public static string GetStringFor(string target)
	{
		List<FluxRecipe> matches = FluxRecipes.FindAll(cur => cur.RecipeName == target);
		if (matches.Count == 0) return "";
		return JsonConvert.SerializeObject(matches[0]);
	}

	public static void WriteIntoConfig()
	{
		if (!File.Exists(configPath))
		{
			File.Create(configPath).Close();
		}
		string content = StringFromData();
		File.WriteAllText(configPath, content, Encoding.Unicode);
	}

	public static void ReadFromConfig()
	{
		if (!File.Exists(configPath))
		{
			File.Create(configPath).Close();
		}
		else
		{
			string content = File.ReadAllText(configPath, Encoding.Unicode);
			LoadFromString(content, false);
		}
	}

	public struct PartialMenuItem(string Name, FluxRecipe recipe, Action<ProtoFluxTool, ProtoFluxElementProxy, FluxRecipe> onMenuPress)
	{
		public string DisplayName = Name;

		public FluxRecipe recipe = recipe;

		public Action<ProtoFluxTool, ProtoFluxElementProxy, FluxRecipe> onMenuPress = onMenuPress;
	}

	public static IEnumerable<PartialMenuItem> GetItems(ProtoFluxTool tool, ProtoFluxElementProxy proxy)
	{
		bool IsProxyOutput = proxy is ProtoFluxOutputProxy outputProxy;
		bool IsProxyInput = proxy is ProtoFluxInputProxy inputProxy;
		if (!IsProxyOutput && !IsProxyInput)
		{
			yield break;
		}

		List<FluxRecipe> ValidDirectionItems = FluxRecipes.FindAll(recipe => recipe.IsOutputProxy == IsProxyOutput);

		Type? proxyType = null;
		if (IsProxyOutput)
		{
			proxyType = ((ProtoFluxOutputProxy)proxy).OutputType.Value;
		}
		if (IsProxyInput)
		{
			proxyType = ((ProtoFluxInputProxy)proxy).InputType.Value;
		}

		List<FluxRecipe> ValidTypeItems = ValidDirectionItems.FindAll(recipe => recipe.AllowedProxyTypes.Exists(t => t == proxyType));

		foreach (FluxRecipe recipe in ValidTypeItems)
		{
			yield return new PartialMenuItem(
					recipe.RecipeName,
					recipe,
					ConstructFluxRecipe
			);
		}
	}

	public static void ConstructFluxRecipe(ProtoFluxTool tool, ProtoFluxElementProxy proxy, FluxRecipe target)
	{
		if (target.NodeDefinitions.Any(def => def.IsTypeNull)) return;
		tool.StartTask(async () =>
		{

			List<Type> spawningTypes = target.NodeDefinitions.Select(def => def.NodeType).ToList();

			Dictionary<int, ProtoFluxNode> spawnedNodes = [];

			ProtoFluxNode RootNode = null;

			for (int i = 0; i < spawningTypes.Count; i++)
			{
				Type realType = ProtoFluxHelper.GetBindingForNode(spawningTypes[i]);
				tool.SpawnNode(realType, node =>
				{
					node.EnsureElementsInDynamicLists();
					spawnedNodes[i] = node;
					node.EnsureVisual();
					if (target.NodeDefinitions[i].IsRoot) RootNode = node;
				});
			}

			await new Updates(3);

			if (spawnedNodes.Values.Any(node => node == null)) return;

			tool.World.BeginUndoBatch($"Create Recipe: {target.RecipeName}");

			foreach (ProtoFluxNode node in spawnedNodes.Values)
			{
				node.Slot.CreateSpawnUndoPoint("Spawn Node");
			}

			if (RootNode == null)
			{
				tool.World.EndUndoBatch();
				foreach (ProtoFluxNode node in spawnedNodes.Values)
				{
					node.Slot.Destroy();
				}
				return;
			}

			try
			{
				// Position stuff
				Slot rootNodeSlot = RootNode.Slot;
				float3 baseUp = rootNodeSlot.Up;
				float3 baseRight = rootNodeSlot.Right;
				float3 baseForward = rootNodeSlot.Forward;

				void LocalTransformNode(ProtoFluxNode input, float3 offset)
				{
					Slot target = input.Slot;
					target.CopyTransform(rootNodeSlot);
					target.GlobalPosition += (baseUp * offset.Y) + (baseRight * offset.X) + (baseForward * offset.Z);
				}

				for (int i = 0; i < target.NodeDefinitions.Count; i++)
				{
					NodeDef thisNodeDef = target.NodeDefinitions[i];
					List<byte3> cons = thisNodeDef.NodeConnections;
					if (cons.Count == 0) continue;
					ProtoFluxNode thisNode = spawnedNodes[i];
					foreach (byte3 con in cons)
					{
						if (con.x == 0xff)
						{
							if (target.IsOutputProxy)
							{
								if (proxy is not ProtoFluxOutputProxy outProxy) continue;
								ISyncRef NodeInput = thisNode.GetInput(con.z);
								thisNode.TryConnectInput(NodeInput, outProxy.NodeOutput.Target, allowExplicitCast: false, undoable: true);
							}
							else
							{
								if (proxy is not ProtoFluxInputProxy inProxy) continue;

								INodeOutput NodeOutput = thisNode.GetOutput(con.z);
								proxy.Node.Target.TryConnectInput(inProxy.NodeInput.Target, NodeOutput, allowExplicitCast: false, undoable: true);
							}
							continue;
						}
						ProtoFluxNode from = spawnedNodes[con.x];
						INodeOutput output = from.GetOutput(con.y);
						ISyncRef input = thisNode.GetInput(con.z);

						input.Target = output;
					}

					float3 offset = thisNodeDef.Offset;

					LocalTransformNode(thisNode, offset);
				}
				tool.World.EndUndoBatch();

			}
			catch
			{
				tool.World.EndUndoBatch();
				foreach (ProtoFluxNode node in spawnedNodes.Values)
				{
					node.Slot.Destroy();
				}
			}

		});
	}

	public static void RecipeFromSlot(Slot target)
	{
		if (target.ChildrenCount == 0) return;	

		DynamicValueVariable<string> recipeNameVar = target.GetComponent<DynamicValueVariable<string>>();
		if (recipeNameVar == null) return;
		DynamicValueVariable<bool> outputVar = target.GetComponent<DynamicValueVariable<bool>>();
		if (outputVar == null) return;
		DynamicTypeVariable typeVar = target.GetComponent<DynamicTypeVariable>();
		if (typeVar == null) return;
		if (string.IsNullOrEmpty(recipeNameVar.Value.Value)) return;

		DynamicReferenceVariable<Slot> rootNodeVar = target.GetComponent<DynamicReferenceVariable<Slot>>();
		Slot rootNode = rootNodeVar.Reference.Target ?? target[0];
		ProtoFluxNode rootNodeNode = rootNode.GetComponent<ProtoFluxNode>();
		if (rootNodeNode == null) return;

		List<ProtoFluxNode> nodes = rootNode.Parent.GetComponentsInChildren<ProtoFluxNode>();
		nodes.RemoveAll(node => !node.Slot.PersistentSelf); // non persistent nodes are nodes we dont want in here
		nodes.OrderBy(node => node == rootNodeNode).ThenBy(node => node.Slot.ChildIndex);
		FluxRecipe newRecipe = new()
		{
			RecipeName = recipeNameVar.Value.Value,
			IsOutputProxy = outputVar.Value.Value,
			AllowedProxyTypes = [typeVar.Value.Value],
			NodeDefinitions = []
		};

		Dictionary<INodeOutput, int2> NodeOutputs = [];
		Dictionary<int2, ISyncRef> NodeInputRefs = [];
		for (int i = 0; i < nodes.Count; i++)
		{
			ProtoFluxNode node = nodes[i];
			for (int j = 0; j < node.NodeOutputCount; j++)
			{
				int2 key = new(i, j);
				NodeOutputs.Add(node.GetOutput(j), key);
			}
			for (int j = 0; j < node.NodeInputCount; j++)
			{
				int2 key = new(i, j);
				NodeInputRefs.Add(key, node.GetInput(j));
			}
		}

		Dictionary<int, List<byte3>> NodeConnectionValues = [];

		foreach (var kv in NodeInputRefs)
		{
			int2 key = kv.Key;
			ISyncRef val = kv.Value;

			int node = key.x;
			int field = key.y;

			INodeOutput targetOutput = (INodeOutput)val.Target;
			if (targetOutput == null) continue;

			bool found = NodeOutputs.TryGetValue(targetOutput, out int2 fromNode);

			byte targetNode = 0xff;
			byte targetNodeIndex = 0;

			if (found)
			{
				targetNode = (byte)fromNode.x;
				targetNodeIndex = (byte)fromNode.y;
			}

			byte3 output = new(targetNode, targetNodeIndex, (byte)field);
			bool foundList = NodeConnectionValues.TryGetValue(node, out List<byte3>? thisNodeList);
			if (!found) {
				NodeConnectionValues.Add(node, []);
				thisNodeList = NodeConnectionValues[node];
			}
			thisNodeList ??= [];
			thisNodeList.Add(output);
		}

		float3 rootPos = rootNode.LocalPosition;

		float3 relativePos(ProtoFluxNode node)
		{
			if (node == rootNodeNode) return float3.Zero;
			return rootPos - node.Slot.LocalPosition;
		}

		for (int i = 0; i < nodes.Count; i++)
		{
			ProtoFluxNode node = nodes[i];
			NodeConnectionValues.TryGetValue(i, out List<byte3>? connections);
			Type thisNodeType = node.NodeType;
			NodeDef thisNode = new()
			{
				NodeType = thisNodeType,
				IsRoot = node == rootNodeNode,
				IsTypeNull = thisNodeType == null,
				NodeConnections = connections ?? [],
				Offset = relativePos(node)
			};

			newRecipe.NodeDefinitions.Add(thisNode);
		}

		OnAddItem(newRecipe);
	}
}

public struct FluxRecipe(string name, bool isOutput, List<Type?> types, List<NodeDef> nodes)
{
	public string RecipeName = name;
	public bool IsOutputProxy = isOutput;
	public List<Type?> AllowedProxyTypes = types;
	public List<NodeDef> NodeDefinitions = nodes;
}

public struct NodeDef(bool root, Type? node, float3 offset, List<byte3> connections)
{
	public bool IsRoot = root;

	[JsonIgnore]
	public bool IsTypeNull = node == null;
	public Type NodeType = node ?? typeof(object);

	public float3 Offset = offset;

	public List<byte3> NodeConnections = connections;
}

[HarmonyPatch(typeof(DynamicImpulseTriggerWithObject<Slot>), "Trigger")]
public static class RecipeSlotInterface
{
	public static bool Prefix(DynamicImpulseTriggerWithObject<Slot> __instance, Slot hierarchy, string tag, bool excludeDisabled, FrooxEngineContext context)
	{
		UniLog.Log("CAUGHT A SLOT");
		if (tag == "FluxRecipe_AddRecipe")
		{
			UniLog.Log("Was recipe??");
			ObjectInput<Slot> Value = __instance.Value;
			Slot? instance = Value.Evaluate(context);
			UniLog.Log("Before Return, may not skip");
			if (instance == null) return true;
			UniLog.Log("After Return. Skip base");
			FluxRecipeConfig.RecipeFromSlot(instance);
			return false;
		}
		UniLog.Log("Wasnt, Dont skip please");
		return true;
	}
}

[HarmonyPatch(typeof(DynamicImpulseTriggerWithObject<string>), "Trigger")]
public static class RecipeStringInterface
{
	public static bool Prefix(DynamicImpulseTriggerWithObject<string> __instance, Slot hierarchy, string tag, bool excludeDisabled, FrooxEngineContext context)
	{
		ObjectInput<string> Value = __instance.Value;
		string? instance = Value.Evaluate(context);

		UniLog.Log("CAUGHT A STRING");
		if (tag == "FluxRecipe_RemoveRecipe")
		{
			UniLog.Log("Before Return, may not skip");
			if (instance == null) return true;
			UniLog.Log("After Return. Skip base");
			FluxRecipeConfig.OnRemoveItem(instance);
			return false;
		}
		if (tag == "FluxRecipe_Reload")
		{
			FluxRecipeConfig.ReadFromConfig();
			UniLog.Log("Skip base");
			return false;
		}
		if (tag == "FluxRecipe_Get")
		{
			UniLog.Log("Before Return, may not skip");
			if (instance == null) return true;
			UniLog.Log("After Return. Skip base");
			if (hierarchy.GetComponent<DynamicVariableSpace>() == null)
			{
				var space = hierarchy.AttachComponent<DynamicVariableSpace>();
				space.SpaceName.Value = "FluxRecipeData";
			}
			var data = hierarchy.GetComponent<DynamicValueVariable<string>>();
			if (data == null)
			{
				data = hierarchy.AttachComponent<DynamicValueVariable<string>>();
			}
			data.VariableName.Value = "FluxRecipeData/ThisRecipe";
			data.Value.Value = FluxRecipeConfig.GetStringFor(instance);
			UniLog.Log("After Return. Skip base");
			return false;
		}
		if (tag == "FluxRecipe_GetAll")
		{
			if (hierarchy.GetComponent<DynamicVariableSpace>() == null)
			{
				var space = hierarchy.AttachComponent<DynamicVariableSpace>();
				space.SpaceName.Value = "FluxRecipeData";
			}
			var data = hierarchy.GetComponent<DynamicValueVariable<string>>();
			if (data == null)
			{
				data = hierarchy.AttachComponent<DynamicValueVariable<string>>();
			}
			data.VariableName.Value = "FluxRecipeData/AllRecipes";
			data.Value.Value = FluxRecipeConfig.StringFromData();
			UniLog.Log("Skip base");
			return false;
		}
		if (tag == "FluxRecipe_SetAll")
		{
			UniLog.Log("Before Return, may not skip");
			if (instance == null) return true;
			UniLog.Log("After Return. Skip base");
			FluxRecipeConfig.LoadFromString(instance);
			return false;
		}
		if (tag == "FluxRecipe_AddRecipe")
		{
			UniLog.Log("Before Return, may not skip");
			if (instance == null) return true;
			UniLog.Log("After Return. Skip base");
			FluxRecipeConfig.LoadSingleString(instance);
			return false;
		}
		if (tag == "FluxRecipe_AddMultiple")
		{
			UniLog.Log("Before Return, may not skip");
			if (instance == null) return true;
			UniLog.Log("After Return. Skip base");
			FluxRecipeConfig.LoadMultiString(instance);
			return false;
		}
		UniLog.Log("Wasnt, Dont skip");
		return true;
	}
}