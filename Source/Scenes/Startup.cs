using System.Text.Json;

namespace Celeste64;

/// <summary>
/// Creates a slight delay so the window looks OK before we load Assets
/// TODO: Would be nice if Foster could hide the Window till assets are ready.
/// </summary>
public class Startup : Scene
{
	private int loadDelay = 5;

	private void BeginGame()
	{
		// load save file
		{
			var saveFile = Path.Join(App.UserPath, Save.FileName);

			if (File.Exists(saveFile))
				Save.Instance = Save.Deserialize(File.ReadAllText(saveFile)) ?? new();
			else
				Save.Instance = new();
			Save.Instance.SyncSettings();
		}

		// load assets
		// this currently needs to happen after the save file loads, because this also loads mods, which get their saved settings from the save file.
		Assets.Load();

		// make sure the active language is ready for use,
		// since the save file may have loaded a different language than default.
		Language.Current.Use();

		// try to load controls, or overwrite with defaults if they don't exist
		{
			var controlsFile = Path.Join(App.UserPath, ControlsConfig.FileName);

			ControlsConfig? controls = null;
			if (File.Exists(controlsFile))
			{
				try
				{
					controls = JsonSerializer.Deserialize(File.ReadAllText(controlsFile), ControlsConfigContext.Default.ControlsConfig);
				}
				catch
				{
					controls = null;
				}
			}

			// create defaults if not found
			if (controls == null)
			{
				controls = ControlsConfig.Defaults;
				using var stream = File.Create(controlsFile);
				JsonSerializer.Serialize(stream, ControlsConfig.Defaults, ControlsConfigContext.Default.ControlsConfig);
				stream.Flush();
			}

			Controls.Load(controls);
		}

		// enter game
		//Assets.Levels[0].Enter(new AngledWipe());
		Game.Instance.Goto(new Transition()
		{
			Mode = Transition.Modes.Replace,
			Scene = () => new Titlescreen(),
			ToBlack = null,
			FromBlack = new AngledWipe(),
		});
	}

	public override void Update()
	{
		if (loadDelay > 0)
		{
			loadDelay--;
			if (loadDelay <= 0)
				BeginGame();
		}
	}

	public override void Render(Target target)
	{
		target.Clear(Color.Black);
	}
}
