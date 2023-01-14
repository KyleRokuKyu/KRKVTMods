using HarmonyLib;
using VoxelTycoon;
using VoxelTycoon.Generators;
using VoxelTycoon.Modding;

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
	[HarmonyPatch(typeof(WorldSettings), "get_RegionSizeMultiplier")]
	private static void GetRegionSizeMultiplier(ref float __result)
	{
		__result *= WorldSettings.Current.GetFloat<WorldGenOverhaulSettings>(WorldGenOverhaulSettings.RegionSizeMultiplier);
	}

	/*
	[HarmonyPostfix]
	[HarmonyPatch(typeof(WorldManager), "FlattenHeight")]
	public static void FlattenHeight(Xyz xyz)
	{
		(LazyManager<WorldGeneratorManager>.Current.Factory() as WorldGenOverhaulGenerator).SetOriginalHeight(new Xz(xyz.X, xyz.Z), xyz.Y);
	}
	*/
}