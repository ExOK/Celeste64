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

	private List<GameMod> mods = new List<GameMod>();

	public void RegisterMod(GameMod mod)
	{
		mods.Add(mod);
		mod.OnModLoaded();
	}

	public void DeregisterMod(GameMod mod)
	{
		mods.Remove(mod);
		mod.OnModUnloaded();
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