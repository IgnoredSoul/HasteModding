using TMPro;
using Unity.Mathematics;
using UnityEngine.Localization;
using Zorro.Settings;

/// <summary>
/// Encapsulates configuration options for a float setting, including value bounds, event hooks, and whole-number enforcement.
/// </summary>
public struct FloatOptions(
	float min,
	float max,
	float defaultValue,
	Action<float> onApplied = null!,
	Action<float> onLoad = null!,
	bool isWhole = false
)
{
	/// <summary>
	/// The default value for the setting.
	/// </summary>
	public float DefaultValue { get; set; } = defaultValue;

	/// <summary>
	/// If true, restricts the setting to whole numbers only.
	/// </summary>
	public bool IsWhole { get; set; } = isWhole;

	/// <summary>
	/// The minimum and maximum allowed values for the setting.
	/// </summary>
	public float2 MinMax { get; set; } = new float2(min, max);

	/// <summary>
	/// Action to invoke when the value is applied.
	/// </summary>
	public Action<float> OnApplied { get; set; } = onApplied;

	/// <summary>
	/// Action to invoke when the value is loaded.
	/// </summary>
	public Action<float> OnLoad { get; set; } = onLoad;
}

/// <summary>
/// Represents a floating-point setting with configuration, localization, and event support for MonoMod-based Unity mods.
/// </summary>
public class HastyFloat : FloatSetting, IHastySetting
{
	private readonly HastySetting _config = null!;
	private readonly LocalizedString _displayName = null!;
	private readonly FloatOptions _options = default;

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
		{ throw new ArgumentNullException(nameof(config), "No config was provided. Unable to create \"HastyFloat\"."); }
		if (string.IsNullOrEmpty(name))
		{ throw new ArgumentNullException(nameof(name), $"{config.AsmPFX} Name cannot be null or empty."); }
		if (string.IsNullOrEmpty(description))
		{ UnityEngine.Debug.LogWarning($"{config.AsmPFX} No description was given to: \"{name}\" of type: HastyFloat. This may cause errors."); }

		_config = config;
		_options = options;
		_displayName = config.CreateDisplayName(name, description);

		Key = $"{config.ModName}.{name}";
		UUID = Guid.NewGuid().ToString();

		_config.Add(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyFloat"/> class as a child of a collapsible group.
	/// </summary>
	/// <param name="collapsible">The parent collapsible group.</param>
	/// <param name="name">The name of the setting.</param>
	/// <param name="description">The description of the setting.</param>
	/// <param name="options">Additional options for the setting.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="collapsible"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
	public HastyFloat(HastyCollapsible collapsible, string name, string description, FloatOptions options = default)
		: this(collapsible._config, name, description, options)
	{
		ParentCollapsible = collapsible;
		collapsible.Content.Add(this);
	}

	/// <inheritdoc/>
	public HastyData HastyData { get; set; } = null!;

	/// <summary>
	/// Gets a value indicating whether this setting should use whole numbers only.
	/// </summary>
	public bool IsWhole { get => _options.IsWhole; }

	/// <inheritdoc/>
	public string Key { get; } = string.Empty;

	/// <inheritdoc/>
	public HastyCollapsible ParentCollapsible { get; set; } = null!;

	/// <inheritdoc/>
	public string UUID { get; set; } = string.Empty;

	/// <summary>
	/// Gets the TMP_InputField used to display the value.
	/// </summary>
	private TMP_InputField? ValueText
	{
		get
		{
			if (_valueText == null)
			{
				if (HastyData == null)
				{ throw new NullReferenceException("HastyData is not set. Cannot access ValueText."); }
				if (HastyData.SettingsUICell == null)
				{ throw new NullReferenceException("SettingsUICell is not set in HastyData. Cannot access ValueText."); }
				_valueText = HastyData.SettingsUICell.transform.Find("InputParent/FLOAT INPUT(Clone)/InputField (TMP)")?.GetComponent<TMP_InputField>() ?? null!;
			}
			return _valueText;
		}
	}

	/// <summary>
	/// Applies the current value and invokes the <see cref="FloatOptions.OnApplied"/> action if set.
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
	/// Loads the value from the provided loader, or uses the default if not found. Invokes the <see cref="FloatOptions.OnLoad"/> action if set.
	/// </summary>
	/// <param name="loader">The settings loader.</param>
	public override void Load(ISettingsSaveLoad loader)
	{
		_saveLoad = loader;

		Value = (loader.TryLoadFloat(Key, out float value) ? value : GetDefaultValue());
		MinValue = GetMinMaxValue().x;
		MaxValue = GetMinMaxValue().y;

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
			ValueText.text = Value.ToString("0.00");
		}

		Save(_saveLoad);
	}

	/// <summary>
	/// Saves the current value using the provided saver.
	/// </summary>
	/// <param name="saver">The settings saver.</param>
	public override void Save(ISettingsSaveLoad saver)
		=> saver.SaveFloat(Key, Value);

	/// <summary>
	/// Gets the default value for this setting.
	/// </summary>
	/// <returns>The default value.</returns>
	protected override float GetDefaultValue()
		=> _options.DefaultValue;

	/// <summary>
	/// Gets the minimum and maximum allowed values for this setting.
	/// </summary>
	/// <returns>A <see cref="float2"/> containing the minimum and maximum values.</returns>
	protected override float2 GetMinMaxValue()
		=> _options.MinMax;
}
