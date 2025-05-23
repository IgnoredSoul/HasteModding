using TMPro;
using UnityEngine.Localization;
using Zorro.Settings;

/// <summary>
/// Encapsulates configuration options for a button setting, including button text and click event.
/// </summary>
public struct ButtonOptions
{
	/// <summary>
	/// The text to display on the button.
	/// </summary>
	public string ButtonText;

	/// <summary>
	/// The action to invoke when the button is clicked.
	/// </summary>
	public Action OnClicked;

	/// <summary>
	/// Initializes a new instance of the <see cref="ButtonOptions"/> struct.
	/// </summary>
	/// <param name="buttonText">The text to display on the button.</param>
	/// <param name="onClicked">The action to invoke when the button is clicked.</param>
	public ButtonOptions(string buttonText = "", Action onClicked = null!)
	{
		ButtonText = buttonText;
		OnClicked = onClicked;
	}
}

/// <summary>
/// Represents a button setting with configuration, localization, and event support for MonoMod-based Unity mods.
/// </summary>
public class HastyButton : ButtonSetting, IHastySetting, IExposedSetting
{
	public ButtonOptions Options = default;
	private readonly HastySetting _config;
	private readonly LocalizedString _displayName;

	private TextMeshProUGUI _valueText = null!;

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyButton"/> class.
	/// </summary>
	/// <param name="config">The parent configuration object.</param>
	/// <param name="name">The name of the button setting.</param>
	/// <param name="description">The description of the button setting.</param>
	/// <param name="options">Additional options for the button.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
	public HastyButton(HastySetting config, string name, string description, ButtonOptions options = default)
	{
		if (config == null)
			throw new ArgumentNullException(nameof(config), $"{HastySetting.LogName}No config was provided. Unable to create \"HastyButton\".");
		if (string.IsNullOrEmpty(name))
			throw new ArgumentException($"{HastySetting.LogName}No name was given to the button. Either it's empty or null.", nameof(name));
		if (string.IsNullOrEmpty(description))
			UnityEngine.Debug.LogWarning($"{HastySetting.LogName}No description was given to: \"{name}\" of type: HastyButton. This may cause errors.");

		_config = config;
		_displayName = _config.CreateDisplayName(name, description);

		Key = $"{System.Reflection.Assembly.GetExecutingAssembly().FullName}.{_config.ModName}.{name}.{description}";
		Options = options;
		UUID = Guid.NewGuid().ToString();

		_config.Add(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyButton"/> class as a child of a collapsible group.
	/// </summary>
	/// <param name="config">The parent collapsible group.</param>
	/// <param name="name">The name of the button setting.</param>
	/// <param name="description">The description of the button setting.</param>
	/// <param name="options">Additional options for the button.</param>
	public HastyButton(HastyCollapsible config, string name, string description, ButtonOptions options = default)
		: this(config._config, name, description, options)
	{
		if (config == null)
			throw new ArgumentNullException(nameof(config), $"{HastySetting.LogName}No config was provided. Unable to create \"HastyButton\".");
		if (string.IsNullOrEmpty(name))
			throw new ArgumentException($"{HastySetting.LogName}No name was given to the button. Either it's empty or null.", nameof(name));
		if (string.IsNullOrEmpty(description))
			UnityEngine.Debug.LogWarning($"{HastySetting.LogName}No description was given to \"{name}\" of type: HastyButton. This may cause errors.");

		ParentCollapsible = config;
		config.Content.Add(this);
	}

	/// <inheritdoc/>
	public HastyData HastyData { get; set; } = null!;

	/// <inheritdoc/>
	public string Key { get; private set; } = string.Empty;

	/// <inheritdoc/>
	public HastyCollapsible ParentCollapsible { get; set; } = null!;

	/// <inheritdoc/>
	public string UUID { get; set; } = string.Empty;

	/// <summary>
	/// Gets the TextMeshProUGUI used to display and edit the value.
	/// </summary>
	private TextMeshProUGUI ValueText
	{
		get => _valueText ??= HastyData.SettingsUICell.transform.Find("InputParent/BUTTON INPUT(Clone)/EscapeMenuButton/Text").GetComponent<TextMeshProUGUI>();
	}

	/// <summary>
	/// Gets the text to display on the button.
	/// </summary>
	/// <returns>The button text.</returns>
	public override string GetButtonText() => Options.ButtonText;

	/// <summary>
	/// Gets the category (mod name) for this button setting.
	/// </summary>
	/// <returns>The mod name.</returns>
	public string GetCategory() => _config.ModName;

	/// <summary>
	/// Gets the localized display name for this button setting.
	/// </summary>
	/// <returns>The localized display name.</returns>
	public LocalizedString GetDisplayName() => _displayName;

	/// <summary>
	/// Handles the click event for this button, invoking the configured action if set.
	/// </summary>
	/// <param name="settingHandler">The setting handler (not used).</param>
	public override void OnClicked(ISettingHandler settingHandler) => Options.OnClicked?.Invoke();

	/// <summary>
	/// Resets the button setting. (No operation for button settings.)
	/// </summary>
	public void Reset()
	{ SetValueText(Options.ButtonText); }

	/// <summary>
	/// Sets the text of the value display to the specified text.
	/// </summary>
	public void SetValueText(object text) => ValueText.text = text.ToString();
}
