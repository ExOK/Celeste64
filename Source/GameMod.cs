namespace Celeste64;

public abstract class GameMod
{
	#region Internally Used Data
	internal Game? game;
	internal World? world;
	internal Map? map;

	public ModInfo? ModInfo { get; internal set; }
	public string ModFolder { get; internal set; } = "";
	public bool Enabled { get { return this is VanillaGameMod || ModSaveData.Enabled; } }
	internal Save.ModRecord ModSaveData { get { return Save.Instance.GetOrMakeMod(ModInfo?.Id ?? ""); } }
	internal IModFilesystem? Filesystem { get; set; }

	// Used for storing the assets loaded for this mod specifically.
	internal readonly Dictionary<string, Map> Maps = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Shader> Shaders = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Texture> Textures = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Subtexture> Subtextures = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, SkinnedTemplate> Models = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Font> Fonts = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Dictionary<string, string>> Strings = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Dictionary<string, List<Language.Line>>> DialogLines = new(StringComparer.OrdinalIgnoreCase);
	internal readonly List<LevelInfo> Levels = new();
	#endregion


	// This is here to give mods easier access to these objects, so they don't have to get them themselves
	// Warning, these may be null if they haven't been initialized yet, so you should always do a null check before using them.
	public Game? Game { get { return game; } }
	public World? World { get { return world; } }
	public Map? Map { get { return map; } }

	/// <summary>
	/// Cleanup tasks that have to be performed when this mod gets unloaded.
	/// Called after <see cref="OnModUnloaded"/>, not inside of it,
	/// to make sure that a mod can't accidentally skip calling this due to not calling base.OnModUnloaded.
	/// </summary>
	internal Action? OnUnloadedCleanup { get; private set; }

	public GameMod()
	{
	}

	#region Save Functions
	/// <summary>
	/// These functions allow modders to save data and get save data from the save file.
	/// These are done as wrapper functions mostly to make it harder to accidentally mess up the save data in an unexpected way
	/// And so we can change how they work later if needed.
	/// </summary>
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
	#endregion


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

	/// <summary>
	/// Registers the provided custom player state,
	/// and ensures it will be deregistered once the mod unloads.
	/// </summary>
	public void AddPlayerState<T>() where T : CustomPlayerState, new()
	{
		CustomPlayerStateRegistry.Register<T>();
		OnUnloadedCleanup += CustomPlayerStateRegistry.Deregister<T>;
	}

	/// <summary>
	/// Saves data to the save file for this mod, that can be accessed with a given key.
	/// </summary>
	public void SaveData(string key, string data)
	{
		if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(data) && ModInfo != null)
		{
			Save.Instance.GetOrMakeMod(ModInfo.Id).Settings.TryAdd(key, data);
		}
	}

	// Event Functions. Purposely left blank.
	public virtual void OnModLoaded()
	{

	}

	public virtual void OnModUnloaded()
	{

	}
	public virtual void OnAssetsLoaded()
	{

	}

	public virtual void OnGameLoaded(Game game)
	{ 
			
	}

	public virtual void OnPreMapLoaded(World world, Map map)
	{

	}

	public virtual void OnMapLoaded(Map map)
	{

	}

	public virtual void OnWorldLoaded(World world)
	{

	}

	public virtual void OnActorCreated(Actor actor)
	{

	}

	public virtual void OnActorAdded(Actor actor)
	{

	}

	public virtual void OnActorDestroyed(Actor actor)
	{

	}

	public virtual void OnPlayerKilled(Player ply)
	{

	}

	public virtual void OnPlayerStateChanged(Player ply, Player.States? state)
	{

	}

	public virtual void OnPlayerLanded(Player ply)
	{

	}

	public virtual void OnPlayerSkinChange(Player player, SkinInfo skin)
	{

	}


	public virtual void OnItemPickup(Player ply, IPickup item)
	{

	}

	public virtual void Update(float deltaTime)
	{

	}
}
