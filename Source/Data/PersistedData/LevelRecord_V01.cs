using System.Text.Json;
using System.Text.Json.Serialization;

namespace Celeste64;

/// <summary>
/// Stored data associated with a single level
/// </summary>
public sealed class LevelRecord_V01 : PersistedData
{
	public override int Version => 1;

	public string ID { get; set; } = string.Empty;
	public string Checkpoint { get; set; } = string.Empty;
	public HashSet<string> Strawberries { get; set; } = [];
	public HashSet<string> CompletedSubMaps { get; set; } = [];
	public Dictionary<string, int> Flags { get; set; } = [];
	public int Deaths { get; set; } = 0;
	public TimeSpan Time { get; set; } = new();

	public int GetFlag(string name, int defaultValue = 0)
		=> Flags.TryGetValue(name, out int value) ? value : defaultValue;

	public int SetFlag(string name, int value = 1)
		=> Flags[name] = value;

	public int IncFlag(string name)
		=> Flags[name] = GetFlag(name) + 1;
}

internal class LevelRecord_V01Converter : JsonConverter<LevelRecord_V01>
{
	public override LevelRecord_V01? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using (var jsonDoc = JsonDocument.ParseValue(ref reader))
		{
			return new LevelRecord_V01().Deserialize(jsonDoc.RootElement.GetRawText()) as LevelRecord_V01;
		}
	}

	public override void Write(Utf8JsonWriter writer, LevelRecord_V01 value, JsonSerializerOptions options)
	{
		value.Serialize(writer, value);
	}
}

[JsonSourceGenerationOptions(WriteIndented = true, AllowTrailingCommas = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(LevelRecord_V01))]
internal partial class LevelRecord_V01Context : JsonSerializerContext { }
