using TMPro;
using Unity.Mathematics;
using UnityEngine.Localization;
using Zorro.Settings;

/// <summary>
/// Encapsulates configuration options for an int setting, including value bounds and event hooks.
/// </summary>
public struct IntOptions
{
	/// <summary>
	/// Gets or sets the default value for the setting.
	/// </summary>
	public int DefaultValue;

	/// <summary>
	/// Gets or sets the minimum and maximum allowed values for the setting.
	/// </summary>
	public int2 MinMax;

	/// <summary>
	/// Gets or sets the action to invoke when the value is applied.
	/// </summary>
	public Action<int> OnApplied;

	/// <summary>
	/// Gets or sets the action to invoke when the value is loaded.
	/// </summary>
	public Action<int> OnLoad;

	/// <summary>
	/// Initializes a new instance of the <see cref="IntOptions"/> struct.
	/// </summary>
	/// <param name="min">The minimum allowed value.</param>
	/// <param name="max">The maximum allowed value.</param>
	/// <param name="defaultValue">The default value.</param>
	/// <param name="onApplied">The action to invoke when applied.</param>
	/// <param name="onLoad">The action to invoke when loaded.</param>
	public IntOptions(
		int min = int.MinValue,
		int max = int.MaxValue,
		int defaultValue = 0,
		Action<int> onApplied = null!,
		Action<int> onLoad = null!)
	{
		DefaultValue = defaultValue;
		MinMax = new int2(min, max);
		OnApplied = onApplied;
		OnLoad = onLoad;
	}
}

/// <summary>
/// Represents an integer setting with configuration, localization, and event support for MonoMod-based Unity mods.
/// </summary>
public class HastyInt : IntSetting, IHastySetting
{
	public IntOptions Options = default;
	private readonly HastySetting _config = null!;
	private readonly LocalizedString _displayName = null!;
	private ISettingsSaveLoad _saveLoad = null!;
	private TMP_InputField _valueText = null!;

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
			throw new ArgumentNullException(nameof(config), $"{HastySetting.LogName}No config was provided. Unable to create \"HastyInt\".");
		if (string.IsNullOrEmpty(name))
			throw new ArgumentException($"{HastySetting.LogName}No name was given to the setting. Either it's empty or null.", nameof(name));
		if (string.IsNullOrEmpty(description))
			UnityEngine.Debug.LogWarning($"{HastySetting.LogName}No description was given to: \"{name}\" of type: HastyInt. This may cause errors.");

		_config = config;
		_displayName = _config.CreateDisplayName(name, description);

		Key = $"{System.Reflection.Assembly.GetExecutingAssembly().FullName}.{_config.ModName}.{name}.{description}";
		Options = options;
		UUID = Guid.NewGuid().ToString();

		_config.Add(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyInt"/> class as a child of a collapsible group.
	/// </summary>
	/// <param name="config">The parent collapsible group.</param>
	/// <param name="name">The name of the setting.</param>
	/// <param name="description">The description of the setting.</param>
	/// <param name="options">Additional options for the setting.</param>
	public HastyInt(HastyCollapsible config, string name, string description, IntOptions options = default)
		: this(config._config, name, description, options)
	{
		if (config == null)
			throw new ArgumentNullException(nameof(config), $"{HastySetting.LogName}No config was provided. Unable to create \"HastyInt\".");
		if (string.IsNullOrEmpty(name))
			throw new ArgumentException($"{HastySetting.LogName}No name was given to the setting. Either it's empty or null.", nameof(name));
		if (string.IsNullOrEmpty(description))
			UnityEngine.Debug.LogWarning($"{HastySetting.LogName}No description was given to \"{name}\" of type: HastyInt. This may cause errors.");

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
	/// Gets the TMP_InputField used to display and edit the value.
	/// </summary>
	private TMP_InputField ValueText
	{
		get => _valueText ??= HastyData.SettingsUICell.transform.Find("InputParent/INt INPUT(Clone)/InputField (TMP)").GetComponent<TMP_InputField>();
	}

	/// <summary>
	/// Applies the current value and invokes the <see cref="IntOptions.OnApplied"/> action if set.
	/// </summary>
	public override void ApplyValue() => Options.OnApplied?.Invoke(Value);

	/// <summary>
	/// Gets the category (mod name) for this setting.
	/// </summary>
	/// <returns>The mod name.</returns>
	public string GetCategory() => _config.ModName;

	/// <summary>
	/// Gets the localized display name for this setting.
	/// </summary>
	/// <returns>The localized display name.</returns>
	public LocalizedString GetDisplayName() => _displayName;

	/// <summary>
	/// Loads the value from the provided loader, or uses the default if not found. Invokes the <see cref="IntOptions.OnLoad"/> action if set.
	/// </summary>
	/// <param name="loader">The settings loader.</param>
	public override void Load(ISettingsSaveLoad loader)
	{
		_saveLoad = loader;
		Value = loader.TryLoadInt(Key, out var value) ? value : GetDefaultValue();
		Options.OnLoad?.Invoke(Value);
	}

	/// <summary>
	/// Resets the value to the default.
	/// </summary>
	public void Reset()
	{ Value = Options.DefaultValue; SetValueText(Value.ToString()); }

	/// <summary>
	/// Saves the current value using the provided saver.
	/// </summary>
	/// <param name="saver">The settings saver.</param>
	public override void Save(ISettingsSaveLoad saver) => saver.SaveInt(Key, Value);

	/// <summary>
	/// Sets the text of the value display to the specified text.
	/// </summary>
	public void SetValueText(object text) => ValueText.text = text.ToString();

	/// <summary>
	/// Gets the default value for this setting.
	/// </summary>
	/// <returns>The default value.</returns>
	protected override int GetDefaultValue() => Options.DefaultValue;
}
