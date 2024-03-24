using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Celeste64;

public class Save_V02 : VersionedPersistedData<Save_V01>
{
	public override int Version => 2;

	public override JsonTypeInfo GetTypeInfo()
	{
		return Save_V02Context.Default.Save_V02;
	}

	public string FileName = "save.json";

	/// <summary>
	/// The last level that was entered
	/// </summary>
	public string LevelID { get; set; } = "NONE";

	/// <summary>
	/// Records for each level
	/// </summary>
	public List<LevelRecord_V01> Records { get; set; } = [];

	/// <summary>
	/// Fuji Custom - Currently equipped skin name
	/// </summary>
	public string SkinName { get; set; } = "Madeline";

	/// <summary>
	/// Fuji Custom - Records for each mod
	/// </summary>
	public List<ModRecord_V01> ModRecords { get; set; } = [];

	public void SetFileName(string file_name)
	{
		FileName = file_name;
	}

	public override object? UpgradeFrom(string data)
	{
		Save_V01 oldSave = new Save_V01().Deserialize(data) as Save_V01 ?? new Save_V01();
		Save_V02 newSave = new Save_V02();

		newSave.Records = oldSave.Records;
		newSave.ModRecords = oldSave.ModRecords;
		newSave.SkinName = oldSave.SkinName;
		newSave.LevelID = oldSave.LevelID;

		if (!File.Exists(Path.Join(App.UserPath, Settings.DefaultFileName)))
		{
			Settings_V01 newSettings = new Settings_V01();
			newSettings.SfxVolume = oldSave.SfxVolume;
			newSettings.MusicVolume = oldSave.MusicVolume;
			newSettings.EnableDebugMenu = oldSave.EnableDebugMenu;
			newSettings.Fullscreen = oldSave.Fullscreen;
			newSettings.InvertCamera = oldSave.InvertCamera;
			newSettings.Language = oldSave.Language;
			newSettings.SpeedrunTimer = oldSave.SpeedrunTimer;
			newSettings.WriteLog = oldSave.WriteLog;
			newSettings.ZGuide = oldSave.ZGuide;
			Settings.Instance = newSettings;
			Settings.SaveToFile();
		}

		return newSave;
	}
}

[JsonSourceGenerationOptions(
	WriteIndented = true,
	AllowTrailingCommas = true,
	UseStringEnumConverter = true,
	Converters = [typeof(LevelRecord_V01Converter), typeof(ModRecord_V01Converter)]
)]
[JsonSerializable(typeof(Save_V02))]
internal partial class Save_V02Context : JsonSerializerContext { }
