
namespace Celeste64;

public static class Controls
{
	public static readonly VirtualStick Move = new("Move", VirtualAxis.Overlaps.TakeNewer, 0.35f);
	public static readonly VirtualStick Menu = new("Menu", VirtualAxis.Overlaps.TakeNewer, 0.35f);
	public static readonly VirtualStick Camera = new("Camera", VirtualAxis.Overlaps.TakeNewer, 0.35f);
	public static readonly VirtualButton Jump = new("Jump", .1f);
	public static readonly VirtualButton Dash = new("Dash", .1f);
	public static readonly VirtualButton Climb = new("Climb");

	public static readonly VirtualButton Confirm = new("Confirm");
	public static readonly VirtualButton Cancel = new("Cancel");
	public static readonly VirtualButton Pause = new("Pause");
	
	public static bool IsNintendo(VirtualButton button, VirtualButton.IBinding binding) => 
		(binding is VirtualButton.ButtonBinding b && Input.Controllers[b.Controller].Gamepad == Gamepads.Nintendo);

	public static bool IsNotNintendo(VirtualButton button, VirtualButton.IBinding binding) => 
		(binding is VirtualButton.ButtonBinding b && Input.Controllers[b.Controller].Gamepad != Gamepads.Nintendo);

	public static void Load()
	{
		Move.Clear();
		Move.AddLeftJoystick(0);
		Move.AddDPad(0);
		Move.AddArrowKeys();

		Camera.Clear();
		Camera.AddRightJoystick(0, 0.50f, 0.70f);
		Camera.Add(Keys.A, Keys.D, Keys.W, Keys.S);

		Jump.Clear();
		Jump.Add(0, Buttons.South, Buttons.North);
		Jump.Add(Keys.C);

		Dash.Clear();
		Dash.Add(0, Buttons.West, Buttons.East);
		Dash.Add(Keys.X);

		Climb.Clear();
		Climb.Add(0, Buttons.LeftShoulder, Buttons.RightShoulder);
		Climb.Add(0, Axes.RightTrigger, 1, .4f);
		Climb.Add(0, Axes.LeftTrigger, 1, .4f);
		Climb.Add(Keys.Z, Keys.V, Keys.LeftShift, Keys.RightShift);
		
		Menu.Clear();
		Menu.AddLeftJoystick(0, 0.50f, 0.50f);
		Menu.AddDPad(0);
		Menu.AddArrowKeys();
		
		Confirm.Clear();
		Confirm.Add(IsNotNintendo, 0, Buttons.South);
		Confirm.Add(IsNintendo, 0, Buttons.East);
		Confirm.Add(0, Keys.C);
		
		Cancel.Clear();
		Cancel.Add(IsNotNintendo, 0, Buttons.East);
		Cancel.Add(IsNintendo, 0, Buttons.South);
		Cancel.Add(0, Keys.X);
		
		Pause.Clear();
		Pause.Add(0, Buttons.Start, Buttons.Select, Buttons.Back);
		Pause.Add(0, Keys.Enter, Keys.Escape);
	}

	public static void Consume()
	{
		Move.Consume();
		Menu.Consume();
		Camera.Consume();
		Jump.Consume();
		Dash.Consume();
		Climb.Consume();
		Confirm.Consume();
		Cancel.Consume();
		Pause.Consume();
		
	}

	private static readonly Dictionary<Gamepads, Dictionary<string, string>> prompts = [];

	private static string GetControllerName(Gamepads pad) => pad switch
    {
        Gamepads.DualShock4 => "PlayStation 4",
        Gamepads.DualSense => "PlayStation 5",
        Gamepads.Nintendo => "Nintendo Switch",
        Gamepads.Xbox => "Xbox Series",
        _ => "Xbox Series",
    };

	private static string GetPromptLocation(string name)
	{
	        var gamepadPure = Input.Controllers[0];
	        var gamepad = gamepadPure.Gamepad;

		if (!prompts.TryGetValue(gamepad, out var list))
			prompts[gamepad] = list = new();

		if (!list.TryGetValue(name, out var lookup))
	            list[name] = lookup = !gamepadPure.Connected
	                    ? $"Controls/PC/{name}"
	                    : $"Controls/{GetControllerName(gamepad)}/{name}";
		
		return lookup;
	}

	public static string GetPromptLocation(VirtualButton button)
	{
		// TODO: instead, query the button's actual bindings and look up a
		// texture based on that! no time tho
		if (button == Confirm)
			return GetPromptLocation("confirm");
		else
			return GetPromptLocation("cancel");
	}

	public static Subtexture GetPrompt(VirtualButton button)
	{
		return Assets.Subtextures.GetValueOrDefault(GetPromptLocation(button));
	}
}
