public interface IHastySetting : IExposedSetting
{
	/// <summary>
	/// Stores data about the type of setting
	/// </summary>
	HastyData HastyData { get; set; }

	/// <summary>
	/// A unique key for loading the setting, typically a combination of mod name and setting name
	/// </summary>
	string Key { get; }

	/// <summary>
	/// If the setting is a child of a collapsible group, this property holds a reference to the parent collapsible
	/// </summary>
	HastyCollapsible ParentCollapsible { get; set; }

	/// <summary>
	/// A unique UUID for comparason. And yeah, I could use the Key instead, but idk I prefer this I guess
	/// </summary>
	string UUID { get; }

	/// <summary>
	/// Resets the setting to its default value.
	/// </summary>
	void Reset();
}
