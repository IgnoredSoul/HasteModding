public enum InformType
{
	Info,
	Warn,
	Error,
}

/// <summary>
/// Just a silly little library for better-ish logging ig.
/// Not much error handling and shit. Oh well :3
/// </summary>
public static class Informer
{
	private static Zorro.Core.CLI.DebugUIHandler debugUIHandler = null!;
	private static readonly string ASM = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

    /// <summary>
    /// Prints a message with a log level
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="lvl"></param>
    public static void Inform(object msg, InformType lvl = InformType.Warn)
	{
		debugUIHandler ??= UnityEngine.Resources.FindObjectsOfTypeAll<Zorro.Core.CLI.DebugUIHandler>().FirstOrDefault(c => c.gameObject.name == "Console(Clone)");
		msg = $"[{ASM}]: {msg}";
		switch (lvl)
		{
			case InformType.Error:
				debugUIHandler?.AddLog(msg.ToString(), "", UnityEngine.LogType.Error, true);
				break;

			case InformType.Warn:
				debugUIHandler?.AddLog(msg.ToString(), "", UnityEngine.LogType.Warning, true);
				break;

			case InformType.Info:
			default:
				debugUIHandler?.AddLog(msg.ToString(), "", UnityEngine.LogType.Log, true);
				break;
		}
	}

	/// <summary>
	/// Prints a semi-psudo trace and an info message with a log level
	/// </summary>
	/// <param name="trace"></param>
	/// <param name="msg"></param>
	/// <param name="lvl"></param>
    public static void Inform(this object trace, object msg, InformType lvl = InformType.Warn)
	{
		debugUIHandler ??= UnityEngine.Resources.FindObjectsOfTypeAll<Zorro.Core.CLI.DebugUIHandler>().FirstOrDefault(c => c.gameObject.name == "Console(Clone)");
		msg = $"[{ASM}]: {msg}";
		switch (lvl)
		{
			case InformType.Error:
				debugUIHandler?.AddLog(msg.ToString(), trace.ToString(), UnityEngine.LogType.Error, true);
				break;

			case InformType.Warn:
				debugUIHandler?.AddLog(msg.ToString(), trace.ToString(), UnityEngine.LogType.Warning, true);
				break;

			case InformType.Info:
			default:
				debugUIHandler?.AddLog(msg.ToString(), trace.ToString(), UnityEngine.LogType.Log, true);
				break;
		}
	}

	/// <summary>
	/// Prints a <seealso cref="System.Diagnostics.StackTrace"/> and an info message with a log level
	/// </summary>
	/// <param name="trace"></param>
	/// <param name="msg"></param>
	/// <param name="lvl"></param>
	public static void Inform(this System.Diagnostics.StackTrace trace, object msg, InformType lvl = InformType.Warn)
	{
		debugUIHandler ??= UnityEngine.Resources.FindObjectsOfTypeAll<Zorro.Core.CLI.DebugUIHandler>().FirstOrDefault(static c => c.gameObject.name == "Console(Clone)");
		msg = $"[{ASM}]: {msg}";
		switch (lvl)
		{
			case InformType.Error:
				debugUIHandler?.AddLog(msg.ToString(), trace.ToString(), UnityEngine.LogType.Error, true);
				break;

			case InformType.Warn:
				debugUIHandler?.AddLog(msg.ToString(), trace.ToString(), UnityEngine.LogType.Warning, true);
				break;

			case InformType.Info:
			default:
				debugUIHandler?.AddLog(msg.ToString(), trace.ToString(), UnityEngine.LogType.Log, true);
				break;
		}
	}

	/// <summary>
	/// Prints a <seealso cref="Exception"/> and an info message with a log level
	/// </summary>
	/// <param name="trace"></param>
	/// <param name="msg"></param>
	/// <param name="lvl"></param>
	public static void Inform(this Exception trace, object msg = null!, InformType lvl = InformType.Warn)
	{
		debugUIHandler ??= UnityEngine.Resources.FindObjectsOfTypeAll<Zorro.Core.CLI.DebugUIHandler>().FirstOrDefault(static c => c.gameObject.name == "Console(Clone)");
		msg = $"[{ASM}]: {msg}";
		switch (lvl)
		{
			case InformType.Error:
				debugUIHandler?.AddLog(msg.ToString(), trace.StackTrace.ToString(), UnityEngine.LogType.Error, true);
				break;

			case InformType.Warn:
				debugUIHandler?.AddLog(msg.ToString(), trace.StackTrace.ToString(), UnityEngine.LogType.Warning, true);
				break;

			case InformType.Info:
			default:
				debugUIHandler?.AddLog(msg.ToString(), trace.StackTrace.ToString(), UnityEngine.LogType.Log, true);
				break;
		}
	}
}
