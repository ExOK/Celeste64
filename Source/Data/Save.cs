
using Celeste64.Mod;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Celeste64;

public class Save
{
	public const string FileName = "save.json";

	public enum InvertCameraOptions
	{
		None,
		X,
		Y,
		Both
	}

	/// <summary>
	/// Stored data associated with a single level
	/// </summary>
	public class LevelRecord
	{
		public string ID { get; set; } = string.Empty;
		public string Checkpoint { get; set; } = string.Empty;
		public HashSet<string> Strawberries { get; set; } = [];
		public HashSet<string> CompletedSubMaps { get; set; } = [];
		public Dictionary<string, int> Flags { get; set; } = []; 
		public int Deaths { get; set; } = 0;
		public TimeSpan Time { get; set; } = new();

		public int GetFlag(string name, int defaultValue = 0) 
			=> Flags.TryGetValue(name, out int value) ? value : defaultValue;

		public int SetFlag(string name, int value = 1) 
			=> Flags[name] = value;

		public int IncFlag(string name) 
			=> Flags[name] = GetFlag(name) + 1;
	}

	/// <summary>
	/// Stored data associated with a mod
	/// </summary>
	public class ModRecord
	{
		public string ID { get; set; } = string.Empty;
		public bool Enabled { get; set; } = true;
		public Dictionary<string, string> Settings { get; set; } = [];
		public Dictionary<string, string> StringData { get; set; } = [];
		public Dictionary<string, int> IntData { get; set; } = [];
		public Dictionary<string, float> FloatData { get; set; } = [];
		public Dictionary<string, bool> BoolData { get; set; } = [];

		public string GetString(string name, string defaultValue = "")
			=> StringData.TryGetValue(name, out string? value) ? value : defaultValue;

		public string SetString(string name, string value = "")
			=> StringData[name] = value;

		public int GetInt(string name, int defaultValue = 0)
			=> IntData.TryGetValue(name, out int value) ? value : defaultValue;

		public int SetInt(string name, int value = 1)
			=> IntData[name] = value;

		public float GetFloat(string name, float defaultValue = 0)
			=> FloatData.TryGetValue(name, out float value) ? value : defaultValue;

		public float SetFloat(string name, float value = 1)
			=> FloatData[name] = value;

		public bool GetBool(string name, bool defaultValue = false)
			=> BoolData.TryGetValue(name, out bool value) ? value : defaultValue;

		public bool SetBool(string name, bool value = false)
			=> BoolData[name] = value;
	}

	public static Save Instance = new();

	/// <summary>
	/// Gets the Record for the current Level.
	/// </summary>
	public static LevelRecord CurrentRecord => Instance.GetOrMakeRecord(Instance.LevelID);

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
	public List<LevelRecord> Records { get; set; } = [];

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
	public List<ModRecord> ModRecords { get; set; } = [];

	/// <summary>
	/// Finds the record associated with a specific level, or adds it if not found
	/// </summary>
	public LevelRecord GetOrMakeRecord(string levelID)
	{
		if (TryGetRecord(levelID) is { } record)
			return record;

		record = new LevelRecord() { ID = levelID };
		Records.Add(record);
		return record;
	}

	/// <summary>
	/// Tries to get a Level Record, returns null if not found
	/// </summary>
	public LevelRecord? TryGetRecord(string levelID)
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
		for (int i = 0; i < Records.Count; i ++)
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
	public ModRecord GetOrMakeMod(string modID)
	{
		if (TryGetMod(modID) is { } record)
			return record;

		record = new ModRecord() { ID = modID, Enabled = true };
		ModRecords.Add(record);
		return record;
	}

	/// <summary>
	/// Tries to get a Mod Record, returns null if not found
	/// </summary>
	public ModRecord? TryGetMod(string modID)
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

		// validate that the temp path worked, and overwride existing if it did.
		if (File.Exists(tempPath) &&
			Deserialize(File.ReadAllText(tempPath)) != null)
		{
			File.Copy(tempPath, savePath, true);
		}
	}

	public static void Serialize(Stream stream, Save instance)
	{
		JsonSerializer.Serialize(stream, instance, SaveContext.Default.Save);
	}

	public static Save? Deserialize(string data)
	{
		try
		{
			return JsonSerializer.Deserialize(data, SaveContext.Default.Save);
		}
		catch (Exception e)
		{
			Log.Error(e.ToString());
			return null;
		}
	}
}

[JsonSourceGenerationOptions(WriteIndented = true, AllowTrailingCommas = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(Save))]
internal partial class SaveContext : JsonSerializerContext {}