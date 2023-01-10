using UnityEngine;
using VoxelTycoon.Modding;
using System.Collections.Generic;
using Newtonsoft.Json;
using VoxelTycoon;
using System.IO;

namespace ReadAllThings
{
    public class MainClass : Mod
    {
		protected struct Thing
        {
			public string Name;
			public List<string> Components;
        }
		private List<Thing> things;

		protected override void OnGameStarted()
		{
			things = new List<Thing>();
			Thing tempThing;
			bool test;
			foreach (GameObject g in Object.FindObjectsOfType<GameObject>())
            {
				test = false;
				foreach (Thing t in things)
				{
					if (t.Name == g.name) {
						test = true;
						break;
					}
				}
				if (!test)
                {
					tempThing = new Thing()
					{
						Name = g.name,
						Components = new List<string>()
					};
					foreach (Component c in g.GetComponents(typeof(Component)))
					{
						tempThing.Components.Add(c.name);
					}
					things.Add(tempThing);
				}
            }
			string output = JsonConvert.SerializeObject(things, Formatting.Indented);
			string pathToThis = Manager<AssetLibrary>.Current.GetAssetInfo(Manager<AssetLibrary>.Current.GetAssetId("ReadAllThings/mod.json")).FilePath;
			pathToThis = Directory.GetParent(pathToThis).FullName;
			File.WriteAllText(pathToThis + "/SceneComposition.json", output);
		}
	}
}
