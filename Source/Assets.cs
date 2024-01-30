﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
				while (!Directory.Exists(Path.Join(baseFolder, searchUpPath, AssetFolder)) && up++ < 5)
					searchUpPath = Path.Join(searchUpPath, "..");
				if (!Directory.Exists(Path.Join(baseFolder, searchUpPath, AssetFolder)))
					throw new Exception($"Unable to find {AssetFolder} Directory from '{baseFolder}'");
				contentPath = Path.Join(baseFolder, searchUpPath, AssetFolder);
			}

			return contentPath;
		}
	}

	public record struct DialogLine(string Face, string Text, string Voice);

	public static readonly CultureInfo Culture = new CultureInfo("en-US");
	public static readonly Dictionary<string, Map> Maps = new(StringComparer.OrdinalIgnoreCase);
	public static readonly Dictionary<string, Shader> Shaders = new(StringComparer.OrdinalIgnoreCase);
	public static readonly Dictionary<string, Texture> Textures = new(StringComparer.OrdinalIgnoreCase);
	public static readonly Dictionary<string, SkinnedTemplate> Models = new(StringComparer.OrdinalIgnoreCase);
	public static readonly Dictionary<string, Subtexture> Subtextures = new(StringComparer.OrdinalIgnoreCase);
	public static readonly Dictionary<string, SpriteFont> Fonts = new(StringComparer.OrdinalIgnoreCase);
	public static Dictionary<string, List<DialogLine>> Dialog { get; private set; } = [];
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

		var maps = new ConcurrentBag<Map>();
		var images = new ConcurrentBag<(string, Image)>();
		var models = new ConcurrentBag<(string, SkinnedTemplate)>();
		var tasks = new List<Task>();

		// load map files
		{
			var mapsPath = Path.Join(ContentPath, "Maps");
			foreach (var file in Directory.EnumerateFiles(mapsPath, "*.map", SearchOption.AllDirectories))
			{
				var name = GetResourceName(mapsPath, file);
				if (name.StartsWith("autosave", StringComparison.OrdinalIgnoreCase))
					continue;

				tasks.Add(Task.Run(() =>
				{
					var map = new Map(name, file);
					maps.Add(map);
				}));
			}
		}

		// load texture pngs
		var texturesPath = Path.Join(ContentPath, "Textures");
		foreach (var file in Directory.EnumerateFiles(texturesPath, "*.png", SearchOption.AllDirectories))
		{
			var name = GetResourceName(texturesPath, file);
			tasks.Add(Task.Run(() =>
			{
				var img = new Image(file);
				img.Premultiply();
				images.Add((name, img));
			}));
		}

		// load faces
		var facesPath = Path.Join(ContentPath, "Faces");
		foreach (var file in Directory.EnumerateFiles(facesPath, "*.png", SearchOption.AllDirectories))
		{
			var name = $"faces/{GetResourceName(facesPath, file)}";
			tasks.Add(Task.Run(() =>
			{
				var img = new Image(file);
				img.Premultiply();
				images.Add((name, img));
			}));
		}

		// load glb models
		var modelPath = Path.Join(ContentPath, "Models");
		foreach (var file in Directory.EnumerateFiles(modelPath, "*.glb", SearchOption.AllDirectories))
		{
			var name = GetResourceName(modelPath, file);

			tasks.Add(Task.Run(() =>
			{
				var input = SharpGLTF.Schema2.ModelRoot.Load(file);
				var model = new SkinnedTemplate(input);
				models.Add((name, model));
			}));
		}

		// load audio
		Audio.Load(Path.Join(ContentPath, "Audio"));

		// load level json
		{
			var data = File.ReadAllText(Path.Join(ContentPath, "Levels.json"));
			Levels = JsonSerializer.Deserialize(data, LevelInfoListContext.Default.ListLevelInfo) ?? [];
		}

		// load dialog json
		{
			var data = File.ReadAllText(Path.Join(ContentPath, "Dialog.json"));
			Dialog = JsonSerializer.Deserialize(data, DialogLineDictContext.Default.DictionaryStringListDialogLine) ?? [];
		}

		// load glsl shaders
		var shadersPath = Path.Join(ContentPath, "Shaders");
		foreach (var file in Directory.EnumerateFiles(shadersPath, "*.glsl"))
		{
			if (LoadShader(file) is Shader shader)
			{
				shader.Name = GetResourceName(shadersPath, file);
				Shaders[shader.Name] = shader;
			}
		}

		// load font files
		var fontsPath = Path.Join(ContentPath, "Fonts");
		foreach (var file in Directory.EnumerateFiles(fontsPath, "*.*", SearchOption.AllDirectories))
			if (file.EndsWith(".ttf") || file.EndsWith(".otf"))
				Fonts.Add(GetResourceName(fontsPath, file), new SpriteFont(file, FontSize));

		// pack sprites into single texture
		{
			var packer = new Packer
			{
				Trim = false,
				CombineDuplicates = false,
				Padding = 1
			};

			var spritesPath = Path.Join(ContentPath, "Sprites");
            foreach (var file in Directory.EnumerateFiles(spritesPath, "*.png", SearchOption.AllDirectories))
				packer.Add(GetResourceName(spritesPath, file), new Image(file));

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

		// Load in controls
		string controlsPath = Path.Join(ContentPath, "Controls.json");
		if (controlsPath != null && File.Exists(controlsPath))
        {
#pragma warning disable CS8600
            JsonNode doc = JsonNode.Parse(File.ReadAllText(controlsPath)); 
			if (doc != null)
				Controls.HandleCustomControls(doc);
		}

#pragma warning restore CS8600
        // wait for tasks to finish
        {
            foreach (var task in tasks)
				task.Wait();
			foreach (var (name, img) in images)
				Textures.Add(name, new Texture(img) { Name = name });
			foreach (var map in maps)
				Maps[map.Name] = map;
			foreach (var (name, model) in models)
			{
				model.ConstructResources();
				Models[name] = model;
			}
		}


		Log.Info($"Loaded Assets in {timer.ElapsedMilliseconds}ms");
	}

	private static string GetResourceName(string contentFolder, string path)
	{
		var fullname = Path.Join(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
		var relative = Path.GetRelativePath(contentFolder, fullname);
		var normalized = relative.Replace("\\", "/");
		return normalized;
	}

	private static Shader? LoadShader(string file)
	{
		ShaderCreateInfo? data = null;

		if (File.Exists(file))
		{
			StringBuilder vertex = new();
			StringBuilder fragment = new();
			StringBuilder? target = null;
			foreach (var line in File.ReadAllLines(file))
			{
				if (line.StartsWith("VERTEX:"))
					target = vertex;
				else if (line.StartsWith("FRAGMENT:"))
					target = fragment;
				else if (line.StartsWith("#include"))
				{
					var path = Path.Join(Path.GetDirectoryName(file), line[9..]);
					if (File.Exists(path))
						target?.Append(File.ReadAllText(path));
					else
						throw new Exception($"Unable to find shader include: '{path}'");
				}
				else
					target?.AppendLine(line);
			}

			data = new(
				vertexShader: vertex.ToString(),
				fragmentShader: fragment.ToString()
			);
		}

		return data.HasValue ? new Shader(data.Value) : null;
	}
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, List<Assets.DialogLine>>))]
internal partial class DialogLineDictContext : JsonSerializerContext {}