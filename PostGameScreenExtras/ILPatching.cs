using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace PostGameScreenExtras;

public static class ILPatching
{
	public static event Action<PostGameScreen> OnRunEnded = null!;

	public static event Action OnShowExtraStats = null!;

	internal static void PGS_Patch()
	{
		// When the PostGameScreen MonoBehaviour calls the method "Start"
		MethodInfo startMethod = typeof(PostGameScreen).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
		if (startMethod == null) throw new Exception("Couldn't find Start method");

		// When the PostGameScreen calls the method "Continue"
		MethodInfo continueField = typeof(PostGameScreen).GetMethod("Continue", BindingFlags.Public | BindingFlags.Instance);
		if (continueField == null) throw new Exception("Couldn't find continueField backing method");

		// Get the ILPatching method of "OnRunEnded"
		FieldInfo runEndedField = typeof(ILPatching).GetField("OnRunEnded", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
		if (runEndedField == null) throw new Exception("Couldn't find runEndedField");

		// Get the ILPatching method of "ShowExtraStats"
		FieldInfo showStatsField = typeof(ILPatching).GetField("OnShowExtraStats", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
		if (showStatsField == null) throw new Exception("Couldn't find showStatsField");

		new ILHook(startMethod, il =>
		{
			ILCursor c = new(il);

			c.Goto(0);

			// Load the delegate
			c.Emit(OpCodes.Ldsfld, runEndedField); // or Ldarg_0 + Ldfld for instance field

			// Duplicate and null-check
			ILLabel notNull = c.DefineLabel();
			c.Emit(OpCodes.Dup);
			c.Emit(OpCodes.Brtrue_S, notNull);
			c.Emit(OpCodes.Pop);
			c.Emit(OpCodes.Br, c.MarkLabel());

			// Invoke the delegate
			c.MarkLabel(notNull);
			c.Emit(OpCodes.Callvirt, typeof(Action).GetMethod("Invoke"));
		});

		new ILHook(continueField, il =>
		{
			ILCursor c = new(il);

			if (!c.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt<PostGameScreen>("SetActiveIndex")))
			{ throw new Exception("Failed to find call to SetActiveIndex"); }

			c.Emit(OpCodes.Ldsfld, showStatsField); // load the delegate
			c.Emit(OpCodes.Callvirt, typeof(Action).GetMethod("Invoke")); // no args
		});
	}
}
