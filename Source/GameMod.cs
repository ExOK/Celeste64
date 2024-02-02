﻿namespace Celeste64;

public abstract class GameMod
{
	internal Game? game;
	internal World? world;
	internal Map? map;

	public Game? Game { get { return game; } }
	public World? World { get { return world; } }
	public Map? Map { get { return map; } }

	internal string modName = "";

	public string ModName { get { return modName; } }
	
	public IModFilesystem Filesystem { get; internal set; }

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
		if (!Map.ModActorFactories.ContainsKey(name)) {
			Map.ModActorFactories.Add(name, factory);
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

	public virtual void OnPlayerSkinChange(Player player, Assets.SkinInfo skin)
	{

	}


	public virtual void OnItemPickup(Player ply, IPickup item)
	{

	}

	public virtual void Update(float deltaTime)
	{

	}
}
