using System.Text.Json;
using System.Text.Json.Serialization;

namespace Celeste64;

/// <summary>
/// Stored data associated with a mod
/// </summary>
public sealed class ModRecord_V01 : PersistedData
{
	public override int Version => 1;

	public string ID { get; set; } = string.Empty;
	public bool Enabled { get; set; } = true;
	public Dictionary<string, string> StringData { get; set; } = [];
	public Dictionary<string, int> IntData { get; set; } = [];
	public Dictionary<string, float> FloatData { get; set; } = [];
	public Dictionary<string, bool> BoolData { get; set; } = [];
	public Dictionary<string, string> SettingsStringData { get; set; } = [];
	public Dictionary<string, int> SettingsIntData { get; set; } = [];
	public Dictionary<string, float> SettingsFloatData { get; set; } = [];
	public Dictionary<string, bool> SettingsBoolData { get; set; } = [];
}

internal class ModRecord_V01Converter : JsonConverter<ModRecord_V01>
{
	public override ModRecord_V01? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using (var jsonDoc = JsonDocument.ParseValue(ref reader))
		{
			return new ModRecord_V01().Deserialize(jsonDoc.RootElement.GetRawText()) as ModRecord_V01;
		}
	}

	public override void Write(Utf8JsonWriter writer, ModRecord_V01 value, JsonSerializerOptions options)
	{
		value.Serialize(writer, value);
	}
}

[JsonSourceGenerationOptions(WriteIndented = true, AllowTrailingCommas = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ModRecord_V01))]
internal partial class ModRecord_V01Context : JsonSerializerContext { }
