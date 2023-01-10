// VoxelTycoon.Generators.WorldGenerator
using System;
using UnityEngine;
using VoxelTycoon;
using VoxelTycoon.Buildings;
using VoxelTycoon.Generators;

public class WorldGeneratorOriginal
{
	private struct Node
	{
		public Xz Center;

		public Biome Biome;

		public float Bumpiness;
	}

	private struct CacheValue
	{
		public Color32 BodyColor;

		public Color32 CoverColor;

		public Biome Biome;

		public float Bumpiness;

		public int OriginalHeight;

		public bool IsPlain;
	}

	private static readonly Color SnowColor = ColorHelper.FromHexString("f0f0f0");

	private readonly LazyDictionary<Xz, Node> _indexToNode = new LazyDictionary<Xz, Node>();

	private readonly LazyDictionary<Xz, CacheValue> _cache = new LazyDictionary<Xz, CacheValue>();

	private QuickRandom _nodeRandom;

	private QuickRandom _decorateRandom;

	private float _baseSeedX;

	private float _baseSeedZ;

	private float _mountainSeedX;

	private float _mountainSeedZ;

	public WorldGeneratorOriginal()
	{
		_indexToNode.ValueFactory = delegate (Xz index)
		{
			_nodeRandom.SetSeed(index);
			float num12 = 0.2f;
			Xz xz4 = default(Xz);
			xz4.X = Mathf.RoundToInt(((float)index.X + _nodeRandom.RangeFloat(num12, 1f - num12)) * (float)WorldSettings.Current.BiomeCellSize);
			xz4.Z = Mathf.RoundToInt(((float)index.Z + _nodeRandom.RangeFloat(num12, 1f - num12)) * (float)WorldSettings.Current.BiomeCellSize);
			Xz xz5 = xz4;
			Xz regionCenter = Manager<RegionManager>.Current.GetRegionCenter(xz5);
		//float @float = WorldSettings.Current.Temperature.GetFloat(regionCenter.X, regionCenter.Z);
		float @float = Mathf.Max(Mathf.Abs(regionCenter.Z / 100.0f), 1);
			float float2 = WorldSettings.Current.Humidity.GetFloat(regionCenter.X, regionCenter.Z);
		//float2 *= @float;
		Biome biome2 = Manager<BiomeManager>.Current.GetBiome(@float, float2);
			Node result2 = default(Node);
			result2.Center = xz5;
			result2.Biome = biome2;
			result2.Bumpiness = biome2.Bumpiness;
			return result2;
		};
		_cache.ValueFactory = delegate (Xz xz)
		{
			Counters.ComputeBiome++;
			Xz xz2 = new Xz(Mathf.FloorToInt((float)xz.X / (float)WorldSettings.Current.BiomeCellSize), Mathf.FloorToInt((float)xz.Z / (float)WorldSettings.Current.BiomeCellSize));
			Xz xz3 = xz + new Xz((int)WorldSettings.Current.BiomeDistortionX.GetFloat(xz.X, xz.Z), (int)WorldSettings.Current.BiomeDistortionZ.GetFloat(xz.X, xz.Z));
			int num = int.MaxValue;
			Biome biome = null;
			float num2 = 0f;
			Vector4 vector = default(Vector4);
			Vector4 vector2 = default(Vector4);
			float num3 = 0f;
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
					GetBiomeColors(node.Biome, out var localBodyColor, out var localCoverColor);
					vector += num5 * (Vector4)localBodyColor;
					vector2 += num5 * (Vector4)localCoverColor;
				}
			}
			num2 /= num3;
			Color color = vector / num3;
			Color color2 = vector2 / num3;
			float num6 = PerlinNoise(xz.X, xz.Z, _baseSeedX, _baseSeedZ, 150f, 1f, 3, 3.5f);
			float num7 = Mathf.InverseLerp(-3f, 7f, 0.5f);
			num6 -= num7;
			num6 *= ((num6 > 0f) ? WorldSettings.Current.MountainsMultiplier : WorldSettings.Current.LakesMultiplier);
			num6 += num7;
			float num8 = Mathf.LerpUnclamped(-3f, 7f, num6);
			float num9 = num2 * Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0.6f, 0.75f, num6));
			if (num9 > 0f)
			{
				float num10 = Mathf.LerpUnclamped(0f, 25f, RidgedNoise(xz.X, xz.Z, _mountainSeedX, _mountainSeedZ, 55f, 2.5f, 3, 1.9f));
				num8 += num10 * num9;
			}
			int num11 = Mathf.RoundToInt(num8);
			bool isPlain = num9 < 0.1f && num11 >= WorldSettings.Current.WaterHeight;
			CacheValue result = default(CacheValue);
			result.Biome = biome;
			result.Bumpiness = num2;
			result.BodyColor = color;
			result.CoverColor = color2;
			result.OriginalHeight = num11;
			result.IsPlain = isPlain;
			return result;
		};
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

	public void OnGameSettingsInitialized()
	{
		_nodeRandom = new QuickRandom(WorldSettings.Current.Seed, 3L);
		_decorateRandom = new QuickRandom(WorldSettings.Current.Seed, 7L);
		QuickRandom quickRandom = new QuickRandom(WorldSettings.Current.Seed, 15L);
		_mountainSeedX = quickRandom.RangeShort();
		_mountainSeedZ = quickRandom.RangeShort();
		_baseSeedX = quickRandom.RangeShort();
		_baseSeedZ = quickRandom.RangeShort();
	}

	public Biome GetBiome(Xz xz)
	{
		return _cache[xz].Biome;
	}

	public int GetOriginalHeight(Xz xz)
	{
		return Math.Max(_cache[xz].OriginalHeight, WorldSettings.Current.WaterHeight);
	}

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
		if (xyz.Y > WorldSettings.Current.MaxHeight)
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
				Item item2 = Manager<AssetLibrary>.Current.Get<Item>("base/water.item");
				result = default(Voxel);
				result.Color = item2.Color;
				result.IsLiquid = true;
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

	internal void ClearCache()
	{
		_cache.Clear();
	}

	private static void GetBiomeColors(Biome biome, out Color localBodyColor, out Color localCoverColor)
	{
		localBodyColor = biome.BodyColor;
		localCoverColor = biome.CoverColor;
	}
}