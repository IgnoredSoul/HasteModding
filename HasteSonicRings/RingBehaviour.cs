using UnityEngine;

namespace HasteSonicRings;

public class RingBehaviour : MonoBehaviour
{
	private float dist_start { get => Config.ShaderDistStart.Value; }
	private Material mat { get; set; } = null!;
	private float maxStrength { get => Config.ShaderMax.Value; }
	private float minStrength { get => Config.ShaderMin.Value; }
	private float spinSpeed { get => Config.RingSpinSpeed.Value; }

	private void Start()
	{
		mat = GetComponentInChildren<MeshRenderer>().material;
		transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(45, 270), 0);
	}

	private void Update()
	{
		float dist = Vector3.Distance(transform.position, Camera.main.transform.position);

		if (mat.HasProperty("_SnapStrength"))
		{
			// Set it to it's minimum value
			float snapStrength = minStrength;

			// When the player distance is getting reducing aka getting closer to the ring
			if (dist <= dist_start)
			{
				// Normalize distance: 0 when far, 1 when close
				float t = Mathf.InverseLerp(dist_start, 10, dist);

				// Lerp from minStrength (far) to maxStrength (close)
				snapStrength = Mathf.Lerp(minStrength, maxStrength, t);
			}

			// Set the shader float value to the strength via the material
			mat.SetFloat("_SnapStrength", snapStrength);
		}

		// Change the pixelation resolution based of the config value
		mat.SetFloat("_PixelResolution", Config.ShaderRes.Value);

		// Rotote the ring (20 units * (spin speed * frame time)) on it's Y axis
		// Time.deltaTime = seconds between the last frame and the current frame
		transform.Rotate(0f, (20 * spinSpeed) * Time.deltaTime, 0f, Space.World);
	}
}
