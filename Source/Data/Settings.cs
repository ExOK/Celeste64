using Celeste64.Mod;

namespace Celeste64;

public enum InvertCameraOptions
{
	None,
	X,
	Y,
	Both
}

public sealed class Settings
{
	public const string DefaultFileName = "settings.json";

	public static Settings_V01 Instance = new();

	/// <summary>
	/// If Fullscreen should be enabled
	/// </summary>
	public static bool Fullscreen => Instance.Fullscreen;

	/// <summary>
	/// If the Vertical Z Guide should be drawn below the Player
	/// </summary>
	public static bool ZGuide => Instance.ZGuide;

	/// <summary>
	/// If the Speedrun Timer should be visible while playing
	/// </summary>
	public static bool SpeedrunTimer => Instance.SpeedrunTimer;

	/// <summary>
	/// 0-10 Music volume level
	/// </summary>
	public static int MusicVolume => Instance.MusicVolume;

	/// <summary>
	/// 0-10 Sfx Volume level
	/// </summary>
	public static int SfxVolume => Instance.SfxVolume;

	/// <summary>
	/// Invert the camera in given directions
	/// </summary>
	public static InvertCameraOptions InvertCamera => Instance.InvertCamera;

	/// <summary>
	/// Current Language ID
	/// </summary>
	public static string Language => Instance.Language;

	/// <summary>
	/// Fuji Custom - Whether we should write to the log file or not.
	/// </summary>
	public static bool WriteLog => Instance.WriteLog;

	/// <summary>
	/// Fuji Custom - Whether The debug menu should be enabled
	/// </summary>
	public static bool EnableDebugMenu => Instance.EnableDebugMenu;

	public static void ToggleFullscreen()
	{
		Instance.Fullscreen = !Instance.Fullscreen;
		SyncSettings();
	}

	public static void ToggleWriteLog()
	{
		Instance.WriteLog = !Instance.WriteLog;
	}

	public static void ToggleEnableDebugMenu()
	{
		Instance.EnableDebugMenu = !Instance.EnableDebugMenu;
	}

	public static void ToggleZGuide()
	{
		Instance.ZGuide = !Instance.ZGuide;
	}

	public static void SetCameraInverted(InvertCameraOptions value)
	{
		Instance.InvertCamera = value;
	}

	public static void ToggleTimer()
	{
		Instance.SpeedrunTimer = !Instance.SpeedrunTimer;
	}

	public static void SetMusicVolume(int value)
	{
		Instance.MusicVolume = Calc.Clamp(value, 0, 10);
		SyncSettings();
	}

	public static void SetSfxVolume(int value)
	{
		Instance.SfxVolume = Calc.Clamp(value, 0, 10);
		SyncSettings();
	}

	public static void SetLanguage(string language)
	{
		Instance.Language = language;
	}

	public static void SyncSettings()
	{
		App.Fullscreen = Instance.Fullscreen;
		Audio.SetVCAVolume("vca:/music", Calc.Clamp(Instance.MusicVolume / 10.0f, 0, 1));
		Audio.SetVCAVolume("vca:/sfx", Calc.Clamp(Instance.SfxVolume / 10.0f, 0, 1));

		Audio.MusicGroup.setVolume(Calc.Clamp(Instance.MusicVolume / 10.0f, 0, 1));
		Audio.SoundEffectGroup.setVolume(Calc.Clamp(Instance.SfxVolume / 10.0f, 0, 1));
	}

	public static void SaveToFile()
	{
		var savePath = Path.Join(App.UserPath, DefaultFileName);
		var tempPath = Path.Join(App.UserPath, DefaultFileName + ".backup");

		// first save to a temporary file
		{
			using var stream = File.Create(tempPath);
			Instance.Serialize(stream, Instance);
			stream.Flush();
		}

		// validate that the temp path worked, and overwrite existing if it did.
		if (File.Exists(tempPath) &&
			Instance.Deserialize(File.ReadAllText(tempPath)) != null)
		{
			File.Copy(tempPath, savePath, true);
		}
	}

	[DisallowHooks]
	internal static void LoadSettingsByFileName(string file_name)
	{
		if (file_name == string.Empty) file_name = DefaultFileName;
		var settingsFile = Path.Join(App.UserPath, file_name);

		if (File.Exists(settingsFile))
			Instance = Instance.Deserialize(File.ReadAllText(settingsFile)) as Settings_V01 ?? Instance;
		else
			Instance = new();
		SyncSettings();
	}
}
