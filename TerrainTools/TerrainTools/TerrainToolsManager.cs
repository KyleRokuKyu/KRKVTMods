using UnityEngine;
using UnityEngine.UI;
using VoxelTycoon;
using VoxelTycoon.AssetEditing;
using VoxelTycoon.AssetLoading;
using VoxelTycoon.Game.UI.ModernUI;
using VoxelTycoon.UI;
using System.IO;

public class TerrainToolsManager : LazyManager<TerrainToolsManager>
{
	private bool _imperfections = true;
	private bool _liquid = false;

	private TerrainTools floorTool;
	private RectTransform settings;
	private RectTransform colorPicker;
	private RectTransform imperfectionToggle;
	private RectTransform imperfectionText;
	private RectTransform liquidToggle;
	private RectTransform liquidText;
	private RectTransform labelText;
	private RectTransform hexLabelField;
	private RectTransform hexInputField;
	private Button toggleButton;
	private Image imperfectionsButtonImage;
	private Image liquidButtonImage;
	private string pathToThis;
	ModSettings modSettings;

	public class Vector
	{
		public float x;
		public float y;
		public float z;
		public float w;

		public Vector()
		{
			x = 0;
			y = 0;
			z = 0;
			w = 0;
		}

		public Vector(float inX, float inY, float inZ = 0, float inW = 0)
		{
			x = inX;
			y = inY;
			z = inZ;
			w = inW;
		}

		public Vector4 Convert ()
		{
			if (z == 0 && w == 0)
			{
				return new Vector2(x, y);
			}
			if (w == 0)
			{
				return new Vector3(x, y, z);
			}
			return new Vector4(x, y, z, w);
        }
	}

	public struct ModSettings
	{
		public string SettingsBackgroundColor;
		public Vector SettingsSize;
		public Vector ColorPickerSize;
		public Vector ImperfectionsSize;
		public Vector ImperfectionsTextSize;
		public Vector LabelTextSize;
		public Vector HexLabelFieldSize;
		public Vector HexInputFieldSize;
		public Vector LiquidSize;
		public Vector LiquidTextSize;
	}

	public void LoadSettings()
	{
		pathToThis = Manager<AssetLibrary>.Current.GetAssetInfo(Manager<AssetLibrary>.Current.GetAssetId("terraintools/settings.json")).FilePath;
		pathToThis = Directory.GetParent(pathToThis).FullName;
		modSettings = AssetHandler.LoadData<ModSettings>(Manager<AssetLibrary>.Current.GetAssetInfo(Manager<AssetLibrary>.Current.GetAssetId("terraintools/settings.json")));
		floorTool = new TerrainTools(Color.red);
		ConstructUI();
	}

	private void ConstructUI ()
	{
		GameObject referenceObject = new GameObject();
		if (settings != null)
		{
			Object.Destroy(settings);
		}

		Toolbar.Current.AddButton(FontIcon.Ketizoloto(I.Terra3), "Terrain Tools", new ToolToolbarAction(() => floorTool));

		settings = new GameObject().AddComponent<RectTransform>();
		settings.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		settings.localPosition = Vector3.zero;
		settings.SetParent(Toolbar.Current.transform.parent);
		settings.anchorMin = new Vector2(modSettings.SettingsSize.Convert().x, modSettings.SettingsSize.Convert().y);
		settings.anchorMax = new Vector2(modSettings.SettingsSize.Convert().z, modSettings.SettingsSize.Convert().w);
		settings.sizeDelta = Vector2.zero;
		settings.offsetMin = Vector2.zero;
		settings.offsetMax = Vector2.zero;
		settings.gameObject.layer = (int)Layer.UI;

		ColorUtility.TryParseHtmlString("#" + modSettings.SettingsBackgroundColor, out Color bgColor);
		Image settingsBG = settings.gameObject.AddComponent<Image>();
		//UIElements.SliceTextureIntoObject(ref settingsBG, UIElements.LoadImage(pathToThis + "/Images/panelBG.png"), new Vector4(42, 42, 42, 42), bgColor);
		settingsBG.sprite = UIElements.ToSprite(UIElements.LoadImage(pathToThis + "/Images/TerrainToolBG.png"));

		imperfectionText = Object.Instantiate(referenceObject, settings).AddComponent<RectTransform>();
		imperfectionText.localScale = Vector3.one;
		imperfectionText.localPosition = Vector3.zero;
		imperfectionText.anchorMin = new Vector2(modSettings.ImperfectionsTextSize.Convert().x, modSettings.ImperfectionsTextSize.Convert().y);
		imperfectionText.anchorMax = new Vector2(modSettings.ImperfectionsTextSize.Convert().z, modSettings.ImperfectionsTextSize.Convert().w);
		imperfectionText.sizeDelta = Vector2.zero;
		imperfectionText.offsetMin = Vector2.zero;
		imperfectionText.offsetMax = Vector2.zero;
		imperfectionText.gameObject.AddComponent<Text>();
		imperfectionText.GetComponent<Text>().text = "Imperfections";
		imperfectionText.GetComponent<Text>().resizeTextForBestFit = true;
		imperfectionText.GetComponent<Text>().font = Font.CreateDynamicFontFromOSFont("arial", 14);
		imperfectionText.GetComponent<Text>().color = Color.black;

		labelText = Object.Instantiate(referenceObject, settings).AddComponent<RectTransform>();
		labelText.localScale = Vector3.one;
		labelText.localPosition = Vector3.zero;
		labelText.anchorMin = new Vector2(modSettings.LabelTextSize.Convert().x, modSettings.LabelTextSize.Convert().y);
		labelText.anchorMax = new Vector2(modSettings.LabelTextSize.Convert().z, modSettings.LabelTextSize.Convert().w);
		labelText.sizeDelta = Vector2.zero;
		labelText.offsetMin = Vector2.zero;
		labelText.offsetMax = Vector2.zero;
		labelText.gameObject.AddComponent<Text>();
		labelText.GetComponent<Text>().text = "Terrain Tools";
		labelText.GetComponent<Text>().resizeTextForBestFit = true;
		labelText.GetComponent<Text>().font = Font.CreateDynamicFontFromOSFont("arial", 14);
		labelText.GetComponent<Text>().color = Color.black;

		colorPicker = Object.Instantiate(R.AssetEditing.ColorPicker, Toolbar.Current.transform).GetComponent<RectTransform>();
		colorPicker.SetParent(settings);
		colorPicker.localScale = Vector3.one;
		colorPicker.localPosition = Vector3.zero;
		colorPicker.anchorMin = new Vector2(modSettings.ColorPickerSize.Convert().x, modSettings.ColorPickerSize.Convert().y);
		colorPicker.anchorMax = new Vector2(modSettings.ColorPickerSize.Convert().z, modSettings.ColorPickerSize.Convert().w);
		colorPicker.sizeDelta = Vector2.zero;
		colorPicker.offsetMin = Vector2.zero;
		colorPicker.offsetMax = Vector2.zero;
		colorPicker.gameObject.layer = (int)Layer.UI;

		hexLabelField = Object.Instantiate(referenceObject, settings).AddComponent<RectTransform>();
		hexLabelField.localScale = Vector3.one;
		hexLabelField.localPosition = Vector3.zero;
		hexLabelField.anchorMin = new Vector2(modSettings.HexLabelFieldSize.Convert().x, modSettings.HexLabelFieldSize.Convert().y);
		hexLabelField.anchorMax = new Vector2(modSettings.HexLabelFieldSize.Convert().z, modSettings.HexLabelFieldSize.Convert().w);
		hexLabelField.sizeDelta = Vector2.zero;
		hexLabelField.offsetMin = Vector2.zero;
		hexLabelField.offsetMax = Vector2.zero;
		hexLabelField.gameObject.AddComponent<Text>();
		hexLabelField.GetComponent<Text>().text = "Hex";
		hexLabelField.GetComponent<Text>().resizeTextForBestFit = true;
		hexLabelField.GetComponent<Text>().font = Font.CreateDynamicFontFromOSFont("arial", 14);
		hexLabelField.GetComponent<Text>().color = Color.black;

		hexInputField = Object.Instantiate(referenceObject, settings).AddComponent<RectTransform>();
		hexInputField.localScale = Vector3.one;
		hexInputField.localPosition = Vector3.zero;
		hexInputField.anchorMin = new Vector2(modSettings.HexInputFieldSize.Convert().x, modSettings.HexInputFieldSize.Convert().y);
		hexInputField.anchorMax = new Vector2(modSettings.HexInputFieldSize.Convert().z, modSettings.HexInputFieldSize.Convert().w);
		hexInputField.sizeDelta = Vector2.zero;
		hexInputField.offsetMin = Vector2.zero;
		hexInputField.offsetMax = Vector2.zero;
		hexInputField.gameObject.AddComponent<InputField>();
		hexInputField.GetComponent<InputField>().contentType = InputField.ContentType.Alphanumeric;
		hexInputField.GetComponent<InputField>().characterLimit = 6;

		RectTransform inputBG = Object.Instantiate(referenceObject, hexInputField).AddComponent<RectTransform>();
		inputBG.localScale = Vector3.one;
		inputBG.localPosition = Vector3.zero;
		inputBG.anchorMin = Vector2.zero;
		inputBG.anchorMax = Vector2.one;
		inputBG.sizeDelta = Vector2.one;
		inputBG.offsetMin = Vector2.zero;
		inputBG.offsetMax = Vector2.zero;
		Image inputBGImage = inputBG.gameObject.AddComponent<Image>();

		RectTransform textComponent = Object.Instantiate(referenceObject, hexInputField).AddComponent<RectTransform>();
		textComponent.localScale = Vector3.one;
		textComponent.localPosition = Vector3.zero;
		textComponent.anchorMin = new Vector2(0.05f, 0.1f);
		textComponent.anchorMax = new Vector2(0.95f, 0.95f);
		textComponent.sizeDelta = Vector2.zero;
		textComponent.offsetMin = Vector2.zero;
		textComponent.offsetMax = Vector2.zero;
		textComponent.gameObject.AddComponent<Text>();
		textComponent.GetComponent<Text>().resizeTextForBestFit = true;
		textComponent.GetComponent<Text>().font = Font.CreateDynamicFontFromOSFont("arial", 10);
		textComponent.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
		hexInputField.GetComponent<InputField>().textComponent = textComponent.GetComponent<Text>();

		hexInputField.GetComponent<InputField>().textComponent.resizeTextForBestFit = true;
		hexInputField.GetComponent<InputField>().textComponent.font = Font.CreateDynamicFontFromOSFont("arial", 14);
		hexInputField.GetComponent<InputField>().textComponent.color = Color.black;
		hexInputField.gameObject.layer = (int)Layer.UI;

		colorPicker.GetComponent<ColorPicker>().OnColorChanging += delegate (Color color) { OnSettingsChanged(color); };
		colorPicker.GetComponent<ColorPicker>().OnColorChanging += delegate (Color color) {
			hexInputField.GetComponent<InputField>().text = ColorUtility.ToHtmlStringRGB(color);
		};

		hexInputField.GetComponent<InputField>().text = ColorUtility.ToHtmlStringRGB(colorPicker.GetComponent<ColorPicker>().Color);
		hexInputField.GetComponent<InputField>().onEndEdit.AddListener(delegate
		{
			Color color = Color.white;
			ColorUtility.TryParseHtmlString("#" + hexInputField.GetComponent<InputField>().textComponent.text, out color);
			colorPicker.GetComponent<ColorPicker>().Color = color;
			OnSettingsChanged(color);
		});

		foreach (Transform child in colorPicker.transform)
        {
			if (child.name == "SatValSlider")
            {
				child.localPosition = Vector3.zero;
            }
		}

		imperfectionToggle = Object.Instantiate(referenceObject, settings).AddComponent<RectTransform>();
		imperfectionToggle.localScale = Vector3.one;
		imperfectionToggle.localPosition = Vector3.zero;
		toggleButton = imperfectionToggle.gameObject.AddComponent<Button>();
		Image imperfectionsToggleButtonImage = toggleButton.gameObject.AddComponent<Image>();
		UIElements.SliceTextureIntoObject(ref imperfectionsToggleButtonImage, UIElements.LoadImage(pathToThis + "/Images/panelBG.png"), new Vector4(42, 42, 42, 42), bgColor);
		toggleButton.image = imperfectionsToggleButtonImage;
		imperfectionsButtonImage = imperfectionToggle.GetComponent<Image>();
		imperfectionsButtonImage.color = _imperfections ? new Color(255f / 256f, 226f / 256f, 64f / 256f) : new Color(0.2f, 0.2f, 0.2f);
		toggleButton.onClick.AddListener(OnImperfectionsToggleClicked);
		imperfectionToggle.gameObject.layer = (int)Layer.UI;
		imperfectionToggle.anchorMin = new Vector2(modSettings.ImperfectionsSize.Convert().x, modSettings.ImperfectionsSize.Convert().y);
		imperfectionToggle.anchorMax = new Vector2(modSettings.ImperfectionsSize.Convert().z, modSettings.ImperfectionsSize.Convert().w);
		imperfectionToggle.sizeDelta = Vector2.zero;
		imperfectionToggle.offsetMin = Vector2.zero;
		imperfectionToggle.offsetMax = Vector2.zero;

		liquidText = Object.Instantiate(referenceObject, settings).AddComponent<RectTransform>();
		liquidText.localScale = Vector3.one;
		liquidText.localPosition = Vector3.zero;
		liquidText.anchorMin = new Vector2(modSettings.LiquidTextSize.Convert().x, modSettings.LiquidTextSize.Convert().y);
		liquidText.anchorMax = new Vector2(modSettings.LiquidTextSize.Convert().z, modSettings.LiquidTextSize.Convert().w);
		liquidText.sizeDelta = Vector2.zero;
		liquidText.offsetMin = Vector2.zero;
		liquidText.offsetMax = Vector2.zero;
		liquidText.gameObject.AddComponent<Text>();
		liquidText.GetComponent<Text>().text = "Is Liquid";
		liquidText.GetComponent<Text>().resizeTextForBestFit = true;
		liquidText.GetComponent<Text>().font = Font.CreateDynamicFontFromOSFont("arial", 14);
		liquidText.GetComponent<Text>().color = Color.black;

		liquidToggle = Object.Instantiate(referenceObject, settings).AddComponent<RectTransform>();
		liquidToggle.localScale = Vector3.one;
		liquidToggle.localPosition = Vector3.zero;
		toggleButton = liquidToggle.gameObject.AddComponent<Button>();
		Image liquidToggleButtonImage = toggleButton.gameObject.AddComponent<Image>();
		UIElements.SliceTextureIntoObject(ref liquidToggleButtonImage, UIElements.LoadImage(pathToThis + "/Images/panelBG.png"), new Vector4(42, 42, 42, 42), bgColor);
		toggleButton.image = liquidToggleButtonImage;
		liquidButtonImage = liquidToggle.GetComponent<Image>();
		liquidButtonImage.color = _liquid ? new Color(255f / 256f, 226f / 256f, 64f / 256f) : new Color(0.2f, 0.2f, 0.2f);
		toggleButton.onClick.AddListener(OnLiquidToggleClicked);
		liquidToggle.gameObject.layer = (int)Layer.UI;
		liquidToggle.anchorMin = new Vector2(modSettings.LiquidSize.Convert().x, modSettings.LiquidSize.Convert().y);
		liquidToggle.anchorMax = new Vector2(modSettings.LiquidSize.Convert().z, modSettings.LiquidSize.Convert().w);
		liquidToggle.sizeDelta = Vector2.zero;
		liquidToggle.offsetMin = Vector2.zero;
		liquidToggle.offsetMax = Vector2.zero;

		Object.Destroy(referenceObject);

		settings.gameObject.SetActive(false);
	}

	public void ShowSettings(bool state)
	{
		settings.gameObject.SetActive(state);
	}

	private void OnSettingsChanged(Color color)
	{
		floorTool.color = color;
	}

	private void OnImperfectionsToggleClicked()
	{
		_imperfections = !_imperfections;
		imperfectionsButtonImage.color = _imperfections ? new Color(255f / 256f, 226f / 256f, 64f / 256f) : new Color(0.2f, 0.2f, 0.2f);
	}

	private void OnLiquidToggleClicked()
	{
		_liquid = !_liquid;
		liquidButtonImage.color = _liquid ? new Color(255f / 256f, 226f / 256f, 64f / 256f) : new Color(0.2f, 0.2f, 0.2f);
	}

	public bool Imperfections
	{
		get { return _imperfections; }
	}

	public bool Liquid
	{
		get { return _liquid; }
	}

	public TerrainTools GetFloorTool
    {
		get { return floorTool; }
    }
}