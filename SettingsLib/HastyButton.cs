using UnityEngine.Localization;
using Zorro.Settings;

/// <summary>
/// Encapsulates configuration options for a button setting, including button text and click event.
/// </summary>
public struct ButtonOptions(
	string text = "Unassigned",
	Action? onClick = null)
{
	/// <summary>
	/// The action to invoke when the button is clicked.
	/// </summary>
	public Action OnClick { get; set; } = onClick ?? (() => UnityEngine.Debug.LogWarning("This button was never assigned an action"));

	/// <summary>
	/// The text to display on the button.
	/// </summary>
	public string Text { get; set; } = text;
}

/// <summary>
/// Represents a button setting with configuration, localization, and event support for MonoMod-based Unity mods.
/// </summary>
public class HastyButton : ButtonSetting, IHastySetting
{
	private readonly HastySetting _config = null!;
	private readonly LocalizedString _displayName = null!;
	private readonly ButtonOptions _options = default;

	private TMPro.TextMeshProUGUI? _valueText = null!;

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
        { Informer.Inform(new ArgumentNullException(nameof(config), $"No config was provided. Unable to create \"HastyButton\"."), InformType.Error); return; }
        if (string.IsNullOrEmpty(name))
        { Informer.Inform(new ArgumentException($"No name was given to \"HastyButton\". Either it's empty or null.", nameof(name)), InformType.Error); return; }
        if (string.IsNullOrEmpty(description))
        { Informer.Inform($"No description was given to: \"{name}\" of type: \"HastyButton\". This may cause errors."); }

        _config = config;
		_displayName = _config.CreateDisplayName(name, description);
		_options = options;

		Key = $"{config.ModName}.{name}";
		UUID = Guid.NewGuid().ToString();

		_config.Add(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyButton"/> class as a child of a collapsible group.
	/// </summary>
	/// <param name="collapsible">The parent collapsible group.</param>
	/// <param name="name">The name of the button setting.</param>
	/// <param name="description">The description of the button setting.</param>
	/// <param name="options">Additional options for the button.</param>
	/// 	/// <exception cref="ArgumentNullException">Thrown if <paramref name="collapsible"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
	public HastyButton(HastyCollapsible collapsible, string name, string description, ButtonOptions options = default)
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
	/// Gets the text to display on the button.
	/// </summary>
	/// <returns>The button text.</returns>
	public override string GetButtonText()
		=> _options.Text;

	/// <summary>
	/// Gets the category (mod name) for this button setting.
	/// </summary>
	/// <returns>The mod name.</returns>
	public string GetCategory()
		=> _config.ModName;

	/// <summary>
	/// Gets the localized display name for this button setting.
	/// </summary>
	/// <returns>The localized display name.</returns>
	public LocalizedString GetDisplayName()
		=> _displayName;

	/// <summary>
	/// Handles the click event for this button, invoking the configured action if set.
	/// </summary>
	/// <param name="settingHandler">The setting handler (not used).</param>
	public override void OnClicked(ISettingHandler settingHandler)
		=> _options.OnClick?.Invoke();

	/// <summary>
	/// Resets the button setting. (No operation for button settings.)
	/// </summary>
	public void Reset()
	{
		if (ValueText != null)
		{
			ValueText.text = _options.Text;
		}
	}
}
