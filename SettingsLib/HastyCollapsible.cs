using UnityEngine.Localization;
using Zorro.Settings;

/// <summary>
/// Represents a collapsible group of Hasty settings, providing UI grouping and expand/collapse functionality.
/// </summary>
public class HastyCollapsible : ButtonSetting, IHastySetting, IExposedSetting
{
	/// <summary>
	/// The parent settings configuration for this collapsible group.
	/// </summary>
	public readonly HastySetting _config = null!;

	private readonly LocalizedString _displayName = null!;

	/// <summary>
	/// Initializes a new instance of the <see cref="HastyCollapsible"/> class.
	/// </summary>
	/// <param name="config">The parent configuration object.</param>
	/// <param name="name">The name of the collapsible group.</param>
	/// <param name="description">The description of the group.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty.</exception>
	public HastyCollapsible(HastySetting config, string name, string description)
	{
		if (config == null)
			throw new ArgumentNullException(nameof(config), $"{HastySetting.LogName}No config was provided. Unable to create \"HastyCollapsible\".");
		if (string.IsNullOrEmpty(name))
			throw new ArgumentException($"{HastySetting.LogName}No name was given to the collapsible group. Either it's empty or null.", nameof(name));
		if (string.IsNullOrEmpty(description))
			UnityEngine.Debug.LogWarning($"{HastySetting.LogName}No description was given to: \"{name}\" of type: HastyCollapsible. This may cause errors.");

		_config = config;
		Key = $"{System.Reflection.Assembly.GetExecutingAssembly().FullName}.{_config.ModName}.{name}.{description}";
		_displayName = _config.CreateDisplayName(name, description);
		_config.Add(this);

		UUID = Guid.NewGuid().ToString();
		Clicked = null!;
	}

	/// <summary>
	/// Occurs when the collapsible group is expanded or collapsed.
	/// The event argument indicates the new collapsed state.
	/// </summary>
	public event Action<bool> Clicked;

	/// <summary>
	/// Gets a value indicating whether the group is currently collapsed.
	/// </summary>
	public bool Collapsed { get; private set; } = true;

	/// <summary>
	/// Gets or sets the list of child settings contained in this group.
	/// </summary>
	public List<IHastySetting> Content { get; set; } = new List<IHastySetting>();

	/// <inheritdoc/>
	public HastyData HastyData { get; set; } = null!;

	/// <inheritdoc/>
	public string Key { get; } = string.Empty;

	/// <inheritdoc/>
	public HastyCollapsible ParentCollapsible { get; set; } = null!;

	/// <inheritdoc/>
	public string UUID { get; set; } = string.Empty;

	/// <summary>
	/// Gets the button text for this collapsible group.
	/// </summary>
	/// <returns>Always returns <c>null</c> (UI is handled elsewhere).</returns>
	public override string GetButtonText() => null!;

	/// <summary>
	/// Gets the category (mod name) for this collapsible group.
	/// </summary>
	/// <returns>The mod name.</returns>
	public string GetCategory() => _config.ModName;

	/// <summary>
	/// Gets the localized display name for this collapsible group.
	/// </summary>
	/// <returns>The localized display name.</returns>
	public LocalizedString GetDisplayName() => _displayName;

	/// <summary>
	/// Handles the click event for this collapsible group, toggling its collapsed state and raising the <see cref="Clicked"/> event.
	/// </summary>
	/// <param name="settingHandler">The setting handler (not used).</param>
	public override void OnClicked(ISettingHandler settingHandler) => Clicked?.Invoke(Collapsed = !Collapsed);

	/// <summary>
	/// Resets the group. (No operation for collapsible groups.)
	/// </summary>
	public void Reset()
	{ }
}
