using MonoMod.RuntimeDetour;

namespace Celeste64.Mod;

public abstract class GameMod
{
	#region Internally Used Data
	internal Save.ModRecord ModSaveData { get { return Save.Instance.GetOrMakeMod(ModInfo.Id); } }

	// They get set as part of the Mod Loading step, not the constructor.
	internal IModFilesystem Filesystem { get; set; } = null!;
	internal ModInfo ModInfo { get; set; } = null!;

	// Used for storing the assets loaded for this mod specifically.
	internal readonly Dictionary<string, Map> Maps = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Shader> Shaders = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Texture> Textures = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Subtexture> Subtextures = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, SkinnedTemplate> Models = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Font> Fonts = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, FMOD.Sound> Sounds = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, FMOD.Sound> Music = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Dictionary<string, string>> Strings = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Dictionary<string, List<Language.Line>>> DialogLines = new(StringComparer.OrdinalIgnoreCase);
	internal readonly List<LevelInfo> Levels = new();

	/// <summary>
	/// Cleanup tasks that have to be performed when this mod gets unloaded.
	/// Called after <see cref="OnModUnloaded"/>, not inside of it,
	/// to make sure that a mod can't accidentally skip calling this due to not calling base.OnModUnloaded.
	/// </summary>
	internal Action? OnUnloadedCleanup { get; private set; }
	#endregion

	// This is here to give mods easier access to these objects, so they don't have to get them themselves
	// Warning, these may be null if they haven't been initialized yet, so you should always do a null check before using them.
	public Game? Game { get { return Game.Instance; } }
	public World? World { get { return Game != null ? Game.World : null; } }
	public Map? Map { get { return World != null ? World.Map : null; } }
	public Player? Player { get { return World != null ? World.Get<Player>() : null; } }

	// Common Metadata about this mod.
	public bool Enabled { get { return this is VanillaGameMod || ModSaveData.Enabled; } }

	/// <summary>
	/// List of currently used <see cref="ImGuiHandler"/>s by this mod.
	/// </summary>
	public List<ImGuiHandler> ImGuiHandlers = [];

	/// <summary>
	/// List of skins added by this mod.
	/// </summary>
	public readonly List<SkinInfo> Skins = [];
	
	/// <summary>
	/// Disables hook protections.
	/// Hooking Fuji or other mods can break and cause issues when they update.
	/// Please consider reaching out to the authors first, so they can provide a stable public API.
	/// By enabling this property, you understand those risks and are aware that your mod might break. 
	/// </summary>
	public bool PreventHookProtectionYesIKnowThisIsDangerousAndCanBreak { get; protected set; } = false;

	#region Save Functions
	// These functions allow modders to save data and get save data from the save file.
	// These are done as wrapper functions mostly to make it harder to accidentally mess up the save data in an unexpected way
	// And so we can change how they work later if needed.

	public string SaveString(string key, string value)
	{
		return ModSaveData.SetString(key, value);
	}
	public string GetString(string key)
	{
		return ModSaveData.GetString(key);
	}
	public int SaveInt(string key, int value)
	{
		return ModSaveData.SetInt(key, value);
	}
	public int GetInt(string key)
	{
		return ModSaveData.GetInt(key);
	}
	public float SaveFloat(string key, float value)
	{
		return ModSaveData.SetFloat(key, value);
	}
	public float GetFloat(string key)
	{
		return ModSaveData.GetFloat(key);
	}
	public bool SaveBool(string key, bool value)
	{
		return ModSaveData.SetBool(key, value);
	}
	public bool GetBool(string key)
	{
		return ModSaveData.GetBool(key);
	}
	#endregion


	/// <summary>
	/// Get all mods which depend on this mod.
	/// </summary>
	public List<GameMod> GetDependents()
	{
		List<GameMod> depMods = new List<GameMod>();

		foreach (GameMod mod in ModManager.Instance.Mods)
		{
			if (mod.ModInfo.Dependencies.ContainsKey(ModInfo.Id) && mod.Enabled)
			{
				depMods.Add(mod);
			}
		}

		return depMods;
	}

	/// <summary>
	/// Disables the mod "safely" (accounts for dependent mods, etc.)
	/// If it returns true, this means it is not safe to disable the mod.
	/// You should first simulate the operation with DisableSafe(true).
	/// If it is not safe to disable the mod (if the function returns true), it's recommended that you don't go through with it.
	/// </summary>
	/// <param name="simulate"></param>
	public bool DisableSafe(bool simulate)
	{
		bool shouldEvac = false;

		foreach (GameMod dependent in this.GetDependents())
		{
			if (!simulate)
			{
				Save.Instance.GetOrMakeMod(dependent.ModInfo.Id).Enabled = false;
				dependent.OnModUnloaded();
			}

			if (dependent == ModManager.Instance.CurrentLevelMod)
			{
				shouldEvac = true;
			} // We'll want to adjust behaviour if the current level's parent mod must be disabled.
		}

		if (!simulate) { this.OnModUnloaded(); }

		if (shouldEvac && !simulate)
		{
			Game.Instance.Goto(new Transition()
			{
				Mode = Transition.Modes.Replace,
				Scene = () => new Titlescreen(),
				ToPause = true,
				ToBlack = new AngledWipe(),
				PerformAssetReload = true
			});
		} // If necessary, evacuate to main menu!!

		return shouldEvac;
	}

	/// <summary>
	/// Enables the mod's dependencies.
	/// </summary>
	public void EnableDependencies()
	{
		foreach (var dep in ModInfo.Dependencies.Keys.ToList())
		{
			Save.Instance.GetOrMakeMod(dep).Enabled = true;
		}
	}

	/// <summary>
	/// This allows modders to add their own actors to the actor factory system.
	/// This can also be used to replace existing actors, but be warned that only one mod can replace something at a time.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="factory"></param>
	public void AddActorFactory(string name, Map.ActorFactory factory)
	{
		if (Map.ModActorFactories.TryAdd(name, factory))
		{
			OnUnloadedCleanup += () => Map.ModActorFactories.Remove(name);
		}
		else
		{
			Log.Warning($"An actor factory with the name {name} was already loaded. Factory won't be loaded.");
		}
	}

	// Passthrough functions to simplify adding stuff to the Hook Manager.
	public static void RegisterHook(Hook hook) => HookManager.Instance.RegisterHook(hook);
	public static void RegisterILHook(ILHook iLHook) => HookManager.Instance.RegisterILHook(iLHook);
	public static void RemoveHook(Hook hook) => HookManager.Instance.RemoveHook(hook);
	public static void RemoveILHook(ILHook iLHook) => HookManager.Instance.RemoveILHook(iLHook);
	/// <summary>
	/// Registers the provided custom player state,
	/// and ensures it will be deregistered once the mod unloads.
	/// </summary>
	public void AddPlayerState<T>() where T : CustomPlayerState, new()
	{
		CustomPlayerStateRegistry.Register<T>();
		OnUnloadedCleanup += CustomPlayerStateRegistry.Deregister<T>;
	}

	// Game Event Functions. These are used to provide an "interface" of sorts that mods can easily override.
	// They will not be called if the mod is disabled.

	/// <summary>
	/// Called when the Mod is first loaded, or when it becomes enabled
	/// </summary>
	public virtual void OnModLoaded(){}

	/// <summary>
	/// Called when a mod is unloaded, or when it becomes disabled
	/// </summary>
	public virtual void OnModUnloaded(){}

	/// <summary>
	/// Called once every frame
	/// </summary>
	/// <param name="deltaTime">How much time passed since the previous update</param>
	public virtual void Update(float deltaTime) {}

	/// <summary>
	/// Called at the very beginning of when the game is loaded
	/// </summary>
	/// <param name="game"></param>
	public virtual void OnGameLoaded(Game game){}

	/// <summary>
	/// Called after all assets have been loaded or reloaded.
	/// </summary>
	public virtual void OnAssetsLoaded() { }

	/// <summary>
	/// Called right before the Map load starts.
	/// This is probably the ideal place to register custom mod actors
	/// </summary>
	/// <param name="world">A reference to the world</param>
	/// <param name="map">A reference to the map that was loaded</param>
	public virtual void OnPreMapLoaded(World world, Map map){}

	/// <summary>
	/// Called after a map is finished loading.
	/// </summary>
	/// <param name="map">A reference to the map that was loaded</param>
	public virtual void OnMapLoaded(Map map){}

	/// <summary>
	/// Called after a scene transistion either when a scene is first loaded, or reloaded.
	/// </summary>
	/// <param name="scene">A reference to the Scene that was entered</param>
	public virtual void OnSceneEntered(Scene scene){}

	/// <summary>
	/// Called after the world finishes loading.
	/// </summary>
	/// <param name="world">A reference to the World object that was created</param>
	public virtual void OnWorldLoaded(World world){}

	/// <summary>
	/// Called whenever a new actor is first created.
	/// </summary>
	/// <param name="actor">A reference to the Actor that was created.</param>
	public virtual void OnActorCreated(Actor actor){}

	/// <summary>
	/// Called after an actor is actually added to the world.
	/// </summary>
	/// <param name="actor">A reference to the Actor that was added</param>
	public virtual void OnActorAdded(Actor actor){}

	/// <summary>
	/// Called when an actor is destroyed.
	/// </summary>
	/// <param name="actor">A reference to the actor that was destroyed</param>
	public virtual void OnActorDestroyed(Actor actor){}

	/// <summary>
	/// Called when the player is killed
	/// </summary>
	/// <param name="player">A reference to the player</param>
	public virtual void OnPlayerKilled(Player player) {}

	/// <summary>
	/// Called whenever a player lands on the ground.
	/// </summary>
	/// <param name="player">A reference to the player</param>
	public virtual void OnPlayerLanded(Player player) {}

	/// <summary>
	/// Called whenever a player jumps.
	/// </summary>
	/// <param name="player">A reference to the player</param>
	public virtual void OnPlayerJumped(Player player, Player.JumpType jumpType) { }


	/// <summary>
	/// Called whenever the player's state changes
	/// </summary>
	/// <param name="player">A reference to the player</param>
	/// <param name="state">The new state</param>
	public virtual void OnPlayerStateChanged(Player player, Player.States? state){}

	/// <summary>
	/// Called when the current skin is changed.
	/// </summary>
	/// <param name="player">A reference to the player</param>
	/// <param name="skin">The new skin that this changed to</param>
	public virtual void OnPlayerSkinChange(Player player, SkinInfo skin){}

	/// <summary>
	/// Called whenever an item is pickuped up by the player
	/// </summary>
	/// <param name="player">The player that picked up the item</param>
	/// <param name="item">The IPickup item that was picked up</param>
	public virtual void OnItemPickup(Player player, IPickup item){}
}
