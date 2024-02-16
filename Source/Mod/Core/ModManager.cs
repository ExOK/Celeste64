using System.Collections.Frozen;
using System.Collections.ObjectModel;

namespace Celeste64.Mod;

public static class ModManager
{
	public static LayeredFilesystem GlobalFilesystem { get; } = new();
		
	private static CancellationTokenSource _modFilesystemCleanupTimerToken = new();
	
	internal static List<GameMod> Mods = [];

	internal static IEnumerable<GameMod> EnabledMods { get {  return Mods.Where(mod => mod.Enabled); } }

	internal static VanillaGameMod? VanillaGameMod { get; set; }

	internal static GameMod? CurrentLevelMod { get; set; }

	internal static void Unload()
	{
		_modFilesystemCleanupTimerToken.Cancel();
		_modFilesystemCleanupTimerToken = new();
		HookManager.Instance.ClearHooks();

		var modsCopy = Mods.ToList();
		foreach (var mod in modsCopy)
		{
			DeregisterMod(mod);
		}
	}
	
	internal static void InitializeFilesystemBackgroundCleanup()
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
	
	internal static void RegisterMod(GameMod mod)
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

	internal static void DeregisterMod(GameMod mod)
	{
		Mods.Remove(mod);
		GlobalFilesystem.Remove(mod);

		if (mod.Filesystem is { } fs)
		{
			fs.OnFileChanged -= OnModFileChanged;
			fs.Dispose();
		}

		mod.ModInfo.AssemblyContext?.Dispose(); 
		
		if (!mod.Enabled)
		{
			mod.OnModUnloaded();
		}

		mod.OnUnloadedCleanup?.Invoke();
	}

	internal static void OnModFileChanged(ModFileChangedCtx ctx)
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
			    (dir.StartsWith(Assets.ShadersFolder) && extension == $".{Assets.ShadersExtension}") ||
			    (dir.StartsWith(Assets.FontsFolder) && extension is $".{Assets.FontsExtensionTTF}" or $".{Assets.FontsExtensionOTF}") ||
			    (dir.StartsWith(Assets.SpritesFolder) && extension == $".{Assets.SpritesExtension}") ||
			    (dir.StartsWith(Assets.SkinsFolder) && extension == $".{Assets.SkinsExtension}") ||
			    (dir.StartsWith(Assets.LibrariesFolder) && extension == $".{Assets.LibrariesExtensionAssembly}") ||
			    filepath == Assets.LevelsJSON ||			    
			    filepath == Assets.FujiJSON)
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
		
		// TODO: Only reload what actually changed
		Game.Instance.ReloadAssets();
	}

	internal static void Update(float deltaTime)
	{
		foreach (var mod in EnabledMods)
		{
			mod.Update(deltaTime);
		}
	}
	
	internal static void OnAssetsLoaded()
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnAssetsLoaded();
		}
	}

	internal static void OnSceneEntered(Scene scene)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnSceneEntered(scene);
		}
	}

	internal static void OnGameLoaded(Game game)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnGameLoaded(game);
		}
	}

	internal static void OnPreMapLoaded(World world, Map map)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnPreMapLoaded(world, map);
		}
	}

	internal static void OnMapLoaded(Map map)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnMapLoaded(map);
		}
	}

	internal static void OnWorldLoaded(World world)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnWorldLoaded(world);
		}
	}

	internal static void OnActorCreated(Actor actor)
	{ 
		foreach (var mod in EnabledMods)
		{
			mod.OnActorCreated(actor);
		}
	}

	internal static void OnActorAdded(Actor actor)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnActorAdded(actor);
		}
	}

	internal static void OnActorDestroyed(Actor actor)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnActorDestroyed(actor);
		}
	}

	internal static void OnPlayerKill(Player player)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnPlayerKilled(player);
		}
	}

	internal static void OnPlayerLanded(Player player)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnPlayerLanded(player);
		}
	}

	internal static void OnPlayerJumped(Player player, Player.JumpType jumpType)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnPlayerJumped(player, jumpType);
		}
	}

	internal static void OnPlayerSkinChange(Player player, SkinInfo skin)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnPlayerSkinChange(player, skin);
		}
	}

	internal static void OnItemPickup(Player player, IPickup item)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnItemPickup(player, item);
		}
	}


	internal static void OnPlayerStateChanged(Player player, Player.States? state)
	{
		foreach (var mod in EnabledMods)
		{
			mod.OnPlayerStateChanged(player, state);
		}
	}
}
