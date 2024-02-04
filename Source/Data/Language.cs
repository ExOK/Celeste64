using System.Text;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Celeste64;

public class Language
{
	private static readonly Language EmptyLanguage = new() { Font = "Renogare" };
	private static readonly List<Line> EmptyLines = [];

	public record struct Line(string Face, string Text, string Voice);

	public string ID { get; set; } = string.Empty;
	public string Label { get; set; } = string.Empty;
	public string Font { get; set; } = string.Empty;
	public Dictionary<string, string> Strings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
	public Dictionary<string, List<Line>> Dialog { get; set; } = new(StringComparer.OrdinalIgnoreCase);

	[JsonIgnore]
	private SpriteFont? spriteFont;

	public string GetString(string key)
	{
		if (Strings.TryGetValue(key, out var value))
			return value;
		return "<MISSING>";
	}

	public List<Line> GetLines(string key)
	{
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
		foreach (var (key, value) in Strings)
		{
			for (int i = 0; i < value.Length; i ++)
			{
				codepoints.Add(char.ConvertToUtf32(value, i));
				if (char.IsSurrogate(value[i]))
					i++;
			}
		}

		// dialog lines
		foreach (var (key, lines) in Dialog)
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

	public void Absorb(Language other)
	{
		foreach (var (k, v) in other.Strings)
			Strings[k] = v;
		foreach (var (k, v) in other.Dialog)
			Dialog[k] = v;
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
	public static List<Language.Line> Lines(string key) => Language.Current.GetLines(key);
	public static bool HasLines(string key) => Language.Current.Dialog.ContainsKey(key);
}

[JsonSourceGenerationOptions(
	WriteIndented = true, 
	PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
	AllowTrailingCommas = true
)]
[JsonSerializable(typeof(Language))]
internal partial class LanguageContext : JsonSerializerContext {}