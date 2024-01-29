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
		// load assets
		Assets.Load();

		// load save file
		{
			var saveFile = Path.Join(App.UserPath, Save.FileName);

			if (File.Exists(saveFile))
				Save.Instance = Save.Deserialize(File.ReadAllText(saveFile)) ?? new();
			else
				Save.Instance = new();
			Save.Instance.SyncSettings();
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