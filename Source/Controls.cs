using Foster.Framework;
using System.Text;
using System.Text.Json.Nodes;

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

    public static bool MouseMovementActive = false;

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
		Jump.Add(0, Buttons.A, Buttons.Y);
		Jump.Add(Keys.C);

		Dash.Clear();
		Dash.Add(0, Buttons.X, Buttons.B);
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
		Confirm.Add(0, Buttons.A);
		Confirm.Add(0, Keys.C);
		
		Cancel.Clear();
		Cancel.Add(0, Buttons.B);
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
		var gamepad = Input.Controllers[0].Gamepad;

		if (!prompts.TryGetValue(gamepad, out var list))
			prompts[gamepad] = list = new();

		if (!list.TryGetValue(name, out var lookup))
			list[name] = lookup = $"Controls/{GetControllerName(gamepad)}/{name}";
		
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


    private static List<(string, object)> controls = new(8)
    {
        ("Move", Move ),
        ("Menu Move", Menu ),
        ("Camera", Camera ),
        ("Jump", Jump ),
        ("Dash", Dash ),
        ("Climb", Climb ),
        ("Menu Confirm", Confirm ),
        ("Menu Back", Cancel ),
        ("Pause", Pause )
    };

#pragma warning disable CS8600
#pragma warning disable CS8604
    // i probably should have handled it a bit better but :(, im just hoping that this isn't the slowest code in the game. - Viv
    public static void HandleCustomControls(JsonNode root)
	{

		JsonArray emptyArray = new JsonArray();
        foreach ((string, object) control in controls)
        {
            if (root[control.Item1] is { } ctrl)
            {
                bool clearCheck = false;
                foreach (JsonNode node in ctrl["Controller"]?.AsArray() ?? emptyArray)
                { // error handled
                    if (control.Item2 is VirtualStick stick) HandleStickConfig(node, stick, ref clearCheck, false);
                    else if (control.Item2 is VirtualButton button) HandleButtonConfig(node, button, ref clearCheck, false);
                }
                foreach (JsonNode node in ctrl["Keyboard"]?.AsArray() ?? emptyArray)
                { // error handled
                    if (control.Item2 is VirtualStick stick) HandleStickConfig(node, stick, ref clearCheck, true);
                    else if (control.Item2 is VirtualButton button) HandleButtonConfig(node, button, ref clearCheck, true);
                }
            }
        }
	}

    private static void HandleButtonConfig(JsonNode node, VirtualButton button, ref bool clearCheck, bool keyboard)
    {
        string key = node["Control"]!.AsValue().ToString().Trim();
        if (keyboard) {
            if (key.EndsWith("Arrow")) key = key.Substring(0, key.Length - 5).Trim();
            if (Enum.TryParse<Keys>(key, true, out Keys result))
            {
                if (!clearCheck) { button.Clear(); clearCheck = true; }
                button.Add(result);
            }
        } else if (key == "LeftStick" || key == "RightStick") {
            if (!clearCheck) { button.Clear(); clearCheck = true; }
            float threshold = 0.4f;
            if (node["Deadzone"] is { } subNode && subNode.GetValueKind() is System.Text.Json.JsonValueKind.Number)
            {
                float f = subNode.GetValue<float>();
                threshold = f;
            }
            switch (node["Direction"]!.AsValue().ToString().ToLower())
            {
                case "up": button.Add(0, key == "RightStick" ? Axes.RightY : Axes.LeftY, -1, threshold); break;
                case "down": button.Add(0, key == "RightStick" ? Axes.RightY : Axes.LeftY, 1, threshold); break;
                case "left": button.Add(0, key == "RightStick" ? Axes.RightX : Axes.LeftX, -1, threshold); break;
                case "right": button.Add(0, key == "RightTrigger" ? Axes.RightX : Axes.LeftX, 1, threshold); break;
            }
        } else if (key == "LeftTrigger" || key == "RightTrigger") {
            float threshold = 0.4f;
            if (node["Deadzone"] is { } subNode && subNode.GetValueKind() is System.Text.Json.JsonValueKind.Number)
            {
                float f = subNode.GetValue<float>();
                threshold = f;
            }
            button.Add(0, key == "RightTrigger" ? Axes.RightTrigger : Axes.LeftTrigger, 1, threshold);
        } else {
            if (Enum.TryParse<Buttons>(key, true, out Buttons result))
            {
                if (!clearCheck) { button.Clear(); clearCheck = true; }
                button.Add(0, result);
            }
        }
    }

    private static void HandleStickConfig(JsonNode node, VirtualStick stick, ref bool clearCheck, bool keyboard)
    {
        string key = node["Control"]!.AsValue().ToString();

        if (keyboard) {
            if (Enum.TryParse<Keys>(key, true, out var result))
            {
                if (!clearCheck) { stick.Clear(); clearCheck = true; }
                switch (node["Direction"]!.AsValue().ToString().ToLower())
                {
                    case "up": stick.Vertical.Negative.Add(result); break;
                    case "down": stick.Vertical.Positive.Add(result); break;
                    case "left": stick.Horizontal.Negative.Add(result); break;
                    case "right": stick.Horizontal.Positive.Add(result); break;
                }
            }
        } else if (key == "Mouse") { 
            if (!clearCheck) { stick.Clear(); clearCheck = true; }
            MouseMovementActive = true;
        } else if (key == "LeftStick" || key == "RightStick") {
            if (!clearCheck) { stick.Clear(); clearCheck = true; }
            float dzH = 0, dzV = 0;
            if (node["Deadzone"] is { } subNode)
            {
                if (subNode is JsonArray arr)
                {
                    dzH = ((float?)arr[0]) ?? 0;
                    dzV = ((float?)arr[1]) ?? 0;
                } else if (subNode.GetValueKind() is System.Text.Json.JsonValueKind.Number)
                {
                    float f = subNode.GetValue<float>();
                    dzH = dzV = f;
                }
            }
            stick.Horizontal.Add(0, key == "RightStick" ? Axes.RightX : Axes.LeftX, dzH);
            stick.Vertical.Add(0, key == "RightStick" ? Axes.RightY : Axes.LeftY, dzV);
        } else if (key == "D-Pad" || key == "Dpad" || key == "dpad") {
            stick.AddDPad(0);
        } else if (key == "LeftTrigger" || key == "RightTrigger") {
            float threshold = 0.4f;
            if (node["Deadzone"] is { } subNode && subNode.GetValueKind() is System.Text.Json.JsonValueKind.Number)
            {
                float f = subNode.GetValue<float>();
                threshold = f;
            }
            switch (node["Direction"]!.AsValue().ToString().ToLower())
            {
                case "up": stick.Vertical.Negative.Add(0, key == "RightTrigger" ? Axes.RightTrigger : Axes.LeftTrigger, 1, threshold); break;
                case "down": stick.Vertical.Positive.Add(0, key == "RightTrigger" ? Axes.RightTrigger : Axes.LeftTrigger, 1, threshold); break;
                case "left": stick.Horizontal.Negative.Add(0, key == "RightTrigger" ? Axes.RightTrigger : Axes.LeftTrigger, 1, threshold); break;
                case "right": stick.Horizontal.Positive.Add(0, key == "RightTrigger" ? Axes.RightTrigger : Axes.LeftTrigger, 1, threshold); break;
            }
        } else {
            if (Enum.TryParse<Buttons>(key, true, out var result))
            {
                if (!clearCheck) { stick.Clear(); clearCheck = true; }
                switch (node["Direction"]!.AsValue().ToString().ToLower())
                {
                    case "up": stick.Vertical.Negative.Add(0, result); break;
                    case "down": stick.Vertical.Positive.Add(0, result); break;
                    case "left": stick.Horizontal.Negative.Add(0, result); break;
                    case "right": stick.Horizontal.Positive.Add(0, result); break;
                }
            }
        }
    }
#pragma warning restore CS8600
#pragma warning restore CS8604
}
