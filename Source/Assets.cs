using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Celeste64;

public static class Assets
{
	public const float FontSize = Game.RelativeScale * 16;
	public const string AssetFolder = "Content";

	private static string? contentPath = null;

	public static string ContentPath
	{
		get
		{
			if (contentPath == null)
			{
				var baseFolder = AppContext.BaseDirectory;
				var searchUpPath = "";
				int up = 0;
				while (!Directory.Exists(Path.Join(baseFolder, searchUpPath, AssetFolder)) && up++ < 6)
					searchUpPath = Path.Join(searchUpPath, "..");
				if (!Directory.Exists(Path.Join(baseFolder, searchUpPath, AssetFolder)))
					throw new Exception($"Unable to find {AssetFolder} Directory from '{baseFolder}'");
				contentPath = Path.Join(baseFolder, searchUpPath, AssetFolder);
			}

			return contentPath;
		}
	}

	public record struct DialogLine(string Face, string Text, string Voice);
	public record struct SkinInfo(string Model, bool HideHair, string Name, int HairNormal, int HairNoDash, int HairTwoDash, int HairRefillFlash, int HairFeather);

	public static readonly Dictionary<string, Map> Maps = new(StringComparer.OrdinalIgnoreCase);
	public static readonly Dictionary<string, Shader> Shaders = new(StringComparer.OrdinalIgnoreCase);
	public static readonly Dictionary<string, Texture> Textures = new(StringComparer.OrdinalIgnoreCase);
	public static readonly Dictionary<string, SkinnedTemplate> Models = new(StringComparer.OrdinalIgnoreCase);
	public static readonly Dictionary<string, Subtexture> Subtextures = new(StringComparer.OrdinalIgnoreCase);
	public static readonly Dictionary<string, SpriteFont> Fonts = new(StringComparer.OrdinalIgnoreCase);
	public static Dictionary<string, List<DialogLine>> Dialog { get; private set; } = [];
	public static List<SkinInfo> Skins { get; private set; } = [];
	public static List<LevelInfo> Levels { get; private set; } = [];

	public static void Load()
	{
		var timer = Stopwatch.StartNew();

		Levels.Clear();
		Maps.Clear();
		Shaders.Clear();
		Textures.Clear();
		Subtextures.Clear();
		Models.Clear();
		Fonts.Clear();
		Audio.Unload();
		
		ModLoader.RegisterAllMods();

		var maps = new ConcurrentBag<Map>();
		var images = new ConcurrentBag<(string, Image)>();
		var models = new ConcurrentBag<(string, SkinnedTemplate)>();
		var tasks = new List<Task>();

		var globalFs = ModManager.Instance.GlobalFilesystem;
		foreach (var (file, mod) in globalFs.FindFilesInDirectoryRecursiveWithMod("Maps", "map"))
		{
			// Skip the "autosave" folder
			if (file.StartsWith("Maps/autosave", StringComparison.OrdinalIgnoreCase))
				continue;

			tasks.Add(Task.Run(() =>
			{
				if (mod.Filesystem.TryOpenFile(file, 
					    stream => new Map(GetResourceNameFromVirt(file, "Maps"), file, stream), out var map))
				{
					maps.Add(map);
				}
			}));
		}

		// load texture pngs
		foreach (var (file, mod) in globalFs.FindFilesInDirectoryRecursiveWithMod("Textures", "png"))
		{
			tasks.Add(Task.Run(() =>
			{
				if (mod.Filesystem.TryLoadImage(file, out var image))
				{
					images.Add((GetResourceNameFromVirt(file, "Textures"), image));
				}
			}));
		}

		// load faces
		foreach (var (file, mod) in globalFs.FindFilesInDirectoryRecursiveWithMod("Faces", "png"))
		{
			tasks.Add(Task.Run(() =>
			{
				var name = $"faces/{GetResourceNameFromVirt(file, "Faces")}";
				if (mod.Filesystem.TryLoadImage(file, out var image))
				{
					images.Add((name, image));
				}
			}));
		}

		// load glb models
		foreach (var (file, mod) in globalFs.FindFilesInDirectoryRecursiveWithMod("Models", "glb"))
		{
			tasks.Add(Task.Run(() =>
			{
				if (mod.Filesystem.TryOpenFile(file, stream => SharpGLTF.Schema2.ModelRoot.ReadGLB(stream),
					    out var input))
				{
					var model = new SkinnedTemplate(input);
					models.Add((GetResourceNameFromVirt(file, "Models"), model));
				}
			}));
		}

		// load audio
		var allBankFiles = globalFs.FindFilesInDirectoryRecursiveWithMod("Audio", "bank").ToList();
		// load strings first
		foreach (var (file, mod) in allBankFiles)
		{
			if (file.EndsWith(".strings.bank"))
				mod.Filesystem.TryOpenFile(file, Audio.LoadBankFromStream);
		}
		// load banks second
		foreach (var (file, mod) in allBankFiles)
		{
			if (file.EndsWith(".bank") && !file.EndsWith(".strings.bank"))
				mod.Filesystem.TryOpenFile(file, Audio.LoadBankFromStream);
		}

		// load level, dialog jsons
		Levels = [];
		Dialog = [];
		foreach (var fs in globalFs.InnerFilesystems)
		{
			if (fs.TryOpenFile("Levels.json", 
				    stream => JsonSerializer.Deserialize(stream, LevelInfoListContext.Default.ListLevelInfo) ?? [], 
				    out var levels))
			{
				Levels.AddRange(levels);
			}
			
			if (fs.TryOpenFile("Dialog.json", 
				    stream => JsonSerializer.Deserialize(stream, DialogLineDictContext.Default.DictionaryStringListDialogLine) ?? [], 
				    out var dialog))
			{
				foreach (var (key, value) in dialog)
				{
					Dialog[key] = value;
				}
			}
		}

		// load glsl shaders
		foreach (var (file, mod) in globalFs.FindFilesInDirectoryRecursiveWithMod("Shaders", "glsl"))
		{
			if (mod.Filesystem.TryOpenFile(file, stream => LoadShader(file, stream), out var shader))
			{
				shader.Name = GetResourceNameFromVirt(file, "Shaders");
				Shaders[shader.Name] = shader;
			}
		}

		// load font files
		foreach (var (file, mod) in globalFs.FindFilesInDirectoryRecursiveWithMod("Fonts", ""))
		{
			if (file.EndsWith(".ttf") || file.EndsWith(".otf"))
			{
				if (mod.Filesystem.TryOpenFile(file, stream => new SpriteFont(stream, FontSize), out var font))
					Fonts.Add(GetResourceNameFromVirt(file, "Fonts"), font);
			}
		}

		// pack sprites into single texture
		{
			var packer = new Packer
			{
				Trim = false,
				CombineDuplicates = false,
				Padding = 1
			};
			
			foreach (var (file, mod) in globalFs.FindFilesInDirectoryRecursiveWithMod("Sprites", "png"))
			{
				if (mod.Filesystem.TryOpenFile(file, stream => new Image(stream), out var img))
					packer.Add(GetResourceNameFromVirt(file, "Sprites"), img);
			}

			var result = packer.Pack();
			var pages = new List<Texture>();
			foreach (var it in result.Pages)
			{
				it.Premultiply();
				pages.Add(new Texture(it));
			}

			foreach (var it in result.Entries)
				Subtextures.Add(it.Name, new Subtexture(pages[it.Page], it.Source, it.Frame));
		}

		// wait for tasks to finish
		{
			foreach (var task in tasks)
				task.Wait();
			foreach (var (name, img) in images)
				Textures[name] = new Texture(img) { Name = name };
			foreach (var map in maps)
				Maps[map.Name] = map;
			foreach (var (name, model) in models)
			{
				model.ConstructResources();
				Models[name] = model;
			}
		}

		// Load Skins
		Skins = [
			new SkinInfo("player", false, "Default", 0xdb2c00, 0x6ec0ff, 0xfa91ff, 0xffffff, 0xf2d450)
		];

		foreach (var (file, mod) in globalFs.FindFilesInDirectoryRecursiveWithMod("Skins", "json"))
		{
			if (mod.Filesystem.TryOpenFile(file,
				    stream => JsonSerializer.Deserialize(stream, SkinInfoContext.Default.SkinInfo), out var skin))
			{
				Skins.Add(skin);
			}
		}

		Log.Info($"Loaded Assets in {timer.ElapsedMilliseconds}ms");
	}

	internal static string GetResourceNameFromVirt(string virtPath, string folder)
	{
		var ext = Path.GetExtension(virtPath);
		// +1 to account for the forward slash
		return virtPath.AsSpan((folder.Length+1)..^ext.Length).ToString();
	}
	
	internal static Shader? LoadShader(string virtPath, Stream file)
	{
		using var reader = new StreamReader(file);
		var code = reader.ReadToEnd();

		StringBuilder vertex = new();
		StringBuilder fragment = new();
		StringBuilder? target = null;
		
		foreach (var l in code.Split('\n'))
		{
			var line = l.Trim('\r');
			
			if (line.StartsWith("VERTEX:"))
				target = vertex;
			else if (line.StartsWith("FRAGMENT:"))
				target = fragment;
			else if (line.StartsWith("#include"))
			{
				var path = $"{Path.GetDirectoryName(virtPath)}/{line[9..]}";

				if (ModManager.Instance.GlobalFilesystem.TryLoadText(path, out var include))
					target?.Append(include);
				else
					throw new Exception($"Unable to find shader include: '{path}'");
			}
			else
				target?.AppendLine(line);
		}

		return new Shader(new(
			vertexShader: vertex.ToString(),
			fragmentShader: fragment.ToString()
		));
	}
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, List<Assets.DialogLine>>))]
internal partial class DialogLineDictContext : JsonSerializerContext {}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Assets.SkinInfo))]
internal partial class SkinInfoContext : JsonSerializerContext { }