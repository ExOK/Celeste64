using System.Text.Json;
using System.Text.Json.Serialization;

namespace Celeste64;

public sealed class ControlsConfigStick_V01 : PersistedData
{
	public float Deadzone { get; set; } = 0;
	public List<ControlsConfigBinding_V01> Up { get; set; } = [];
	public List<ControlsConfigBinding_V01> Down { get; set; } = [];
	public List<ControlsConfigBinding_V01> Left { get; set; } = [];
	public List<ControlsConfigBinding_V01> Right { get; set; } = [];

	public override int Version => 1;

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
}

public class ControlsConfigStick_V01Converter : JsonConverter<ControlsConfigStick_V01>
{
	public override ControlsConfigStick_V01? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using (var jsonDoc = JsonDocument.ParseValue(ref reader))
		{
			return new ControlsConfigStick_V01().Deserialize(jsonDoc.RootElement.GetRawText()) as ControlsConfigStick_V01;
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
