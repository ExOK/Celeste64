namespace Celeste64;

public class FallingBlock : Solid, IUnlockStrawberry
{
	public virtual float MaxFallSpeed => -160;
	public virtual float Gravity => 600;

	public enum States { Wait, Shake, Fall, Landed, Respawn }

	public Vec3? EndPosition;
	public bool Secret = false;

	public States State = States.Wait;
	public Vec3 StartPosition;
	public bool Triggered;
	public float RespawnDelay;

	public virtual bool Satisfied => State == States.Landed;

	public override void Added()
	{
		base.Added();

		StartPosition = Position;
		if (World.SolidRayCast(WorldBounds.Center, -Vec3.UnitZ, 200, out var hit)
		&& hit.Actor is not FallingBlock)
			EndPosition = hit.Point - Vec3.UnitZ * LocalBounds.Min.Z;
	}

	public virtual void Trigger()
	{
		UpdateOffScreen = true;
		Triggered = true;
	}

	public override void Update()
	{
		base.Update();

		if (State == States.Wait)
		{
			if (Triggered || HasPlayerRider())
			{
				Audio.Play(Sfx.sfx_fallingblock_shake, Position);
				State = States.Shake;
				TShake = .4f;
				UpdateOffScreen = true;

				if (Secret)
					Audio.Play(Sfx.sfx_secret, Position);
			}
		}
		else if (State == States.Shake)
		{
			if (TShake <= 0)
			{
				Audio.Play(Sfx.sfx_fallingblock_fall, Position);
				State = States.Fall;
			}
		}
		else if (State == States.Fall)
		{
			Calc.Approach(ref Velocity.Z, MaxFallSpeed, Gravity * Time.Delta);

			if (EndPosition.HasValue)
			{
				if (Position.Z <= EndPosition.Value.Z)
				{
					Audio.Play(Sfx.sfx_fallingblock_land, Position);
					State = States.Landed;
					TShake = .2f;
					Velocity = Vec3.Zero;
					MoveTo(EndPosition.Value);
				}
			}
			else if (WorldBounds.Max.Z < World.DeathPlane - 10)
			{
				if (World.Entry.Submap || !string.IsNullOrEmpty(GroupName) || Secret)
				{
					Destroy();
				}
				else
				{
					RespawnDelay = 5.0f;
					Velocity = Vec3.Zero;
					State = States.Respawn;
				}
			}
		}
		else if (State == States.Landed)
		{
			UpdateOffScreen = false;
		}
		else if (State == States.Respawn)
		{
			RespawnDelay -= Time.Delta;
			if (RespawnDelay <= 0)
			{
				MoveTo(StartPosition);
				TShake = 0.2f;
				Triggered = false;
				State = States.Wait;
				UpdateOffScreen = false;
			}
		}
	}

	public virtual void Destroy()
	{
		foreach (var att in Attachers)
			World.Destroy(att);
		World.Destroy(this);
	}

	public override void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		if (State != States.Respawn)
			base.CollectModels(populate);
	}
}
