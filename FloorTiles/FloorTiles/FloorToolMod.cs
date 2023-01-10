using UnityEngine;
using VoxelTycoon;
using VoxelTycoon.Modding;

public class FloorToolMod : Mod
{
	protected override void Initialize()
	{
		Debug.Log("FloorTool is Initialize!");
	}

	protected override void Deinitialize()
	{
		Debug.Log("FloorTool is Deinitialize!");
	}

	protected override void OnGameStarted()
	{
		LazyManager<FloorToolManager>.Current.LoadSettings();
	}
}