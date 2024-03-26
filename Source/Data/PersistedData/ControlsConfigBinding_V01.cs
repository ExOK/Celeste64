using System.Text.Json;
using System.Text.Json.Serialization;

namespace Celeste64;

public sealed class ControlsConfigBinding_V01 : PersistedData
{
	public Keys? Key { get; set; }
	public MouseButtons? MouseButton { get; set; }
	public Buttons? Button { get; set; }
	public Axes? Axis { get; set; }
	public float AxisDeadzone { get; set; }
	public bool AxisInverted { get; set; }
	public Gamepads? OnlyFor { get; set; }
	public Gamepads? NotFor { get; set; }

	public override int Version => 1;

	public ControlsConfigBinding_V01() { }
	public ControlsConfigBinding_V01(Keys input) => Key = input;
	public ControlsConfigBinding_V01(MouseButtons input) => MouseButton = input;
	public ControlsConfigBinding_V01(Buttons input) => Button = input;
	public ControlsConfigBinding_V01(Axes input, float deadzone, bool inverted)
	{
		Axis = input;
		AxisDeadzone = deadzone;
		AxisInverted = inverted;
	}

	private bool Condition(VirtualButton vb, VirtualButton.IBinding binding)
	{
		if (!OnlyFor.HasValue && !NotFor.HasValue)
			return true;

		int index;
		if (binding is VirtualButton.ButtonBinding btn)
			index = btn.Controller;
		else if (binding is VirtualButton.AxisBinding axs)
			index = axs.Controller;
		else
			return true;

		if (OnlyFor.HasValue && Input.Controllers[index].Gamepad != OnlyFor.Value)
			return false;

		if (NotFor.HasValue && Input.Controllers[index].Gamepad == NotFor.Value)
			return false;

		return true;
	}

	public void BindTo(VirtualButton button)
	{
		if (Key.HasValue)
			button.Add(Key.Value);

		if (Button.HasValue)
			button.Add(Condition, 0, Button.Value);

		if (MouseButton.HasValue)
			button.Add(MouseButton.Value);

		if (Axis.HasValue)
			button.Add(Condition, 0, Axis.Value, AxisInverted ? -1 : 1, AxisDeadzone);
	}
}

public class ControlsConfigBinding_V01Converter : JsonConverter<ControlsConfigBinding_V01>
{
	public override ControlsConfigBinding_V01? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using (var jsonDoc = JsonDocument.ParseValue(ref reader))
		{
			return new ControlsConfigBinding_V01().Deserialize(jsonDoc.RootElement.GetRawText()) as ControlsConfigBinding_V01;
		}
	}

	public override void Write(Utf8JsonWriter writer, ControlsConfigBinding_V01 value, JsonSerializerOptions options)
	{
		// All of this is just so the Binding values are on a single line to increase readability
		var data =
			"\n" +
			new string(' ', writer.CurrentDepth * 2) +
			JsonSerializer.Serialize(value, ControlsConfigBinding_V01Context.Default.ControlsConfigBinding_V01);
		writer.WriteRawValue(data);
	}
}

[JsonSourceGenerationOptions(
	WriteIndented = true,
	UseStringEnumConverter = true,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
	AllowTrailingCommas = true
)]
[JsonSerializable(typeof(ControlsConfigBinding_V01))]
internal partial class ControlsConfigBinding_V01Context : JsonSerializerContext { }
