using Landfall.Haste;
using On.Zorro.Settings.UI;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Zorro.Settings;

public class HastySetting
{
	private static readonly ConditionalWeakTable<HastyData, IHastySetting> SettingsMap = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="HastySetting"/> class.
	/// </summary>
	/// <param name="modName">The name of the mod using this settings handler.</param>
	/// <exception cref="ArgumentException">Thrown if <paramref name="modName"/> is null or empty.</exception>
	public HastySetting(string modName)
	{
		if (string.IsNullOrEmpty(modName))
		{ throw new ArgumentNullException(nameof(modName), "Mod name cannot be null or empty."); }

		ModName = modName;

        On.HasteSettingsHandler.RegisterPage += HasteSettingsHandler_RegisterPage;
		On.SettingsUICell.Setup += SettingsUICell_Setup;
		ButtonSettingUI.Setup += ButtonSettingUI_Setup;
	}

	/// <summary>
	/// Invokes when the config is initialized.
	/// </summary>
	public event Action OnConfig = null!;

	public string ModName { get; } = "";

	/// <summary>
	/// Adds a new setting to the handler, loads its value, and applies it.
	/// </summary>
	/// <typeparam name="T">The type of the setting to add.</typeparam>
	/// <param name="setting">The setting instance to add.</param>
	public void Add<T>(T setting) where T : Setting
	{
        HasteSettingsHandler handler = GameHandler.Instance.SettingsHandler;

        FieldInfo settingsField = typeof(HasteSettingsHandler).GetField("settings", BindingFlags.Instance | BindingFlags.NonPublic);
        if (settingsField == null) { throw new MissingFieldException("HasteSettingsHandler", "settings"); }
        ((List<Setting>)settingsField.GetValue(handler)).Add(setting);

        FieldInfo loadField = typeof(HasteSettingsHandler).GetField("_settingsSaveLoad", BindingFlags.Instance | BindingFlags.NonPublic);
        if (loadField == null) { throw new MissingFieldException("HasteSettingsHandler", "_settingsSaveLoad"); }
        setting.Load((ISettingsSaveLoad)loadField.GetValue(handler));

		setting.ApplyValue();
	}

	/// <summary>
	/// Creates a localized display name for a setting, optionally including a description.
	/// </summary>
	/// <param name="name">The display name of the setting.</param>
	/// <param name="description">The description of the setting (optional).</param>
	/// <returns>
	/// A <see cref="LocalizedString"/> representing the display name and description.
	/// </returns>
	internal LocalizedString CreateDisplayName(string name, string description = "")
		=> new UnlocalizedString(string.IsNullOrEmpty(description) ? name : $"{name}\n<size=60%><alpha=#50>{description}");

	private void ButtonSettingUI_Setup(ButtonSettingUI.orig_Setup orig, Zorro.Settings.UI.ButtonSettingUI self, Setting setting, ISettingHandler handler)
	{
		orig(self, setting, handler);
		if (setting is IHastySetting hastySetting)
		{
			HastyData hastyData = GetHDByHS(hastySetting) ?? new();

			hastyData.ButtonSettingUI = self;
			hastyData.HastySetting = hastySetting;

			if (setting is HastyCollapsible collapsible)
			{
				hastyData.HastyCollapsible = collapsible;
				self.Label.text = collapsible.Collapsed ? "► Expand" : "▼ Collapse";
				collapsible.HastyData = hastyData;

				collapsible.Clicked += collapsed =>
				{
					self.Label.text = collapsible.Collapsed ? "► Expand" : "▼ Collapse";
                    foreach (IHastySetting c in collapsible.Content)
					{
						if (c.HastyData is { } childData) // Wacky bullshit I pulled out my ass
						{
							if (childData.LayoutElement == null)
							{ throw new Exception($"LayoutElement is null for {c.GetDisplayName()}"); }
							if (childData.CanvasGroup == null)
							{ throw new Exception($"CanvasGroup is null for {c.GetDisplayName()}"); }
							if (childData.GameObject == null)
							{ throw new Exception($"Object is null for {c.GetDisplayName()}"); }

							childData.LayoutElement.ignoreLayout = collapsed;
							childData.CanvasGroup.blocksRaycasts = !collapsed;
							childData.CanvasGroup.alpha = 0f;
                            if (childData.GameObject.TryGetComponent(out SettingsUICell cell)) { cell.enabled = !collapsed; }

							childData.GameObject.SetActive(!collapsed);
						}
					}
				};
			}

			SetHDForHS(hastyData, hastySetting);
		}
	}

	private HastyData? GetHDByHS(IHastySetting setting)
	{
		try { return SettingsMap.FirstOrDefault(kvp => kvp.Value.UUID == setting.UUID).Key; }
		catch (Exception ex) { Informer.Inform(ex); }
		return null;
	}

	private void HasteSettingsHandler_RegisterPage(On.HasteSettingsHandler.orig_RegisterPage orig, HasteSettingsHandler self)
	{
		orig(self);
		OnConfig?.Invoke();
	}

	private void SetHDForHS(HastyData hastyData, IHastySetting setting)
	{
		try
		{
			// If the hastyData exists, remove it
			if (SettingsMap.TryGetValue(hastyData, out _)) { SettingsMap.Remove(hastyData); }

			setting.HastyData = hastyData;
			SettingsMap.Add(hastyData, setting);
		}
		catch (Exception ex)
		{
			Informer.Inform(ex);
		}
	}

	/// <summary>
	/// Patch for <see cref="SettingsUICell.Setup"/>.
	/// Sets up Hasty-specific UI and data for settings cells.
	/// </summary>
	/// <param name="orig"></param>
	/// <param name="self"></param>
	/// <param name="setting"></param>
	private void SettingsUICell_Setup(On.SettingsUICell.orig_Setup orig, SettingsUICell self, Zorro.Settings.Setting setting)
	{
		// Do original stuff
		orig(self, setting);

		// If the canvas group has not been obtained, get it.
        FieldInfo field = typeof(SettingsUICell).GetField("m_canvasGroup", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null) { throw new MissingFieldException("SettingsUICell", "m_canvasGroup"); }

		// Then if the setting is of this mod;
		if (setting is IHastySetting hastySetting && setting is IExposedSetting exposedSetting)
		{
			HastyData hastyData = GetHDByHS(hastySetting) ?? new();

			// Assign fields
			hastyData.HastySetting = hastySetting;
			hastyData.SettingsUICell = self;
			hastyData.CanvasGroup = (CanvasGroup)field.GetValue(self);
			hastyData.GameObject = self.gameObject;
			hastyData.LayoutElement = self.gameObject.AddComponent<LayoutElement>();
			hastyData.ExposedSetting = exposedSetting;
			hastyData.HastyCollapsible = setting as HastyCollapsible ?? null!;

			// If the setting is a child of a collapsible
			if (hastySetting.ParentCollapsible != null)
			{
                bool collapsed = hastySetting.ParentCollapsible.Collapsed;
				hastyData.LayoutElement.ignoreLayout = collapsed;
				hastyData.CanvasGroup.blocksRaycasts = !collapsed;
				hastyData.CanvasGroup.alpha = 0f;
                if (hastyData.GameObject.TryGetComponent(out SettingsUICell cell)) { cell.enabled = !collapsed; }
				if (hastyData.GameObject.transform.GetChild(0).TryGetComponent(out Image image)) { image.color = new Color(0.0161f, 0.0576f, 0.0615f, 0.6157f); }

				hastyData.GameObject.SetActive(!collapsed);
			}

			// If the float slider should use whole numbers
			if (hastySetting is HastyFloat hastyFloat && hastyFloat.IsWhole)
			{
                Slider slider = hastyData.GameObject.GetComponentInChildren<Slider>();
				if (slider != null) { slider.wholeNumbers = true; }

					hastyFloat.ApplyValue();
			}

			SetHDForHS(hastyData, hastySetting);
		}
	}
}
