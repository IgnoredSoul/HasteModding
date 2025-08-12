using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static PostGameScreenExtras.Ext;

namespace PostGameScreenExtras;

internal class StatsHolder
{
	internal static GameObject PostStats_Prefab = null!;
	internal GameObject Edge_Prefab = null!;
	internal List<GameObject> extraStats = null!;
	internal GameObject Stat_Prefab = null!;
	internal GameObject Stats_Prefab = null!;
	internal GameObject Title_Prefab = null!;

	private TextMeshProUGUI TitleText = null!;

	/// <summary>
	/// This constructor should only be used when the post game stats screen is active and is showing the stats, not the sparks or item.
	/// </summary>
	public StatsHolder()
	{
	}

	private void CreateNewListing()
	{
		// Get the post game screen prefab
		PostStats_Prefab ??= GameObject.FindFirstObjectByType<PostGameScreen>().gameObject;

		// Get the edge background
		Edge_Prefab = PostStats_Prefab.transform.GetChild(3).gameObject;

		// Get the title prefab
		Title_Prefab = PostStats_Prefab.transform.GetChild(4).gameObject;

		// Get the statsholder prefab
		Stats_Prefab = PostStats_Prefab.transform.GetChild(5).gameObject;

		// Get the stat prefab
		Stat_Prefab = Stats_Prefab.transform.GetChild(0).GetChild(1).gameObject;

		// Create new edge, offset it to the left and set it's scale
		GameObject landingsEdge = GameObject.Instantiate(Edge_Prefab, Edge_Prefab.transform.parent);
		landingsEdge.SetActive(false);
		landingsEdge.name = "landingsEdge";
		landingsEdge.transform.localScale = new(1, 0.15f, 1);

		// Create new title
		GameObject landingsTitle = GameObject.Instantiate(Title_Prefab, Title_Prefab.transform.parent);
		landingsTitle.SetActive(false);
		landingsTitle.name = "landingsTitle";
		TitleText = landingsTitle.GetComponent<TextMeshProUGUI>();

		// Create new stats, offset it to the left and remove every child stat except the Perfect and Bad landings
		GameObject stats = GameObject.Instantiate(Stats_Prefab, PostStats_Prefab.transform);
		stats.SetActive(false);
		stats.name = "landingsStats";
		stats.transform.localPosition = new(-565, 0, 0);
		stats.transform.GetChild(0).DestroyChildren(name => name == "UI_UStatRow_8" || name == "UI_UStatRow_9");
		stats.GetComponentInChildren<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
	}
}
