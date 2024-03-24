using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Celeste64;

/// <summary>
/// Stored data associated with a mod
/// </summary>
public class ModRecord_V02 : VersionedPersistedData<ModRecord_V01>
{
	public override int Version => 1;

	public override JsonTypeInfo GetTypeInfo()
	{
		return ModRecord_V01Context.Default.ModRecord_V01;
	}

	public string ID { get; set; } = string.Empty;
	public Dictionary<string, string> StringData { get; set; } = [];
	public Dictionary<string, int> IntData { get; set; } = [];
	public Dictionary<string, float> FloatData { get; set; } = [];
	public Dictionary<string, bool> BoolData { get; set; } = [];

	public string GetString(string name, string defaultValue = "")
		=> StringData.TryGetValue(name, out string? value) ? value : defaultValue;

	public string SetString(string name, string value = "")
		=> StringData[name] = value;

	public int GetInt(string name, int defaultValue = 0)
		=> IntData.TryGetValue(name, out int value) ? value : defaultValue;

	public int SetInt(string name, int value = 1)
		=> IntData[name] = value;

	public float GetFloat(string name, float defaultValue = 0)
		=> FloatData.TryGetValue(name, out float value) ? value : defaultValue;

	public float SetFloat(string name, float value = 1)
		=> FloatData[name] = value;

	public bool GetBool(string name, bool defaultValue = false)
		=> BoolData.TryGetValue(name, out bool value) ? value : defaultValue;

	public bool SetBool(string name, bool value = false)
		=> BoolData[name] = value;

	public override object? UpgradeFrom(ModRecord_V01? oldRecord)
	{
		if (oldRecord == null) return null;
		ModRecord_V02 newRecord = new ModRecord_V02();
		newRecord.ID = oldRecord.ID;
		newRecord.StringData = oldRecord.StringData;
		newRecord.IntData = oldRecord.IntData;
		newRecord.FloatData = oldRecord.FloatData;
		newRecord.BoolData = oldRecord.BoolData;

		return newRecord;
	}
}

internal class ModRecord_V02Converter : JsonConverter<ModRecord_V02>
{
	public override ModRecord_V02? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using (var jsonDoc = JsonDocument.ParseValue(ref reader))
		{
			return new ModRecord_V02().Deserialize(jsonDoc.RootElement.GetRawText()) as ModRecord_V02;
		}
	}

	public override void Write(Utf8JsonWriter writer, ModRecord_V02 value, JsonSerializerOptions options)
	{
		value.Serialize(writer, value);
	}
}

[JsonSourceGenerationOptions(WriteIndented = true, AllowTrailingCommas = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ModRecord_V02))]
internal partial class ModRecord_V02Context : JsonSerializerContext { }
