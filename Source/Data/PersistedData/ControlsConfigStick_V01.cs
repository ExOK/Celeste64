using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Celeste64;

public sealed class ControlsConfigStick_V01 : PersistedData
{
	public override int Version => 1;

	public float Deadzone { get; set; } = 0;
	public List<ControlsConfigBinding_V01> Up { get; set; } = [];
	public List<ControlsConfigBinding_V01> Down { get; set; } = [];
	public List<ControlsConfigBinding_V01> Left { get; set; } = [];
	public List<ControlsConfigBinding_V01> Right { get; set; } = [];

	public void BindTo(VirtualStick stick)
	{
		stick.CircularDeadzone = Deadzone;
		stick.Horizontal.OverlapBehaviour = VirtualAxis.Overlaps.TakeNewer;
		stick.Vertical.OverlapBehaviour = VirtualAxis.Overlaps.TakeNewer;
		foreach (var it in Up)
			it.BindTo(stick.Vertical.Negative);
		foreach (var it in Down)
			it.BindTo(stick.Vertical.Positive);
		foreach (var it in Left)
			it.BindTo(stick.Horizontal.Negative);
		foreach (var it in Right)
			it.BindTo(stick.Horizontal.Positive);
	}

	public override JsonTypeInfo GetTypeInfo()
	{
		return ControlsConfigStick_V01Context.Default.ControlsConfigStick_V01;
	}
}

public class ControlsConfigStick_V01Converter : JsonConverter<ControlsConfigStick_V01>
{
	public override ControlsConfigStick_V01? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using (var jsonDoc = JsonDocument.ParseValue(ref reader))
		{
			return new ControlsConfigStick_V01().Deserialize<ControlsConfigStick_V01>(jsonDoc.RootElement.GetRawText());
		}
	}

	public override void Write(Utf8JsonWriter writer, ControlsConfigStick_V01 value, JsonSerializerOptions options)
	{
		value.Serialize(writer, value);
	}
}

[JsonSourceGenerationOptions(
	WriteIndented = true,
	UseStringEnumConverter = true,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
	AllowTrailingCommas = true
)]
[JsonSerializable(typeof(ControlsConfigStick_V01))]
internal partial class ControlsConfigStick_V01Context : JsonSerializerContext { }
