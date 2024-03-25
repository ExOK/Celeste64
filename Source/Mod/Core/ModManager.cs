namespace Celeste64.Mod;

public sealed class ModManager
{
	private ModManager() { }

	private static ModManager? instance = null;
	public static ModManager Instance => instance ??= new ModManager();

	public LayeredFilesystem GlobalFilesystem { get; } = new();

	private CancellationTokenSource _modFilesystemCleanupTimerToken = new();

	internal List<GameMod> Mods = [];

	internal IEnumerable<GameMod> EnabledMods => Mods.Where(mod => mod.Enabled);

	internal IEnumerable<GameMod> EnabledModsWithLevels => Mods.Where(mod => mod.ModLevels.Count > 0 && mod.Enabled);

	internal VanillaGameMod? VanillaGameMod { get; set; }

	internal GameMod? CurrentLevelMod { get; set; }

	internal void Unload()
	{
		_modFilesystemCleanupTimerToken.Cancel();
		_modFilesystemCleanupTimerToken = new();
		HookManager.Instance.ClearHooks();

		// Unload in reverse order to
		// 1) Not need to make a copy, since entries are removed from 'Mods'
		// 2) Respect mod dependencies, so that dependencies are unloaded after the mod which requires them
		for (int i = Mods.Count - 1; i >= 0; i--)
		{
			DeregisterMod(Mods[i]);
		}
	}

	internal void InitializeFilesystemBackgroundCleanup()
	{
		// Initialize background mod filesystem cleanup task
		var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
		Task.Run(async () =>
		{
			while (await timer.WaitForNextTickAsync(_modFilesystemCleanupTimerToken.Token))
			{
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
		if (mod.Filesystem != null)
			mod.Filesystem.OnFileChanged += OnModFileChanged;

		if (mod.Enabled)
		{
			mod.OnModLoaded();
			mod.Loaded = true;
		}
	}

	internal void DeregisterMod(GameMod mod)
	{
		Mods.Remove(mod);
		GlobalFilesystem.Remove(mod);

		if (mod.Filesystem is { } fs)
		{
			fs.OnFileChanged -= OnModFileChanged;
			fs.Dispose();
		}

		if (mod.Loaded)
		{
			mod.OnModUnloaded();
			mod.Loaded = false;
		}

		mod.OnUnloadedCleanup?.Invoke();
		
		mod.ModInfo.AssemblyContext?.Dispose();
	}

	internal void OnModFileChanged(ModFileChangedCtx ctx)
	{
		if (ctx.Path is { } filepath)
		{
			var extension = Path.GetExtension(filepath);
			var dir = Path.GetDirectoryName(filepath) ?? "";

			// Important assets taken from Assets.Load()
			// TODO: Support non-toplevel mods?
			if ((dir.StartsWith(Assets.MapsFolder) && extension == $".{Assets.MapsExtension}" && !dir.StartsWith($"{Assets.MapsFolder}/autosave")) ||
				(dir.StartsWith(Assets.TexturesFolder) && extension == $".{Assets.TexturesExtension}") ||
				(dir.StartsWith(Assets.FacesFolder) && extension == $".{Assets.FacesExtension}") ||
				(dir.StartsWith(Assets.ModelsFolder) && extension == $".{Assets.ModelsExtension}") ||
				(dir.StartsWith(Assets.TextFolder) && extension == $".{Assets.TextExtension}") ||
				(dir.StartsWith(Assets.AudioFolder) && extension == $".{Assets.AudioExtension}") ||
				(dir.StartsWith(Assets.SoundsFolder) && extension == $".{Assets.SoundsExtension}") ||
				(dir.StartsWith(Assets.MusicFolder) && extension == $".{Assets.MusicExtension}") ||
				(dir.StartsWith(Assets.ShadersFolder) && extension == $".{Assets.ShadersExtension}") ||
				(dir.StartsWith(Assets.FontsFolder) && extension is $".{Assets.FontsExtensionTTF}" or $".{Assets.FontsExtensionOTF}") ||
				(dir.StartsWith(Assets.SpritesFolder) && extension == $".{Assets.SpritesExtension}") ||
				(dir.StartsWith(Assets.SkinsFolder) && extension == $".{Assets.SkinsExtension}") ||
				(dir == Assets.LibrariesFolder && extension is $".{Assets.LibrariesExtensionAssembly}") ||
				(dir == $"{Assets.LibrariesFolder}/lib" && extension is ".dll" or ".so" or ".dylib") ||
				filepath.ToLower() == Assets.LevelsJSON.ToLower() ||
				filepath.ToLower() == Assets.FujiJSON.ToLower())
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

	internal void OnPlayerJumped(Player player, Player.JumpType jumpType)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnPlayerJumped(player, jumpType);
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
