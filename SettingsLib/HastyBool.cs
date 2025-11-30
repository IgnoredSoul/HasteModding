using UnityEngine.Localization;
using Zorro.Settings;

/// <summary>
/// Encapsulates configuration options for a boolean setting, including display strings and click event.
/// </summary>
public struct BoolOptions(
	string offString = "Off",
	string onString = "On",
	bool defaultValue = false,
	Action<bool> onClicked = null!,
	Action<bool> onLoad = null!)
{
	/// <summary>
	/// The default value for the setting.
	/// </summary>
	public bool DefaultValue { get; set; } = defaultValue;

	/// <summary>
	/// The text to display when the value is <c>false</c>.
	/// </summary>
	public string OffString { get; set; } = offString;

	/// <summary>
	/// The action to invoke when the value is toggled.
	/// </summary>
	public Action<bool> OnClicked { get; set; } = onClicked;

	/// <summary>
	/// The action to invoke when the value is loaded.
	/// </summary>
	public Action<bool> OnLoad { get; set; } = onLoad;

	/// <summary>
	/// The text to display when the value is <c>true</c>.
	/// </summary>
	public string OnString { get; set; } = onString;
}

/// <summary>
/// Represents a boolean setting with configuration, localization, and event support for MonoMod-based Unity mods.
/// </summary>
public class HastyBool : ButtonSetting, IHastySetting, IExposedSetting
{
	private readonly HastySetting _config = null!;
	private readonly LocalizedString _displayName = null!;
	private readonly BoolOptions _options = default;

	private ISettingsSaveLoad _saveLoad = null!;
	private TMPro.TextMeshProUGUI? _valueText = null!;

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
		{ Informer.Inform(new ArgumentNullException(nameof(config), $"No config was provided. Unable to create \"HastyBool\"."), InformType.Error); return; }
		if (string.IsNullOrEmpty(name))
		{ Informer.Inform(new ArgumentException($"No name was given to \"HastyBool\". Either it's empty or null.", nameof(name)), InformType.Error); return; }
		if (string.IsNullOrEmpty(description))
		{ Informer.Inform($"No description was given to: \"{name}\" of type: \"HastyBool\". This may cause errors."); }

		_config = config;
		_displayName = _config.CreateDisplayName(name, description);
		_options = options;

		Key = $"{config.ModName}.{name}";
		UUID = Guid.NewGuid().ToString();

		_config.Add(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyBool"/> class as a child of a collapsible group.
	/// </summary>
	/// <param name="collapsible">The parent collapsible group.</param>
	/// <param name="name">The name of the boolean setting.</param>
	/// <param name="description">The description of the boolean setting.</param>
	/// <param name="options">Additional options for the boolean setting.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="collapsible"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
	public HastyBool(HastyCollapsible collapsible, string name, string description, BoolOptions options = default)
		: this(collapsible._config, name, description, options)
	{
		ParentCollapsible = collapsible;
		collapsible.Content.Add(this);
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
	public bool Value { get; set; } = default;

	/// <summary>
	/// Gets the TextMeshProUGUI used to display the value.
	/// </summary>
	private TMPro.TextMeshProUGUI? ValueText
	{
		get
		{
			if (_valueText == null)
			{
				if (HastyData == null)
				{ throw new NullReferenceException("HastyData is not set. Cannot access ValueText."); }
				if (HastyData.SettingsUICell == null)
				{ throw new NullReferenceException("SettingsUICell is not set in HastyData. Cannot access ValueText."); }
				_valueText = HastyData.SettingsUICell.transform.Find("InputParent/BUTTON INPUT(Clone)/EscapeMenuButton/Text")?.GetComponent<TMPro.TextMeshProUGUI>() ?? null!;
			}
			return _valueText;
		}
	}

	/// <summary>
	/// Gets the text to display on the button, depending on the current value.
	/// </summary>
	/// <returns>The button text for the current value.</returns>
	public override string GetButtonText()
		=> (Value ? _options.OnString : _options.OffString);

	/// <summary>
	/// Gets the category (mod name) for this boolean setting.
	/// </summary>
	/// <returns>The mod name.</returns>
	public string GetCategory()
		=> _config.ModName;

	/// <summary>
	/// Gets the localized display name for this boolean setting.
	/// </summary>
	/// <returns>The localized display name.</returns>
	public LocalizedString GetDisplayName()
		=> _displayName;

	/// <summary>
	/// Loads the value from the provided loader, or uses the default if not found.
	/// Updates the button label and logs the load operation.
	/// </summary>
	/// <param name="loader">The settings loader.</param>
	public override void Load(ISettingsSaveLoad loader)
	{
		_saveLoad ??= loader;

		Value = (loader.TryLoadBool(Key, out bool value) ? value : _options.DefaultValue);

		_options.OnLoad?.Invoke(Value);
	}

	/// <summary>
	/// Handles the click event for this boolean setting, toggling its value, invoking the configured action, and saving the new value.
	/// </summary>
	/// <param name="settingHandler">The setting handler (not used).</param>
	public override void OnClicked(ISettingHandler settingHandler)
	{
		_options.OnClicked?.Invoke((Value = !Value));

		Save(_saveLoad);
	}

	/// <summary>
	/// Resets the value to the default and saves it.
	/// </summary>
	public void Reset()
	{
		Value = _options.DefaultValue;

		if (ValueText != null)
		{
			ValueText.text = GetButtonText();
		}

		Save(_saveLoad);
	}

	/// <summary>
	/// Saves the current value using the provided saver and updates the button label.
	/// </summary>
	/// <param name="saver">The settings saver.</param>
	public override void Save(ISettingsSaveLoad saver)
	{
		saver.SaveBool(Key, Value);
		_saveLoad ??= saver;

		if (ValueText != null)
		{
			ValueText.text = GetButtonText();
		}
	}
}
