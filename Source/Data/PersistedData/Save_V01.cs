using System.Text.Json.Serialization;

namespace Celeste64;

public sealed class Save_V01 : PersistedData
{
	public override int Version => 1;

	public string FileName = "save.json";

	public static Save_V01 Instance = new();

	/// <summary>
	/// The last level that was entered
	/// </summary>
	public string LevelID { get; set; } = "NONE";

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
	/// Records for each level
	/// </summary>
	public List<LevelRecord_V01> Records { get; set; } = [];

	/// <summary>
	/// Fuji Custom - Currently equipped skin name
	/// </summary>
	public string SkinName { get; set; } = "Madeline";

	/// <summary>
	/// Fuji Custom - Whether we should write to the log file or not.
	/// </summary>
	public bool WriteLog { get; set; } = true;

	/// <summary>
	/// Fuji Custom - Whether The debug menu should be enabled
	/// </summary>
	public bool EnableDebugMenu { get; set; } = false;

	/// <summary>
	/// Fuji Custom - Whether to enable additional logs
	/// </summary>
	public bool EnableAdditionalLogging { get; set; } = false;

	/// <summary>
	/// Fuji Custom - Records for each mod
	/// </summary>
	public List<ModRecord_V01> ModRecords { get; set; } = [];
}

[JsonSourceGenerationOptions(
	WriteIndented = true,
	AllowTrailingCommas = true,
	UseStringEnumConverter = true,
	Converters = [typeof(LevelRecord_V01Converter), typeof(ModRecord_V01Converter)]
)]
[JsonSerializable(typeof(Save_V01))]
internal partial class Save_V01Context : JsonSerializerContext { }
