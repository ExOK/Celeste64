using Celeste64.Mod;
using Celeste64.Mod.Patches;
using System.Diagnostics;
using System.Text;

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
	// ModloaderCustom
	public const string GameTitle = "Celeste 64: Fragments of the Mountain + Fuji Mod Loader";
	public static readonly Version GameVersion = typeof(Game).Assembly.GetName().Version!;
	public static readonly string VersionString = $"Celeste 64: v.{GameVersion.Major}.{GameVersion.Minor}.{GameVersion.Build}";
	public static string LoaderVersion { get; set; } = "";

	public const int DefaultWidth = 640;
	public const int DefaultHeight = 360;

	public static event Action OnResolutionChanged = () => { };

	private static float _resolutionScale = 1.0f;
	public static float ResolutionScale
	{
		get => _resolutionScale;
		set
		{
			if (_resolutionScale == value)
				return;

			_resolutionScale = value;
			OnResolutionChanged.Invoke();
		}
	}

	public static bool IsDynamicRes;

	public static int Width => IsDynamicRes ? App.WidthInPixels : (int)(DefaultWidth * _resolutionScale);
	public static int Height => IsDynamicRes ? App.HeightInPixels : (int)(DefaultHeight * _resolutionScale);
	private int Height_old = (int)(DefaultHeight * _resolutionScale);
	private int Width_old = (int)(DefaultWidth * _resolutionScale);

	/// <summary>
	/// Used by various rendering elements to proportionally scale if you change the default game resolution
	/// </summary>
	public static float RelativeScale => _resolutionScale;

	private static Game? instance;
	public static Game Instance => instance ?? throw new Exception("Game isn't running");

	private readonly Stack<Scene> scenes = new();
	private Target target = new(Width, Height, [TextureFormat.Color, TextureFormat.Depth24Stencil8]);
	private readonly Batcher batcher = new();
	private Transition transition;
	private TransitionStep transitionStep = TransitionStep.None;
	private readonly FMOD.Studio.EVENT_CALLBACK audioEventCallback;
	private int audioBeatCounter;
	private bool audioBeatCounterEvent;

	private ImGuiManager imGuiManager;

	public AudioHandle Ambience;
	public AudioHandle Music;

	public SoundHandle? AmbienceWav;
	public SoundHandle? MusicWav;

	public Scene? Scene => scenes.TryPeek(out var scene) ? scene : null;
	public World? World => Scene as World;

	internal bool NeedsReload = false;

	public Game()
	{
		if (IsDynamicRes)
		{
			Log.Warning("Dynamic resolution is an experimental feature. Certain UI elements may not be adjusted correctly.");
		}

		OnResolutionChanged += () =>
		{
			target.Dispose();
			target = new(Width, Height, [TextureFormat.Color, TextureFormat.Depth24Stencil8]);
		};

		// If this isn't stored, the delegate will get GC'd and everything will crash :)
		audioEventCallback = MusicTimelineCallback;
		imGuiManager = new ImGuiManager();
	}

	public override void Startup()
	{
		instance = this;

		// Fuji: apply patches
		Patches.Load();

		Time.FixedStep = true;
		App.VSync = true;
		App.Title = GameTitle;
		Audio.Init();

		scenes.Push(new Startup());
		ModManager.Instance.OnGameLoaded(this);
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

		// Fuji: remove patches
		Patches.Unload();

		scenes.Clear();
		instance = null;

		Log.Info("Shutting down...");
		WriteToLog();
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

	public void UnsafelySetScene(Scene next)
	{
		scenes.Clear();
		scenes.Push(next);
	}

	private void HandleError(Exception e)
	{
		if (scenes.Peek() is GameErrorMessage)
		{
			throw e; // If we're already on the error message screen, accept our fate: it's a fatal crash!
		}

		scenes.Clear();
		Log.Error("== ERROR ==\n\n" + e.ToString());
		WriteToLog();
		UnsafelySetScene(new GameErrorMessage(e));
		return;
	}

	public override void Update()
	{
		if (IsDynamicRes)
		{
			if (Height_old != Height || Width_old != Width)
			{
				OnResolutionChanged.Invoke();
			}

			Height_old = Height;
			Width_old = Width;
		}

		imGuiManager.UpdateHandlers();

		scenes.TryPeek(out var scene); // gets the top scene

		// update top scene
		try
		{
			if (scene != null)
			{
				var pausing =
					transitionStep == TransitionStep.FadeIn && transition.FromPause ||
					transitionStep == TransitionStep.FadeOut && transition.ToPause;

				if (!pausing)
					scene.Update();
			}

			if (!(scene is GameErrorMessage))
			{
				ModManager.Instance.Update(Time.Delta);
			}
		}
		catch (Exception e)
		{
			HandleError(e);
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
			Scene? newScene = transition.Scene != null ? transition.Scene() : null;
			if (Settings.EnableAdditionalLogging && newScene != null) Log.Info("Switching scene: " + newScene.GetType());

			Audio.StopBus(Sfx.bus_gameplay_world, false);

			// exit last scene
			if (scenes.TryPeek(out var lastScene))
			{
				try
				{
					lastScene?.Exited();
					if (transition.Mode != Transition.Modes.Push)
						lastScene?.Disposed();
				}
				catch (Exception e)
				{
					transitionStep = TransitionStep.None;
					HandleError(e);
				}
			}

			// reload assets if requested
			if (transition.PerformAssetReload)
			{
				Assets.Load();
			}

			// perform game save between transitions
			if (transition.Saving)
			{
				Save.SaveToFile();
				Settings.SaveToFile();
			}

			// perform transition
			switch (transition.Mode)
			{
				case Transition.Modes.Replace:
					Debug.Assert(newScene != null);
					if (scenes.Count > 0)
						scenes.Pop();
					scenes.Push(newScene);
					break;
				case Transition.Modes.Push:
					Debug.Assert(newScene != null);
					scenes.Push(newScene);
					audioBeatCounter = 0;
					break;
				case Transition.Modes.Pop:
					scenes.Pop();
					break;
			}

			// run a single update when transition happens so stuff gets established
			if (scenes.TryPeek(out var nextScene))
			{
				try
				{
					nextScene.Entered();
					ModManager.Instance.OnSceneEntered(nextScene);
					nextScene.Update();
				}
				catch (Exception e)
				{
					transitionStep = TransitionStep.None;
					HandleError(e);
				}
			}

			// don't let the game sit in a sceneless place
			if (scenes.Count <= 0)
				scenes.Push(new Overworld(false));

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

				string lastWav = MusicWav is { IsPlaying: true } && lastScene != null ? lastScene.MusicWav : string.Empty;
				string nextWav = nextScene?.MusicWav ?? string.Empty;
				if (lastWav != nextWav)
				{
					MusicWav?.Stop();
					if (!string.IsNullOrEmpty(nextWav))
					{
						MusicWav = Audio.PlayMusic(nextWav);
					}
				}
			}

			// switch ambience
			{
				var last = Ambience.IsPlaying && lastScene != null ? lastScene.Ambience : string.Empty;
				var next = nextScene?.Ambience ?? string.Empty;
				if (next != last)
				{
					Ambience.Stop();
					if (!string.IsNullOrEmpty(next))
					{
						Ambience = Audio.Play(next);
					}
				}

				string lastWav = AmbienceWav is { IsPlaying: true } && lastScene != null ? lastScene.AmbienceWav : string.Empty;
				string nextWav = nextScene?.AmbienceWav ?? string.Empty;
				if (lastWav != nextWav)
				{
					AmbienceWav?.Stop();
					if (string.IsNullOrEmpty(nextWav))
					{
						AmbienceWav = Audio.PlayMusic(nextWav);
					}
				}
			}

			// in case new music was played
			Settings.SyncSettings();
			transitionStep = TransitionStep.FadeIn;

			WriteToLog();
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
				Settings.ToggleFullscreen();

			// reload state
			if (Input.Keyboard.Ctrl && Input.Keyboard.Pressed(Keys.R) && !IsMidTransition)
			{
				ReloadAssets();
			}
		}
	}

	internal void ReloadAssets()
	{
		if (!scenes.TryPeek(out var scene))
			return;

		if (IsMidTransition)
			return;

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

	public override void Render()
	{
		Graphics.Clear(Color.Black);

		imGuiManager.RenderHandlers();

		if (transitionStep != TransitionStep.Perform && transitionStep != TransitionStep.Hold)
		{
			// draw the world to the target
			if (scenes.TryPeek(out var scene))
				try
				{
					scene.Render(target);
				}
				catch (Exception e)
				{
					HandleError(e);
				}

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
				imGuiManager.RenderTexture(batcher);
				batcher.Render();
				batcher.Clear();
			}
		}
	}

	// Fuji Custom
	public static void WriteToLog()
	{
		if (!Settings.WriteLog)
		{
			return;
		}

		// construct a log message
		const string LogFileName = "Log.txt";
		StringBuilder log = new();
		lock (Log.Logs)
			log.AppendLine(Log.Logs.ToString());

		// write to file
		string path = LogFileName;
		{
			if (App.Running)
			{
				try
				{
					path = Path.Join(App.UserPath, LogFileName);
				}
				catch
				{
					path = LogFileName;
				}
			}

			File.WriteAllText(path, log.ToString());
		}
	}

	internal static void OpenLog()
	{
		const string LogFileName = "Log.txt";
		string path = "";
		if (App.Running)
		{
			try
			{
				path = Path.Join(App.UserPath, LogFileName);
			}
			catch
			{
				path = LogFileName;
			}
		}
		if (File.Exists(path))
		{
			new Process { StartInfo = new ProcessStartInfo(path) { UseShellExecute = true } }.Start();
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
