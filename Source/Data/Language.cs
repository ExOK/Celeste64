using Celeste64.Mod;
using System.Text.Json.Serialization;

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
		if (!KnownMissingKeys.Contains(key)) // we shouldn't spam the logs with keys that are already known
		{
			KnownMissingKeys.Add(key);

			Log.Warning($"Attempt to access a missing dialog key: {key}");
		}
	}

	/// <summary>
	/// Returns a localized string.
	/// If no localized string exists for the given key, return the key instead with a # in front of it.
	/// </summary>
	/// <param name="key">The key we are trying to find a localized string for.</param>
	/// <returns>The localized string or its key</returns>
	public string GetString(string key)
	{
		if (ModStrings.TryGetValue(key, out var modValue))
			return modValue;
		else if (Strings.TryGetValue(key, out var value))
			return value;

		TryLogMissing(key);
		return $"#{key}";
	}

	/// <summary>
	/// Try to get a localized string
	/// If no localized string exists for the given key, return false.
	/// </summary>
	/// <param name="key">The key we are trying to find a localized string for.</param>
	/// <param name="value">The localized string if it exists</param>
	/// <returns>Returns true if the localized string exists or false if it doesn't</returns>
	public bool TryGetString(string key, out string value)
	{
		if (ModStrings.TryGetValue(key, out var modValue))
		{
			value = modValue;
			return true;
		}
		else if (Strings.TryGetValue(key, out var stringValue))
		{
			value = stringValue;
			return true;
		}

		value = string.Empty;
		return false;
	}

	/// <summary>
	/// Returns a localized string for a specific mod.
	/// If no localized string exists for the given key, return the key instead with a # in front of it.
	/// </summary>
	/// <param name="mod">The mod we want to pull a localized string from.</param>
	/// <param name="key">The key we are trying to find a localized string for.</param>
	/// <returns>The localized string or its key</returns>
	public string GetModString(GameMod mod, string key)
	{
		if (mod.Strings.TryGetValue(Current.ID, out var dictionary) && dictionary.ContainsKey(key))
		{
			return dictionary[key];
		}

		TryLogMissing($"{Current.ID}:{key}");
		return $"#{key}";
	}

	/// <summary>
	/// Try to get a localized string for a specific mod.
	/// If no localized string exists for the given key, return false.
	/// </summary>
	/// <param name="mod">The mod we want to pull a localized string from.</param>
	/// <param name="key">The key we are trying to find a localized string for.</param>
	/// <param name="value">The localized string if it exists</param>
	/// <returns>Returns true if the localized string exists or false if it doesn't</returns>
	public bool TryGetModString(GameMod mod, string key, out string value)
	{
		if (mod.Strings.TryGetValue(Current.ID, out var dictionary) && dictionary.ContainsKey(key))
		{
			value = dictionary[key];
			return true;
		}

		value = string.Empty;
		return false;
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
			for (int i = 0; i < value.Length; i++)
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
				for (int i = 0; i < line.Text.Length; i++)
				{
					codepoints.Add(char.ConvertToUtf32(line.Text, i));
					if (char.IsSurrogate(line.Text[i]))
						i++;
				}
			}
		}

		spriteFont = new SpriteFont(Assets.Fonts[Font], Assets.FontSize, codepoints.ToArray());
		Game.OnResolutionChanged += () => spriteFont = new SpriteFont(Assets.Fonts[Font], Assets.FontSize, codepoints.ToArray());
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
			if (Assets.Languages.TryGetValue(Settings.Language, out var lang))
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
	public static bool TryGetString(string key, out string value) => Language.Current.TryGetString(key, out value);
	public static bool TryGetModString(GameMod mod, string key, out string value) => Language.Current.TryGetModString(mod, key, out value);

	public static List<Language.Line> Lines(string key) => Language.Current.GetLines(key);
	public static bool HasLines(string key) => Language.Current.Dialog.ContainsKey(key);
	public static bool HasKey(string key) => Language.Current.Strings.ContainsKey(key) || Language.Current.ModStrings.ContainsKey(key);

	public class Localized(string key)
	{
		protected string Key => key;

		/// <summary>
		/// Return the localized string.
		/// If the string cannot be localized, this will return the key with a # in front of it instead
		/// </summary>
		/// <returns>The localized string.</returns>
		public override string ToString()
		{
			return Str(key);
		}

		/// <summary>
		/// Return the localized string if the key exists.
		/// If the key cannot be localized, return an empty string
		/// </summary>
		/// <returns>The localized string or an empty string.</returns>
		public virtual string StringOrEmpty()
		{
			return HasKey(key) ? Str(key) : string.Empty;
		}

		/// <summary>
		/// Returns the key without localization.
		/// </summary>
		/// <returns>The unlocalized key.</returns>
		public virtual string GetKey()
		{
			return key;
		}

		/// <summary>
		/// Get a full subkey for this localizion string.
		/// </summary>
		/// <param name="subkey">The final subkey part</param>
		/// <returns>the combined subkey in the format of {key}.{subkey}</returns>
		public Localized GetSub(string subkey)
		{
			return new($"{key}.{subkey}");
		}

		public static implicit operator Localized(string s) => new(s);
		public static implicit operator string(Localized s) => s.ToString();
	}

	/// <summary>
	/// This class gives us a way to pass in unlocalized strings where a localized string would normally be required.
	/// </summary>
	/// <param name="value">The unlocalized string that will be displayed.</param>
	public class Unlocalized(string value) : Localized(value)
	{
		public override string ToString()
		{
			return Key;
		}

		public override string StringOrEmpty()
		{
			return Key;
		}

		public override string GetKey()
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
internal partial class LanguageContext : JsonSerializerContext { }
