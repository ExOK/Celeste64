using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Celeste64;

public class ModSettings_V01 : PersistedData
{
	public override int Version => 1;

	public override JsonTypeInfo GetTypeInfo()
	{
		return ModSettings_V01Context.Default.ModSettings_V01;
	}

	public static ModSettings_V01 Instance = new();

	public List<ModSettingsRecord_V01> ModSettingsRecords { get; set; } = [];
}

[JsonSourceGenerationOptions(
	WriteIndented = true,
	AllowTrailingCommas = true,
	UseStringEnumConverter = true,
	Converters = [typeof(ModSettingsRecord_V01Converter)]
)]
[JsonSerializable(typeof(ModSettings_V01))]
internal partial class ModSettings_V01Context : JsonSerializerContext { }
