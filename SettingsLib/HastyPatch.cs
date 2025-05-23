using HarmonyLib;
using System.Runtime.CompilerServices;
using UnityEngine.UI;
using UnityEngine;
using Zorro.Settings.UI;
using Zorro.Settings;

/// <summary>
/// Provides Harmony patches and utility methods for integrating Hasty settings with the UI and settings system.
/// </summary>
public class HastyPatch
{
	/// <summary>
	/// Maps <see cref="HastyData"/> instances to their corresponding <see cref="IHastySetting"/> instances.
	/// </summary>
	private static readonly ConditionalWeakTable<HastyData, IHastySetting> SettingsMap = new();

	/// <summary>
	/// Event invoked when configuration is applied.
	/// </summary>
	internal static event Action OnConfig = null!;

	/// <summary>
	/// Field reference to the private <c>m_canvasGroup</c> field of <see cref="SettingsUICell"/>.
	/// </summary>
	private static AccessTools.FieldRef<SettingsUICell, CanvasGroup> CanvasGroupRef
		=> AccessTools.FieldRefAccess<SettingsUICell, CanvasGroup>("m_canvasGroup");

	/// <summary>
	/// Harmony postfix patch for <see cref="ButtonSettingUI.Setup"/>.
	/// Sets up Hasty-specific UI and data for button settings.
	/// </summary>
	/// <param name="__instance">The <see cref="ButtonSettingUI"/> instance being set up.</param>
	/// <param name="setting">The associated <see cref="Setting"/> instance.</param>
	[HarmonyPatch(typeof(ButtonSettingUI), "Setup")]
	[HarmonyPostfix]
	private static void ButtonSettingSetup(ButtonSettingUI __instance, Setting setting)
	{
		if (setting is not IHastySetting hastySetting)
			return;

		HastyData hastyData = GetHDByHastySetting(hastySetting) ?? new();

		hastyData.ButtonSettingUI = __instance;
		hastyData.HastySetting = hastySetting;

		if (setting is HastyCollapsible collapsible)
		{
			hastyData.HastyCollapsible = collapsible;
			__instance.Label.text = collapsible.Collapsed ? "► Expand" : "▼ Collapse";
			collapsible.HastyData = hastyData;

			collapsible.Clicked += collapsed =>
			{
				__instance.Label.text = collapsible.Collapsed ? "► Expand" : "▼ Collapse";
				foreach (var c in collapsible.Content)
				{
					if (c.HastyData is { } childData) // Wacky bullshit I pulled out my ass
					{
						childData.LayoutElement.ignoreLayout = collapsed;
						childData.CanvasGroup.blocksRaycasts = !collapsed;
						childData.CanvasGroup.alpha = 0f;
						if (childData.GameObject.TryGetComponent<SettingsUICell>(out var cell))
						{ cell.enabled = !collapsed; }
						childData.GameObject.SetActive(!collapsed);
					}
				}
			};
		}

		SetHDForHastySetting(hastyData, hastySetting);
	}

	/// <summary>
	/// Harmony postfix patch for <see cref="SettingsUICell.Setup"/>.
	/// Sets up Hasty-specific UI and data for settings cells.
	/// </summary>
	/// <param name="__instance">The <see cref="SettingsUICell"/> instance being set up.</param>
	/// <param name="setting">The associated <see cref="Setting"/> instance.</param>
	[HarmonyPatch(typeof(SettingsUICell), "Setup")]
	[HarmonyPostfix]
	private static void CellSetupPostfix(SettingsUICell __instance, Setting setting)
	{
		if (setting is not IHastySetting hastySetting || setting is not IExposedSetting exposedSetting)
			return;

		var hastyData = GetHDByHastySetting(hastySetting) ?? new();

		hastyData.HastySetting = hastySetting;
		hastyData.SettingsUICell = __instance;
		hastyData.CanvasGroup = CanvasGroupRef.Invoke(__instance);
		hastyData.GameObject = __instance.gameObject;
		hastyData.LayoutElement = __instance.gameObject.AddComponent<LayoutElement>();
		hastyData.ExposedSetting = exposedSetting;
		hastyData.HastyCollapsible = setting as HastyCollapsible ?? null!;

		if (hastySetting.ParentCollapsible != null)
		{
			var collapsed = hastySetting.ParentCollapsible.Collapsed;
			hastyData.LayoutElement.ignoreLayout = collapsed;
			hastyData.CanvasGroup.blocksRaycasts = !collapsed;
			hastyData.CanvasGroup.alpha = 0f;
			if (hastyData.GameObject.TryGetComponent<SettingsUICell>(out var cell))
				cell.enabled = !collapsed;

			var image = hastyData.GameObject.transform.GetChild(0).GetComponent<Image>();
			if (image != null)
				image.color = new Color(0.0161f, 0.0576f, 0.0615f, 0.6157f);

			hastyData.GameObject.SetActive(!collapsed);
		}

		SetHDForHastySetting(hastyData, hastySetting);

		if (hastySetting is HastyFloat hastyFloat && hastyFloat.IsWhole)
		{
			var slider = hastyData.GameObject.GetComponentInChildren<Slider>();
			if (slider != null)
				slider.wholeNumbers = true;
		}
	}

	/// <summary>
	/// Retrieves the <see cref="HastyData"/> instance associated with the specified <see cref="IHastySetting"/> by UUID.
	/// </summary>
	/// <param name="setting">The <see cref="IHastySetting"/> to look up.</param>
	/// <returns>The associated <see cref="HastyData"/>, or <c>null</c> if not found.</returns>
	private static HastyData? GetHDByHastySetting(IHastySetting setting)
	{
		try
		{
			return SettingsMap.FirstOrDefault(kvp => kvp.Value.UUID == setting.UUID).Key;
		}
		catch (Exception ex)
		{
			Debug.LogError($"Error in GetHDByHastySetting: {ex}");
			return null;
		}
	}

	/// <summary>
	/// Harmony prefix patch for <see cref="HasteSettingsHandler.RegisterPage"/>.
	/// Invokes the <see cref="OnConfig"/> event before the page is registered.
	/// </summary>
	/// <param name="__instance">The <see cref="HasteSettingsHandler"/> instance.</param>
	[HarmonyPatch(typeof(HasteSettingsHandler), "RegisterPage")]
	[HarmonyPrefix]
	private static void RegisterPagePrefix(HasteSettingsHandler __instance) => OnConfig?.Invoke();

	/// <summary>
	/// Associates a <see cref="HastyData"/> instance with an <see cref="IHastySetting"/> in the settings map.
	/// </summary>
	/// <param name="hastyData">The <see cref="HastyData"/> instance.</param>
	/// <param name="setting">The <see cref="IHastySetting"/> to associate.</param>
	private static void SetHDForHastySetting(HastyData hastyData, IHastySetting setting)
	{
		try
		{
			if (SettingsMap.TryGetValue(hastyData, out _))
				SettingsMap.Remove(hastyData);
			setting.HastyData = hastyData;
			SettingsMap.Add(hastyData, setting);
		}
		catch (Exception ex)
		{
			Debug.LogError($"Error in SetHDForHastySetting: {ex}");
		}
	}
}
