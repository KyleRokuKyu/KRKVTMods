using UnityEngine;
using VoxelTycoon.Modding;
using HarmonyLib;
using VoxelTycoon;
using VoxelTycoon.Cities;
using System.Collections.Generic;
using VoxelTycoon.Tracks.Roads;
using System;

public class SingleCityMod : Mod
{
	private Harmony _harmony;
	public static bool localCall = false;
	public static bool spawned = false;

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
class CityInitialize
{
	static bool Prefix(ref City __instance)
	{
		if (SingleCityMod.localCall)
		{
			return true;
		}
		return false;
	}
}

[HarmonyPatch(typeof(City), "Grow")]
class CityGrow
{
	static bool Prefix(ref City __instance, ref CityRoadCollection ____roads)
	{
		if (__instance.Population > 0 || LazyManager<TimeManager>.Current.WorldTime < 1)
		{
			return true;
		}
		ImmutableList<Road> roads = ____roads.ToImmutableList();
		for (int i = 0; i < roads.Count; i++)
			____roads.Remove(roads[i]);
		return false;
	}
}

[HarmonyPatch(typeof(City), "TryCreateIndicator")]
class CityTryCreateIndicator
{
	static bool Prefix(ref City __instance)
	{
		if (SingleCityMod.localCall || __instance.Population > 0)
		{
			return true;
		}
		return false;
	}
}

[HarmonyPatch(typeof(Region), "SpawnObjects")]
class RegionSpawnObjects
{
	static void Postfix(ref List<City> ____cities)
	{
		Xz spawnPoint = Xz.Zero;
		while (____cities.Count > 0)
		{
			if (spawnPoint.Equals(Xz.Zero) && ____cities[____cities.Count - 1].Region == RegionManager.Current.HomeRegion)
			{
				spawnPoint = ____cities[____cities.Count - 1].Position;
			}
			____cities.RemoveAt(____cities.Count - 1);
		}

		if (!SingleCityMod.spawned)
		{
			SingleCityMod.spawned = true;
			SingleCityMod.localCall = true;
			City city = new City(RegionManager.Current.HomeRegion, spawnPoint, CityType.Mixed, 1);
			____cities.Add(city);
			SingleCityMod.localCall = false;
		}
	}
}

[HarmonyPatch(typeof(CityManager), "OnUpdate")]
class CityManagerOnUpdate
{
	static bool Prefix(ref List<City> ____cities)
	{
		foreach (City c in ____cities)
		{
			if (c == null)
				continue;
			if (c.Population > 0)
			{
				try
				{
					c.OnUpdate();
				}
				catch (Exception e)
				{
					Debug.Log(e);
				}
				break;
			}
		}
		return false;
	}
}