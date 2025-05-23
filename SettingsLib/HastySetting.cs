using HarmonyLib;
using Landfall.Haste;
using System.Diagnostics;
using System.Reflection;
using UnityEngine.Localization;
using Zorro.Core.CLI;
using Zorro.Settings;

/// <summary>
/// Provides a mod-specific settings handler for registering and managing custom settings.
/// </summary>
public class HastySetting
{
	/// <summary>
	/// Initializes a new instance of the <see cref="HastySetting"/> class.
	/// </summary>
	/// <param name="modName">The name of the mod using this settings handler.</param>
	/// <param name="logTraces">Whether to enable trace logging for this mod's settings.</param>
	/// <exception cref="ArgumentException">Thrown if <paramref name="modName"/> is null or empty.</exception>
	public HastySetting(string modName, bool logTraces = true)
	{
		if (string.IsNullOrEmpty(modName))
			throw new ArgumentException("Mod name cannot be null or empty.", nameof(modName));

		ModName = modName;

		// Patch all methods in HastyPatch for this mod's settings.
		new Harmony($"com.github.ignoredsoul.SettingsLib.{Guid.NewGuid().ToString()}").PatchAll(typeof(HastyPatch));
	}

	/// <summary>
	/// Occurs when the configuration is applied.
	/// </summary>
	public event Action OnConfig
	{
		add => HastyPatch.OnConfig += value;
		remove => HastyPatch.OnConfig -= value;
	}

	/// <summary>
	/// Represents the type of information for logging or informing the user.
	/// </summary>
	public enum InformType
	{
		/// <summary>
		/// Informational message.
		/// </summary>
		Info,

		/// <summary>
		/// Warning message.
		/// </summary>
		Warn,

		/// <summary>
		/// Error message.
		/// </summary>
		Error,
	}

	public static string LogName => $"[HastyLib ({Assembly.GetExecutingAssembly().GetName().Name})]: ";

	/// <summary>
	/// Gets the name of the mod associated with this settings handler.
	/// </summary>
	public string ModName { get; }

	/// <summary>
	/// Gets a reference to the list of settings managed by the settings handler.
	/// </summary>
	private static AccessTools.FieldRef<HasteSettingsHandler, List<Setting>> SettingsRef
		=> AccessTools.FieldRefAccess<HasteSettingsHandler, List<Setting>>("settings");

	/// <summary>
	/// Gets a reference to the settings save/load handler.
	/// </summary>
	private static AccessTools.FieldRef<HasteSettingsHandler, ISettingsSaveLoad> SettingsSaveLoadRef
		=> AccessTools.FieldRefAccess<HasteSettingsHandler, ISettingsSaveLoad>("_settingsSaveLoad");

	/// <summary>
	/// Gets or sets the debug UI handler for this settings instance.
	/// </summary>
	private DebugUIHandler DebugUIHandler { get; set; } = null!;

	/// <summary>
	/// Adds a new setting to the handler, loads its value, and applies it.
	/// </summary>
	/// <typeparam name="T">The type of the setting to add.</typeparam>
	/// <param name="setting">The setting instance to add.</param>
	public void Add<T>(T setting) where T : Setting
	{
		var handler = GameHandler.Instance.SettingsHandler;
		SettingsRef(handler).Add(setting);
		setting.Load(SettingsSaveLoadRef(handler));
		setting.ApplyValue();
	}

	/// <summary>
	/// Creates a localized display name for a setting, optionally including a description.
	/// </summary>
	/// <param name="name">The display name of the setting.</param>
	/// <param name="description">The description of the setting (optional).</param>
	/// <returns>
	/// A <see cref="LocalizedString"/> representing the display name and description.
	/// </returns>
	internal LocalizedString CreateDisplayName(string name, string description = "") =>
		new UnlocalizedString(
			string.IsNullOrEmpty(description)
				? name
				: $"{name}\n<size=60%><alpha=#50>{description}"
		);
}
