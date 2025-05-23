using TMPro;
using UnityEngine.Localization;
using Zorro.Settings;

/// <summary>
/// Encapsulates configuration options for a boolean setting, including display strings and click event.
/// </summary>
public struct BoolOptions
{
	/// <summary>
	/// The default value for the setting.
	/// </summary>
	public bool DefaultValue = default;

	/// <summary>
	/// The text to display when the value is <c>false</c>.
	/// </summary>
	public string OffString = "Off";

	/// <summary>
	/// The action to invoke when the value is toggled.
	/// </summary>
	public Action<bool> OnClicked = null!;

	public Action<bool> OnLoad = null!;

	/// <summary>
	/// The text to display when the value is <c>true</c>.
	/// </summary>
	public string OnString = "On";

	/// <summary>
	/// Initializes a new instance of the <see cref="BoolOptions"/> struct.
	/// </summary>
	/// <param name="offString">The text to display when the value is <c>false</c>.</param>
	/// <param name="onString">The text to display when the value is <c>true</c>.</param>
	/// <param name="defaultValue">The default value.</param>
	/// <param name="onClicked">The action to invoke when the value is toggled.</param>
	public BoolOptions(string offString = "Off", string onString = "On", bool defaultValue = false, Action<bool> onClicked = null!)
	{
		OffString = offString;
		OnString = onString;
		DefaultValue = defaultValue;
		OnClicked = onClicked;
	}
}

/// <summary>
/// Represents a boolean setting with configuration, localization, and event support for MonoMod-based Unity mods.
/// </summary>
public class HastyBool : ButtonSetting, IHastySetting, IExposedSetting
{
	public BoolOptions Options = default;
	private readonly HastySetting _config = null!;
	private readonly LocalizedString _displayName = null!;
	private ISettingsSaveLoad _saveLoad = null!;
	private TextMeshProUGUI _valueText = null!;

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyBool"/> class.
	/// </summary>
	/// <param name="config">The parent configuration object.</param>
	/// <param name="name">The name of the boolean setting.</param>
	/// <param name="description">The description of the boolean setting.</param>
	/// <param name="options">Additional options for the boolean setting.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
	public HastyBool(HastySetting config, string name, string description, BoolOptions options = default)
	{
		if (config == null)
			throw new ArgumentNullException(nameof(config), $"{HastySetting.LogName}No config was provided. Unable to create \"HastyBool\".");
		if (string.IsNullOrEmpty(name))
			throw new ArgumentException($"{HastySetting.LogName}No name was given to the boolean setting. Either it's empty or null.", nameof(name));
		if (string.IsNullOrEmpty(description))
			UnityEngine.Debug.LogWarning($"{HastySetting.LogName}No description was given to: \"{name}\" of type: HastyBool. This may cause errors.");

		_config = config;
		_displayName = _config.CreateDisplayName(name, description);

		Key = $"{System.Reflection.Assembly.GetExecutingAssembly().FullName}.{_config.ModName}.{name}.{description}";
		Options = options;
		UUID = Guid.NewGuid().ToString();

		_config.Add(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyBool"/> class as a child of a collapsible group.
	/// </summary>
	/// <param name="config">The parent collapsible group.</param>
	/// <param name="name">The name of the boolean setting.</param>
	/// <param name="description">The description of the boolean setting.</param>
	/// <param name="options">Additional options for the boolean setting.</param>
	public HastyBool(HastyCollapsible config, string name, string description, BoolOptions options = default)
		: this(config._config, name, description, options)
	{
		if (config == null)
			throw new ArgumentNullException(nameof(config), $"{HastySetting.LogName}No config was provided. Unable to create \"HastyBool\".");
		if (string.IsNullOrEmpty(name))
			throw new ArgumentException($"{HastySetting.LogName}No name was given to the boolean setting. Either it's empty or null.", nameof(name));
		if (string.IsNullOrEmpty(description))
			UnityEngine.Debug.LogWarning($"{HastySetting.LogName}No description was given to \"{name}\" of type: HastyBool. This may cause errors.");

		ParentCollapsible = config;
		config.Content.Add(this);
	}

	/// <inheritdoc/>
	public HastyData HastyData { get; set; } = null!;

	/// <inheritdoc/>
	public string Key { get; } = string.Empty;

	/// <inheritdoc/>
	public HastyCollapsible ParentCollapsible { get; set; } = null!;

	/// <inheritdoc/>
	public string UUID { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the current value of the boolean setting.
	/// </summary>
	public bool Value { get; set; }

	/// <summary>
	/// Gets the TextMeshProUGUI used to display and edit the value.
	/// </summary>
	private TextMeshProUGUI ValueText
	{
		get => _valueText ??= HastyData.SettingsUICell.transform.Find("InputParent/BUTTON INPUT(Clone)/EscapeMenuButton/Text").GetComponent<TextMeshProUGUI>();
	}

	/// <summary>
	/// Gets the text to display on the button, depending on the current value.
	/// </summary>
	/// <returns>The button text for the current value.</returns>
	public override string GetButtonText() => Value ? Options.OnString : Options.OffString;

	/// <summary>
	/// Gets the category (mod name) for this boolean setting.
	/// </summary>
	/// <returns>The mod name.</returns>
	public string GetCategory() => _config.ModName;

	/// <summary>
	/// Gets the localized display name for this boolean setting.
	/// </summary>
	/// <returns>The localized display name.</returns>
	public LocalizedString GetDisplayName() => _displayName;

	/// <summary>
	/// Loads the value from the provided loader, or uses the default if not found.
	/// Updates the button label and logs the load operation.
	/// </summary>
	/// <param name="loader">The settings loader.</param>
	public override void Load(ISettingsSaveLoad loader)
	{
		_saveLoad = loader;
		Value = loader.TryLoadBool(Key, out var value) ? value : Options.DefaultValue;
		Options.OnLoad?.Invoke(Value);
	}

	/// <summary>
	/// Handles the click event for this boolean setting, toggling its value, invoking the configured action, and saving the new value.
	/// </summary>
	/// <param name="settingHandler">The setting handler (not used).</param>
	public override void OnClicked(ISettingHandler settingHandler)
	{
		Value = !Value;
		Options.OnClicked?.Invoke(Value);
		Save(_saveLoad);
	}

	/// <summary>
	/// Resets the value to the default and saves it.
	/// </summary>
	public void Reset()
	{ Value = Options.DefaultValue; SetValueText(GetButtonText()); Save(_saveLoad); }

	/// <summary>
	/// Saves the current value using the provided saver and updates the button label.
	/// </summary>
	/// <param name="saver">The settings saver.</param>
	public override void Save(ISettingsSaveLoad saver)
	{
		saver.SaveBool(Key, Value);
		_saveLoad ??= saver;

		if (HastyData?.ButtonSettingUI != null)
		{ HastyData.ButtonSettingUI.Label.text = GetButtonText(); }
	}

	/// <summary>
	/// Sets the text of the value display to the specified text.
	/// </summary>
	public void SetValueText(object text) => ValueText.text = text.ToString();
}
