using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using VoxelTycoon;
using VoxelTycoon.AssetLoading;
using VoxelTycoon.Buildings;
using VoxelTycoon.Generators;

class WorldGenOverhaulGenerator : IWorldGenerator
{
	public struct WeightedBiome
	{
		public Biome biome;

		public float accumulatedWeight;
	}

	public struct Node
	{
		public Xz Center;

		public Biome Biome;

		public float Bumpiness;

		public float RiverDispersion;

		public float RiverSize;

		public string WaterColor;

		public float MinimumHeight;

		public float MaximumHeight;

		public float Probability;

		public bool Rivers;

		public bool ExcludeFromSpawn;
	}

	public struct CacheValue
	{
		public Color32 BodyColor;

		public Color32 CoverColor;

		public Biome Biome;

		public float Bumpiness;

		public int OriginalHeight;

		public bool IsPlain;

		public bool Rivers;

		public Color32 WaterColor;
	}

	public struct ModSettings
	{
		public float RegionSizeMultiplier;

		public string TemperatureGeneration;

		public float StartingLatitude;

		public bool Water;

		public bool Rivers;

		public float PlanetSize;

		public bool BiggerElevationChanges;

		public Dictionary<string, bool> ActiveBiomes;
	}

	public static readonly Color SnowColor = ColorHelper.FromHexString("f0f0f0");

	public readonly LazyDictionary<Xz, Node> _indexToNode = new LazyDictionary<Xz, Node>();

	public readonly LazyDictionary<Xz, CacheValue> _cache = new LazyDictionary<Xz, CacheValue>();

	public QuickRandom _nodeRandom;

	public QuickRandom _decorateRandom;

	public float _baseSeedX;

	public float _baseSeedZ;

	public float _mountainSeedX;

	public float _mountainSeedZ;

	public float _latitudeSeed;

	public ModSettings modSettings;

	public WorldGenOverhaulGenerator ()
	{
		_indexToNode.ValueFactory = delegate (Xz index)
		{
			if (_nodeRandom == null)
			{
				_nodeRandom = new QuickRandom();
			}
			_nodeRandom.SetSeed(index);
			float num12 = 0.2f;
			Xz xz4 = default(Xz);
			xz4.X = Mathf.RoundToInt(((float)index.X + _nodeRandom.RangeFloat(num12, 1f - num12)) * (float)WorldSettings.Current.BiomeCellSize);
			xz4.Z = Mathf.RoundToInt(((float)index.Z + _nodeRandom.RangeFloat(num12, 1f - num12)) * (float)WorldSettings.Current.BiomeCellSize);
			Xz xz5 = xz4;
			Xz regionCenter = Manager<RegionManager>.Current.GetRegionCenter(xz5);
			float @float;
			float float2 = WorldSettings.Current.Humidity.GetFloat(regionCenter.X, regionCenter.Z);
			Biome biome2;
			switch (modSettings.TemperatureGeneration)
			{
				case "Latitude":
					@float = 1f - Mathf.Clamp(Mathf.Abs((regionCenter.Z + _latitudeSeed * modSettings.PlanetSize) / modSettings.PlanetSize), 0, 1);
					biome2 = GetBiome(@float, float2, regionCenter);
					break;
				case "LatitudeWrapped":
					float regionZ = Mathf.Abs(regionCenter.Z + _latitudeSeed);
					regionZ %= modSettings.PlanetSize * 2f;
					regionZ -= modSettings.PlanetSize;
					regionZ /= modSettings.PlanetSize;
					regionZ = Mathf.Pow(Mathf.Abs(regionZ), 2);
					@float = Mathf.Clamp(regionZ, 0, 1);
					biome2 = GetBiome(@float, float2, regionCenter);
					break;
				case "Default":
					@float = WorldSettings.Current.Temperature.GetFloat(regionCenter.X, regionCenter.Z);
					float2 *= @float;
					biome2 = Manager<BiomeManager>.Current.GetBiome(@float, float2);
					break;
				default:
					@float = WorldSettings.Current.Temperature.GetFloat(regionCenter.X, regionCenter.Z);
					float2 *= @float;
					biome2 = Manager<BiomeManager>.Current.GetBiome(@float, float2);
					break;

			}
			Node result2 = default(Node);
			result2.Center = xz5;
			result2.Biome = biome2;
			result2.Bumpiness = biome2.Bumpiness;
			result2.RiverDispersion = 0.25f;
			result2.RiverSize = 0.25f;
			result2.WaterColor = "2abddc";
			result2.MinimumHeight = 0;
			result2.MaximumHeight = 100;
			result2.Probability = 1.0f;

			string URI = AssetLibrary.Current.GetAssetUri(biome2.AssetId);
			Node tempNode = default;
			if (ChopString(URI, URI.Length - 5, true) == "base/")
			{
				string biomeName = ChopString(URI, "base/");
				tempNode = AssetHandler.LoadData<Node>(AssetLibrary.Current.GetAssetInfo(AssetLibrary.Current.GetAssetId("biomeizer/" + biomeName)));
			}
			else
			{
				tempNode = AssetHandler.LoadData<Node>(AssetLibrary.Current.GetAssetInfo(biome2.AssetId));
			}

			result2.RiverDispersion = tempNode.RiverDispersion;
			result2.RiverSize = tempNode.RiverSize;
			result2.WaterColor = tempNode.WaterColor;
			result2.MinimumHeight = tempNode.MinimumHeight;
			result2.MaximumHeight = tempNode.MaximumHeight;
			result2.Probability = tempNode.Probability;
			result2.Rivers = tempNode.Rivers;
			result2.ExcludeFromSpawn = tempNode.ExcludeFromSpawn;

			return result2;
		};
		_cache.ValueFactory = delegate (Xz xz)
		{
			Counters.ComputeBiome++;

			Random.InitState(WorldSettings.Current.Seed);
			_baseSeedX = Random.Range(0.0f, 999999.0f);
			_baseSeedZ = Random.Range(0.0f, 999999.0f);
			_mountainSeedX = Random.Range(0.0f, 999999.0f);
			_mountainSeedZ = Random.Range(0.0f, 999999.0f);

			_latitudeSeed = modSettings.StartingLatitude/90f * modSettings.PlanetSize;

			Xz xz2 = new Xz(Mathf.FloorToInt((float)xz.X / (float)WorldSettings.Current.BiomeCellSize), Mathf.FloorToInt(((float)xz.Z) / (float)WorldSettings.Current.BiomeCellSize));
			Xz xz3 = xz + new Xz((int)WorldSettings.Current.BiomeDistortionX.GetFloat(xz.X, xz.Z), (int)WorldSettings.Current.BiomeDistortionZ.GetFloat(xz.X, xz.Z));
			int num = int.MaxValue;
			Biome biome = null;
			float num2 = 0f;
			Vector4 vector = default(Vector4);
			Vector4 vector2 = default(Vector4);
			float num3 = 0f;
			float riverDispersion = 0f;
			float riverDispersionInterpolate = 0f;
			float riverSize = 0f;
			float riverSizeInterpolate = 0f;
			Color waterColor = Color.blue;
			Vector4 waterVector = default(Vector4);

			float minHeight = 0f;
			float minHeightInterpolate = 0f;
			float maxHeight = 0f;
			float maxHeightInterpolate = 0f;

			float rivers = 0f;
			float riversInterpolate = 0f;

			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					Node node = _indexToNode[xz2 + new Xz(i, j)];
					int num4 = Xz.SquaredDistance(xz3, node.Center);
					if (num4 < num)
					{
						num = num4;
						biome = node.Biome;
					}
					float num5 = 1f;
					if (num4 != 0)
					{
						num5 = 1f / Mathf.Pow(num4, WorldSettings.Current.InterpolationPower);
					}
					num3 += num5;
					num2 += num5 * node.Bumpiness;

					riverDispersionInterpolate += num5 * node.RiverDispersion;
					riverSizeInterpolate += num5 * node.RiverSize;
					Color newCol;
					if (ColorUtility.TryParseHtmlString("#" + node.WaterColor, out newCol))
					{
						waterVector += num5 * (Vector4)newCol;
					}

					minHeightInterpolate += num5 * node.MinimumHeight;
					maxHeightInterpolate += num5 * node.MaximumHeight;

					riversInterpolate += num5 * (node.Rivers ? 1f : 0f);

					GetBiomeColors(node.Biome, out var localBodyColor, out var localCoverColor);
					vector += num5 * (Vector4)localBodyColor;
					vector2 += num5 * (Vector4)localCoverColor;
				}
			}
			num2 /= num3;

			riverDispersion = riverDispersionInterpolate / num3;
			riverSize = riverSizeInterpolate / num3;
			waterColor = waterVector / num3;

			minHeight = minHeightInterpolate / num3;
			maxHeight = maxHeightInterpolate / num3;

			rivers = riversInterpolate / num3;

			Color color = vector / num3;
			Color color2 = vector2 / num3;

			float num6 = PerlinNoise(xz.X, xz.Z, _baseSeedX, _baseSeedZ, 100f, 1f, 3, 1f);
			float num7 = Mathf.InverseLerp(-3f, 7f, 0.5f);
			num6 -= num7;
			num6 *= ((num6 > 0f) ? WorldSettings.Current.MountainsMultiplier : WorldSettings.Current.LakesMultiplier);
			num6 += num7;
			float num8 = Mathf.LerpUnclamped(-3f, 7f, num6);
			if (modSettings.BiggerElevationChanges)
			{
				num8 = Mathf.LerpUnclamped(minHeight, maxHeight, num6);
			}
			float num9 = num2 * Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0.6f - (num2 * num2), 1f, num6));
			if (num9 > 0f)
			{
				float num10 = Mathf.LerpUnclamped(0f, 15f, RidgedNoise(xz.X, xz.Z, _mountainSeedX, _mountainSeedZ, 45f, 1f, 2, 1f) * Mathf.Lerp(0.5f, 1.0f, PerlinNoise(xz.X, xz.Z, _mountainSeedX, _mountainSeedZ, 60f, 1f, 4, 1f)));
				num8 += num10 * num9;
			}

			num8 *= Mathf.Pow(Mathf.Clamp(num2, -1, 1), (1 / 3));

			num8 = Mathf.Clamp(num8, minHeight, maxHeight);

			if (modSettings.Rivers)
			{
				float distortionStrength = 300f * riverDispersion;
				float wiggliness = 10f;
				float xDistortion = distortionStrength * Distort((float)xz.X + 2.3f, (float)xz.Z + 2.9f, wiggliness);
				float yDistortion = distortionStrength * Distort((float)xz.X - 3.1f, (float)xz.Z - 4.3f, wiggliness);

				num8 += 1f;
				num8 *= 1f - (RidgedNoise((float)xz.X + xDistortion, (float)xz.Z + xDistortion, _baseSeedZ, _baseSeedZ, 350f, 4f * Mathf.Abs(1f - riverSize), 1, 10f));
				num8 -= 1f;
			}

			int num11 = Mathf.RoundToInt(num8);
			bool isPlain = num11 >= WorldSettings.Current.WaterHeight;
			CacheValue result = default(CacheValue);
			result.Biome = biome;
			result.Bumpiness = num2;
			result.BodyColor = color;
			result.CoverColor = color2;
			result.OriginalHeight = num11;
			result.IsPlain = isPlain;
			result.WaterColor = waterColor;
			if (rivers + Mathf.Pow((Random.value * 2f - 1f), 4) <= 0)
				result.Rivers = false;
			else
				result.Rivers = true;
			return result;
		};
	}

	public string ChopString(string input, string bitToChop, bool chopEnd = false)
	{
		string returnString = "";
		if (chopEnd)
		{
			for (int i = 0; i < input.Length - bitToChop.Length; i++)
			{
				returnString += input[i];
			}
		}
		else
		{
			for (int i = bitToChop.Length; i < input.Length; i++)
			{
				returnString += input[i];
			}
		}
		return returnString;
	}

	public string ChopString(string input, int bitToChop, bool chopEnd = false)
	{
		string returnString = "";
		if (chopEnd)
		{
			for (int i = 0; i < input.Length - bitToChop; i++)
			{
				returnString += input[i];
			}
		}
		else
		{
			for (int i = bitToChop; i < input.Length; i++)
			{
				returnString += input[i];
			}
		}
		return returnString;
	}

	private Biome GetBiome(float temperature, float humidity, Xz xz)
	{
		Vector2 vector = new Vector2(temperature, humidity);
		float num = float.MaxValue;
		for (int b = 0; b < BiomeManager.Current.Biomes.Count; b++)
		{
			string URI = AssetLibrary.Current.GetAssetUri(BiomeManager.Current.Biomes[b].AssetId);
			Node tempNode = default;
			if (ChopString(URI, URI.Length - 5, true) == "base/")
			{
				string biomeName = ChopString(URI, "base/");
				tempNode = AssetHandler.LoadData<Node>(AssetLibrary.Current.GetAssetInfo(AssetLibrary.Current.GetAssetId("biomeizer/" + biomeName)));
			}
			else
			{
				tempNode = AssetHandler.LoadData<Node>(AssetLibrary.Current.GetAssetInfo(BiomeManager.Current.Biomes[b].AssetId));
			}

			string checkingBiome = AssetLibrary.Current.GetAssetUri(BiomeManager.Current.Biomes[b].AssetId);
			checkingBiome = checkingBiome.Split('/')[checkingBiome.Split('/').Length - 1].Split(".biome")[0].Trim();
			if (!SettingsModHelper.GetSetting<bool, WorldGenOverhaulSettings>(checkingBiome, true) || (xz.X == 0 && xz.Z == 0 && tempNode.ExcludeFromSpawn))
			{
				continue;
			}
			float sqrMagnitude = (vector - new Vector2(BiomeManager.Current.Biomes[b].Temperature, BiomeManager.Current.Biomes[b].Humidity)).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
			}
		}
		float accumulatedWeight = 0;
		WeightedBiome[] possibleBiomes = System.Array.Empty<WeightedBiome>();
		for (int b = 0; b < BiomeManager.Current.Biomes.Count; b++)
		{
			string URI = AssetLibrary.Current.GetAssetUri(BiomeManager.Current.Biomes[b].AssetId);
			Node tempNode = default;
			if (ChopString(URI, URI.Length - 5, true) == "base/")
			{
				string biomeName = ChopString(URI, "base/");
				tempNode = AssetHandler.LoadData<Node>(AssetLibrary.Current.GetAssetInfo(AssetLibrary.Current.GetAssetId("biomeizer/" + biomeName)));
			}
			else
			{
				tempNode = AssetHandler.LoadData<Node>(AssetLibrary.Current.GetAssetInfo(BiomeManager.Current.Biomes[b].AssetId));
			}

			string checkingBiome = AssetLibrary.Current.GetAssetUri(BiomeManager.Current.Biomes[b].AssetId);
			checkingBiome = checkingBiome.Split('/')[checkingBiome.Split('/').Length - 1].Split(".biome")[0].Trim();

			if (!SettingsModHelper.GetSetting<bool, WorldGenOverhaulSettings>(checkingBiome, true) || (xz.X == 0 && xz.Z == 0 && tempNode.ExcludeFromSpawn))
			{
				continue;
			}

			float sqrMagnitude = (vector - new Vector2(BiomeManager.Current.Biomes[b].Temperature, BiomeManager.Current.Biomes[b].Humidity)).sqrMagnitude;
			if (sqrMagnitude <= num)
			{
				WeightedBiome tempWeightedBiome = new WeightedBiome();
				accumulatedWeight += tempNode.Probability;
				tempWeightedBiome.biome = BiomeManager.Current.Biomes[b];
				tempWeightedBiome.accumulatedWeight = accumulatedWeight;
				possibleBiomes = AddToArray(possibleBiomes, tempWeightedBiome);
			}
		}

		float roll = Random.Range(0, accumulatedWeight);
		for (int i = 0; i < possibleBiomes.Length; i++)
		{
			if (roll <= possibleBiomes[i].accumulatedWeight)
			{
				return possibleBiomes[i].biome;
			}
		}
		if (possibleBiomes.Length > 0)
			return possibleBiomes[0].biome;
		return BiomeManager.Current.Biomes[0];
	}

	public static T[] AddToArray<T>(T[] array, T item)
	{
		T[] newArray = new T[array.Length + 1];
		array.CopyTo(newArray, 0);
		newArray[array.Length] = item;
		return newArray;
	}

	private float PerlinNoise(float x, float z, float seedX, float seedZ, float period, float power, int octaves, float octaveFactor)
	{
		x += seedX;
		z += seedZ;
		float num = 0f;
		float num2 = 1f;
		float num3 = 0f;
		for (int i = 0; i < octaves; i++)
		{
			num += Mathf.Pow(Mathf.PerlinNoise(x / period, z / period), power) * num2;
			num3 += num2;
			num2 /= octaveFactor;
			period /= octaveFactor;
		}
		return num / num3;
	}

	private float RidgedNoise(float x, float z, float seedX, float seedZ, float period, float power, int octaves, float octaveFactor)
	{
		x += seedX;
		z += seedZ;
		float num = 0f;
		float num2 = 1f;
		float num3 = 0f;
		for (int i = 0; i < octaves; i++)
		{
			num += Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(Mathf.PerlinNoise(x / period, z / period) - 0.5f) * 2f), power) * num2;
			num3 += num2;
			num2 /= octaveFactor;
			period /= octaveFactor;
		}
		return num / num3;
	}
	private float Distort(float x, float y, float wiggliness)
	{
		return PerlinNoise(x * wiggliness, y * wiggliness, _mountainSeedX, _mountainSeedZ, 400f, 2f, 4, 10f);
	}

	public Biome GetBiome(Xz xz)
	{
		return _cache[xz].Biome;
	}

	public int GetOriginalHeight(Xz xz)
	{
		return Mathf.Max(_cache[xz].OriginalHeight, WorldSettings.Current.WaterHeight);
	}

	/*  This is a vague fix for the cost issue but is also insanely laggy
	public void SetOriginalHeight(Xz xz, int newHeight)
	{
		CacheValue cache = _cache[xz];
		cache.OriginalHeight = newHeight;
		_cache[xz] = cache;
	}
	*/

	public bool IsPlain(Xz xz)
	{
		return _cache[xz].IsPlain;
	}

	public Color32 GetBodyColor(Xz xz)
	{
		return _cache[xz].BodyColor;
	}

	public Color32 GetCoverColor(Xz xz)
	{
		return _cache[xz].CoverColor;
	}

	public Voxel ComputeVoxel(Xyz xyz)
	{
		Counters.ComputeVoxelContents++;
		Voxel result;
		if (!modSettings.BiggerElevationChanges && xyz.Y > WorldSettings.Current.MaxHeight)
		{
			result = default(Voxel);
			return result;
		}
		if (xyz.Y <= WorldSettings.Current.LavaHeight)
		{
			Item item = Manager<AssetLibrary>.Current.Get<Item>("base/lava.item");
			result = default(Voxel);
			result.Color = item.Color;
			result.IsLiquid = true;
			return result;
		}

		CacheValue cacheValue = _cache[(Xz)xyz];
		int originalHeight = cacheValue.OriginalHeight;
		if (xyz.Y > originalHeight)
		{
			if (xyz.Y <= WorldSettings.Current.WaterHeight)
			{
				if (modSettings.Water && cacheValue.Rivers)
				{
					//Item item2 = Manager<AssetLibrary>.Current.Get<Item>("base/water.item");
					result = default(Voxel);
					//result.Color = item2.Color;
					result.Color = cacheValue.WaterColor;
					result.IsLiquid = true;
					return result;
				}
				Color b = TintBodyColor(cacheValue.BodyColor, xyz, originalHeight);
				b = Color.Lerp(b, SnowColor, ComputeSnowness(xyz));
				result = default(Voxel);
				result.Color = b;
				return result;
			}
			result = default(Voxel);
			return result;
		}
		Color a = TintBodyColor(cacheValue.BodyColor, xyz, originalHeight);
		a = Color.Lerp(a, SnowColor, ComputeSnowness(xyz));
		result = default(Voxel);
		result.Color = a;
		return result;
	}

	private Color TintBodyColor(Color bodyColor, Xyz xyz, int originalHeight)
	{
		float num = Mathf.InverseLerp(originalHeight, WorldSettings.Current.LavaHeight, xyz.Y);
		float @float = WorldSettings.Current.BodyTintNoise.GetFloat(xyz.X, xyz.Z);
		float num2 = 1f - Mathf.Pow((num - 0.5f) * 2f, 2f);
		num += @float * num2;
		Color b = R.Prefabs.BodyTintGradient.Value.Evaluate(num);
		return Color.Lerp(bodyColor, b, b.a);
	}

	public float ComputeColdness(Xyz xyz)
	{
		float num = Mathf.Clamp01(((float)xyz.Y - (ComputeSnowHeight(xyz) - (float)WorldSettings.Current.ColdDepth)) / (float)WorldSettings.Current.ColdDepth);
		if (num < WorldSettings.Current.ColdThreshold)
		{
			num = 0f;
		}
		return num;
	}

	public float ComputeSnowness(Xyz xyz)
	{
		float num = Mathf.Clamp01(((float)xyz.Y - (ComputeSnowHeight(xyz) - (float)WorldSettings.Current.SnowDepth)) / (float)WorldSettings.Current.SnowDepth);
		if (num < WorldSettings.Current.SnowThreshold)
		{
			num = 0f;
		}
		return num;
	}

	private float ComputeSnowHeight(Xyz xyz)
	{
		return WorldSettings.Current.SnowHeight.GetFloat(xyz.X, xyz.Z);
	}

	public void Decorate(Chunk chunk)
	{
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				Xz xz = new Xz(i + chunk.Min.X, j + chunk.Min.Z);
				Xyz xyz = new Xyz(xz.X, Manager<WorldManager>.Current.GetHeight(xz), xz.Z);
				if (xyz.Y == 31)
				{
					continue;
				}
				Biome biome = GetBiome(xz);
				float num = 1f - ComputeColdness(xyz);
				ImmutableList<PlantProbability> plants = biome.Plants;
				if (plants.Count == 0)
				{
					continue;
				}
				Building building = null;
				if (_decorateRandom == null)
				{
					_decorateRandom = new QuickRandom();
				}
				_decorateRandom.SetSeed(xyz);
				float plantsMultiplier = WorldSettings.Current.PlantsMultiplier;
				float num2 = _decorateRandom.UnitFloat();
				float num3 = 0f;
				for (int k = 0; k < plants.Count; k++)
				{
					PlantProbability plantProbability = plants[k];
					float num4 = num3 + plantProbability.Probability * plantsMultiplier * num;
					if (num2 >= num3 && num2 < num4)
					{
						building = plantProbability.Plant.GetComponent<Building>();
						break;
					}
					num3 = num4;
				}
				if (!(building == null))
				{
					Building rotatedAsset = LazyManager<BuildingManager>.Current.GetRotatedAsset<Building>(building.AssetId, BuildingRotationHelper.Rotations.Random());
					Xyz position = xyz + new Xyz(0, 1, 0);
					if (CanBuildHelper.CanBuildPlant(position, rotatedAsset.Size, ignoreRegions: true))
					{
						Building building2 = UnityEngine.Object.Instantiate(rotatedAsset);
						building2.transform.parent = Manager<WorldManager>.Current.PlantsParent;
						building2.Build(position, BuildStrategy.DefaultWithoutDust);
					}
				}
			}
		}
	}

	private static void GetBiomeColors(Biome biome, out Color localBodyColor, out Color localCoverColor)
	{
		localBodyColor = biome.BodyColor;
		localCoverColor = biome.CoverColor;
	}

	public void ClearCache()
	{
		_cache.Clear();
	}

	public void OnGameSettingsInitialized()
	{
		_nodeRandom = new QuickRandom(WorldSettings.Current.Seed, 3L);
		_decorateRandom = new QuickRandom(WorldSettings.Current.Seed, 7L);
		QuickRandom quickRandom = new QuickRandom(WorldSettings.Current.Seed, 15L);
		_mountainSeedX = quickRandom.RangeShort();
		_mountainSeedZ = quickRandom.RangeShort();
		_baseSeedX = quickRandom.RangeShort();
		_baseSeedZ = quickRandom.RangeShort();

		modSettings = new ModSettings();
		try
		{
			modSettings.RegionSizeMultiplier = WorldSettings.Current.GetFloat<WorldGenOverhaulSettings>(WorldGenOverhaulSettings.RegionSizeMultiplier);
			modSettings.TemperatureGeneration = WorldSettings.Current.GetString<WorldGenOverhaulSettings>(WorldGenOverhaulSettings.TemperatureGeneration);
			modSettings.StartingLatitude = WorldSettings.Current.GetFloat<WorldGenOverhaulSettings>(WorldGenOverhaulSettings.StartingLatitude);
			modSettings.Water = WorldSettings.Current.GetBool<WorldGenOverhaulSettings>(WorldGenOverhaulSettings.Water);
			modSettings.Rivers = WorldSettings.Current.GetBool<WorldGenOverhaulSettings>(WorldGenOverhaulSettings.Rivers);
			modSettings.PlanetSize = WorldSettings.Current.GetFloat<WorldGenOverhaulSettings>(WorldGenOverhaulSettings.PlanetSize);
			modSettings.BiggerElevationChanges = WorldSettings.Current.GetBool<WorldGenOverhaulSettings>(WorldGenOverhaulSettings.BiggerElevationChanges);
		}
		catch
		{
			modSettings.RegionSizeMultiplier = 1.0f;
			modSettings.TemperatureGeneration = "LatitudeWrapped";
			modSettings.StartingLatitude = 20;
			modSettings.Water = true;
			modSettings.Rivers = true;
			modSettings.PlanetSize = 2500;
			modSettings.BiggerElevationChanges = true;

			SettingsModHelper.SetSetting<float, WorldGenOverhaulSettings>(WorldGenOverhaulSettings.RegionSizeMultiplier, 1.0f, WorldSettings.Current);
			SettingsModHelper.SetSetting<string, WorldGenOverhaulSettings>(WorldGenOverhaulSettings.TemperatureGeneration, "LatitudeWrapped", WorldSettings.Current);
			SettingsModHelper.SetSetting<float, WorldGenOverhaulSettings>(WorldGenOverhaulSettings.StartingLatitude, 20.0f, WorldSettings.Current);
			SettingsModHelper.SetSetting<bool, WorldGenOverhaulSettings>(WorldGenOverhaulSettings.Water, true, WorldSettings.Current);
			SettingsModHelper.SetSetting<bool, WorldGenOverhaulSettings>(WorldGenOverhaulSettings.Rivers, true, WorldSettings.Current);
			SettingsModHelper.SetSetting<float, WorldGenOverhaulSettings>(WorldGenOverhaulSettings.PlanetSize, 2500f, WorldSettings.Current);
			SettingsModHelper.SetSetting<bool, WorldGenOverhaulSettings>(WorldGenOverhaulSettings.BiggerElevationChanges, true, WorldSettings.Current);
		}
	}
}