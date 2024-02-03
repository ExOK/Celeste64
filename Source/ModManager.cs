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

	private List<GameMod> mods = [];

	internal void Unload()
	{
		_modFilesystemCleanupTimerToken.Cancel();
		_modFilesystemCleanupTimerToken = new();

		var modsCopy = mods.ToList();
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
				foreach (var mod in mods)
				{
					mod.Filesystem.BackgroundCleanup();
				}
			}
		}, _modFilesystemCleanupTimerToken.Token);
	}
	
	internal void RegisterMod(GameMod mod)
	{
		mods.Add(mod);
		GlobalFilesystem.Add(mod);
		mod.Filesystem.OnFileChanged += OnModFileChanged;
		mod.OnModLoaded();
	}

	internal void DeregisterMod(GameMod mod)
	{
		mods.Remove(mod);
		GlobalFilesystem.Remove(mod);
		mod.Filesystem.Dispose();
		mod.Filesystem.OnFileChanged -= OnModFileChanged;
		mod.OnModUnloaded();
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
			
			Log.Info($"File Changed: {filepath} (From mod {ctx.Mod.ModName}). Reloading assets.");
		}
		else
		{
			Log.Info($"Mod archive for mod {ctx.Mod.ModName} changed. Reloading assets.");
		}
		
		Game.Instance.ReloadAssets();
	}

	internal void Update(float deltaTime)
	{
		foreach (var mod in mods)
		{
			mod.Update(deltaTime);
		}
	}

	internal void OnGameLoad(Game game)
	{
		foreach (var mod in mods)
		{
			mod.game = game;
			mod.OnGameLoaded(game);
		}
	}

	internal void OnMapLoaded(Map map)
	{
		foreach (var mod in mods)
		{
			mod.map = map;
			mod.OnMapLoaded(map);
		}
	}

	internal void OnWorldLoaded(World world)
	{
		foreach (var mod in mods)
		{
			mod.world = world;
			mod.OnWorldLoaded(world);
		}
	}

	internal void OnActorCreated(Actor actor)
	{ 
		foreach (var mod in mods)
		{
			mod.OnActorCreated(actor);
		}
	}

	internal void OnActorAdded(Actor actor)
	{
		foreach (var mod in mods)
		{
			mod.OnActorAdded(actor);
		}
	}

	internal void OnActorDestroyed(Actor actor)
	{
		foreach (var mod in mods)
		{
			mod.OnActorDestroyed(actor);
		}
	}

	internal void OnPlayerKill(Player player)
	{
		foreach (var mod in mods)
		{
			mod.OnPlayerKilled(player);
		}
	}

	internal void OnPlayerLanded(Player player)
	{
		foreach (var mod in mods)
		{
			mod.OnPlayerLanded(player);
		}
	}

	internal void OnPlayerSkinChange(Player player, Assets.SkinInfo skin)
	{
		foreach (var mod in mods)
		{
			mod.OnPlayerSkinChange(player, skin);
		}
	}

	internal void OnItemPickup(Player player, IPickup item)
	{
		foreach (var mod in mods)
		{
			mod.OnItemPickup(player, item);
		}
	}


	internal void OnPlayerStateChanged(Player player, Player.States? state)
	{
		foreach (var mod in mods)
		{
			mod.OnPlayerStateChanged(player, state);
		}
	}
}