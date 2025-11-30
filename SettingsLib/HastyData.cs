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
	/// <param name="hastyCollapsible">The <see cref="HastyCollapsible"/>.</param>
	/// <param name="exposed">The <see cref="IExposedSetting"/> interface for the setting.</param>
	/// <param name="hasty">The <see cref="IHastySetting"/> instance this data is associated with.</param>
	/// <param name="settingUI">The <see cref="SettingsUICell"/> associated with this setting, if any.</param>
	public HastyData(
        Zorro.Settings.UI.ButtonSettingUI? buttonUI = null,
		UnityEngine.CanvasGroup? canvas = null,
        UnityEngine.GameObject? gameObject = null,
        UnityEngine.UI.LayoutElement? layout = null,
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

	/// <summary>
	/// Gets or sets the <see cref="ButtonSettingUI"/> associated with this setting, if any.
	/// </summary>
	public Zorro.Settings.UI.ButtonSettingUI? ButtonSettingUI { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="CanvasGroup"/> for UI visibility and interaction control.
	/// </summary>
	public UnityEngine.CanvasGroup? CanvasGroup { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="IExposedSetting"/> interface for the setting.
	/// </summary>
	public IExposedSetting? ExposedSetting { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="GameObject"/> representing the setting in the UI.
	/// </summary>
	public UnityEngine.GameObject? GameObject { get; set; }

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
	public UnityEngine.UI.LayoutElement? LayoutElement { get; set; }

	/// <summary>
	/// Gets or sets the <see cref="SettingsUICell"/> associated with this setting.
	/// </summary>
	public SettingsUICell? SettingsUICell { get; set; }
}
