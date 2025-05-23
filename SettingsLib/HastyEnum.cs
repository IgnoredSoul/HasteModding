using TMPro;
using UnityEngine.Localization;
using Zorro.Settings;

/// <summary>
/// Encapsulates configuration options for an enum setting, including choices, default value, and event hooks.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
public struct EnumOptions<T> where T : unmanaged, Enum
{
	/// <summary>
	/// The list of choices to display for the enum. If null, all enum names are used.
	/// </summary>
	public IEnumerable<string>? Choices;

	/// <summary>
	/// The default value for the setting.
	/// </summary>
	public T DefaultValue;

	/// <summary>
	/// Action to invoke when the value is applied.
	/// </summary>
	public Action<T> OnApplied;

	/// <summary>
	/// Action to invoke when the value is loaded.
	/// </summary>
	public Action<T> OnLoad;

	/// <summary>
	/// Initializes a new instance of the <see cref="EnumOptions{T}"/> struct.
	/// </summary>
	/// <param name="defaultValue">Default value.</param>
	/// <param name="onApplied">Action to invoke when applied.</param>
	/// <param name="onLoad">Action to invoke when loaded.</param>
	/// <param name="choices">Choices to display. If null, all enum names are used.</param>
	public EnumOptions(
		T defaultValue,
		Action<T> onApplied = null!,
		Action<T> onLoad = null!,
		IEnumerable<string> choices = null!)
	{
		DefaultValue = defaultValue;
		OnApplied = onApplied;
		OnLoad = onLoad;
		Choices = choices;
	}
}

/// <summary>
/// Represents an enum setting with configuration, localization, and event support for MonoMod-based Unity mods.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
public class HastyEnum<T> : EnumSetting<T>, IEnumSetting, IHastySetting where T : unmanaged, Enum
{
	public EnumOptions<T> Options = default;
	private readonly List<string> _choices;
	private readonly HastySetting _config;
	private readonly LocalizedString _displayName;
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
			throw new ArgumentNullException(nameof(config), $"{HastySetting.LogName}No config was provided. Unable to create \"HastyEnum\".");
		if (string.IsNullOrEmpty(name))
			throw new ArgumentException($"{HastySetting.LogName}No name was given to the enum. Either it's empty or null.", nameof(name));
		if (string.IsNullOrEmpty(description))
			UnityEngine.Debug.LogWarning($"{HastySetting.LogName}No description was given to: \"{name}\" of type: HastyEnum. This may cause errors.");
		if (options.Choices == null)
			UnityEngine.Debug.LogWarning($"{HastySetting.LogName}Choices is null, will try to get a list of options from the given type: {typeof(T)}");

		_choices = Options.Choices != null ? [.. Options.Choices] : Enum.GetNames(typeof(T)).ToList();
		_config = config;
		_displayName = _config.CreateDisplayName(name, description);

		Key = $"{System.Reflection.Assembly.GetExecutingAssembly().FullName}.{_config.ModName}.{name}.{description}";
		Options = options;
		UUID = Guid.NewGuid().ToString();

		_config.Add(this);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyEnum{T}"/> class as a child of a collapsible group.
	/// </summary>
	/// <param name="config">The parent collapsible group.</param>
	/// <param name="name">The name of the setting.</param>
	/// <param name="description">The description of the setting.</param>
	/// <param name="options">Additional options for the setting.</param>
	public HastyEnum(HastyCollapsible config, string name, string description, EnumOptions<T> options = default)
		: this(config._config, name, description, options)
	{
		if (config == null)
			throw new ArgumentNullException(nameof(config), $"{HastySetting.LogName}No config was provided. Unable to create \"HastyEnum\".");
		if (string.IsNullOrEmpty(name))
			throw new ArgumentException($"{HastySetting.LogName}No name was given to the enum. Either it's empty or null.", nameof(name));
		if (string.IsNullOrEmpty(description))
			UnityEngine.Debug.LogWarning($"{HastySetting.LogName}No description was given to \"{name}\" of type: HastyEnum. This may cause errors.");

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
	/// Gets the TextMeshProUGUI used to display and edit the value.
	/// </summary>
	private TextMeshProUGUI ValueText
	{
		get => _valueText ??= HastyData.SettingsUICell.transform.Find("InputParent/ENUM DROPDOWN(Clone)/Dropdown/Label").GetComponent<TextMeshProUGUI>();
	}

	/// <summary>
	/// Applies the current value and invokes the <see cref="EnumOptions{T}.OnApplied"/> action if set.
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
	/// Gets the list of localized choices for this enum setting.
	/// </summary>
	/// <returns>Always returns <c>null</c> (unlocalized choices are used).</returns>
	public override List<LocalizedString> GetLocalizedChoices() => null!;

	/// <summary>
	/// Gets the list of unlocalized choices for this enum setting.
	/// </summary>
	/// <returns>The list of choices as strings.</returns>
	List<string> IEnumSetting.GetUnlocalizedChoices() => _choices;

	/// <summary>
	/// Loads the value from the provided loader, or uses the default if not found. Invokes the <see cref="EnumOptions{T}.OnLoad"/> action if set.
	/// </summary>
	/// <param name="loader">The settings loader.</param>
	public override void Load(ISettingsSaveLoad loader)
	{
		_saveLoad = loader;
		Value = loader.TryLoadString(Key, out var value) ? (T)Enum.Parse(typeof(T), value) : GetDefaultValue();
		Options.OnLoad?.Invoke(Value);
	}

	/// <summary>
	/// Resets the value to the default.
	/// </summary>
	public void Reset()
	{ Value = Options.DefaultValue; SetValueText(Value); Save(_saveLoad); }

	/// <summary>
	/// Saves the current value using the provided saver.
	/// </summary>
	/// <param name="saver">The settings saver.</param>
	public override void Save(ISettingsSaveLoad saver) => saver.SaveString(Key, Value.ToString());

	/// <summary>
	/// Sets the text of the value display to the specified text.
	/// </summary>
	public void SetValueText(object text) => ValueText.text = text.ToString();

	/// <summary>
	/// Gets the default value for this setting.
	/// </summary>
	/// <returns>The default value.</returns>
	protected override T GetDefaultValue() => Options.DefaultValue;
}
