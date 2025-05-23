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

	public static void Inform(object msg, InformType typ = InformType.Warn)
	{
		debugUIHandler ??= UnityEngine.Resources.FindObjectsOfTypeAll<Zorro.Core.CLI.DebugUIHandler>().FirstOrDefault(static c => c.gameObject.name == "Console(Clone)");
		msg = $"[{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}] " + msg.ToString();
		switch (typ)
		{
			case InformType.Error:
				debugUIHandler.AddLog(msg.ToString(), "", UnityEngine.LogType.Error, true);
				break;

			case InformType.Warn:
				debugUIHandler.AddLog(msg.ToString(), "", UnityEngine.LogType.Warning, true);
				break;

			case InformType.Info:
			default:
				debugUIHandler.AddLog(msg.ToString(), "", UnityEngine.LogType.Log, true);
				break;
		}
	}

	public static void InformTrace(this object trace, object msg, InformType typ = InformType.Warn)
	{
		debugUIHandler ??= UnityEngine.Resources.FindObjectsOfTypeAll<Zorro.Core.CLI.DebugUIHandler>().FirstOrDefault(static c => c.gameObject.name == "Console(Clone)");
		msg = $"[{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}] " + msg.ToString();
		switch (typ)
		{
			case InformType.Error:
				debugUIHandler.AddLog(msg.ToString(), trace.ToString(), UnityEngine.LogType.Error, true);
				break;

			case InformType.Warn:
				debugUIHandler.AddLog(msg.ToString(), trace.ToString(), UnityEngine.LogType.Warning, true);
				break;

			case InformType.Info:
			default:
				debugUIHandler.AddLog(msg.ToString(), trace.ToString(), UnityEngine.LogType.Log, true);
				break;
		}
	}

	public static void InformTrace(this System.Diagnostics.StackTrace trace, object msg, InformType typ = InformType.Warn)
	{
		debugUIHandler ??= UnityEngine.Resources.FindObjectsOfTypeAll<Zorro.Core.CLI.DebugUIHandler>().FirstOrDefault(static c => c.gameObject.name == "Console(Clone)");
		msg = $"[{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}] " + msg.ToString();
		switch (typ)
		{
			case InformType.Error:
				debugUIHandler.AddLog(msg.ToString(), trace.ToString(), UnityEngine.LogType.Error, true);
				break;

			case InformType.Warn:
				debugUIHandler.AddLog(msg.ToString(), trace.ToString(), UnityEngine.LogType.Warning, true);
				break;

			case InformType.Info:
			default:
				debugUIHandler.AddLog(msg.ToString(), trace.ToString(), UnityEngine.LogType.Log, true);
				break;
		}
	}

	public static void InformTrace(this System.Exception trace, object msg, InformType typ = InformType.Warn)
	{
		debugUIHandler ??= UnityEngine.Resources.FindObjectsOfTypeAll<Zorro.Core.CLI.DebugUIHandler>().FirstOrDefault(static c => c.gameObject.name == "Console(Clone)");
		msg = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + msg.ToString();
		switch (typ)
		{
			case InformType.Error:
				debugUIHandler.AddLog(msg.ToString(), trace.StackTrace.ToString(), UnityEngine.LogType.Error, true);
				break;

			case InformType.Warn:
				debugUIHandler.AddLog(msg.ToString(), trace.StackTrace.ToString(), UnityEngine.LogType.Warning, true);
				break;

			case InformType.Info:
			default:
				debugUIHandler.AddLog(msg.ToString(), trace.StackTrace.ToString(), UnityEngine.LogType.Log, true);
				break;
		}
	}
}
