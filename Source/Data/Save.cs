using Celeste64.Mod;

namespace Celeste64;

public sealed class Save
{
	public const string DefaultFileName = "save.json";

	public static Save_V02 Instance = new();

	/// <summary>
	/// Gets the Record for the current Level.
	/// </summary>
	public static LevelRecord_V01 CurrentRecord => GetOrMakeRecord(Instance.LevelID);
	/// <summary>
	/// Finds the record associated with a specific level, or adds it if not found
	/// </summary>
	public static LevelRecord_V01 GetOrMakeRecord(string levelID)
	{
		if (TryGetRecord(levelID) is { } record)
			return record;

		record = new LevelRecord_V01() { ID = levelID };
		Instance.Records.Add(record);
		return record;
	}

	/// <summary>
	/// Tries to get a Level Record, returns null if not found
	/// </summary>
	public static LevelRecord_V01? TryGetRecord(string levelID)
	{
		foreach (var record in Instance.Records)
			if (record.ID == levelID)
				return record;
		return null;
	}

	/// <summary>
	/// Erases a Level Record
	/// </summary>
	public static void EraseRecord(string levelID)
	{
		for (int i = 0; i < Instance.Records.Count; i++)
		{
			if (Instance.Records[i].ID == levelID)
			{
				Instance.Records.RemoveAt(i);
				break;
			}
		}
	}

	/// <summary>
	/// Finds the record associated with a specific mod, or adds it if not found
	/// </summary>
	public static ModRecord_V02 GetOrMakeMod(string modID)
	{
		if (TryGetMod(modID) is { } record)
			return record;

		record = new ModRecord_V02() { ID = modID };
		Instance.ModRecords.Add(record);
		return record;
	}

	/// <summary>
	/// Tries to get a Mod Record, returns null if not found
	/// </summary>
	public static ModRecord_V02? TryGetMod(string modID)
	{
		foreach (var record in Instance.ModRecords)
			if (record.ID == modID)
				return record;
		return null;
	}

	/// <summary>
	/// Erases a Mod Record
	/// </summary>
	public static void EraseModRecord(string modID)
	{
		for (int i = 0; i < Instance.ModRecords.Count; i++)
		{
			if (Instance.ModRecords[i].ID == modID)
			{
				Instance.ModRecords.RemoveAt(i);
				break;
			}
		}
	}

	public static void SetSkinName(string skin)
	{
		Instance.SkinName = skin;
	}

	public static SkinInfo GetSkin()
	{
		return Assets.EnabledSkins.FirstOrDefault(s => s.Name == Instance.SkinName) ??
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
			Instance.Deserialize<Save_V02>(File.ReadAllText(tempPath)) != null)
		{
			File.Copy(tempPath, savePath, true);
		}
	}

	internal static void LoadSaveByFileName(string fileName)
	{
		if (fileName == string.Empty) fileName = "save.json";
		var saveFile = Path.Join(App.UserPath, fileName);

		if (File.Exists(saveFile))
			Instance = Instance.Deserialize<Save_V02>(File.ReadAllText(saveFile)) ?? new();
		else
			Instance = new();
		Instance.FileName = fileName;
	}
}
