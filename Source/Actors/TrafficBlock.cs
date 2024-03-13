namespace Celeste64;

public class TrafficBlock(Vec3 end) : Solid
{
	public virtual float Acceleration => 400;
	public virtual float MaxSpeed => 600;
	public virtual float ReturnSpeed => 50;

	public Vec3 Start;
	public Vec3 End = end;

	public readonly Routine Routine = new();
	public Sound? SfxMove;
	public Sound? SfxRetract;

	public override void Added()
	{
		base.Added();
		Start = Position;
		Routine.Run(Sequence());
		SfxMove = World.Add(new Sound(this, Sfx.sfx_zipmover_loop));
		SfxRetract = World.Add(new Sound(this, Sfx.sfx_zipmover_retract_loop));
	}

	public override void Update()
	{
		base.Update();
		Routine.Update();
	}

	private CoEnumerator Sequence()
	{
		while (true)
		{
			while (!HasPlayerRider())
				yield return Co.SingleFrame;

			Audio.Play(Sfx.sfx_zipmover_start, Position);
			TShake = .15f;
			UpdateOffScreen = true;
			yield return .15f;

			// move to target
			{
				SfxMove?.Resume();
				var target = End;
				var normal = (target - Position).Normalized();
				while (Position != target && Vec3.Dot((target - Position).Normalized(), normal) >= 0)
				{
					Velocity = Utils.Approach(Velocity, MaxSpeed * normal, Acceleration * Time.Delta);
					yield return Co.SingleFrame;
				}

				SfxMove?.Stop();
				Velocity = Vec3.Zero;
				MoveTo(target);
			}

			Audio.Play(Sfx.sfx_zipmover_stop, Position);
			TShake = .2f;
			yield return .8f;

			// Move back to start
			{
				Audio.Play(Sfx.sfx_zipmover_retract_start, Position);
				SfxRetract?.Resume();
				var target = Start;
				var normal = (target - Position).Normalized();
				while (Vec3.Dot((target - Position).Normalized(), normal) >= 0)
				{
					Velocity = normal * ReturnSpeed;
					yield return Co.SingleFrame;
				}

				SfxRetract?.Stop();
				Velocity = Vec3.Zero;
				MoveTo(target);
			}

			//Reactivate
			{
				Audio.Play(Sfx.sfx_zipmover_retract_stop, Position);
				TShake = .1f;
				UpdateOffScreen = false;
				yield return .5f;
			}
		}
	}
}
