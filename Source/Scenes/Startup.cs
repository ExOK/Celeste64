using Celeste64.Mod.Data;

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
			SaveManager.Instance.LoadSaveByFileName(SaveManager.Instance.GetLastLoadedSave());
		}

		// load settings file
		{
			Settings.LoadSettingsByFileName(Settings.DefaultFileName);
		}

		// load mod settings file
		{
			ModSettings.LoadModSettingsByFileName(ModSettings.DefaultFileName);
		}

		// load assets
		// this currently needs to happen after the save file loads, because this also loads mods, which get their saved settings from the save file.
		Assets.Load();

		// make sure the active language is ready for use,
		// since the save file may have loaded a different language than default.
		Language.Current.Use();

		// try to load controls, or overwrite with defaults if they don't exist
		{
			Controls.LoadControlsByFileName(Controls.DefaultFileName);
		}

		// enter game
		//Assets.Levels[0].Enter(new AngledWipe());
		if (Input.Keyboard.CtrlOrCommand && !Game.Instance.IsMidTransition && Settings.EnableQuickStart)
		{
			var entry = new Overworld.Entry(Assets.Levels[0], null);
			entry.Level.Enter();
		}
		else
		{
			Game.Instance.Goto(new Transition()
			{
				Mode = Transition.Modes.Replace,
				Scene = () => new Titlescreen(),
				ToBlack = null,
				FromBlack = new AngledWipe(),
			});
		}
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
