using System.Collections.Frozen;

namespace Celeste64;

public sealed class ModManager
{
	private ModManager() { }

	private static ModManager? instance = null;
	public static ModManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new ModManager();
			}
			return instance;
		}
	}

	public LayeredFilesystem GlobalFilesystem { get; } = new();
		
	private CancellationTokenSource _modFilesystemCleanupTimerToken = new();
	
	internal List<GameMod> Mods = [];

	internal IEnumerable<GameMod> EnabledMods { get {  return Mods.Where(mod => mod.Enabled); } }

	internal VanillaGameMod? VanillaGameMod { get; set; }

	internal GameMod? CurrentLevelMod { get; set; }

	internal void Unload()
	{
		_modFilesystemCleanupTimerToken.Cancel();
		_modFilesystemCleanupTimerToken = new();

		var modsCopy = Mods.ToList();
		foreach (var mod in modsCopy)
		{
			DeregisterMod(mod);
		}
	}
	
	internal void InitializeFilesystemBackgroundCleanup()
	{
		// Initialize background mod filesystem cleanup task
		var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
		Task.Run(async () => {
			while (await timer.WaitForNextTickAsync(_modFilesystemCleanupTimerToken.Token)) {
				foreach (var mod in Mods)
				{
					mod.Filesystem?.BackgroundCleanup();
				}
			}
		}, _modFilesystemCleanupTimerToken.Token);
	}
	
	internal void RegisterMod(GameMod mod)
	{
		Mods.Add(mod);
		GlobalFilesystem.Add(mod);
		if(mod.Filesystem != null)
			mod.Filesystem.OnFileChanged += OnModFileChanged;

		if(mod.Enabled)
		{
			mod.OnModLoaded();
		}
	}

	internal void DeregisterMod(GameMod mod)
	{
		Mods.Remove(mod);
		GlobalFilesystem.Remove(mod);
		mod.Filesystem?.Dispose();
		if(mod.Filesystem != null)
			mod.Filesystem.OnFileChanged -= OnModFileChanged;
		if (!mod.Enabled)
		{
			mod.OnModUnloaded();
		}
		mod.OnUnloadedCleanup?.Invoke();
	}

	internal void OnModFileChanged(ModFileChangedCtx ctx)
	{
		if (ctx.Path is { } filepath)
		{
			var extension = Path.GetExtension(filepath);
			var dir = Path.GetDirectoryName(filepath) ?? "";
			
			// Important assets taken from Assets.Load()
			// TODO: Support non-toplevel mods?
			if ((dir.StartsWith("Maps") && extension == ".map" && !dir.StartsWith("Maps/autosave")) || // Maps/**.map except Maps/autosave/** 
			    (dir.StartsWith("Textures") && extension == ".png") || // Textures/**.png
			    (dir.StartsWith("Faces") && extension == ".png") || // Faces/**.png
			    (dir.StartsWith("Models") && extension == ".glb") || // Models/**.glb
			    (dir.StartsWith("Text") && extension == ".json") || // Text/**.json
			    (dir.StartsWith("Audio") && extension == ".bank") || // Audio/**.bank
			    (dir.StartsWith("Shaders") && extension == ".glsl") || // Shaders/**.glsl
			    (dir.StartsWith("Fonts") && extension is ".ttf" or ".otf") || // Fonts/**.ttf and Fonts/**.otf
			    (dir.StartsWith("Sprites") && extension == ".png") || // Sprites/**.png
			    (dir.StartsWith("Skins") && extension == ".json") || // Skins/**.json
			    (dir.StartsWith("DLLs") && extension is ".dll") || // DLLs/**.dll
			    filepath == "Levels.json" ||			    
			    filepath == "Fuji.json"))
			{
				Log.Info($"File Changed: {filepath} (From mod {ctx.Mod.ModInfo.Name}). Reloading assets.");
			} 
			else
			{
				// Unimportant file
				return;
			}
		}
		else
		{
			Log.Info($"Mod archive for mod {ctx.Mod.ModInfo.Name} changed. Reloading assets.");
		}
		
		Game.Instance.ReloadAssets();
	}

	internal void Update(float deltaTime)
	{
		foreach (var mod in EnabledMods)
		{
			mod.Update(deltaTime);
		}
	}

	internal void OnAssetsLoaded()
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnAssetsLoaded();
		}
	}

	internal void OnSceneEntered(Scene scene)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnSceneEntered(scene);
		}
	}

	internal void OnGameLoaded(Game game)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnGameLoaded(game);
		}
	}

	internal void OnPreMapLoaded(World world, Map map)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnPreMapLoaded(world, map);
		}
	}

	internal void OnMapLoaded(Map map)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnMapLoaded(map);
		}
	}

	internal void OnWorldLoaded(World world)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnWorldLoaded(world);
		}
	}

	internal void OnActorCreated(Actor actor)
	{ 
		foreach (var mod in EnabledMods)
		{
			mod.OnActorCreated(actor);
		}
	}

	internal void OnActorAdded(Actor actor)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnActorAdded(actor);
		}
	}

	internal void OnActorDestroyed(Actor actor)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnActorDestroyed(actor);
		}
	}

	internal void OnPlayerKill(Player player)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnPlayerKilled(player);
		}
	}

	internal void OnPlayerLanded(Player player)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnPlayerLanded(player);
		}
	}

	internal void OnPlayerSkinChange(Player player, SkinInfo skin)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnPlayerSkinChange(player, skin);
		}
	}

	internal void OnItemPickup(Player player, IPickup item)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnItemPickup(player, item);
		}
	}


	internal void OnPlayerStateChanged(Player player, Player.States? state)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnPlayerStateChanged(player, state);
		}
	}
}
