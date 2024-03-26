using System.Text.Json.Serialization;

namespace Celeste64;

public sealed class Save_V02 : VersionedPersistedData<Save_V01>
{
	public override int Version => 2;

	public string FileName = "save.json";

	/// <summary>
	/// The last level that was entered
	/// </summary>
	public string LevelID { get; set; } = "NONE";

	/// <summary>
	/// Fuji Custom - Currently equipped skin name
	/// </summary>
	public string SkinName { get; set; } = "Madeline";

	/// <summary>
	/// Records for each level
	/// </summary>
	public List<LevelRecord_V01> Records { get; set; } = [];

	/// <summary>
	/// Fuji Custom - Records for each mod
	/// </summary>
	public List<ModRecord_V02> ModRecords { get; set; } = [];

	public void SetFileName(string file_name)
	{
		FileName = file_name;
	}

	public override PersistedData? UpgradeFrom(Save_V01? oldSave)
	{
		if (oldSave == null) return null;

		Save_V02 newSave = new Save_V02();

		newSave.Records = oldSave.Records;
		newSave.LevelID = oldSave.LevelID;
		newSave.SkinName = oldSave.SkinName;

		ModSettings_V01? modSettings = null;

		if (!File.Exists(Path.Join(App.UserPath, Settings.DefaultFileName)))
		{
			modSettings = new ModSettings_V01();
		}

		foreach (var oldModRecord in oldSave.ModRecords)
		{
			if (new ModRecord_V02().UpgradeFrom(oldModRecord) is ModRecord_V02 newRecord)
			{
				newSave.ModRecords.Add(newRecord);
			}

			if (modSettings != null)
			{
				ModSettingsRecord_V01 newSettingsRecord = new ModSettingsRecord_V01();
				newSettingsRecord.ID = oldModRecord.ID;
				newSettingsRecord.Enabled = oldModRecord.Enabled;
				newSettingsRecord.SettingsStringData = oldModRecord.SettingsStringData;
				newSettingsRecord.SettingsIntData = oldModRecord.SettingsIntData;
				newSettingsRecord.SettingsFloatData = oldModRecord.SettingsFloatData;
				newSettingsRecord.SettingsBoolData = oldModRecord.SettingsBoolData;

				modSettings.ModSettingsRecords.Add(newSettingsRecord);
			}
		}

		if (modSettings != null)
		{
			ModSettings.Instance = modSettings;
			ModSettings.SaveToFile();
		}

		if (!File.Exists(Path.Join(App.UserPath, Settings.DefaultFileName)))
		{
			Settings_V01 newSettings = new Settings_V01();
			newSettings.SfxVolume = oldSave.SfxVolume;
			newSettings.MusicVolume = oldSave.MusicVolume;
			newSettings.EnableAdditionalLogging = oldSave.EnableAdditionalLogging;
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
