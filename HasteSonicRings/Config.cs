namespace HasteSonicRings;

public class Config
{
	public static HastyFloat RingSpinSpeed = null!;

	public static HastyFloat ShaderDistStart = null!;

	public static HastyFloat ShaderMax = null!;

	public static HastyFloat ShaderMin = null!;

	public static HastyFloat ShaderRes = null!;

	public Config(HastySetting cfg)
	{
		// Settings
		RingSpinSpeed = new(cfg, "Ring Spin Speed", "Changes how fast the rings spin", new(1, 10, 5));
		ShaderDistStart = new(cfg, "Shader Dist Start", "Changes how close you need to be for the shader to finish applying", new(1, 100, 20));
		ShaderMax = new(cfg, "Shader Max Strength", "Changes how much the shader applies.", new(0, 1, 0.7f));
		ShaderMin = new(cfg, "Shader Min Strength", "Changes how little the shader applies.", new()
		{
			MinMax = new(0, 1),
			DefaultValue = 0.3f,

			// When the min value is set, we update the max slider to (min + 0.1)
			OnApplied = (float v) => { ShaderMax.Options.MinMax = new(v + 0.1f, ShaderMax.Options.MinMax.y); }
		});
		ShaderRes = new(cfg, "Shader Resolution", "Changes the resolution on the shader, creaing more of a pixelated effect.", new(0, 1080, 25));
	}
}
