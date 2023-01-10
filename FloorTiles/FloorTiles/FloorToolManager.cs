using UnityEngine;
using UnityEngine.UI;
using VoxelTycoon;
using VoxelTycoon.AssetEditing;
using VoxelTycoon.AssetLoading;
using VoxelTycoon.Game.UI.ModernUI;
using VoxelTycoon.UI;
using System.IO;

public class FloorToolManager : LazyManager<FloorToolManager>
{
	private bool _imperfections = true;

	private FloorTool floorTool;
	private RectTransform settings;
	private RectTransform colorPicker;
	private RectTransform imperfectionToggle;
	private RectTransform imperfectionText;
	private RectTransform labelText;
	private Button toggleButton;
	private Image buttonImage;
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

		public UnityEngine.Vector4 Convert ()
		{
			if (z == 0 && w == 0)
			{
				return new UnityEngine.Vector2(x, y);
			}
			if (w == 0)
			{
				return new UnityEngine.Vector3(x, y, z);
			}
			return new UnityEngine.Vector4(x, y, z, w);
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
	}

	public void LoadSettings()
	{
		pathToThis = Manager<AssetLibrary>.Current.GetAssetInfo(Manager<AssetLibrary>.Current.GetAssetId("floortiles/settings.json")).FilePath;
		pathToThis = Directory.GetParent(pathToThis).FullName;
		modSettings = AssetHandler.LoadData<ModSettings>(Manager<AssetLibrary>.Current.GetAssetInfo(Manager<AssetLibrary>.Current.GetAssetId("floortiles/settings.json")));
		floorTool = new FloorTool(Color.red);
		ConstructUI();
	}

	private void ConstructUI ()
	{
		if (settings != null)
		{
			Object.Destroy(settings);
		}

		Toolbar.Current.AddButton(FontIcon.Ketizoloto(I.Terra3), "Floor Painter", new ToolToolbarAction(() => floorTool));

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
		UIElements.SliceTextureIntoObject(ref settingsBG, UIElements.LoadImage(pathToThis + "/Images/panelBG.png"), new Vector4(42, 42, 42, 42), bgColor);

		imperfectionText = Object.Instantiate(new GameObject(), settings).AddComponent<RectTransform>();
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

		labelText = Object.Instantiate(new GameObject(), settings).AddComponent<RectTransform>();
		labelText.localScale = Vector3.one;
		labelText.localPosition = Vector3.zero;
		labelText.anchorMin = new Vector2(modSettings.LabelTextSize.Convert().x, modSettings.LabelTextSize.Convert().y);
		labelText.anchorMax = new Vector2(modSettings.LabelTextSize.Convert().z, modSettings.LabelTextSize.Convert().w);
		labelText.sizeDelta = Vector2.zero;
		labelText.offsetMin = Vector2.zero;
		labelText.offsetMax = Vector2.zero;
		labelText.gameObject.AddComponent<Text>();
		labelText.GetComponent<Text>().text = "Floor Painter";
		labelText.GetComponent<Text>().resizeTextForBestFit = true;
		labelText.GetComponent<Text>().font = Font.CreateDynamicFontFromOSFont("arial", 14);
		labelText.GetComponent<Text>().color = Color.black;

		colorPicker = Object.Instantiate(R.AssetEditing.ColorPicker, Toolbar.Current.transform).GetComponent<RectTransform>();
		colorPicker.GetComponent<ColorPicker>().OnColorChanging = delegate (Color color) { OnSettingsChanged(color); };
		colorPicker.SetParent(settings);
		colorPicker.localScale = Vector3.one;
		colorPicker.localPosition = Vector3.zero;
		colorPicker.anchorMin = new Vector2(modSettings.ColorPickerSize.Convert().x, modSettings.ColorPickerSize.Convert().y);
		colorPicker.anchorMax = new Vector2(modSettings.ColorPickerSize.Convert().z, modSettings.ColorPickerSize.Convert().w);
		colorPicker.sizeDelta = Vector2.zero;
		colorPicker.offsetMin = Vector2.zero;
		colorPicker.offsetMax = Vector2.zero;
		colorPicker.gameObject.layer = (int)Layer.UI;

		foreach (Transform child in colorPicker.transform)
        {
			if (child.name == "SatValSlider")
            {
				child.localPosition = Vector3.zero;
            }
        }

		imperfectionToggle = Object.Instantiate(new GameObject(), settings).AddComponent<RectTransform>();
		imperfectionToggle.localScale = Vector3.one;
		imperfectionToggle.localPosition = Vector3.zero;
		toggleButton = imperfectionToggle.gameObject.AddComponent<Button>();
		Image toggleButtonImage = toggleButton.gameObject.AddComponent<Image>();
		UIElements.SliceTextureIntoObject(ref toggleButtonImage, UIElements.LoadImage(pathToThis + "/Images/panelBG.png"), new Vector4(42, 42, 42, 42), bgColor);
		toggleButton.image = toggleButtonImage;
		buttonImage = imperfectionToggle.GetComponent<Image>();
		buttonImage.color = _imperfections ? new Color(255f/256f, 226f/256f, 64f/256f) : new Color(0.2f, 0.2f, 0.2f);
		toggleButton.onClick.AddListener(OnToggleClicked);
		imperfectionToggle.gameObject.layer = (int)Layer.UI;
		imperfectionToggle.anchorMin = new Vector2(modSettings.ImperfectionsSize.Convert().x, modSettings.ImperfectionsSize.Convert().y);
		imperfectionToggle.anchorMax = new Vector2(modSettings.ImperfectionsSize.Convert().z, modSettings.ImperfectionsSize.Convert().w);
		imperfectionToggle.sizeDelta = Vector2.zero;
		imperfectionToggle.offsetMin = Vector2.zero;
		imperfectionToggle.offsetMax = Vector2.zero;

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

	private void OnToggleClicked ()
	{
		_imperfections = !_imperfections;
		buttonImage.color = _imperfections ? new Color(255f / 256f, 226f / 256f, 64f / 256f) : new Color(0.2f, 0.2f, 0.2f);
	}

	public bool Imperfections
    {
		get { return _imperfections; }
    }

	public FloorTool GetFloorTool
    {
		get { return floorTool; }
    }
}