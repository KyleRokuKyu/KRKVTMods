﻿using System;
using UnityEngine;
using VoxelTycoon;
using VoxelTycoon.Game.UI;
using VoxelTycoon.Modding;

public class WorldGenOverhaulSettings : SettingsMod
{
	public const string RegionSizeMultiplier = "RegionSizeMultiplier";
	public const string TemperatureGeneration = "TemperatureGeneration";
	public const string StartingLatitude = "StartingLatitude";
	public const string Water = "Water";
	public const string Rivers = "Rivers";
	public const string PlanetSize = "PlanetSize";
	public const string BiggerElevationChanges = "BiggerElevationChanges";

	public enum GenerationMethods { LatitudeWrapped, Latitude, Default }
	Func<float, string> FloatToString = inFloat => inFloat.ToString("n2");
	Func<float, string> FloatToStringOneDecimal = inFloat => inFloat.ToString("n1");

	protected override void SetDefaults(WorldSettings worldSettings)
	{
		SetSetting(RegionSizeMultiplier, GetSetting(RegionSizeMultiplier, 1.0f), worldSettings);
		SetSetting(TemperatureGeneration, GetSetting(TemperatureGeneration, "LatitudeWrapped"), worldSettings);
		SetSetting(StartingLatitude, GetSetting(StartingLatitude, 20.0f), worldSettings);
		SetSetting(Water, GetSetting(Water, true), worldSettings);
		SetSetting(Rivers, GetSetting(Rivers, true), worldSettings);
		SetSetting(PlanetSize, GetSetting(PlanetSize, 2500f), worldSettings);
		SetSetting(BiggerElevationChanges, GetSetting(BiggerElevationChanges, true), worldSettings);
	}

	protected override void SetupSettingsControl(SettingsControl settingsControl, WorldSettings worldSettings)
	{
		settingsControl.AddSlider("Region Size Multiplier", "How large is too large? (Warning, over 2x, or so, gets REALLY lengthy load times.  Might even run out of memory)",
									() => GetSetting(RegionSizeMultiplier, 1.0f),
									value => SetSetting(RegionSizeMultiplier, value, worldSettings),
									0.1f, 10.0f, FloatToStringOneDecimal);

		settingsControl.AddDropdown("Temperature Generation", "The method used to determine which biomes can be next to each other.",
									() => StringToTempGen(GetSetting(TemperatureGeneration, "LatitudeWrapped")),
									value => SetSetting(TemperatureGeneration, TempGenToString(value), worldSettings));

		settingsControl.AddSlider("Starting Latitude", "From -90 to 90, where on the planet to we start?",
									() => GetSetting(StartingLatitude, 20.0f),
									value => SetSetting(StartingLatitude, value, worldSettings),
									-90f, 90f, FloatToString);

		settingsControl.AddToggle("Water", "Whether or not this world has water",
									() => GetSetting(Water, true),
									value => SetSetting(Water, value, worldSettings));

		settingsControl.AddToggle("Rivers", "Whether or not this world has rivers",
									() => GetSetting(Rivers, true),
									value => SetSetting(Rivers, value, worldSettings));

		settingsControl.AddSlider("Planet Size", "The size of the 'planet' that is used for temperature gradients",
									() => GetSetting(PlanetSize, 2500f),
									value => SetSetting(PlanetSize, value, worldSettings),
									10f, 5000f, FloatToString);

		settingsControl.AddToggle("Bigger Elevation Changes", "Whether or not this world uses the larger elevation changes. (Warning, this means more terraforming is required)",
									() => GetSetting(BiggerElevationChanges, true),
									value => SetSetting(BiggerElevationChanges, value, worldSettings));
	}

	public static GenerationMethods StringToTempGen(string value)
	{
		switch (value)
		{
			case "LatitudeWrapped":
				return GenerationMethods.LatitudeWrapped;
			case "Latitude":
				return GenerationMethods.Latitude;
			case "Default":
				return GenerationMethods.Default;
		}
		return GenerationMethods.Default;
	}

	public static string TempGenToString(GenerationMethods value)
	{
		switch (value)
		{
			case GenerationMethods.LatitudeWrapped:
				return "LatitudeWrapped";
			case GenerationMethods.Latitude:
				return "Latitude";
			case GenerationMethods.Default:
				return "Default";
		}
		return "Default";
	}


	#region Sets
	public static void SetSetting(string name, float value, WorldSettings worldSettings)
	{
		PlayerPrefs.SetFloat(typeof(WorldGenOverhaulSettings).Name + "|" + name, value);
		PlayerPrefs.Save();

		worldSettings.SetFloat<WorldGenOverhaulSettings>(name, value);
	}

	public static void SetSetting(string name, bool value, WorldSettings worldSettings)
	{
		PlayerPrefs.SetInt(typeof(WorldGenOverhaulSettings).Name + "|" + name, value ? 1 : 0);
		PlayerPrefs.Save();

		worldSettings.SetBool<WorldGenOverhaulSettings>(name, value);
	}

	public static void SetSetting(string name, string value, WorldSettings worldSettings)
	{
		PlayerPrefs.SetString(typeof(WorldGenOverhaulSettings).Name + "|" + name, value);
		PlayerPrefs.Save();

		worldSettings.SetString<WorldGenOverhaulSettings>(name, value);
	}

	public static void SetSetting(string name, int value, WorldSettings worldSettings)
	{
		PlayerPrefs.SetInt(typeof(WorldGenOverhaulSettings).Name + "|" + name, value);
		PlayerPrefs.Save();

		worldSettings.SetInt<WorldGenOverhaulSettings>(name, value);
	}

	public static void SetSetting(string name, double value, WorldSettings worldSettings)
	{
		PlayerPrefs.SetString(typeof(WorldGenOverhaulSettings).Name + "|" + name, value.ToString());
		PlayerPrefs.Save();

		worldSettings.SetDouble<WorldGenOverhaulSettings>(name, value);
	}
	#endregion

	#region Gets
	public static float GetSetting(string name, float defaultValue)
	{
		return PlayerPrefs.GetFloat(typeof(WorldGenOverhaulSettings).Name + "|" + name, defaultValue);
	}

	public static bool GetSetting(string name, bool defaultValue)
	{
		return PlayerPrefs.GetInt(typeof(WorldGenOverhaulSettings).Name + "|" + name, defaultValue ? 1 : 0) == 1;
	}

	public static string GetSetting(string name, string defaultValue)
	{
		return PlayerPrefs.GetString(typeof(WorldGenOverhaulSettings).Name + "|" + name, defaultValue);
	}

	public static int GetSetting(string name, int defaultValue)
	{
		return PlayerPrefs.GetInt(typeof(WorldGenOverhaulSettings).Name + "|" + name, defaultValue);
	}

	public static double GetSetting(string name, double defaultValue)
	{
		string returnData = PlayerPrefs.GetString(typeof(WorldGenOverhaulSettings).Name + "|" + name, defaultValue.ToString());
		// Blindly attempting to parse because, if it fails, it'll just return default value anyways.  No harm no foul
		// Storing as string because Unity doesn't support doubles as PlayerPrefs
		try
		{
			return double.Parse(returnData);
		}
		catch
		{
			return defaultValue;
		}
	}
	#endregion
}