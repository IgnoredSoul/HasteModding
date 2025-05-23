using TMPro;
using Unity.Mathematics;
using UnityEngine.Localization;
using Zorro.Settings;

/// <summary>
/// Encapsulates configuration options for a float setting, including value bounds, event hooks, and whole-number enforcement.
/// </summary>
public struct FloatOptions
{
	/// <summary>
	/// The default value for the setting.
	/// </summary>
	public float DefaultValue;

	/// <summary>
	/// The minimum and maximum allowed values for the setting.
	/// </summary>
	public float2 MinMax;

	/// <summary>
	/// Action to invoke when the value is applied.
	/// </summary>
	public Action<float> OnApplied;

	/// <summary>
	/// Action to invoke when the value is loaded.
	/// </summary>
	public Action<float> OnLoad;

	/// <summary>
	/// If true, restricts the setting to whole numbers only.
	/// </summary>
	public bool Whole;

	/// <summary>
	/// Initializes a new instance of the <see cref="FloatOptions"/> struct.
	/// </summary>
	/// <param name="min">Minimum allowed value.</param>
	/// <param name="max">Maximum allowed value.</param>
	/// <param name="defaultValue">Default value.</param>
	/// <param name="onApplied">Action to invoke when applied.</param>
	/// <param name="onLoad">Action to invoke when loaded.</param>
	/// <param name="whole">If true, restricts the setting to whole numbers only.</param>
	public FloatOptions(
		float min,
		float max,
		float defaultValue,
		Action<float> onApplied = null!,
		Action<float> onLoad = null!,
		bool whole = false)
	{
		MinMax = new float2(min, max);
		DefaultValue = defaultValue;
		OnApplied = onApplied;
		OnLoad = onLoad;
		Whole = whole;
	}
}

/// <summary>
/// Represents a floating-point setting with configuration, localization, and event support for MonoMod-based Unity mods.
/// </summary>
public class HastyFloat : FloatSetting, IHastySetting
{
	public FloatOptions Options = default;
	private readonly HastySetting _config = null!;
	private readonly LocalizedString _displayName = null!;
	private ISettingsSaveLoad _saveLoad = null!;

	private TMP_InputField _valueText = null!;

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyFloat"/> class.
	/// </summary>
	/// <param name="config">The parent configuration object.</param>
	/// <param name="name">The name of the setting.</param>
	/// <param name="description">The description of the setting.</param>
	/// <param name="options">Additional options for the setting.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
	public HastyFloat(HastySetting config, string name, string description, FloatOptions options = default)
	{
		if (config == null)
			throw new ArgumentNullException(nameof(config), $"{HastySetting.LogName}No config was provided. Unable to create \"HastyFloat\".");
		if (string.IsNullOrEmpty(name))
			throw new ArgumentException($"{HastySetting.LogName}No name was given to the setting. Either it's empty or null.", nameof(name));
		if (string.IsNullOrEmpty(description))
			UnityEngine.Debug.LogWarning($"{HastySetting.LogName}No description was given to: \"{name}\" of type: HastyFloat. This may cause errors.");

		_config = config;
		_displayName = _config.CreateDisplayName(name, description);

		Key = $"{System.Reflection.Assembly.GetExecutingAssembly().FullName}.{_config.ModName}.{name}.{description}";
		Options = options;
		UUID = Guid.NewGuid().ToString();

		_config.Add(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyFloat"/> class as a child of a collapsible group.
	/// </summary>
	/// <param name="config">The parent collapsible group.</param>
	/// <param name="name">The name of the setting.</param>
	/// <param name="description">The description of the setting.</param>
	/// <param name="options">Additional options for the setting.</param>
	public HastyFloat(HastyCollapsible config, string name, string description, FloatOptions options = default)
		: this(config._config, name, description, options)
	{
		if (config == null)
			throw new ArgumentNullException(nameof(config), $"{HastySetting.LogName}No config was provided. Unable to create \"HastyFloat\".");
		if (string.IsNullOrEmpty(name))
			throw new ArgumentException($"{HastySetting.LogName}No name was given to the setting. Either it's empty or null.", nameof(name));
		if (string.IsNullOrEmpty(description))
			UnityEngine.Debug.LogWarning($"{HastySetting.LogName}No description was given to \"{name}\" of type: HastyFloat. This may cause errors.");

		ParentCollapsible = config;
		config.Content.Add(this);
	}

	/// <inheritdoc/>
	public HastyData HastyData { get; set; } = null!;

	/// <summary>
	/// Gets a value indicating whether this setting should use whole numbers only.
	/// </summary>
	public bool IsWhole { get => Options.Whole; }

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
		get => _valueText ??= HastyData.SettingsUICell.transform.Find("InputParent/FLOAT INPUT(Clone)/InputField (TMP)").GetComponent<TMP_InputField>();
	}

	/// <summary>
	/// Applies the current value and invokes the <see cref="FloatOptions.OnApplied"/> action if set.
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
	/// Loads the value from the provided loader, or uses the default if not found. Invokes the <see cref="FloatOptions.OnLoad"/> action if set.
	/// </summary>
	/// <param name="loader">The settings loader.</param>
	public override void Load(ISettingsSaveLoad loader)
	{
		_saveLoad = loader;
		Value = loader.TryLoadFloat(Key, out var value) ? value : GetDefaultValue();
		MinValue = GetMinMaxValue().x;
		MaxValue = GetMinMaxValue().y;
		Options.OnLoad?.Invoke(Value);
	}

	/// <summary>
	/// Resets the value to the default.
	/// </summary>
	public void Reset()
	{ Value = Options.DefaultValue; SetValueText(Value.ToString("0.0")); Save(_saveLoad); }

	/// <summary>
	/// Saves the current value using the provided saver.
	/// </summary>
	/// <param name="saver">The settings saver.</param>
	public override void Save(ISettingsSaveLoad saver) => saver.SaveFloat(Key, Value);

	/// <summary>
	/// Sets the text of the value display to the specified text.
	/// </summary>
	public void SetValueText(object text) => ValueText.text = text.ToString();

	/// <summary>
	/// Gets the default value for this setting.
	/// </summary>
	/// <returns>The default value.</returns>
	protected override float GetDefaultValue() => Options.DefaultValue;

	/// <summary>
	/// Gets the minimum and maximum allowed values for this setting.
	/// </summary>
	/// <returns>A <see cref="float2"/> containing the minimum and maximum values.</returns>
	protected override float2 GetMinMaxValue() => Options.MinMax;
}
