namespace Celeste64;

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

	public GameMod()
	{
	}

	public void AddActorFactory(string name, Map.ActorFactory factory)
	{
		Map.ModActorFactories.Add(name, factory);
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
