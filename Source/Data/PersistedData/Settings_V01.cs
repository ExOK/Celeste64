using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Celeste64;

public sealed class Settings_V01 : PersistedData
{
	public override int Version => 1;

	public static Settings_V01 Instance = new();

	/// <summary>
	/// If Fullscreen should be enabled
	/// </summary>
	public bool Fullscreen { get; set; } = true;

	/// <summary>
	/// If the Vertical Z Guide should be drawn below the Player
	/// </summary>
	public bool ZGuide { get; set; } = true;

	/// <summary>
	/// If the Speedrun Timer should be visible while playing
	/// </summary>
	public bool SpeedrunTimer { get; set; } = false;

	/// <summary>
	/// 0-10 Music volume level
	/// </summary>
	public int MusicVolume { get; set; } = 10;

	/// <summary>
	/// 0-10 Sfx Volume level
	/// </summary>
	public int SfxVolume { get; set; } = 10;

	/// <summary>
	/// Invert the camera in given directions
	/// </summary>
	public InvertCameraOptions InvertCamera { get; set; } = InvertCameraOptions.None;

	/// <summary>
	/// Current Language ID
	/// </summary>
	public string Language { get; set; } = "english";


	/// <summary>
	/// Fuji Custom - Whether we should write to the log file or not.
	/// </summary>
	public bool WriteLog { get; set; } = true;

	/// <summary>
	/// Fuji Custom - Whether to enable additional logs
	/// </summary>
	public bool EnableAdditionalLogging { get; set; } = false;

	/// <summary>
	/// Fuji Custom - Whether The debug menu should be enabled
	/// </summary>
	public bool EnableDebugMenu { get; set; } = false;

	/// <summary>
	/// Fuji Custom - The Current Game Resolution Scale.
	/// </summary>
	public int ResolutionScale { get; set; } = 1;
	
	/// <summary>
	/// Fuji Custom - Whether the QuickStart feature is enabled
	/// </summary>
	public bool EnableQuickStart { get; set; } = true;


	public override JsonTypeInfo GetTypeInfo()
	{
		return Settings_V01Context.Default.Settings_V01;
	}
}

[JsonSourceGenerationOptions(
	WriteIndented = true,
	AllowTrailingCommas = true,
	UseStringEnumConverter = true
)]
[JsonSerializable(typeof(Settings_V01))]
internal partial class Settings_V01Context : JsonSerializerContext { }
