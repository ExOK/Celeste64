using static Celeste64.Assets;

namespace Celeste64;

public abstract class GameMod
{
	internal Game? game;
	internal World? world;
	internal Map? map;

	public Game? Game { get { return game; } }
	public World? World { get { return world; } }
	public Map? Map { get { return map; } }

	public ModInfo? ModInfo { get; internal set; }

	public string ModFolder { get; internal set; } = "";

	// Todo: Hook up way to enable and disable mods
	public bool Enabled { get { return true; } }

	internal readonly Dictionary<string, Map> Maps = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Shader> Shaders = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Texture> Textures = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, SkinnedTemplate> Models = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Font> Fonts = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, string> Strings = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, List<Language.Line>> DialogLines = new(StringComparer.OrdinalIgnoreCase);

	//internal readonly Dictionary<string, List<DialogLine>> Dialog = new(StringComparer.OrdinalIgnoreCase);
	internal readonly List<LevelInfo> Levels = new();

	public IModFilesystem? Filesystem { get; internal set; }

	/// <summary>
	/// Cleanup tasks that have to be performed when this mod gets unloaded.
	/// Called after <see cref="OnModUnloaded"/>, not inside of it,
	/// to make sure that a mod can't accidentally skip calling this due to not calling base.OnModUnloaded.
	/// </summary>
	internal Action? OnUnloadedCleanup { get; private set; }

	public GameMod()
	{
	}

	public void AddActorFactory(string name, Map.ActorFactory factory)
	{
		if (Map.ModActorFactories.TryAdd(name, factory)) {
			OnUnloadedCleanup += () => Map.ModActorFactories.Remove(name);
		}
		else
			Log.Warning($"An actor factory with the name {name} was already loaded. Factory won't be loaded.");
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

	// Event Functions. Purposely left blank.

	public virtual void OnModLoaded()
	{

	}

	public virtual void OnModUnloaded()
	{

	}

	public virtual void OnGameLoaded(Game game)
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
