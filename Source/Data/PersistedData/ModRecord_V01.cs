using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Celeste64;

/// <summary>
/// Stored data associated with a mod
/// </summary>
public class ModRecord_V01 : PersistedData
{
	public override int Version => 1;

	public override JsonTypeInfo GetTypeInfo()
	{
		return ModRecord_V01Context.Default.ModRecord_V01;
	}

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

	public string SettingsGetString(string name, string defaultValue = "")
		=> SettingsStringData.TryGetValue(name, out string? value) ? value : defaultValue;

	public string SettingsSetString(string name, string value = "")
		=> SettingsStringData[name] = value;

	public int SettingsGetInt(string name, int defaultValue = 0)
		=> SettingsIntData.TryGetValue(name, out int value) ? value : defaultValue;

	public int SettingsSetInt(string name, int value = 1)
		=> SettingsIntData[name] = value;

	public float SettingsGetFloat(string name, float defaultValue = 0)
		=> SettingsFloatData.TryGetValue(name, out float value) ? value : defaultValue;

	public float SettingsSetFloat(string name, float value = 1)
		=> SettingsFloatData[name] = value;

	public bool SettingsGetBool(string name, bool defaultValue = false)
		=> SettingsBoolData.TryGetValue(name, out bool value) ? value : defaultValue;

	public bool SettingsSetBool(string name, bool value = false)
		=> SettingsBoolData[name] = value;
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
