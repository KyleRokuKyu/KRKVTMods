using HarmonyLib;
using System;
using UnityEngine;
using VoxelTycoon;
using VoxelTycoon.Generators;
using VoxelTycoon.Modding;
using VoxelTycoon.Tools;

[HarmonyPatch]
public class WorldGenOverhaul : Mod
{
	private Harmony _harmony;

	protected override void Initialize()
	{
		WorldGeneratorManager.Current.Factory = () => new WorldGenOverhaulGenerator();
		Harmony.DEBUG = false;
		_harmony = new Harmony("com.biomesier.worldgenoverhaul");
		FileLog.Reset();
		_harmony.PatchAll();
	}

	protected override void Deinitialize()
	{
		_harmony.UnpatchAll("com.biomesier.worldgenoverhaul");
		_harmony = null;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(WorldSettings), "get_RegionResolution")]
	private static void GetRegionSizeMultiplier(ref int __result)
	{
		__result += (int)WorldSettings.Current.GetFloat<WorldGenOverhaulSettings>(WorldGenOverhaulSettings.RegionSizeMultiplier);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ToolHelper), "GetFlattenPrice",
			new Type[] {
							typeof(Xyz),
							typeof(int),
			})]
	public static void GetFlattenPrice(ref double __result)
	{
		double workerFloat = __result/20000000.0; // hard coding 40,000,000 as the upper limit for now
		workerFloat = Math.Min(workerFloat, 1.0);
		workerFloat = 1.0 - Math.Pow(workerFloat - 1, 4);
		workerFloat *= 1000000.0;
		__result = workerFloat;
	}
}