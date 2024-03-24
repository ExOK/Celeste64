using Celeste64.Mod;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Celeste64;

public class Save_V01 : PersistedData
{
	public override int Version => 1;

	public override JsonTypeInfo GetTypeInfo()
	{
		return Save_V01Context.Default.Save_V01;
	}

	public string FileName = "save.json";

	public static Save_V01 Instance = new();

	/// <summary>
	/// Gets the Record for the current Level.
	/// </summary>
	public static LevelRecord_V01 CurrentRecord => Instance.GetOrMakeRecord(Instance.LevelID);

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
	/// Fuji Custom - Records for each mod
	/// </summary>
	public List<ModRecord_V01> ModRecords { get; set; } = [];

	/// <summary>
	/// Finds the record associated with a specific level, or adds it if not found
	/// </summary>
	public LevelRecord_V01 GetOrMakeRecord(string levelID)
	{
		if (TryGetRecord(levelID) is { } record)
			return record;

		record = new LevelRecord_V01() { ID = levelID };
		Records.Add(record);
		return record;
	}

	public void SetFileName(string file_name)
	{
		FileName = file_name;
	}

	/// <summary>
	/// Tries to get a Level Record, returns null if not found
	/// </summary>
	public LevelRecord_V01? TryGetRecord(string levelID)
	{
		foreach (var record in Records)
			if (record.ID == levelID)
				return record;
		return null;
	}

	/// <summary>
	/// Erases a Level Record
	/// </summary>
	public void EraseRecord(string levelID)
	{
		for (int i = 0; i < Records.Count; i++)
		{
			if (Records[i].ID == levelID)
			{
				Records.RemoveAt(i);
				break;
			}
		}
	}

	/// <summary>
	/// Finds the record associated with a specific mod, or adds it if not found
	/// </summary>
	public ModRecord_V01 GetOrMakeMod(string modID)
	{
		if (TryGetMod(modID) is { } record)
			return record;

		record = new ModRecord_V01() { ID = modID, Enabled = true };
		ModRecords.Add(record);
		return record;
	}

	/// <summary>
	/// Tries to get a Mod Record, returns null if not found
	/// </summary>
	public ModRecord_V01? TryGetMod(string modID)
	{
		foreach (var record in ModRecords)
			if (record.ID == modID)
				return record;
		return null;
	}

	/// <summary>
	/// Erases a Mod Record
	/// </summary>
	public void EraseModRecord(string modID)
	{
		for (int i = 0; i < Records.Count; i++)
		{
			if (ModRecords[i].ID == modID)
			{
				Records.RemoveAt(i);
				break;
			}
		}
	}

	public void ToggleFullscreen()
	{
		Fullscreen = !Fullscreen;
		SyncSettings();
	}

	public void ToggleWriteLog()
	{
		WriteLog = !WriteLog;
	}

	public void ToggleEnableDebugMenu()
	{
		EnableDebugMenu = !EnableDebugMenu;
	}

	public void ToggleZGuide()
	{
		ZGuide = !ZGuide;
	}

	public void SetCameraInverted(InvertCameraOptions value)
	{
		InvertCamera = value;
	}

	public void ToggleTimer()
	{
		SpeedrunTimer = !SpeedrunTimer;
	}

	public void SetMusicVolume(int value)
	{
		MusicVolume = Calc.Clamp(value, 0, 10);
		SyncSettings();
	}

	public void SetSfxVolume(int value)
	{
		SfxVolume = Calc.Clamp(value, 0, 10);
		SyncSettings();
	}

	public void SetSkinName(string skin)
	{
		SkinName = skin;
		SyncSettings();
	}

	public void SetLanguage(string language)
	{
		Language = language;
	}

	public SkinInfo GetSkin()
	{
		return Assets.EnabledSkins.FirstOrDefault(s => s.Name == SkinName) ??
			ModManager.Instance.VanillaGameMod?.Skins.FirstOrDefault() ??
			new SkinInfo
			{
				Name = "Madeline",
				Model = "player",
				HideHair = false,
				HairNormal = 0xdb2c00,
				HairNoDash = 0x6ec0ff,
				HairTwoDash = 0xfa91ff,
				HairRefillFlash = 0xffffff,
				HairFeather = 0xf2d450
			};
	}

	public void SyncSettings()
	{
		App.Fullscreen = Fullscreen;
		Audio.SetVCAVolume("vca:/music", Calc.Clamp(MusicVolume / 10.0f, 0, 1));
		Audio.SetVCAVolume("vca:/sfx", Calc.Clamp(SfxVolume / 10.0f, 0, 1));

		Audio.MusicGroup.setVolume(Calc.Clamp(MusicVolume / 10.0f, 0, 1));
		Audio.SoundEffectGroup.setVolume(Calc.Clamp(SfxVolume / 10.0f, 0, 1));
	}

	public void SaveToFile()
	{
		var savePath = Path.Join(App.UserPath, FileName);
		var tempPath = Path.Join(App.UserPath, FileName + ".backup");

		// first save to a temporary file
		{
			using var stream = File.Create(tempPath);
			Serialize(stream, this);
			stream.Flush();
		}

		// validate that the temp path worked, and overwrite existing if it did.
		if (File.Exists(tempPath) &&
			Deserialize(File.ReadAllText(tempPath)) != null)
		{
			File.Copy(tempPath, savePath, true);
		}
	}
}

[JsonSourceGenerationOptions(
	WriteIndented = true,
	AllowTrailingCommas = true,
	UseStringEnumConverter = true,
	Converters = [typeof(LevelRecord_V01Converter), typeof(ModRecord_V01Converter)]
)]
[JsonSerializable(typeof(Save_V01))]
internal partial class Save_V01Context : JsonSerializerContext { }
