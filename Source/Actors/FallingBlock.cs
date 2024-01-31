
namespace Celeste64;

public sealed class FallingBlock : Solid, IUnlockStrawberry
{
	private const float MaxFallSpeed = -160;
	private const float Gravity = 600;

	private enum States { Wait, Shake, Fall, Landed, Respawn }

	public Vec3? EndPosition;
	public bool Secret = false;

	private States state = States.Wait;
	private Vec3 startPosition;
	private bool triggered;
	private float respawnDelay;

	public bool Satisfied => state == States.Landed;

	public override void Added()
	{
		base.Added();

		startPosition = Position;
		if (World.SolidRayCast(WorldBounds.Center, -Vec3.UnitZ, 200, out var hit)
		&& hit.Actor is not FallingBlock)
			EndPosition = hit.Point - Vec3.UnitZ * LocalBounds.Min.Z;
	}

	public void Trigger()
	{
		UpdateOffScreen = true;
		triggered = true;
	}

	public override void Update()
	{
		base.Update();

		if (state == States.Wait)
		{
			if (triggered || HasPlayerRider())
			{
				Audio.Play(Sfx.sfx_fallingblock_shake, Position);
				state = States.Shake;
				TShake = .4f;
				UpdateOffScreen = true;

				if (Secret)
					Audio.Play(Sfx.sfx_secret, Position);
			}
		}
		else if (state == States.Shake)
		{
			if (TShake <= 0)
			{
				Audio.Play(Sfx.sfx_fallingblock_fall, Position);
				state = States.Fall;
			}
		}
		else if (state == States.Fall)
		{
			Calc.Approach(ref Velocity.Z, MaxFallSpeed, Gravity * Time.Delta);

			if (EndPosition.HasValue)
			{
				if (Position.Z <= EndPosition.Value.Z)
				{
					Audio.Play(Sfx.sfx_fallingblock_land, Position);
					state = States.Landed;
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
					respawnDelay = 5.0f;
					Velocity = Vec3.Zero;
					state = States.Respawn;
				}
			}
		}
		else if (state == States.Landed)
		{
			UpdateOffScreen = false;
		}
		else if (state == States.Respawn)
		{
			respawnDelay -= Time.Delta;
			if (respawnDelay <= 0)
			{
				MoveTo(startPosition);
				TShake = 0.2f;
				triggered = false;
				state = States.Wait;
				UpdateOffScreen = false;
			}
		}
	}

	private void Destroy()
	{
		foreach (var att in Attachers)
			World.Destroy(att);
		World.Destroy(this);
	}

    public override void CollectModels(List<(Actor Actor, Model Model)> populate)
    {
		if (state != States.Respawn)
        	base.CollectModels(populate);
    }
}
