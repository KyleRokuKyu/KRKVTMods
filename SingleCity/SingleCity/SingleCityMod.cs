using UnityEngine;
using VoxelTycoon.Modding;
using HarmonyLib;
using VoxelTycoon.Cities;

public class SingleCityMod : Mod
{
	private Harmony _harmony;

	protected override void Initialize()
	{
		Harmony.DEBUG = false;
		_harmony = new Harmony("com.kylerokukyu.singlecity");
		FileLog.Reset();
		_harmony.PatchAll();
		Debug.Log("SingleCity is Initialized!");
	}

	protected override void Deinitialize()
	{
		_harmony.UnpatchAll("com.kylerokukyu.singlecity");
		_harmony = null;
		Debug.Log("SingleCity is Deinitialized!");
	}
}

[HarmonyPatch(typeof(City), "Initialize")]
class Patch
{
	public static bool first = true;

	static bool Prefix(ref City __instance)
	{
		if (__instance.Region.Center.X == 200 && __instance.Region.Center.Z == 200 && first)
		{
			first = false;
			return true;
		}
		return false;
	}
}