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