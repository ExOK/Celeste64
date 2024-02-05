
namespace Celeste64;

public class Cutscene : Actor, IHaveUI
{
	public readonly Func<Cutscene, CoEnumerator> Running;
	public readonly Routine Routine;
	private float ease = 0.0f;

	private struct Saying
	{
		public string Face;
		public string Text;
		public int Characters;
		public float Ease;
		public bool Talking;
	}
	private Saying saying;
	private AudioHandle dialogSnapshot;
	private float timer = 0;

	public bool FreezeGame = false;

	public Cutscene(Func<Cutscene, CoEnumerator> run, bool freezeEverythingExceptMe = false)
	{
		Running = run;
		Routine = new();
		FreezeGame = freezeEverythingExceptMe;
		UpdateOffScreen = true;
	}

    public override void Destroyed()
    {
		Audio.StopBus(Sfx.bus_dialog, false);
		dialogSnapshot.Stop();
    }

    public CoEnumerator Say(List<Language.Line> lines)
	{
		foreach (var line in lines)
		{
			yield return Co.Run(Say(line.Face, line.Text, line.Voice));
		}

		Audio.StopBus(Sfx.bus_dialog, false);
	}

	public CoEnumerator Say(string face, string line, string? voice = null)
	{
		saying = new Saying() { Face = $"faces/{face}", Text = line, Talking = false };
		dialogSnapshot = Audio.Play(Sfx.snapshot_dialog);

		// ease in
		while (saying.Ease < 1.0f)
		{
			saying.Ease += Time.Delta * 10.0f;
			yield return Co.SingleFrame;
		}

		// start voice sound
		if (!string.IsNullOrEmpty(voice))
			Audio.Play($"event:/sfx/ui/dialog/{voice}");

		// print out dialog
		saying.Talking = saying.Text.Length > 3;
		var counter = 0.0f;
		while (saying.Characters < saying.Text.Length)
		{
			counter += 30 * Time.Delta;
			while (counter >= 1)
			{
				saying.Characters++;
				counter -= 1;
			}

			if (Controls.Confirm.Pressed || Controls.Cancel.Pressed)
			{
				saying.Characters = saying.Text.Length;
				yield return Co.SingleFrame;
				break;
			}

			yield return Co.SingleFrame;
		}

		// wait for confirm
		saying.Talking = false;
		while (!Controls.Confirm.Pressed && !Controls.Cancel.Pressed)
			yield return Co.SingleFrame;
		Audio.Play(Sfx.ui_dialog_advance);

		// ease out
		while (saying.Ease > 0.0f)
		{
			saying.Ease -= Time.Delta * 10.0f;
			yield return Co.SingleFrame;
		}
		
		dialogSnapshot.Stop();
		saying = new();
	}

	public CoEnumerator MoveToDistance(Actor? actor, Vec2 position, float distance)
	{
		if (actor != null)
		{
			var normal = (actor.Position.XY() - position).Normalized();
			yield return Co.Run(MoveTo(actor, position + normal * distance));
		}

		yield return Co.Continue;
	}

	public CoEnumerator MoveTo(Actor? actor, Vec2 position)
	{
		if (actor != null)
		{
			var player = actor as Player;

			if ((actor.Position.XY() - position).Length() > 4)
			{
				yield return Co.Run(Face(actor, new Vec3(position, 0)));

				player?.Model.Play("Run");
			}

			while (actor.Position.XY() != position)
			{
				var v2 = actor.Position.XY();
				Calc.Approach(ref v2, position, 50 * Time.Delta);
				actor.Position = actor.Position.WithXY(v2);
				yield return Co.SingleFrame;
			}

			player?.Model.Play("Idle");
		}
	}

	public CoEnumerator Face(Actor? actor, Vec3 target)
	{
		if (actor != null)
		{
			var facing = (target - actor.Position).XY().Normalized();
			var current = actor.Facing;

			while (MathF.Abs(facing.Angle() - current.Angle()) > 0.05f)
			{
				current = Calc.AngleToVector(Calc.AngleApproach(current.Angle(), facing.Angle(), MathF.Tau * 1.5f * Time.Delta));
				if (actor is Player player)
					player.SetTargetFacing(current);
				else
					actor.Facing = current;
				yield return Co.SingleFrame;
			}
		}

		yield return Co.Continue;
	}

	public CoEnumerator FaceEachOther(Actor? a0, Actor? a1)
	{
		if (a0 != null && a1 != null)
		{
			yield return Co.Run(Face(a0, a1.Position));
			yield return Co.Run(Face(a1, a0.Position));
		}
		yield return Co.Continue;
	}

	private CoEnumerator PerformCutscene()
	{
		Audio.Play(Sfx.sfx_readsign_in);

		while (ease < 1.0f)
		{
			Calc.Approach(ref ease, 1, Time.Delta * 10);
			yield return Co.SingleFrame;
		}

		yield return Co.Run(Running(this));

		Audio.Play(Sfx.sfx_readsign_out);

		while (ease < 1.0f)
		{
			Calc.Approach(ref ease, 0, Time.Delta * 10);
			yield return Co.SingleFrame;
		}
	}

	public override void Added()
	{
		Routine.Run(PerformCutscene());
	}

	public override void Update()
	{
		Routine.Update();
		timer += Time.Delta;

		if (!Routine.IsRunning)
			World.Destroy(this);
	}

	public void RenderUI(Batcher batch, Rect bounds)
	{
		const float BarSize = 40 * Game.RelativeScale;
		const float PortraitSize = 128 * Game.RelativeScale;
		const float TopOffset = 100 * Game.RelativeScale;
		const float EaseOffset = 32 * Game.RelativeScale;
		const float Padding = 8 * Game.RelativeScale;

		batch.Rect(new Rect(bounds.X, bounds.Y, bounds.Width, BarSize * ease), Color.Black);
		batch.Rect(new Rect(bounds.X, bounds.Bottom - BarSize * ease, bounds.Width, BarSize * ease), Color.Black);

		if (saying.Ease > 0 && !string.IsNullOrEmpty(saying.Text) && !World.Paused)
		{
			var ease = Ease.Cube.Out(saying.Ease);
			var font = Language.Current.SpriteFont;
			var size = font.SizeOf(saying.Text);
			var pos = bounds.TopCenter + new Vec2(0, TopOffset) - size / 2 - Vec2.One * Padding + new Vec2(0, EaseOffset * (1 - ease));

			Texture? face = null;

			// try to find taling face
			if (!string.IsNullOrEmpty(saying.Face))
			{
				var oddFrame = Time.BetweenInterval(timer, 0.3f, 0);
				var src = saying.Face;
				if (saying.Talking && Assets.Textures.ContainsKey($"{src}Talk00"))
				{
					if (oddFrame && Assets.Textures.ContainsKey($"{src}Talk01"))
						face = Assets.Textures[$"{src}Talk01"];
					else
						face = Assets.Textures[$"{src}Talk00"];
				}
				else if (!saying.Talking && Assets.Textures.ContainsKey($"{src}Idle00"))
				{
					// idle is blinking so hold on first frame for a long time, then 2nd frame for less time
					oddFrame = (timer % 3) > 2.8f;
					if (oddFrame && Assets.Textures.ContainsKey($"{src}Idle01"))
						face = Assets.Textures[$"{src}Idle01"];
					else
						face = Assets.Textures[$"{src}Idle00"];
				}
				else if (oddFrame && Assets.Textures.ContainsKey($"{src}01"))
				{
					face = Assets.Textures[$"{src}01"];
				}
				else if (Assets.Textures.ContainsKey($"{src}00"))
				{
					face = Assets.Textures[$"{src}00"];
				}
				else
				{
					face = Assets.Textures.GetValueOrDefault(src);
				}
			}

			if (face != null)
				pos.X += PortraitSize / 3;

			var box = new Rect(pos.X, pos.Y, size.X + Padding * 2, size.Y + Padding * 2);
			batch.RectRounded(box + new Vec2(0, 1), 4, Color.Black);
			batch.RectRounded(box, 4, Color.White);

			if (face != null)
			{
				var faceBox = new Rect(pos.X - PortraitSize * 0.8f, pos.Y + box.Height / 2 - PortraitSize / 2 - 10, PortraitSize, PortraitSize);
				batch.ImageFit(new Subtexture(face), faceBox, Vec2.One * 0.5f, Color.White, false, false);
			}

			batch.Text(font, saying.Text.AsSpan(0, saying.Characters), pos + new Vec2(Padding, Padding), Color.Black);
		}
	}
}
