
namespace Celeste64;

public readonly struct ParticleTheme
{
	public readonly float Rate { get; init; }
	public readonly string Sprite { get; init; }
	public readonly Vec3 Gravity { get; init; }
	public readonly Vec3 StartVelocity { get; init; }
	public readonly float Life { get; init; }
	public readonly float Size { get; init; }
}

public class ParticleSystem
{
	private struct Particle
	{
		public Vec3 Position;
		public float Life;
		public Vec3 Velocity;
	}

	public float Accumulator = 0;
	public ParticleTheme Theme;
	public float MaximumDistance = 400;
	public readonly int MaxParticles;

	private readonly List<Particle> Particles;

	public ParticleSystem(int maxParticles, in ParticleTheme theme)
	{
		Particles = new(MaxParticles = maxParticles);
		Theme = theme;
	}

	public void SpawnParticle(Vec3 position, Vec3 velocity, float rateMultiplier)
	{
		Accumulator += rateMultiplier * Theme.Rate * Time.Delta;

		while (Accumulator > 0)
		{
			if (Particles.Count >= MaxParticles)
				Particles.RemoveAt(0);
				
			Particles.Add(new Particle() with
			{
				Position = position,
				Velocity = Theme.StartVelocity + velocity,
				Life = Theme.Life
			});

			Accumulator--;
		}
	}
		
	public void Update(float deltaTime)
	{
		for (int i = Particles.Count - 1; i >= 0; i--)
		{
			var it = Particles[i];

			it.Life -= deltaTime;
			it.Velocity += Theme.Gravity * deltaTime;
			it.Position += it.Velocity * deltaTime;

			if (it.Life <= 0)
				Particles.RemoveAt(i);
			else	
				Particles[i] = it;
		}
	}

	public void CollectSprites(Vec3 source, World world, List<Sprite> populate)
	{
		if ((world.Camera.Position - source).LengthSquared() > MaximumDistance * MaximumDistance)
			return;

		for (int i = Particles.Count - 1; i >= 0; i--)
		{
			if (Particles[i].Life <= 0)
				continue;
				
			populate.Add(Sprite.CreateBillboard(world, Particles[i].Position, Theme.Sprite, Theme.Size * Particles[i].Life / Theme.Life, Color.White));
		}
	}
}