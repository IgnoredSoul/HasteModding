using TMPro;
using UnityEngine.Localization;
using Zorro.Settings;

/// <summary>
/// Encapsulates configuration options for an enum setting, including choices, default value, and event hooks.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
public struct EnumOptions<T>(
		T defaultValue,
		Action<T> onApplied = null!,
		Action<T> onLoad = null!,
		IEnumerable<string> choices = null!) where T : unmanaged, Enum
{
	/// <summary>
	/// The list of choices to display for the enum. If null, all enum names are used.
	/// </summary>
	public IEnumerable<string>? Choices { get; set; } = choices;

	/// <summary>
	/// The default value for the setting.
	/// </summary>
	public T DefaultValue { get; set; } = defaultValue;

	/// <summary>
	/// Action to invoke when the value is applied.
	/// </summary>
	public Action<T> OnApplied { get; set; } = onApplied;

	/// <summary>
	/// Action to invoke when the value is loaded.
	/// </summary>
	public Action<T> OnLoad { get; set; } = onLoad;
}

public class HastyEnum<T> : EnumSetting<T>, IEnumSetting, IHastySetting where T : unmanaged, Enum
{
	private readonly List<string> _choices = null!;
	private readonly HastySetting _config = null!;
	private readonly LocalizedString _displayName = null!;
	private readonly EnumOptions<T> _options = default;

	private ISettingsSaveLoad _saveLoad = null!;
	private TextMeshProUGUI _valueText = null!;

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyEnum{T}"/> class.
	/// </summary>
	/// <param name="config">The parent configuration object.</param>
	/// <param name="name">The name of the setting.</param>
	/// <param name="description">The description of the setting.</param>
	/// <param name="options">Additional options for the setting.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
	public HastyEnum(HastySetting config, string name, string description, EnumOptions<T> options = default)
	{
		if (config == null)
		{ throw new ArgumentNullException(nameof(config), "No config was provided. Unable to create \"HastyEnum\"."); }
		if (string.IsNullOrEmpty(name))
		{ throw new ArgumentNullException(nameof(name), $"{config.AsmPFX} Name cannot be null or empty."); }
		if (string.IsNullOrEmpty(description))
		{ UnityEngine.Debug.LogWarning($"{config.AsmPFX} No description was given to: \"{name}\" of type: HastyEnum. This may cause errors."); }

		_choices = _options.Choices != null ? [.. _options.Choices] : Enum.GetNames(typeof(T)).ToList();
		_config = config;
		_displayName = _config.CreateDisplayName(name, description);
		_options = options;

		Key = $"{config.ModName}.{name}";
		UUID = Guid.NewGuid().ToString();

		_config.Add(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyEnum{T}"/> class as a child of a collapsible group.
	/// </summary>
	/// <param name="collapsible">The parent collapsible group.</param>
	/// <param name="name">The name of the setting.</param>
	/// <param name="description">The description of the setting.</param>
	/// <param name="options">Additional options for the setting.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="collapsible"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
	public HastyEnum(HastyCollapsible collapsible, string name, string description, EnumOptions<T> options = default)
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
	private TextMeshProUGUI? ValueText
	{
		get
		{
			if (_valueText == null)
			{
				if (HastyData == null)
				{ throw new NullReferenceException("HastyData is not set. Cannot access ValueText."); }
				if (HastyData.SettingsUICell == null)
				{ throw new NullReferenceException("SettingsUICell is not set in HastyData. Cannot access ValueText."); }
				_valueText = HastyData.SettingsUICell.transform.Find("InputParent/ENUM DROPDOWN(Clone)/Dropdown/Label")?.GetComponent<TextMeshProUGUI>() ?? null!;
			}
			return _valueText;
		}
	}

	/// <summary>
	/// Applies the current value and invokes the <see cref="EnumOptions{T}.OnApplied"/> action if set.
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
	/// Gets the list of localized choices for this enum setting.
	/// </summary>
	/// <returns>Always returns <c>null</c> (unlocalized choices are used).</returns>
	public override List<LocalizedString> GetLocalizedChoices()
		=> null!;

	/// <summary>
	/// Gets the list of unlocalized choices for this enum setting.
	/// </summary>
	/// <returns>The list of choices as strings.</returns>
	List<string> IEnumSetting.GetUnlocalizedChoices()
		=> _choices;

	/// <summary>
	/// Loads the value from the provided loader, or uses the default if not found. Invokes the <see cref="EnumOptions{T}.OnLoad"/> action if set.
	/// </summary>
	/// <param name="loader">The settings loader.</param>
	public override void Load(ISettingsSaveLoad loader)
	{
		_saveLoad = loader;

		Value = (loader.TryLoadString(Key, out string? value) ? (T)Enum.Parse(typeof(T), value) : GetDefaultValue());

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
			ValueText.text = _options.DefaultValue.ToString();
		}

		Save(_saveLoad);
	}

	/// <summary>
	/// Saves the current value using the provided saver.
	/// </summary>
	/// <param name="saver">The settings saver.</param>
	public override void Save(ISettingsSaveLoad saver)
	{
		saver.SaveString(Key, Value.ToString());
		_saveLoad ??= saver;
	}

	/// <summary>
	/// Gets the default value for this setting.
	/// </summary>
	/// <returns>The default value.</returns>
	protected override T GetDefaultValue()
		=> _options.DefaultValue;
}
