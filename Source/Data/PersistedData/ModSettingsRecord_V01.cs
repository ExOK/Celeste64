using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Celeste64;

/// <summary>
/// Stored settings data associated with a mod
/// </summary>
public class ModSettingsRecord_V01 : PersistedData
{
	public override int Version => 1;

	public override JsonTypeInfo GetTypeInfo()
	{
		return ModSettingsRecord_V01Context.Default.ModSettingsRecord_V01;
	}

	public string ID { get; set; } = string.Empty;
	public bool Enabled { get; set; } = true;
	public Dictionary<string, string> SettingsStringData { get; set; } = [];
	public Dictionary<string, int> SettingsIntData { get; set; } = [];
	public Dictionary<string, float> SettingsFloatData { get; set; } = [];
	public Dictionary<string, bool> SettingsBoolData { get; set; } = [];

	public string GetStringSetting(string name, string defaultValue = "")
		=> SettingsStringData.TryGetValue(name, out string? value) ? value : defaultValue;

	public string SetStringSetting(string name, string value = "")
		=> SettingsStringData[name] = value;

	public int GetIntSetting(string name, int defaultValue = 0)
		=> SettingsIntData.TryGetValue(name, out int value) ? value : defaultValue;

	public int SetIntSetting(string name, int value = 1)
		=> SettingsIntData[name] = value;

	public float GetFloatSetting(string name, float defaultValue = 0)
		=> SettingsFloatData.TryGetValue(name, out float value) ? value : defaultValue;

	public float SetFloatSetting(string name, float value = 1)
		=> SettingsFloatData[name] = value;

	public bool GetBoolSetting(string name, bool defaultValue = false)
		=> SettingsBoolData.TryGetValue(name, out bool value) ? value : defaultValue;

	public bool SetBoolSetting(string name, bool value = false)
		=> SettingsBoolData[name] = value;
}

internal class ModSettingsRecord_V01Converter : JsonConverter<ModSettingsRecord_V01>
{
	public override ModSettingsRecord_V01? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using (var jsonDoc = JsonDocument.ParseValue(ref reader))
		{
			return new ModSettingsRecord_V01().Deserialize(jsonDoc.RootElement.GetRawText()) as ModSettingsRecord_V01;
		}
	}

	public override void Write(Utf8JsonWriter writer, ModSettingsRecord_V01 value, JsonSerializerOptions options)
	{
		value.Serialize(writer, value);
	}
}

[JsonSourceGenerationOptions(WriteIndented = true, AllowTrailingCommas = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ModSettingsRecord_V01))]
internal partial class ModSettingsRecord_V01Context : JsonSerializerContext { }
