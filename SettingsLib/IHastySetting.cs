/// <summary>
/// Defines the contract for a Hasty setting, providing common properties and methods for all Hasty settings.
/// </summary>
public interface IHastySetting : IExposedSetting
{
	/// <summary>
	/// Gets or sets the <see cref="HastyData"/> associated with this setting. <br/>
	/// This object contains UI references and metadata for the setting.
	/// </summary>
	HastyData HastyData { get; set; }

	/// <summary>
	/// Gets the unique key that identifies this setting within the mod configuration.
	/// </summary>
	string Key { get; }

	/// <summary>
	/// Gets or sets the parent <see cref="HastyCollapsible"/> group that contains this setting, if any.
	/// This is <c>null</c> if the setting is not part of a collapsible group.
	/// </summary>
	HastyCollapsible ParentCollapsible { get; set; }

	/// <summary>
	/// Gets the unique identifier (UUID) for this setting instance.
	/// </summary>
	string UUID { get; }

	/// <summary>
	/// Resets the setting to its default value.
	/// Implementations should ensure that the value is restored and any relevant events are triggered.
	/// </summary>
	void Reset();
}
