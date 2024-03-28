using Celeste64.Mod;
using System.Text.Json;

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
	public static readonly VirtualButton CopyFile = new("CopyFile");
	public static readonly VirtualButton DeleteFile = new("DeleteFile");
	public static readonly VirtualButton CreateFile = new("CreateFile");

	public static ControlsConfig_V01 Instance = new();

	public const string DefaultFileName = "controls.json";

	[DisallowHooks]
	internal static void LoadControlsByFileName(string file_name)
	{
		if (file_name == string.Empty) file_name = DefaultFileName;
		var controlsFile = Path.Join(App.UserPath, file_name);

		ControlsConfig_V01? controls = null;
		if (File.Exists(controlsFile))
		{
			try
			{
				controls = Instance.Deserialize<ControlsConfig_V01>(File.ReadAllText(controlsFile)) ?? null;
			}
			catch
			{
				controls = null;
			}
		}

		if (controls == null)
		{
			controls = ControlsConfig_V01.Defaults;
			using var stream = File.Create(controlsFile);
			JsonSerializer.Serialize(stream, ControlsConfig_V01.Defaults, ControlsConfig_V01Context.Default.ControlsConfig_V01);
			stream.Flush();
		}

		Instance = controls;
		LoadConfig(Instance);
	}

	public static void LoadConfig(ControlsConfig_V01? config = null)
	{
		static ControlsConfigStick_V01 FindStick(ControlsConfig_V01? config, string name)
		{
			if (config != null && config.Sticks.TryGetValue(name, out var stick))
				return stick;
			if (ControlsConfig_V01.Defaults.Sticks.TryGetValue(name, out stick))
				return stick;
			throw new Exception($"Missing Stick Binding for '{name}'");
		}

		static List<ControlsConfigBinding_V01> FindAction(ControlsConfig_V01? config, string name)
		{
			if (config != null && config.Actions.TryGetValue(name, out var action))
				return action;
			if (ControlsConfig_V01.Defaults.Actions.TryGetValue(name, out action))
				return action;
			throw new Exception($"Missing Action Binding for '{name}'");
		}

		Clear();

		FindStick(config, "Move").BindTo(Move);
		FindStick(config, "Camera").BindTo(Camera);
		FindStick(config, "Menu").BindTo(Menu);

		foreach (var it in FindAction(config, "Jump"))
			it.BindTo(Jump);
		foreach (var it in FindAction(config, "Dash"))
			it.BindTo(Dash);
		foreach (var it in FindAction(config, "Climb"))
			it.BindTo(Climb);
		foreach (var it in FindAction(config, "Confirm"))
			it.BindTo(Confirm);
		foreach (var it in FindAction(config, "Cancel"))
			it.BindTo(Cancel);
		foreach (var it in FindAction(config, "Pause"))
			it.BindTo(Pause);
		foreach (var it in FindAction(config, "CopyFile"))
			it.BindTo(CopyFile);
		foreach (var it in FindAction(config, "DeleteFile"))
			it.BindTo(DeleteFile);
		foreach (var it in FindAction(config, "CreateFile"))
			it.BindTo(CreateFile);

	}

	public static void Clear()
	{
		Move.Clear();
		Camera.Clear();
		Jump.Clear();
		Dash.Clear();
		Climb.Clear();
		Menu.Clear();
		Confirm.Clear();
		Cancel.Clear();
		Pause.Clear();
		CopyFile.Clear();
		DeleteFile.Clear();
		CreateFile.Clear();
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
		CopyFile.Consume();
		DeleteFile.Consume();
		CreateFile.Consume();
	}

	private static readonly Dictionary<string, Dictionary<string, string>> prompts = [];

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
		var gamepad = Input.Controllers[0];
		var deviceTypeName =
			gamepad.Connected ? GetControllerName(gamepad.Gamepad) : "PC";

		if (!prompts.TryGetValue(deviceTypeName, out var list))
			prompts[deviceTypeName] = list = [];

		if (!list.TryGetValue(name, out var lookup))
			list[name] = lookup = $"Controls/{deviceTypeName}/{name}";

		return lookup;
	}

	public static string GetPromptLocation(VirtualButton button)
	{
		// TODO: instead, query the button's actual bindings and look up a
		// texture based on that! no time tho
		if (button == Confirm)
			return GetPromptLocation("confirm");
		else if (button == Cancel)
			return GetPromptLocation("cancel");
		else if (button == CreateFile)
			return GetPromptLocation("createfile");
		else if (button == DeleteFile)
			return GetPromptLocation("deletefile");
		else if (button == CopyFile)
			return GetPromptLocation("copyfile");
		else
			return GetPromptLocation("pause");
	}

	public static Subtexture GetPrompt(VirtualButton button)
	{
		return Assets.Subtextures.GetValueOrDefault(GetPromptLocation(button));
	}
}
