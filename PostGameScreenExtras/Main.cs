using MonoMod.Cil;
using UnityEngine;
using Mono.Cecil.Cil;
using System.Reflection;
using MonoMod.RuntimeDetour;
using UnityEngine.UI;
using Zorro.Core.CLI;

namespace PostGameScreenExtras;

[Landfall.Modding.LandfallPlugin]
public class Main
{
	public static string GUID = "com.github.ignoredsoul.postgamescreenextras";
	public static string NAME = "PostGameScreenExtras";

	static Main()
	{
		UnityEngine.Debug.LogError("[PostGameScreenExtras]: " + DateTime.Now.ToString("hh:mm:ss:ff"));
		ILPatching.PGS_Patch();

		ILPatching.OnRunEnded += (PostGameScreen instance) =>
		{
			UnityEngine.Debug.LogError($"[PostGameScreenExtras]: OnRunEnded | {instance != null}");
		};
		ILPatching.OnShowExtraStats += () =>
		{
			UnityEngine.Debug.LogError($"[PostGameScreenExtras]: OnShowExtraStats");
		};

		On.PlayerMovement.GetLanding += (prev, movement, hit) =>
		{
			var res = prev.Invoke(movement, hit)!;
			float score = (float)res.GetType().GetField("landingScore").GetValue(res);
			ILPatching.LandingScores.Add(score);
			return res;
		};
	}

	[ConsoleCommand]
	public static void Print()
	{
		//UnityEngine.Debug.LogError(string.Join(", ", ILPatching.LandingScores));
	}
}

//public class ILPatching_
//{
//	internal static List<float> LandingScores = new List<float>();
//	private static GameObject Edge_Prefab = null!;
//	private static List<GameObject> extraStats = null!;
//	private static GameObject Stats_Prefab = null!;
//	private static GameObject Title_Prefab = null!;

//	private static void _destroy(Transform parent, Func<string, bool> shouldKeep)
//	{
//		for (int i = parent.childCount - 1; i >= 0; i--)
//		{
//			Transform child = parent.GetChild(i);
//			if (!shouldKeep(child.name))
//			{
//				GameObject.DestroyImmediate(child.gameObject);
//			}
//		}
//	}

//	private static void CleanOriginalStats(PostGameScreen instance)
//	{
//		// Destroy 8 & 9
//		_destroy(instance.transform.GetChild(0), name => name != "UI_UStatRow_8" && name != "UI_UStatRow_9");

//		// Set Edge's scale
//		Edge_Prefab.transform.localScale = new(1, 0.87f, 1);

//		// Set continue button fucks positions
//		Stats_Prefab.transform.GetChild(1).localPosition = new(0, -220, 0);
//	}

//	// Creates a new sub stats for displaying the players landing types and their averages.
//	private static List<GameObject> CreateLandingStats(PostGameScreen instance)
//	{
//		// Create new edge, offset it to the left and set it's scale
//		GameObject landingsEdge = GameObject.Instantiate(Edge_Prefab, Edge_Prefab.transform.parent);
//		landingsEdge.SetActive(false);
//		landingsEdge.name = "landingsEdge";
//		landingsEdge.transform.localScale = new(1, 0.41f, 1);
//		landingsEdge.transform.localPosition = new(-565, 230, 0);

//		// Create new title
//		GameObject landingsTitle = GameObject.Instantiate(Title_Prefab, Title_Prefab.transform.parent);
//		landingsTitle.SetActive(false);
//		landingsTitle.name = "landingsTitle";
//		landingsTitle.transform.localPosition = new(-565, 300, 0);
//		landingsTitle.GetComponent<Zorro.Localization.LocalizeUIText>().String = new Landfall.Haste.UnlocalizedString($"Landings " + ILPatching.LandingScores?.Average().ToString("0.00") + "%");

//		// Create new stats, offset it to the left and remove every child stat except the Perfect and Bad landings
//		GameObject stats = GameObject.Instantiate(Stats_Prefab, instance.transform);
//		stats.SetActive(false);
//		stats.name = "landingsStats";
//		stats.transform.localPosition = new(-565, 0, 0);
//		_destroy(stats.transform.GetChild(0), name => name == "UI_UStatRow_8" || name == "UI_UStatRow_9");

//		// Configure the stats object
//		stats.GetComponentInChildren<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
//		GameObject statsHolder_prefab = Stats_Prefab.transform.GetChild(0).GetChild(9).gameObject; // Copies the original stats object cause for some reason it just fucks up whe copying my stats objects???
//		UnityEngine.Object.DestroyImmediate(stats.transform.GetChild(1).gameObject); // Removes the fucking continue button

//		// Create Good landing
//		GameObject landingsGood = GameObject.Instantiate(statsHolder_prefab, stats.transform.GetChild(0).transform);
//		landingsGood.SetActive(false);
//		landingsGood.name = "landingsGood";
//		PostGameStat goodPGS = landingsGood.GetComponent<PostGameStat>();
//		goodPGS.statType = HasteStatType.STAT_GOOD_LANDINGS;

//		// Create Ok landing
//		GameObject landingsOk = GameObject.Instantiate(statsHolder_prefab, stats.transform.GetChild(0).transform);
//		landingsOk.SetActive(false);
//		landingsOk.name = "landingsOk";
//		PostGameStat okPGS = landingsOk.GetComponent<PostGameStat>();
//		okPGS.statType = HasteStatType.STAT_OK_LANDINGS;

//		// Set bad landings to last
//		stats.transform.GetChild(0).GetChild(1).SetAsLastSibling();

//		return new() { landingsEdge, landingsTitle, stats, landingsGood, landingsOk };
//	}

//	private static void OnRunEnded(PostGameScreen instance)
//	{
//		extraStats = new();

//		// Get the edge background
//		Edge_Prefab = instance.transform.GetChild(3).gameObject;

//		// Get the title prefab
//		Title_Prefab = instance.transform.GetChild(4).gameObject;

//		// Get the statsholder prefab
//		Stats_Prefab = instance.transform.GetChild(5).gameObject;

//		// Create landings stats
//		CreateLandingStats(instance).ForEach(obj => extraStats.Add(obj));

//		// Resize, change title & buttons
//	}

//	private static void ShowExtraStats()
//	{ extraStats?.ForEach(s => s.SetActive(true)); }
//}
