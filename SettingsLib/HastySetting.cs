using HarmonyLib;
using Landfall.Haste;
using On.Zorro.Settings.UI;
using System.Runtime.CompilerServices;
using UnityEngine.Localization;
using UnityEngine.UI;
using UnityEngine;
using Zorro.Settings;

/// <summary>
/// Provides a mod-specific settings handler for registering and managing custom settings.
/// </summary>
public class HastySetting
{
	/// <summary>
	/// Maps <see cref="HastyData"/> instances to their corresponding <see cref="IHastySetting"/> instances.
	/// </summary>
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

	/// <summary>
	/// Mods prefix for logging
	/// </summary>
	public string AsmPFX => $"[{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}]:";

	/// <summary>
	/// Gets the name of the mod associated with this settings handler.
	/// </summary>
	public string ModName { get; } = "";

	/// <summary>
	/// Field reference to the private <c>m_canvasGroup</c> field of <see cref="SettingsUICell"/>.
	/// </summary>
	private static AccessTools.FieldRef<SettingsUICell, CanvasGroup> CanvasGroupRef
		=> AccessTools.FieldRefAccess<SettingsUICell, CanvasGroup>("m_canvasGroup");

	/// <summary>
	/// Gets a reference to the list of settings managed by the settings handler.
	/// </summary>
	private static AccessTools.FieldRef<HasteSettingsHandler, List<Setting>> SettingsRef
		=> AccessTools.FieldRefAccess<HasteSettingsHandler, List<Setting>>("settings");

	/// <summary>
	/// Gets a reference to the settings save/load handler.
	/// </summary>
	private static AccessTools.FieldRef<HasteSettingsHandler, ISettingsSaveLoad> SettingsSaveLoadRef
		=> AccessTools.FieldRefAccess<HasteSettingsHandler, ISettingsSaveLoad>("_settingsSaveLoad");

	/// <summary>
	/// Adds a new setting to the handler, loads its value, and applies it.
	/// </summary>
	/// <typeparam name="T">The type of the setting to add.</typeparam>
	/// <param name="setting">The setting instance to add.</param>
	public void Add<T>(T setting) where T : Setting
	{
		HasteSettingsHandler handler = GameHandler.Instance.SettingsHandler;
		SettingsRef(handler).Add(setting);
		setting.Load(SettingsSaveLoadRef(handler));
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

	/// <summary>
	/// Ppatch for <see cref="ButtonSettingUI.Setup"/>.
	/// Sets up Hasty-specific UI and data for button settings.
	/// </summary>
	/// <param name="orig"></param>
	/// <param name="self"></param>
	/// <param name="setting"></param>
	/// <param name="handler"></param>
	/// <exception cref="Exception"></exception>
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
					foreach (var c in collapsible.Content)
					{
						if (c.HastyData is { } childData) // Wacky bullshit I pulled out my ass
						{
							if (childData.LayoutElement == null)
							{ throw new Exception($"{AsmPFX} LayoutElement is null for {c.GetDisplayName()}"); }
							if (childData.CanvasGroup == null)
							{ throw new Exception($"{AsmPFX} CanvasGroup is null for {c.GetDisplayName()}"); }
							if (childData.GameObject == null)
							{ throw new Exception($"{AsmPFX} GameObject is null for {c.GetDisplayName()}"); }

							childData.LayoutElement.ignoreLayout = collapsed;
							childData.CanvasGroup.blocksRaycasts = !collapsed;
							childData.CanvasGroup.alpha = 0f;
							if (childData.GameObject.TryGetComponent<SettingsUICell>(out var cell))
							{
								cell.enabled = !collapsed;
							}

							childData.GameObject.SetActive(!collapsed);
						}
					}
				};
			}

			SetHDForHS(hastyData, hastySetting);
		}
	}

	/// <summary>
	/// Retrieves the <see cref="HastyData"/> instance associated with the specified <see cref="IHastySetting"/> by UUID.
	/// </summary>
	/// <param name="setting">The <see cref="IHastySetting"/> to look up.</param>
	/// <returns>The associated <see cref="HastyData"/>, or <c>null</c> if not found.</returns>
	private HastyData? GetHDByHS(IHastySetting setting)
	{
		try
		{
			return SettingsMap.FirstOrDefault(kvp => kvp.Value.UUID == setting.UUID).Key;
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError($"{AsmPFX} Error in GetHDByHastySetting: {ex}");
		}
		return null;
	}

	/// <summary>
	/// Patch for <see cref="HasteSettingsHandler.RegisterPage"/> method.
	/// Tells the handler to invoke the <see cref="OnConfig"/> event after registering the page.
	/// </summary>
	/// <param name="orig"></param>
	/// <param name="self"></param>
	private void HasteSettingsHandler_RegisterPage(On.HasteSettingsHandler.orig_RegisterPage orig, HasteSettingsHandler self)
	{
		orig(self);
		OnConfig?.Invoke();
	}

	/// <summary>
	/// Associates a <see cref="HastyData"/> instance with an <see cref="IHastySetting"/> in the settings map.
	/// </summary>
	/// <param name="hastyData">The <see cref="HastyData"/> instance.</param>
	/// <param name="setting">The <see cref="IHastySetting"/> to associate.</param>
	private void SetHDForHS(HastyData hastyData, IHastySetting setting)
	{
		try
		{
			if (SettingsMap.TryGetValue(hastyData, out _))
			{
				SettingsMap.Remove(hastyData);
			}

			setting.HastyData = hastyData;
			SettingsMap.Add(hastyData, setting);
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError($"Error in SetHDForHastySetting: {ex}");
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
		orig(self, setting);
		if (setting is IHastySetting hastySetting && setting is IExposedSetting exposedSetting)
		{
			HastyData hastyData = GetHDByHS(hastySetting) ?? new();

			hastyData.HastySetting = hastySetting;
			hastyData.SettingsUICell = self;
			hastyData.CanvasGroup = CanvasGroupRef.Invoke(self);
			hastyData.GameObject = self.gameObject;
			hastyData.LayoutElement = self.gameObject.AddComponent<LayoutElement>();
			hastyData.ExposedSetting = exposedSetting;
			hastyData.HastyCollapsible = setting as HastyCollapsible ?? null!;

			if (hastySetting.ParentCollapsible != null)
			{
				var collapsed = hastySetting.ParentCollapsible.Collapsed;
				hastyData.LayoutElement.ignoreLayout = collapsed;
				hastyData.CanvasGroup.blocksRaycasts = !collapsed;
				hastyData.CanvasGroup.alpha = 0f;
				if (hastyData.GameObject.TryGetComponent<SettingsUICell>(out var cell))
				{
					cell.enabled = !collapsed;
				}

				if (hastyData.GameObject.transform.GetChild(0).TryGetComponent(out Image image))
				{
					image.color = new Color(0.0161f, 0.0576f, 0.0615f, 0.6157f);
				}

				hastyData.GameObject.SetActive(!collapsed);
			}

			if (hastySetting is HastyFloat hastyFloat && hastyFloat.IsWhole)
			{
				var slider = hastyData.GameObject.GetComponentInChildren<Slider>();
				if (slider != null)
				{
					slider.wholeNumbers = true;
				}

				hastyFloat.ApplyValue();
			}

			SetHDForHS(hastyData, hastySetting);
		}
	}
}
