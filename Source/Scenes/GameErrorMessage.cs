using Celeste64.Mod;

namespace Celeste64;

public class GameErrorMessage : Scene
{
	private Menu menu = new();
	private Exception exception;

	public GameErrorMessage(Exception e)
	{
		exception = e;

		Audio.StopBus(Sfx.bus_gameplay_world, false);
		Game.Instance.Music.Stop();

		menu.Title = "Uh-oh! You've caught a super rare error!\nCheck your log file for more details.\nIf this error was caused by a mod, you might want to report the issue to the mod author.";

		menu.Add(new Menu.Option("FujiOpenLogFile", () =>
		{
			Game.WriteToLog();
			Game.OpenLog();
		}));

		menu.Add(new Menu.Option("QuitToMainMenu", () =>
		{
			Game.Instance.ReloadAssets();
		}));

		menu.Add(new Menu.Option("Exit", () => throw e)); // This exits the game and forwards the error to the fatal crash handler.
	}

	public override void Update()
	{
		menu.Update();
	}

	public override void Render(Target target)
	{
		target.Clear(Color.Black);

		Batcher batcher = new();
		Rect bounds = new(0, 0, target.Width, target.Height);

		menu.Render(batcher, bounds.Center);

		batcher.Render(target);
		batcher.Clear();
	}
}
