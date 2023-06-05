using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VoxelTycoon;
using VoxelTycoon.AssetLoading;
using VoxelTycoon.AssetLoading.Patching;
using VoxelTycoon.Game;
using VoxelTycoon.Researches;

public class ResearchPatchAssetHandler : AssetHandler
{
	public struct RPatch
	{
		public AssetInfo assetInfo;
		public string targetAssetUri;
		public GameObject targetGameObject;
		public string patchCommandAlias;
		public JObject item;
		public bool optional;
		public Research research;
	}

	public override string Extension => "rpatch";

	private List<RPatch> patches;

	protected override object Import(AssetInfo assetInfo)
	{
		patches = new List<RPatch>();
		foreach (JObject item in JObject.Parse(File.ReadAllText(GetAssetPath(assetInfo))).Value<JArray>("Commands")!)
		{
			RPatch tempPatch = new RPatch();
			tempPatch.assetInfo = assetInfo;
			tempPatch.targetAssetUri = item.ValueRequired<string>("TargetAssetUri");
			foreach (GameObject g in Resources.FindObjectsOfTypeAll(typeof(GameObject))) {
				if (g.name.Contains(tempPatch.targetAssetUri))
				{
					tempPatch.targetGameObject = g;
					Debug.Log("Found RPatch Target for " + tempPatch.assetInfo.Uri);
					break;
				}
			}
			tempPatch.patchCommandAlias = item.ValueRequired<string>("Command");
			tempPatch.item = item;
			tempPatch.optional = item.Value<bool>("Optional");
			tempPatch.research = Manager<AssetLibrary>.Current.Get<Research>(item.ValueRequired<string>("ResearchUri"));
			patches.Add(tempPatch);
		}
		return null;
	}

	public void PatchResearched (Research research)
	{
		if (patches == null)
		{
			return;
		}

		foreach (RPatch p in patches)
		{
			if (p.research == research)
			{
				if (p.targetGameObject == null)
				{
					Debug.Log("Target GameObject null for patch " + p.assetInfo.Uri);
					continue;
				}
				Manager<AssetPatchingManager>.Current.RegisterPatchCommand(p.assetInfo.Id, p.targetAssetUri, p.patchCommandAlias, p.item, p.optional);
				// Asset needs to reload for the patch to apply.
				// Or, like, update the shared data somehow
				Debug.Log("Applying patch " + p.item);
				bool found = false;
				foreach (Component c in p.targetGameObject.GetComponents<Component>())
				{
					try
					{
						foreach (FieldInfo m in c.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
						{
							Debug.Log(c.ToString() + " | " + m.Name);
							if (m.Name == p.item.Value<string>("Path"))
							{
								string value = JObject.Parse(p.item.ToString())["Value"].ToString();
								m.SetValue(m, Convert.ChangeType(value, m.FieldType));
								Debug.Log(c.GetType().GetField(p.item.Value<string>("Path")).GetValue(c));
								found = true;
								break;
							}
						}
						foreach (PropertyInfo m in c.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic))
						{
							Debug.Log(c.ToString() + " | " + m.Name);
							if (m.Name == p.item.Value<string>("Path"))
							{
								string value = JObject.Parse(p.item.ToString())["Value"].ToString();
								m.SetValue(m, Convert.ChangeType(value, m.PropertyType));
								Debug.Log(c.GetType().GetProperty(p.item.Value<string>("Path")).GetValue(c));
								found = true;
								break;
							}
						}
					}
					catch (Exception e)
					{
						Debug.Log("Component " + c.ToString() + " exception: " + e.Message);
					}
					if (found)
						break;
				}
			}
		}
	}
}
