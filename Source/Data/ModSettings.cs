using Celeste64.Mod;

namespace Celeste64;

public sealed class ModSettings
{
	internal const string DefaultFileName = "mod_settings.json";

	internal static ModSettings_V01 Instance = new();

	/// <summary>
	/// Finds the record associated with a specific mod, or adds it if not found
	/// </summary>
	[DisallowHooks]
	internal static ModSettingsRecord_V01 GetOrMakeModSettings(string modID)
	{
		if (TryGetModSettings(modID) is { } record)
			return record;

		record = new ModSettingsRecord_V01() { ID = modID, Enabled = true };
		Instance.ModSettingsRecords.Add(record);
		return record;
	}

	/// <summary>
	/// Tries to get a Mod Settings Record, returns null if not found
	/// </summary>
	[DisallowHooks]
	internal static ModSettingsRecord_V01? TryGetModSettings(string modID)
	{
		foreach (var record in Instance.ModSettingsRecords)
			if (record.ID == modID)
				return record;
		return null;
	}

	/// <summary>
	/// Erases a Mod Settings Record
	/// </summary>
	[DisallowHooks]
	internal static void EraseModSettingsRecord(string modID)
	{
		for (int i = 0; i < Instance.ModSettingsRecords.Count; i++)
		{
			if (Instance.ModSettingsRecords[i].ID == modID)
			{
				Instance.ModSettingsRecords.RemoveAt(i);
				break;
			}
		}
	}

	[DisallowHooks]
	internal static void SaveToFile()
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
	internal static void LoadModSettingsByFileName(string fileName)
	{
		if (fileName == string.Empty) fileName = DefaultFileName;
		var settingsFile = Path.Join(App.UserPath, fileName);

		if (File.Exists(settingsFile))
			Instance = Instance.Deserialize(File.ReadAllText(settingsFile)) as ModSettings_V01 ?? Instance;
		else
			Instance = new();
	}
}
