using System.Diagnostics;
using System.Text.Json;

namespace Celeste64;

public struct Transition
{
	public enum Modes
	{
		Replace,
		Push,
		Pop
	}

	public Modes Mode;
	public Func<Scene>? Scene;
	public ScreenWipe? ToBlack;
	public ScreenWipe? FromBlack;
	public bool ToPause;
	public bool FromPause;
	public bool Saving;
	public bool StopMusic;
	public bool PerformAssetReload;
	public float HoldOnBlackFor;
}

public class Game : Module
{
	private enum TransitionStep
	{
		None,
		FadeOut,
		Hold,
		Perform,
		FadeIn
	}

	public const string GamePath = "Celeste64";
	public const string GameTitle = "Celeste 64: Fragments of the Mountain";
	public const int Width = 640;
	public const int Height = 360;
	public static readonly Version Version = typeof(Game).Assembly.GetName().Version!;
	public static readonly string VersionString = $"v.{Version.Major}.{Version.Minor}.{Version.Build}";

	/// <summary>
	/// Used by various rendering elements to proportionally scale if you change the default game resolution
	/// </summary>
	public const float RelativeScale = Height / 360.0f;

	private static Game? instance;
	public static Game Instance => instance ?? throw new Exception("Game isn't running");

	private readonly Stack<Scene> scenes = new();
	private readonly Target target = new(Width, Height, [TextureFormat.Color, TextureFormat.Depth24Stencil8]);
	private readonly Batcher batcher = new();
	private Transition transition;
	private TransitionStep transitionStep = TransitionStep.None;
	private readonly FMOD.Studio.EVENT_CALLBACK audioEventCallback;
	private int audioBeatCounter;
	private bool audioBeatCounterEvent;

	public AudioHandle Ambience;
	public AudioHandle Music;

	public Game()
	{
		// If this isn't stored, the delegate will get GC'd and everything will crash :)
		audioEventCallback = MusicTimelineCallback;
	}

	public override void Startup()
	{
		instance = this;
		
		Time.FixedStep = true;
		App.VSync = true;
		App.Title = GameTitle;
		Audio.Init();

		scenes.Push(new Startup());
	}

	public override void Shutdown()
	{
		if (scenes.TryPeek(out var topScene))
			topScene.Exited();

		while (scenes.Count > 0)
		{
			var it = scenes.Pop();
			it.Disposed();
		}
		
		scenes.Clear();
		instance = null;
	}

	public bool IsMidTransition => transitionStep != TransitionStep.None;

	public void Goto(Transition next)
	{
		Debug.Assert(
			transitionStep == TransitionStep.None ||
			transitionStep == TransitionStep.FadeIn);
		transition = next;
		transitionStep = scenes.Count > 0 ? TransitionStep.FadeOut : TransitionStep.Perform;
		transition.ToBlack?.Restart(transitionStep != TransitionStep.FadeOut);

		if (transition.StopMusic)
			Music.Stop();
	}

	public override void Update()
	{
		// update top scene
		if (scenes.TryPeek(out var scene))
		{
			var pausing = 
				transitionStep == TransitionStep.FadeIn && transition.FromPause ||
				transitionStep == TransitionStep.FadeOut && transition.ToPause;

			if (!pausing)
				scene.Update();
		}

		// handle transitions
		if (transitionStep == TransitionStep.FadeOut)
		{
			if (transition.ToBlack == null || transition.ToBlack.IsFinished)
			{
				transitionStep = TransitionStep.Hold;
			}
			else
			{
				transition.ToBlack.Update();
			}
		}
		else if (transitionStep == TransitionStep.Hold)
        {
            transition.HoldOnBlackFor -= Time.Delta;
			if (transition.HoldOnBlackFor <= 0)
            {
                if (transition.FromBlack != null)
                    transition.ToBlack = transition.FromBlack;
                transition.ToBlack?.Restart(true);
				transitionStep = TransitionStep.Perform;
            }
        }
		else if (transitionStep == TransitionStep.Perform)
		{
			Audio.StopBus(Sfx.bus_gameplay_world, false);

			// exit last scene
			if (scenes.TryPeek(out var lastScene))
			{
				lastScene?.Exited();
				if (transition.Mode != Transition.Modes.Push)
					lastScene?.Disposed();
			}

			// reload assets if requested
			if (transition.PerformAssetReload)
			{
				Assets.Load();
			}

			// perform game save between transitions
			if (transition.Saving)
				Save.Instance.SaveToFile();

			// perform transition
			switch (transition.Mode)
			{
			case Transition.Modes.Replace:
			Debug.Assert(transition.Scene != null);
			if (scenes.Count > 0)
				scenes.Pop();
			scenes.Push(transition.Scene());
			break;
			case Transition.Modes.Push:
			Debug.Assert(transition.Scene != null);
			scenes.Push(transition.Scene());
			audioBeatCounter = 0;
			break;
			case Transition.Modes.Pop:
			scenes.Pop();
			break;
			}

			// don't let the game sit in a sceneless place
			if (scenes.Count <= 0)
				scenes.Push(new Overworld(false));

			// run a single update when transition happens so stuff gets established
			if (scenes.TryPeek(out var nextScene))
			{
				nextScene.Entered();
				nextScene.Update();
			}

			// switch music
			{
				var last = Music.IsPlaying && lastScene != null ? lastScene.Music : string.Empty;
				var next = nextScene?.Music ?? string.Empty;
				if (next != last)
				{
					Music.Stop();
					Music = Audio.Play(next);
					if (Music)
						Music.SetCallback(audioEventCallback);
				}
			}

			// switch ambience
			{
				var last = Ambience.IsPlaying && lastScene != null ? lastScene.Ambience : string.Empty;
				var next = nextScene?.Ambience ?? string.Empty;
				if (next != last)
				{
					Ambience.Stop();
					Ambience = Audio.Play(next);
				}
			}

			// in case new music was played
			Save.Instance.SyncSettings();
			transitionStep = TransitionStep.FadeIn;
		}
		else if (transitionStep == TransitionStep.FadeIn)
		{
			if (transition.ToBlack == null || transition.ToBlack.IsFinished)
			{
				transitionStep = TransitionStep.None;
				transition = new();
			}
			else
			{
				transition.ToBlack.Update();
			}
		}
		else if (transitionStep == TransitionStep.None)
		{
			// handle audio beat events on main thread
			if (audioBeatCounterEvent)
			{
				audioBeatCounterEvent = false;
				audioBeatCounter++;

				if (scene is World world)
				{
					foreach (var listener in world.All<IListenToAudioCallback>())
						(listener as IListenToAudioCallback)?.AudioCallbackEvent(audioBeatCounter);
				}
			}
		}

		
		if (scene is not Celeste64.Startup)
		{
			// toggle fullsrceen
			if ((Input.Keyboard.Alt && Input.Keyboard.Pressed(Keys.Enter)) || Input.Keyboard.Pressed(Keys.F4))
				Save.Instance.ToggleFullscreen();

			// reload state
			if (Input.Keyboard.Ctrl && Input.Keyboard.Pressed(Keys.R) && !IsMidTransition)
			{
				if (scene is World world)
				{
					Goto(new Transition()
					{
						Mode = Transition.Modes.Replace,
						Scene = () => new World(world.Entry),
						ToPause = true,
						ToBlack = new AngledWipe(),
						PerformAssetReload = true
					});
				}
				else
				{
					Goto(new Transition()
					{
						Mode = Transition.Modes.Replace,
						Scene = () => new Titlescreen(),
						ToPause = true,
						ToBlack = new AngledWipe(),
						PerformAssetReload = true
					});
				}
			}
		}
	}

	public override void Render()
	{
		Graphics.Clear(Color.Black);

		if (transitionStep != TransitionStep.Perform && transitionStep != TransitionStep.Hold)
		{
			// draw the world to the target
			if (scenes.TryPeek(out var scene))
				scene.Render(target);

			// draw screen wipe over top
			if (transitionStep != TransitionStep.None && transition.ToBlack != null)
			{
				transition.ToBlack.Render(batcher, new Rect(0, 0, target.Width, target.Height));
				batcher.Render(target);
				batcher.Clear();
			}

			// draw the target to the window
			{
				var scale = Math.Min(App.WidthInPixels / (float)target.Width, App.HeightInPixels / (float)target.Height);
				batcher.SetSampler(new(TextureFilter.Nearest, TextureWrap.ClampToEdge, TextureWrap.ClampToEdge));
				batcher.Image(target, App.SizeInPixels / 2, target.Bounds.Size / 2, Vec2.One * scale, 0, Color.White);
				batcher.Render();
				batcher.Clear();
			}
		}
	}

	private FMOD.RESULT MusicTimelineCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr _event, IntPtr parameters)
	{
		// notify that an audio event happened (but handle it on the main thread)
		if (transitionStep == TransitionStep.None)
			audioBeatCounterEvent = true;
		return FMOD.RESULT.OK;
	}
}
