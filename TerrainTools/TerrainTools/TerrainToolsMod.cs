using UnityEngine;
using VoxelTycoon;
using VoxelTycoon.Modding;

public class TerrainToolsMod : Mod
{
	protected override void Initialize()
	{
		Debug.Log("TerrainTools is Initialize!");
	}

	protected override void Deinitialize()
	{
		Debug.Log("TerrainTools is Deinitialize!");
	}

	protected override void OnGameStarted()
	{
		LazyManager<TerrainToolsManager>.Current.LoadSettings();
	}
}