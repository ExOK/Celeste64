using System.Text;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Celeste64.Mod;

namespace Celeste64;

public class Language
{
	private static readonly Language EmptyLanguage = new() { Font = "Renogare" };
	private static readonly List<Line> EmptyLines = [];

	public record struct Line(string Face, string Text, string Voice);

	public string ID { get; set; } = string.Empty;
	public string Label { get; set; } = string.Empty;
	public string Font { get; set; } = string.Empty;
	[JsonIgnore]
	public ModAssetDictionary<string> ModStrings { get; set; } = new(
		gamemod => gamemod.Strings.TryGetValue(Current.ID, out var value) ? value : []
	);
	[JsonIgnore]
	public ModAssetDictionary<List<Line>> ModDialog { get; set; } = new(
		gamemod => gamemod.DialogLines.TryGetValue(Current.ID, out var value) ? value : []
	);

	public Dictionary<string, string> Strings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
	public Dictionary<string, List<Line>> Dialog { get; set; } = new(StringComparer.OrdinalIgnoreCase);

	internal List<string> KnownMissingKeys = new();

	[JsonIgnore]
	private SpriteFont? spriteFont;

	private void TryLogMissing(string key)
	{
		if(!KnownMissingKeys.Contains(key)) // we shouldn't spam the logs with keys that are already known
		{
			KnownMissingKeys.Add(key);

			Log.Warning($"Attempt to access a missing dialog key: {key}");
		}
	}

	public string GetString(string key)
	{
		if (ModStrings.TryGetValue(key, out var modValue))
			return modValue;
		else if (Strings.TryGetValue(key, out var value))
			return value;

		TryLogMissing(key);
		return $"#{key}";
	}

	public string GetModString(GameMod mod, string key)
	{
		if(mod.Strings.TryGetValue(Current.ID, out var dictionary) && dictionary.ContainsKey(key))
		{
			return dictionary[key];
		}

		TryLogMissing($"{Current.ID}:{key}");
		return $"#{key}";
	}

	public List<Line> GetLines(string key)
	{
		if (ModDialog.TryGetValue(key, out var modValue))
			return modValue;
		if (Dialog.TryGetValue(key, out var value))
			return value;
		return EmptyLines;
	}

	public void Use()
	{
		if (spriteFont != null)
			return;

		HashSet<int> codepoints = [];

		// add ascii codepoinets always
		for (int i = 32; i < 128; i++)
            codepoints.Add(i);

		// strings values
		foreach (var (key, value) in ModStrings)
		{
			for (int i = 0; i < value.Length; i ++)
			{
				codepoints.Add(char.ConvertToUtf32(value, i));
				if (char.IsSurrogate(value[i]))
					i++;
			}
		}

		// dialog lines
		foreach (var (key, lines) in ModDialog)
		{
			foreach (var line in lines)
			{
				for (int i = 0; i < line.Text.Length; i ++)
				{
					codepoints.Add(char.ConvertToUtf32(line.Text, i));
					if (char.IsSurrogate(line.Text[i]))
						i++;
				}
			}
		}

		spriteFont = new SpriteFont(Assets.Fonts[Font], Assets.FontSize, codepoints.ToArray());
	}

	public void Absorb(Language other, GameMod mod)
	{
		if (!mod.Strings.ContainsKey(ID))
		{
			mod.Strings.Add(ID, new Dictionary<string, string>());
		}
		if (!mod.DialogLines.ContainsKey(ID))
		{
			mod.DialogLines.Add(ID, new Dictionary<string, List<Line>>());
		}
		if (other.Strings != null)
		{
			foreach (var (k, v) in other.Strings)
				mod.Strings[ID].Add(k, v);
		}
		if (other.Dialog != null)
		{
			foreach (var (k, v) in other.Dialog)
				mod.DialogLines[ID].Add(k, v);
		}
	}

	public void OnCreate(GameMod mod)
	{
		mod.Strings.Add(ID, new Dictionary<string, string>());
		mod.DialogLines.Add(ID, new Dictionary<string, List<Line>>());
		foreach (var (k, v) in Strings)
			mod.Strings[ID].Add(k, v);
		foreach (var (k, v) in Dialog)
			mod.DialogLines[ID].Add(k, v);
	}


	public SpriteFont SpriteFont => spriteFont ?? throw new Exception("Call Language.Use() before using its SpriteFont");

	public static Language Current
	{
		get
		{
			if (Assets.Languages.TryGetValue(Save.Instance.Language, out var lang))
				return lang;

			if (Assets.Languages.TryGetValue("English", out lang))
				return lang;

			if (Assets.Languages.Count > 0)
				return Assets.Languages.First().Value;

			return EmptyLanguage;
		}
	}
}

public static class Loc
{
	public static string Str(string key) => Language.Current.GetString(key);
	public static string ModStr(GameMod mod, string key) => Language.Current.GetModString(mod, key);
	public static List<Language.Line> Lines(string key) => Language.Current.GetLines(key);
	public static bool HasLines(string key) => Language.Current.Dialog.ContainsKey(key);
	public static bool HasKey(string key) => Language.Current.Strings.ContainsKey(key) || Language.Current.ModStrings.ContainsKey(key);

	public class Localized(string key) 
	{
		protected string Key => key;
		public override string ToString() 
		{
			return Str(key);
		}

		public string StringOrEmpty()
		{
			return HasKey(key) ? Str(key) : string.Empty;
		}

		public string GetKey()
		{
			return key;
		}

		public static implicit operator Localized(string s) => new(s);
		public static implicit operator string(Localized s) => s.ToString();
	}

	public class Unlocalized(string value) : Localized(value) 
	{
		public override string ToString() 
		{
			return Key;
		}

		public static explicit operator Unlocalized(string s) => new(s);
	}
}

[JsonSourceGenerationOptions(
	WriteIndented = true, 
	PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
	AllowTrailingCommas = true
)]
[JsonSerializable(typeof(Language))]
internal partial class LanguageContext : JsonSerializerContext {}