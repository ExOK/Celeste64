namespace Celeste64.Mod.Data;

public class SaveManager
{
    public static SaveManager Instance = new();

    public string GetLastLoadedSave()
    {
        if (File.Exists(Path.Join(App.UserPath, "save.metadata")))
            return File.ReadAllText(Path.Join(App.UserPath, "save.metadata"));
        else
        {
            File.WriteAllText(Path.Join(App.UserPath, "save.metadata"), "save.json");
            return "save.json";
        }
    }

    public void SetLastLoadedSave(string save_name)
    {
        if (File.Exists(Path.Join(App.UserPath, "save.metadata")))
            File.WriteAllText(Path.Join(App.UserPath, "save.metadata"), save_name);
    }

    public List<string> GetSaves()
    {
        List<string> saves = new List<string>();

        foreach (string savefile in Directory.GetFiles(App.UserPath))
        {
            var saveFileName = Path.GetFileName(savefile);
            if (saveFileName.EndsWith(".json") && saveFileName.StartsWith("save"))
                saves.Add(saveFileName);
        }

        return saves;
    }

    public void CopySave(string filename)
    {
        if (File.Exists(Path.Join(App.UserPath, filename)))
        {
            string new_file_name = $"{filename.Split(".json")[0]}(copy).json";
            File.Copy(Path.Join(App.UserPath, filename), Path.Join(App.UserPath, new_file_name));
        }
    }

    int GetSaveCount()
    {
        return GetSaves().Count;
    }

    public void NewSave()
    {
        Save.Instance.FileName = $"save_{GetSaveCount()}.json";
        var savePath = Path.Join(App.UserPath, Save.Instance.FileName);
        var tempPath = Path.Join(App.UserPath, Save.Instance.FileName + ".backup");

        // first save to a temporary file
        {
            using var stream = File.Create(tempPath);
            Save.Serialize(stream, new Save());
            stream.Flush();
        }

        // validate that the temp path worked, and overwride existing if it did.
        if (File.Exists(tempPath) && Save.Deserialize(File.ReadAllText(tempPath)) != null)
        {
            File.Copy(tempPath, savePath, true);
        }

    }

    public void DeleteSave(string save)
    {
        if (File.Exists(Path.Join(App.UserPath, save)))
        {
            File.Delete(Path.Join(App.UserPath, save));
        }
    }

    public void LoadSaveByFileName(string file_name)
    {
        if (file_name == string.Empty) file_name = "save.json";
        var saveFile = Path.Join(App.UserPath, file_name);

        if (File.Exists(saveFile))
            Save.Instance = Save.Deserialize(File.ReadAllText(saveFile)) ?? new();
        else
            Save.Instance = new();
        Save.Instance.SyncSettings();
    }
}