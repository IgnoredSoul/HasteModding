using UnityEngine;

namespace PostGameScreenExtras;

internal static class Ext
{
	internal static void DestroyChildren(this Transform parent, Func<string, bool> shouldKeep)
	{
		for (int i = parent.childCount - 1; i >= 0; i--)
		{
			Transform child = parent.GetChild(i);
			if (!shouldKeep(child.name))
			{
				GameObject.DestroyImmediate(child.gameObject);
			}
		}
	}
}
