using System.Text.Json;
using System.Text.Json.Serialization;

namespace Celeste64;

public class ControlsConfig
{
	public const string FileName = "controls.json";

	public class Binding
	{
		public Keys? Key { get; set; }
		public MouseButtons? MouseButton { get; set; }
		public Buttons? Button { get; set; }
		public Axes? Axis { get; set; }
		public float AxisDeadzone { get; set; }
		public bool AxisInverted { get; set; }
		public Gamepads? OnlyFor { get; set; }
		public Gamepads? NotFor { get; set; }

		public Binding() {}
		public Binding(Keys input) => Key = input;
		public Binding(MouseButtons input) => MouseButton = input;
		public Binding(Buttons input) => Button = input;
		public Binding(Axes input, float deadzone, bool inverted)
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

	public class Stick
	{
		public float Deadzone { get; set; } = 0;
		public List<Binding> Up { get; set; } = [];
		public List<Binding> Down { get; set; } = [];
		public List<Binding> Left { get; set; } = [];
		public List<Binding> Right { get; set; } = [];

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

	public Dictionary<string, List<Binding>> Actions { get; set; } = [];
	public Dictionary<string, Stick> Sticks { get; set; } = [];

	public static ControlsConfig Defaults = new()
	{
		Actions = new() {
			["Jump"] = [
				new(Keys.C),
				new(Buttons.South),
				new(Buttons.North),
			],
			["Dash"] = [
				new(Keys.X),
				new(Buttons.West),
				new(Buttons.East),
			],
			["Climb"] = [
				new(Keys.Z),new(Keys.V),new(Keys.LeftShift),new(Keys.RightShift),
				new(Buttons.LeftShoulder),new(Buttons.RightShoulder),
				new(Axes.LeftTrigger, 0.4f, false),
				new(Axes.RightTrigger, 0.4f, false),
			],
			["Confirm"] = [
				new(Keys.C),
				new(Buttons.South) { NotFor = Gamepads.Nintendo },
				new(Buttons.East) { OnlyFor = Gamepads.Nintendo },
			],
			["Cancel"] = [
				new(Keys.X),
				new(Buttons.East) { NotFor = Gamepads.Nintendo },
				new(Buttons.South) { OnlyFor = Gamepads.Nintendo },
			],
			["Pause"] = [
				new(Keys.Enter), new(Keys.Escape),
				new(Buttons.Start), new(Buttons.Select), new(Buttons.Back)
			],
		},

		Sticks = new() {
			["Move"] = new() {
				Deadzone = 0.35f,
				Left = [
					new(Keys.Left),
					new(Buttons.Left),
					new(Axes.LeftX, 0.0f, true)
				],
				Right = [
					new(Keys.Right),
					new(Buttons.Right),
					new(Axes.LeftX, 0.0f, false)
				],
				Up = [
					new(Keys.Up),
					new(Buttons.Up),
					new(Axes.LeftY, 0.0f, true)
				],
				Down = [
					new(Keys.Down),
					new(Buttons.Down),
					new(Axes.LeftY, 0.0f, false)
				],
			},
			["Camera"] = new() {
				Deadzone = 0.35f,
				Left = [
					new(Keys.A),
					new(Axes.RightX, 0.0f, true)
				],
				Right = [
					new(Keys.D),
					new(Axes.RightX, 0.0f, false)
				],
				Up = [
					new(Keys.W),
					new(Axes.RightY, 0.0f, true)
				],
				Down = [
					new(Keys.S),
					new(Axes.RightY, 0.0f, false)
				],
			},
			["Menu"] = new() {
				Deadzone = 0.35f,
				Left = [
					new(Keys.Left),
					new(Buttons.Left),
					new(Axes.LeftX, 0.50f, true)
				],
				Right = [
					new(Keys.Right),
					new(Buttons.Right),
					new(Axes.LeftX, 0.50f, false)
				],
				Up = [
					new(Keys.Up),
					new(Buttons.Up),
					new(Axes.LeftY, 0.50f, true)
				],
				Down = [
					new(Keys.Down),
					new(Buttons.Down),
					new(Axes.LeftY, 0.50f, false)
				],
			},
		}
	};
}

// All of this is just so the Binding values are on a single line to increase readability
public class ControlsConfigBindingConverter : JsonConverter<ControlsConfig.Binding>
{
    public override ControlsConfig.Binding? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
		return JsonSerializer.Deserialize(ref reader, ControlsConfigBindingContext.Default.Binding);
    }

    public override void Write(Utf8JsonWriter writer, ControlsConfig.Binding value, JsonSerializerOptions options)
    {
		var data = 
			"\n" + 
			new string(' ', writer.CurrentDepth * 2) + 
			JsonSerializer.Serialize(value, ControlsConfigBindingContext.Default.Binding);
		writer.WriteRawValue(data);
    }
}

[JsonSourceGenerationOptions(
	WriteIndented = false,
	UseStringEnumConverter = true, 
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
	AllowTrailingCommas = true
)]
[JsonSerializable(typeof(ControlsConfig.Binding))]
internal partial class ControlsConfigBindingContext : JsonSerializerContext {}

// normal serialization
[JsonSourceGenerationOptions(
	WriteIndented = true,
	UseStringEnumConverter = true, 
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
	AllowTrailingCommas = true,
	Converters = [typeof(ControlsConfigBindingConverter)]
)]
[JsonSerializable(typeof(ControlsConfig))]
internal partial class ControlsConfigContext : JsonSerializerContext {}