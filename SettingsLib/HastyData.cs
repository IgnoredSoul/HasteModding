using UnityEngine.UI;
using UnityEngine;
using Zorro.Settings.UI;

/// <summary>
/// Encapsulates all UI and metadata references for a Hasty setting, enabling advanced UI integration and manipulation.
/// </summary>
public class HastyData
{
	/// <summary>
	/// Initializes a new instance of the <see cref="HastyData"/> class with optional references to UI and metadata components.
	/// </summary>
	/// <param name="buttonUI">The <see cref="ButtonSettingUI"/> associated with this setting, if any.</param>
	/// <param name="canvas">The <see cref="CanvasGroup"/> for UI visibility and interaction control.</param>
	/// <param name="gameObject">The <see cref="GameObject"/> representing the setting in the UI.</param>
	/// <param name="layout">The <see cref="LayoutElement"/> for layout management.</param>
	/// <param name="exposed">The <see cref="IExposedSetting"/> interface for the setting.</param>
	/// <param name="col">The parent <see cref="HastyCollapsible"/> group, if any.</param>
	/// <param name="hasty">The <see cref="IHastySetting"/> instance this data is associated with.</param>
	/// <param name="settingUI">The <see cref="SettingsUICell"/> associated with this setting, if any.</param>
	public HastyData(
		ButtonSettingUI buttonUI = null!,
		CanvasGroup canvas = null!,
		GameObject gameObject = null!,
		LayoutElement layout = null!,
		IExposedSetting exposed = null!,
		HastyCollapsible col = null!,
		IHastySetting hasty = null!,
		SettingsUICell settingUI = null!)
	{
		ButtonSettingUI = buttonUI;
		CanvasGroup = canvas;
		GameObject = gameObject;
		LayoutElement = layout;
		ExposedSetting = exposed;
		HastyCollapsible = col;
		HastySetting = hasty;
		SettingsUICell = settingUI;
	}

	/// <summary>
	/// Gets or sets the <see cref="ButtonSettingUI"/> associated with this setting, if any.
	/// </summary>
	public ButtonSettingUI ButtonSettingUI { get; set; } = null!;

	/// <summary>
	/// Gets or sets the <see cref="CanvasGroup"/> for UI visibility and interaction control.
	/// </summary>
	public CanvasGroup CanvasGroup { get; set; } = null!;

	/// <summary>
	/// Gets or sets the <see cref="IExposedSetting"/> interface for the setting.
	/// </summary>
	public IExposedSetting ExposedSetting { get; set; } = null!;

	/// <summary>
	/// Gets or sets the <see cref="GameObject"/> representing the setting in the UI.
	/// </summary>
	public GameObject GameObject { get; set; } = null!;

	/// <summary>
	/// Gets or sets the parent <see cref="HastyCollapsible"/> group, if any.
	/// </summary>
	public HastyCollapsible HastyCollapsible { get; set; } = null!;

	/// <summary>
	/// Gets or sets the <see cref="IHastySetting"/> instance this data is associated with.
	/// </summary>
	public IHastySetting HastySetting { get; set; } = null!;

	/// <summary>
	/// Gets or sets the <see cref="LayoutElement"/> for layout management.
	/// </summary>
	public LayoutElement LayoutElement { get; set; } = null!;

	public SettingsUICell SettingsUICell { get; set; } = null!;
}
