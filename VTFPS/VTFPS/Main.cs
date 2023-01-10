using VoxelTycoon.Modding;
using UnityEngine;

namespace VTFPS
{
	public class Main : Mod
	{
		private PlayerController player;

        protected override void OnGameStarted()
        {
            player = new GameObject().AddComponent<PlayerController>();
        }
    }
}
