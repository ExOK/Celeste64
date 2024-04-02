
using ImGuiNET;
namespace Celeste64.Mod;

internal class FujiDebugMenu : ImGuiHandler
{
	private bool visible = false;
	public override bool Active => Settings.EnableDebugMenu;
	public override bool Visible => visible;
	private DebugActorMenu debugActorMenu = new DebugActorMenu();

	public override void Update()
	{
		if (Input.Keyboard.Pressed(Keys.F6))
		{
			visible = !visible;
		}
		if (debugActorMenu.Active)
		{
			debugActorMenu.Update();
		}
	}

	public override void Render()
	{
		ImGui.SetNextWindowSizeConstraints(new Vec2(400, 640), new Vec2(float.PositiveInfinity, float.PositiveInfinity));
		ImGui.Begin("Celeste 64 ~ Debug Menu");

		// TODO: Possibly make a smarter system for handling sub-menus, if we add more of them.
		if (debugActorMenu.Visible)
		{
			debugActorMenu.Render();
		}
		else
		{
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

				if (ImGui.Button("Actor Properties"))
				{
					debugActorMenu.Visible = true;
				}
			}
		}

		ImGui.End();
	}
}
