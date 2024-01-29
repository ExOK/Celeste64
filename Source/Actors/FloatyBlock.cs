
namespace Celeste64;

public sealed class FloatyBlock : Solid
{
	private Vec3 origin;
	private Vec3 offset;
	private float friction = 300;
	private float frictionThreshold = 50;
	private float frequency = 1.2f;
	private float halflife = 3.0f;
	private bool hasPlayerRider;

	public override void Added()
	{
		base.Added();
		origin = Position;
		offset = Vec3.Zero;
	}

	public override void Update()
	{
		base.Update();

		if (!hasPlayerRider && HasPlayerRider())
		{
			hasPlayerRider = true;
			Velocity += World.Get<Player>()!.PreviousVelocity * .8f;
		}
		else if (hasPlayerRider && !HasPlayerRider())
		{
			if (World.Get<Player>() is Player player)
				Velocity -= player.Velocity * .8f;
			hasPlayerRider = false;
		}

		// friction
		friction = 200;
		frictionThreshold = 1;
		if (friction > 0 && Velocity.LengthSquared() > Calc.Squared(frictionThreshold))
			Velocity = Utils.Approach(Velocity, Velocity.Normalized() * frictionThreshold, friction * Time.Delta);

		// spring!
		Vec3 diff = Position - (origin + offset);
		Vec3 normal = diff.Normalized();
		float vel = Vec3.Dot(Velocity, normal);
		float old_vel = vel;
		vel = SpringPhysics.Calculate(diff.Length(), vel, 0, 0, frequency, halflife);
		Velocity += normal * (vel - old_vel);
	}
}
