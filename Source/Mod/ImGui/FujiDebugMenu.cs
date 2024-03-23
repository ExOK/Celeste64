
using ImGuiNET;
using System.Diagnostics;
namespace Celeste64.Mod;

internal class FujiDebugMenu : ImGuiHandler
{
	private bool visible = false;
	public override bool Active => Save.Instance.EnableDebugMenu;
	public override bool Visible => visible;

	bool playerDataVisible;
	Vec3 newPlayerPos = new Vec3();
	Vec3 newPlayerScale = new Vec3();
	bool lockScale;

	bool actorListWindowVisible;
	bool actorTypeListWindowVisible;
	bool actorPropertiesWindowVisible;
	Type typeOfActorInWindow;
	string actorInWindowName;
	Actor actorInWindow;
	Vec3 newActorPos = new Vec3();

	public override void Update()
	{
		if (Input.Keyboard.Pressed(Keys.F6))
		{
			visible = !visible;
		}
	}

	void RenderActorWindow()
	{
		ImGui.Begin(actorInWindowName);
		newActorPos = actorInWindow.Position;
		ImGui.DragFloat3("Position", ref newActorPos);
		actorInWindow.Position = newActorPos;
		ImGui.End();
	}

	void RenderActorTypeListWindow(World world)
	{
		int i = 0;
		ImGui.Begin($"{typeOfActorInWindow.Name} ~ Debug");
		foreach (Actor actor in world.Actors)
		{
			if (actor.GetType().Name == typeOfActorInWindow.Name)
			{
				if (ImGui.Button($"{i + 1}. " + actor.GetType().Name))
				{
					actorPropertiesWindowVisible = !actorPropertiesWindowVisible;
					actorInWindowName = $"{i + 1}. " + actor.GetType().Name;
					actorInWindow = actor;
				}
				i++;
			}
		}
		ImGui.End();
	}

	void RenderActorListWindow(List<Type> actorTypes)
	{
		ImGui.Begin("Actors ~ Debug");
		foreach (var type in actorTypes)
		{
			if (ImGui.Button(type.Name))
			{
				typeOfActorInWindow = type;
				actorTypeListWindowVisible = !actorTypeListWindowVisible;
			}
		}
		ImGui.End();
	}

	public override void Render()
	{
		ImGui.SetNextWindowSizeConstraints(new Vec2(300, 300), new Vec2(float.PositiveInfinity, float.PositiveInfinity));
		ImGui.Begin("Celeste 64 ~ Debug Menu");
		Debug.WriteLine(Path.GetFullPath(Path.Join(Assets.ContentPath, "RenogareTrue.ttf")));
		if (Game.Instance.Scene is World && ModManager.Instance.CurrentLevelMod != null)
		{
			if (ImGui.BeginMenu("Open Map"))
			{
				foreach (var kvp in ModManager.Instance.CurrentLevelMod.Maps)
				{
					if (ImGui.MenuItem(kvp.Key))
					{
						Game.Instance.Goto(new Transition()
						{
							Mode = Transition.Modes.Replace,
							Scene = () => new World(new(kvp.Key, Save.CurrentRecord.Checkpoint, false, World.EntryReasons.Entered)),
							ToBlack = new SpotlightWipe(),
							FromBlack = new SpotlightWipe(),
							StopMusic = true,
							HoldOnBlackFor = 0
						});
					}
				}
				ImGui.EndMenu();
			}
		}

		if (Game.Instance.Scene is World world && world.Get<Player>() is { } player)
		{
			if (world.All<Checkpoint>().Any() && ImGui.BeginMenu("Go to Checkpoint"))
			{
				int i = 0;
				foreach (var actor in world.All<Checkpoint>())
				{
					if (actor is Checkpoint checkpoint)
					{
						string checkpointName = string.IsNullOrEmpty(checkpoint.CheckpointName) ? $"Checkpoint {i}" : checkpoint.CheckpointName;
						if (ImGui.MenuItem(checkpointName))
						{
							player.Position = checkpoint.Position;
						}
						i++;
					}
				}
				ImGui.EndMenu();
			}

			if (ImGui.BeginMenu("Player Actions"))
			{
				if (ImGui.MenuItem("Kill Player"))
				{
					player.Kill();
				}
				if (ImGui.MenuItem("Give Double Dash"))
				{
					player.DashesLocal = 2;
				}
				if (ImGui.MenuItem("Toggle Debug Fly"))
				{
					if (player.StateMachine.State != Player.States.DebugFly)
					{
						player.StateMachine.State = Player.States.DebugFly;
					}
					else
					{
						player.StateMachine.State = Player.States.Normal;
					}
				}
				ImGui.EndMenu();
			}

			if (playerDataVisible)
			{
				ImGui.Begin("Player Data ~ Debug");
				newPlayerPos = player.Position;
				ImGui.DragFloat3("Position", ref newPlayerPos);
				player.Position = newPlayerPos;

				if (!lockScale) newPlayerScale = player.ModelScale;
				ImGui.Checkbox("Lock Scale?", ref lockScale);
				ImGui.DragFloat3("Scale", ref newPlayerScale);
				player.ModelScale = newPlayerScale;

				ImGui.BeginDisabled(true);
				ImGui.Checkbox("Grounded?", ref player.OnGround);
				ImGui.EndDisabled();


				ImGui.End();
			}

			if (actorListWindowVisible)
			{
				List<Type> actorTypes = new List<Type>();
				foreach (Actor actor in world.Actors)
				{
					if (actorTypes.Contains(actor.GetType())) continue;
					actorTypes.Add(actor.GetType());
				}
				RenderActorListWindow(actorTypes);
			} else actorTypeListWindowVisible = false;

			if (actorTypeListWindowVisible)
				RenderActorTypeListWindow(world);
			else actorPropertiesWindowVisible = false;

			if (actorPropertiesWindowVisible)
			{
				RenderActorWindow();
			}

			if (ImGui.Button("Player Data"))
				playerDataVisible = !playerDataVisible;
			if (ImGui.Button("Actors in World"))
				actorListWindowVisible = !actorListWindowVisible;
		}

		ImGui.End();
	}
}
