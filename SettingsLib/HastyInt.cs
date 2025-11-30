using Unity.Mathematics;
using UnityEngine.Localization;
using Zorro.Settings;

/// <summary>
/// Encapsulates configuration options for an int setting, including value bounds and event hooks.
/// </summary>
public struct IntOptions(
		int min,
		int max,
		int defaultValue,
		Action<int> onApplied = null!,
		Action<int> onLoad = null!)
{
	/// <summary>
	/// Gets or sets the default value for the setting.
	/// </summary>
	public int DefaultValue { get; set; } = defaultValue;

	/// <summary>
	/// Gets or sets the minimum and maximum allowed values for the setting.
	/// </summary>
	public int2 MinMax { get; set; } = new int2(min, max);

	/// <summary>
	/// Gets or sets the action to invoke when the value is applied.
	/// </summary>
	public Action<int> OnApplied { get; set; } = onApplied;

	/// <summary>
	/// Gets or sets the action to invoke when the value is loaded.
	/// </summary>
	public Action<int> OnLoad { get; set; } = onLoad;
}

/// <summary>
/// Represents an integer setting with configuration, localization, and event support for MonoMod-based Unity mods.
/// </summary>
public class HastyInt : IntSetting, IHastySetting
{
	private readonly HastySetting _config = null!;
	private readonly LocalizedString _displayName = null!;
	private readonly IntOptions _options = default;

	private ISettingsSaveLoad _saveLoad = null!;
	private TMPro.TMP_InputField? _valueText = null!;

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyInt"/> class.
	/// </summary>
	/// <param name="config">The parent configuration object.</param>
	/// <param name="name">The name of the setting.</param>
	/// <param name="description">The description of the setting.</param>
	/// <param name="options">Additional options for the setting.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
	public HastyInt(HastySetting config, string name, string description, IntOptions options = default)
	{
        if (config == null)
        { Informer.Inform(new ArgumentNullException(nameof(config), $"No config was provided. Unable to create \"HastyInt\"."), InformType.Error); return; }
        if (string.IsNullOrEmpty(name))
        { Informer.Inform(new ArgumentException($"No name was given to \"HastyInt\". Either it's empty or null.", nameof(name)), InformType.Error); return; }
        if (string.IsNullOrEmpty(description))
        { Informer.Inform($"No description was given to: \"{name}\" of type: \"HastyInt\". This may cause errors."); }

        _config = config;
		_displayName = _config.CreateDisplayName(name, description);
		_options = options;

		Key = $"{config.ModName}.{name}";
		UUID = Guid.NewGuid().ToString();

		_config.Add(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyInt"/> class as a child of a collapsible group.
	/// </summary>
	/// <param name="collapsible">The parent collapsible group.</param>
	/// <param name="name">The name of the setting.</param>
	/// <param name="description">The description of the setting.</param>
	/// <param name="options">Additional options for the setting.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="collapsible"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
	public HastyInt(HastyCollapsible collapsible, string name, string description, IntOptions options = default)
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
	/// Gets the TMP_InputField used to display the value.
	/// </summary>
	private TMPro.TMP_InputField? ValueText
	{
		get
		{
			if (_valueText == null)
			{
				if (HastyData == null)
				{ throw new NullReferenceException("HastyData is not set. Cannot access ValueText."); }
				if (HastyData.SettingsUICell == null)
				{ throw new NullReferenceException("SettingsUICell is not set in HastyData. Cannot access ValueText."); }
				_valueText = HastyData.SettingsUICell.transform.Find("InputParent/INt INPUT(Clone)/InputField (TMP)")?.GetComponent<TMPro.TMP_InputField>() ?? null!;
			}
			return _valueText;
		}
	}

	/// <summary>
	/// Applies the current value and invokes the <see cref="IntOptions.OnApplied"/> action if set.
	/// </summary>
	public override void ApplyValue()
		=> _options.OnApplied?.Invoke(Value);

	/// <summary>
	/// Gets the category (mod name) for this setting.
	/// </summary>
	/// <returns>The mod name.</returns>
	public string GetCategory()
		=> _config.ModName;

	/// <summary>
	/// Gets the localized display name for this setting.
	/// </summary>
	/// <returns>The localized display name.</returns>
	public LocalizedString GetDisplayName()
		=> _displayName;

	/// <summary>
	/// Loads the value from the provided loader, or uses the default if not found. Invokes the <see cref="IntOptions.OnLoad"/> action if set.
	/// </summary>
	/// <param name="loader">The settings loader.</param>
	public override void Load(ISettingsSaveLoad loader)
	{
		_saveLoad = loader;

		Value = (loader.TryLoadInt(Key, out int value) ? value : GetDefaultValue());

		_options.OnLoad?.Invoke(Value);
	}

	/// <summary>
	/// Resets the value to the default.
	/// </summary>
	public void Reset()
	{
		Value = _options.DefaultValue;

		if (ValueText != null)
		{
			ValueText.text = Value.ToString();
		}

		Save(_saveLoad);
	}

	/// <summary>
	/// Saves the current value using the provided saver.
	/// </summary>
	/// <param name="saver">The settings saver.</param>
	public override void Save(ISettingsSaveLoad saver)
	 => saver.SaveInt(Key, Value);

	/// <summary>
	/// Gets the default value for this setting.
	/// </summary>
	/// <returns>The default value.</returns>
	protected override int GetDefaultValue()
	 => _options.DefaultValue;
}
