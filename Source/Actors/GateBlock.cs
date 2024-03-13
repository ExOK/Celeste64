namespace Celeste64;

public class GateBlock(Vec3 end) : Solid
{
	public virtual float Acceleration => 400;
	public virtual float MaxSpeed => 200;

	public Vec3 Start;
	public Vec3 End = end;
	public bool Opened;
	public Sound? Sfx;

	public readonly Routine Routine = new();

	public override void Added()
	{
		Sfx = World.Add(new Sound(this, Celeste64.Sfx.sfx_touch_switch_gate_open_move));
		UpdateOffScreen = true;
		Start = Position;
	}

	public override void Update()
	{
		base.Update();

		if (!Opened && !Coin.AnyRemaining(World))
		{
			Opened = true;
			Routine.Run(Sequence());
		}
		else if (Routine.IsRunning)
		{
			Routine.Update();
		}
	}

	public virtual CoEnumerator Sequence()
	{
		TShake = .2f;
		Sfx?.Resume();
		yield return .2f;

		var normal = (End - Position).Normalized();
		while (Position != End && Vec3.Dot((End - Position).Normalized(), normal) >= 0)
		{
			Velocity = Utils.Approach(Velocity, MaxSpeed * normal, Acceleration * Time.Delta);
			yield return Co.SingleFrame;
		}

		Audio.Play(Celeste64.Sfx.sfx_touch_switch_gate_finish, Position);
		Sfx?.Stop();
		Sfx = null;
		Velocity = Vec3.Zero;
		MoveTo(End);
		TShake = .2f;
	}
}
