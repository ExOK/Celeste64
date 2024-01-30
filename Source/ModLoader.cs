using System.Text.Json;
using static Celeste64.Assets;

namespace Celeste64.Source
{
    public class ModLoader
    {
        public const string ModFolder = "Mods";

        private static string? modFolderPath = null;

        public static string ModFolderPath
        {
            get
            {
                if (modFolderPath == null)
                {
                    var baseFolder = AppContext.BaseDirectory;
                    var searchUpPath = "";
                    int up = 0;
                    while (!Directory.Exists(Path.Join(baseFolder, searchUpPath, ModFolder)) && up++ < 5)
                        searchUpPath = Path.Join(searchUpPath, "..");
                    if (!Directory.Exists(Path.Join(baseFolder, searchUpPath, ModFolder)))
                        throw new Exception($"Unable to find {ModFolder} Directory from '{baseFolder}'");
                    modFolderPath = Path.Join(baseFolder, searchUpPath, ModFolder);
                }

                return modFolderPath;
            }
        }

        public static Dictionary<string, string> LoadMaps()
        {
            Dictionary<string, string> maps = [];

            foreach (var directory in Directory.EnumerateDirectories(ModFolderPath))
            {
                var mapsPath = Path.Join(directory, "Maps");
                if (!Directory.Exists(mapsPath)) continue;

                foreach (var file in Directory.EnumerateFiles(mapsPath, "*.map", SearchOption.AllDirectories))
                {
                    var name = Assets.GetResourceName(mapsPath, file);
                    if (name.StartsWith("autosave", StringComparison.OrdinalIgnoreCase))
                        continue;

                    maps.Add(name, file);
                }
            }
            return maps;
        }

        public static Dictionary<string, string> LoadTextures()
        {
            Dictionary<string, string> textures = [];
            foreach (var directory in Directory.EnumerateDirectories(ModFolderPath))
            {
                var texturesPath = Path.Join(directory, "Textures");
                if (!Directory.Exists(texturesPath)) continue;

                foreach (var file in Directory.EnumerateFiles(texturesPath, "*.png", SearchOption.AllDirectories))
                {
                    var name = GetResourceName(texturesPath, file);
                    textures.Add(name, file);
                }
            }

            return textures;
        }

        public static Dictionary<string, string> LoadFaces()
        {
            Dictionary<string, string> faces = [];
            foreach (var directory in Directory.EnumerateDirectories(ModFolderPath))
            {
                var facesPath = Path.Join(directory, "Faces");
                if (!Directory.Exists(facesPath)) continue;

                foreach (var file in Directory.EnumerateFiles(facesPath, "*.png", SearchOption.AllDirectories))
                {
                    var name = $"faces/{GetResourceName(facesPath, file)}";
                    faces.Add(name, file);
                }
            }

            return faces;
        }

        public static Dictionary<string, string> LoadModels()
        {
            Dictionary<string, string> models = [];
            foreach (var directory in Directory.EnumerateDirectories(ModFolderPath))
            {
                var modelPath = Path.Join(directory, "Models");
                if (!Directory.Exists(modelPath)) continue;

                foreach (var file in Directory.EnumerateFiles(modelPath, "*.glb", SearchOption.AllDirectories))
                {
                    var name = GetResourceName(modelPath, file);

                    models.Add(name, file);
                }
            }

            return models;
        }

        public static void LoadAudio()
        {
            foreach (var directory in Directory.EnumerateDirectories(ModFolderPath))
            {
                Audio.Load(Path.Join(directory, "Audio"));
            }
        }

        public static List<LevelInfo> LoadLevels()
        {
            List<LevelInfo> levels = new List<LevelInfo>();

            foreach (var directory in Directory.EnumerateDirectories(ModFolderPath))
            {
                if (File.Exists(Path.Join(directory, "Levels.json")))
                {
                    var data = File.ReadAllText(Path.Join(directory, "Levels.json"));
                    levels.AddRange(JsonSerializer.Deserialize(data, LevelInfoListContext.Default.ListLevelInfo) ?? []);
                }
            }
            return levels;
        }

        public static Dictionary<string, List<DialogLine>> LoadDialog()
        {
            Dictionary<string, List<DialogLine>> dialog = new Dictionary<string, List<DialogLine>>();

            foreach (var directory in Directory.EnumerateDirectories(ModFolderPath))
            {
                if (File.Exists(Path.Join(directory, "Dialog.json")))
                {
                    var data = File.ReadAllText(Path.Join(directory, "Dialog.json"));
                    Dictionary<string, List<DialogLine>> dialogData = JsonSerializer.Deserialize(data, DialogLineDictContext.Default.DictionaryStringListDialogLine) ?? [];

                    dialog = dialog.Concat(dialogData).ToDictionary();
                }
            }
            return dialog;
        }

        public static Dictionary<string, Shader> LoadShaders()
        {
            Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();

            foreach (var directory in Directory.EnumerateDirectories(ModFolderPath))
            {
                var shadersPath = Path.Join(directory, "Shaders");
                if (!Directory.Exists(shadersPath)) continue;

                foreach (var file in Directory.EnumerateFiles(shadersPath, "*.glsl"))
                {
                    if (Assets.LoadShader(file) is Shader shader)
                    {
                        shader.Name = GetResourceName(shadersPath, file);
                        shaders[shader.Name] = shader;
                    }
                }
            }
            return shaders;
        }

        public static Dictionary<string, SpriteFont> LoadFonts()
        {
            Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();

            foreach (var directory in Directory.EnumerateDirectories(ModFolderPath))
            {
                var fontsPath = Path.Join(directory, "Fonts");
                if (!Directory.Exists(fontsPath)) continue;

                foreach (var file in Directory.EnumerateFiles(fontsPath, "*.*", SearchOption.AllDirectories))
                    if (file.EndsWith(".ttf") || file.EndsWith(".otf"))
                        fonts.Add(GetResourceName(fontsPath, file), new SpriteFont(file, FontSize));
            }
            return fonts;
        }

        public static Dictionary<string, string> LoadSprites()
        {
            Dictionary<string, string> sprites = [];
            foreach (var directory in Directory.EnumerateDirectories(ModFolderPath))
            {
                var spritessPath = Path.Join(directory, "Sprites");
                if (!Directory.Exists(spritessPath)) continue;

                foreach (var file in Directory.EnumerateFiles(spritessPath, "*.png", SearchOption.AllDirectories))
                {
                    var name = GetResourceName(spritessPath, file);
                    sprites.Add(name, file);
                }
            }

            return sprites;
        }
    }
}
