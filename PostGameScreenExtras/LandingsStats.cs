using UnityEngine;

namespace PostGameScreenExtras;

internal static class LandingsStats
{
	internal static GameObject Edge_Prefab = null!;
	internal static List<GameObject> extraStats = null!;
	internal static List<float> LandingScores = new List<float>();
	internal static GameObject Stats_Prefab = null!;
	internal static GameObject Title_Prefab = null!;

	// Creates a new sub stats for displaying the players landing types and their averages.
	private static List<GameObject> CreateLandingStats(PostGameScreen instance)
	{
		// Create new edge, offset it to the left and set it's scale
		GameObject landingsEdge = GameObject.Instantiate(Edge_Prefab, Edge_Prefab.transform.parent);
		landingsEdge.SetActive(false);
		landingsEdge.name = "landingsEdge";
		landingsEdge.transform.localScale = new(1, 0.41f, 1);
		landingsEdge.transform.localPosition = new(-565, 230, 0);

		// Create new title
		GameObject landingsTitle = GameObject.Instantiate(Title_Prefab, Title_Prefab.transform.parent);
		landingsTitle.SetActive(false);
		landingsTitle.name = "landingsTitle";
		landingsTitle.transform.localPosition = new(-565, 300, 0);
		landingsTitle.GetComponent<Zorro.Localization.LocalizeUIText>().String = new Landfall.Haste.UnlocalizedString($"Landings " + ILPatching.LandingScores?.Average().ToString("0.00") + "%");

		// Create new stats, offset it to the left and remove every child stat except the Perfect and Bad landings
		GameObject stats = GameObject.Instantiate(Stats_Prefab, instance.transform);
		stats.SetActive(false);
		stats.name = "landingsStats";
		stats.transform.localPosition = new(-565, 0, 0);
		_destroy(stats.transform.GetChild(0), name => name == "UI_UStatRow_8" || name == "UI_UStatRow_9");

		// Configure the stats object
		stats.GetComponentInChildren<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
		GameObject statsHolder_prefab = Stats_Prefab.transform.GetChild(0).GetChild(9).gameObject; // Copies the original stats object cause for some reason it just fucks up whe copying my stats objects???
		UnityEngine.Object.DestroyImmediate(stats.transform.GetChild(1).gameObject); // Removes the fucking continue button

		// Create Good landing
		GameObject landingsGood = GameObject.Instantiate(statsHolder_prefab, stats.transform.GetChild(0).transform);
		landingsGood.SetActive(false);
		landingsGood.name = "landingsGood";
		PostGameStat goodPGS = landingsGood.GetComponent<PostGameStat>();
		goodPGS.statType = HasteStatType.STAT_GOOD_LANDINGS;

		// Create Ok landing
		GameObject landingsOk = GameObject.Instantiate(statsHolder_prefab, stats.transform.GetChild(0).transform);
		landingsOk.SetActive(false);
		landingsOk.name = "landingsOk";
		PostGameStat okPGS = landingsOk.GetComponent<PostGameStat>();
		okPGS.statType = HasteStatType.STAT_OK_LANDINGS;

		// Set bad landings to last
		stats.transform.GetChild(0).GetChild(1).SetAsLastSibling();

		return new() { landingsEdge, landingsTitle, stats, landingsGood, landingsOk };
	}
}
