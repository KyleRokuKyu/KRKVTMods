using HarmonyLib;
using UnityEngine;
using VoxelTycoon;
using VoxelTycoon.AssetLoading;
using VoxelTycoon.Modding;
using VoxelTycoon.Researches;

[HarmonyPatch]
public class ResearchBasedPatch : Mod
{
	private Harmony _harmony;

	protected override void Initialize()
	{
		GameObject go = new GameObject();
		go.AddComponent<WaitAFrame>().Initialize(this);
	}

	public void ForceLoadLast ()
	{
		Harmony.DEBUG = false;
		_harmony = new Harmony("com.krkvtmods.researchbasedpatch");
		FileLog.Reset();
		_harmony.PatchAll();

		Manager<AssetLibrary>.Current.RegisterHandler<ResearchPatchAssetHandler>();
	}

	protected override void Deinitialize()
	{
		_harmony.UnpatchAll("com.krkvtmods.researchbasedpatch");
		_harmony = null;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ResearchManager), "Complete")]
	private static void Complete(Research research)
	{
		AssetHandler assetHandler;
		Manager<AssetLibrary>.Current.TryGetHandler("rpatch", out assetHandler);
		if (assetHandler != null)
		{
			((ResearchPatchAssetHandler)assetHandler).PatchResearched(research);
		}
	}
}