using System.Diagnostics;
using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace HasteSonicRings;

[Landfall.Modding.LandfallPlugin]
public class HasteSonicRings
{
	private static AssetBundle assetBundle = null!;

	private static GameObject ringObject = null!;

	static HasteSonicRings()
	{
		// Create new config menu
		HastySetting cfg = new HastySetting("SonicRings");

		// Register menu creation for our config
		cfg.OnConfig += () => new Config(cfg);

		// Load the asset bundle and the required asset. If it cannot load either the bundle or the asset, we throw
		if (!LoadBundle(out (string, StackTrace) error))
		{ error.Item2.InformTrace(error.Item1, InformType.Error); return; }

		// Register to scene changed event
		SceneManager.activeSceneChanged += OnSceneChanged;
	}

	private static void CreateNPC()
	{
		// Create a new ring that does fucking nothing
		GameObject ring = GameObject.Instantiate(ringObject);

		// We add the same ring behaviour for a "visual representation"
		ring.AddComponent<RingBehaviour>();

		// Set it's position to near the other npcs
		ring.transform.position = new Vector3(6f, 45f, 397f);

		// Create a new NPC at the rings position, marker at the marker position, name the name of the interaction
		NPC npc = new(ring.transform, new Vector3(0, 2, 0), "RingNPC");

		// Create new dialog for the interaction
		using (new DialogBuilder(npc)
		{
			{ Characters.Captain, "This\nIs a ring!"},
			{ Characters.Courier, "...Okay?"}
		}) { }
	}

	private static bool LoadBundle(out (string, StackTrace) error)
	{
		// Just in case we run into an error, let's track it's error message and record a trace.
		StackTrace trace = new(true);
		error = ("", trace);

		try
		{
			// If the bundle has already loaded, we just skip ig
			if (assetBundle) return true;

			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HasteSonicRings.Assets.sonicrings"))
			{
				if (stream == null)
				{
					error.Item1 = "AssetBundle resource not found!";
					return false;
				}

				byte[] bundleData = new byte[stream.Length];
				stream.Read(bundleData, 0, bundleData.Length);

				// Load the AssetBundle from memory
				if ((assetBundle = AssetBundle.LoadFromMemory(bundleData)) == null)
				{
					error.Item1 = "Failed to load AssetBundle from memory.";
					return false;
				}
			}

			if (TryLoadAssetFromBundle("RingObj.prefab", out (GameObject, string) info))
			{
				ringObject = info.Item1;
				return true;
			}
			else
			{
				error.Item1 = info.Item2;
				return false;
			}
		}
		catch (System.Exception e) { error.Item1 = e.ToString(); }
		return false;
	}

	// This needs to be redone, it only works if the player spawns into the hub first.
	// So if the player is already in a run (loaded from save(?)), then it wont work.
	private static void OnSceneChanged(Scene old, Scene newScene)
	{
		if (newScene.name != "FullHub") return;
		foreach (Spark obj in Resources.FindObjectsOfTypeAll<Spark>()) // There is only one so it wont get looped again. Unless game updates. Then uh, oops?
		{
			obj.transform.Find("Anim/UpDown/Cylinder").GetComponent<MeshFilter>().mesh = ringObject.GetComponentInChildren<MeshFilter>().mesh;
			obj.transform.Find("Anim/UpDown/Cylinder").GetComponent<MeshRenderer>().material = ringObject.GetComponentInChildren<MeshRenderer>().material;
			obj.gameObject.AddComponent<RingBehaviour>();

			obj.GetComponent<PerformantRotate>().enabled = false;
			obj.transform.Find("Anim/UpDown/Cylinder").GetComponent<PerformantRotate>().enabled = false;

			UnityEngine.Object.Destroy(obj.transform.GetChild(1).GetComponent<CoinCodeAnimationBurst>());

			CreateNPC();
			break;
		}
	}

	private static bool TryLoadAssetFromBundle<T>(string assetName, out (T, string) asset) where T : UnityEngine.Object
	{
		asset = (null!, null!);

		if (assetBundle == null)
		{
			asset = (null!, "AssetBundle is not loaded.");
			return false;
		}

		asset.Item1 = assetBundle.LoadAsset<T>(assetName);

		if (asset.Item1 == null)
		{
			asset = (null!, $"Asset '{assetName}' of type '{typeof(T)}' not found in bundle.");
			return false;
		}

		return true;
	}
}
