using UnityEngine.UI;
using UnityEngine;
using Zorro.Settings.UI;

/// <summary>
/// Encapsulates all UI and metadata references for a Hasty setting, enabling advanced UI integration and manipulation.
/// </summary>
public class HastyData
{
	// Constructor parameters and properties use nullable reference types (Type?) where they can be null.
	// This is a C# 8.0 feature and should be available if you're using C# 12.0.
	/// <summary>
	/// Initializes a new instance of the <see cref="HastyData"/> class with optional references to UI and metadata components.
	/// </summary>
	/// <param name="buttonUI">The <see cref="ButtonSettingUI"/> associated with this setting, if any.</param>
	/// <param name="canvas">The <see cref="CanvasGroup"/> for UI visibility and interaction control.</param>
	/// <param name="gameObject">The <see cref="GameObject"/> representing the setting in the UI.</param>
	/// <param name="layout">The <see cref="LayoutElement"/> for layout management.</param>
	/// <param name="hastyCollapsible">The <see cref="HastyCollapsible"/>.</param>
	/// <param name="exposed">The <see cref="IExposedSetting"/> interface for the setting.</param>
	/// <param name="hasty">The <see cref="IHastySetting"/> instance this data is associated with.</param>
	/// <param name="settingUI">The <see cref="SettingsUICell"/> associated with this setting, if any.</param>
	public HastyData(
		ButtonSettingUI? buttonUI = null,
		CanvasGroup? canvas = null,
		GameObject? gameObject = null,
		LayoutElement? layout = null,
		IExposedSetting? exposed = null,
		IHastySetting? hasty = null,
		HastyCollapsible hastyCollapsible = null!,
		SettingsUICell? settingUI = null)
	{
		ButtonSettingUI = buttonUI;
		CanvasGroup = canvas;
		GameObject = gameObject;
		LayoutElement = layout;
		ExposedSetting = exposed;
		HastySetting = hasty;
		SettingsUICell = settingUI;
	}

	// Properties explicitly marked as nullable with '?'
	/// <summary>
	/// Gets or sets the <see cref="ButtonSettingUI"/> associated with this setting, if any.
	/// </summary>
	public ButtonSettingUI? ButtonSettingUI { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="CanvasGroup"/> for UI visibility and interaction control.
	/// </summary>
	public CanvasGroup? CanvasGroup { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="IExposedSetting"/> interface for the setting.
	/// </summary>
	public IExposedSetting? ExposedSetting { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="GameObject"/> representing the setting in the UI.
	/// </summary>
	public GameObject? GameObject { get; set; }

	/// <summary>
	/// Gets or sets the parent <see cref="HastyCollapsible"/> group, if any.
	/// </summary>
	public HastyCollapsible HastyCollapsible { get; set; } = null!;

	/// <summary>
	/// Gets or sets the <see cref="IHastySetting"/> instance this data is associated with.
	/// </summary>
	public IHastySetting? HastySetting { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="LayoutElement"/> for layout management.
	/// </summary>
	public LayoutElement? LayoutElement { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="SettingsUICell"/> associated with this setting.
	/// </summary>
	public SettingsUICell? SettingsUICell { get; set; }
}
