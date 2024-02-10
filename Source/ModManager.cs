using System.Collections.Frozen;

namespace Celeste64;

public sealed class ModManager
{
	// File extensions for which we shouldn't hot reload
	private static readonly FrozenSet<string> HotReloadIgnoredExtensions = ((string[])[
		".cs", ".csproj", ".sln", ".pdb", ".user" // C#/Rider related extensions
	]).ToFrozenSet();
	
	// Top-level mod directories for which we shouldn't hot reload
	private static readonly FrozenSet<string> HotReloadIgnoredFolders = ((string[])[
		".idea", "bin", "obj", // C#/Rider related folders
	]).ToFrozenSet();
	
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
			// Filter out paths that we should not reload assets for
			// Sometimes, the asset watcher returns just the directory name instead of filename, so we have to handle that.
			if (HotReloadIgnoredExtensions.Contains(Path.GetExtension(filepath)))
			{
				return;
			}
			
			var dir = Path.GetDirectoryName(filepath) ?? "";
			
			// Filter out top-level directories we don't want
			if (HotReloadIgnoredFolders.Contains(filepath))
			{
				return;
			}
			var firstSepIndex = filepath.IndexOfAny(['/', '\\']);
			if (firstSepIndex != -1)
			{
				var topLevelFolder = dir[..firstSepIndex];
				if (HotReloadIgnoredFolders.Contains(topLevelFolder))
				{
					return;
				}
			}

			if (filepath.StartsWith("Maps", StringComparison.Ordinal))
			{
				// Ignore the autosave folder
				if (dir.EndsWith("autosave", StringComparison.Ordinal)
				    || filepath.EndsWith("autosave", StringComparison.Ordinal))
				{
					return;
				}
			}

			Log.Info($"File Changed: {filepath} (From mod {ctx.Mod.ModInfo?.Name}). Reloading assets.");
		}
		else
		{
			Log.Info($"Mod archive for mod {ctx.Mod.ModInfo?.Name} changed. Reloading assets.");
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

	internal void OnGameLoad(Game game)
	{
		foreach (var mod in EnabledMods)
		{
			mod.game = game;
			mod.OnGameLoaded(game);
		}
	}

	internal void OnPreMapLoaded(World world, Map map)
	{
		foreach (var mod in EnabledMods)
		{
			mod.map = map;
			mod.OnPreMapLoaded(world, map);
		}
	}

	internal void OnMapLoaded(Map map)
	{
		foreach (var mod in EnabledMods)
		{
			mod.map = map;
			mod.OnMapLoaded(map);
		}
	}

	internal void OnWorldLoaded(World world)
	{
		foreach (var mod in EnabledMods)
		{
			mod.world = world;
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