namespace Celeste64;

public class Feather : Actor, IHaveModels, IHaveSprites, IPickup, ICastPointShadow
{
	public SkinnedModel Model = new(Assets.Models["feather"]) { Flags = ModelFlags.Default, };
	public ParticleSystem Particles = new(32, new ParticleTheme()
	{
		Rate = 10.0f,
		Sprite = "particle-star",
		Life = 0.5f,
		Gravity = new Vec3(0, 0, 90),
		Size = 2.5f
	});
	public virtual float PointShadowAlpha { get; set; } = 1.0f;
	public virtual float PickupRadius => 16;

	public float TCooldown;

	public virtual void CollectSprites(List<Sprite> populate)
	{
		if (TCooldown <= 0)
		{
			Particles.CollectSprites(Position, World, populate);
			var haloPos = Position + Vec3.UnitZ * 2 + Vec3.Transform(Vec3.Zero, Model.Transform);
			populate.Add(Sprite.CreateBillboard(World, haloPos, "gradient", 10, new Color(0xeed14f) * 0.50f));
		}
	}

	public override void Added()
	{
		LocalBounds = new BoundingBox(Vec3.Zero, 3);
	}

	public override void Update()
	{
		if (TCooldown > 0)
		{
			TCooldown -= Time.Delta;
			if (TCooldown <= 0)
			{
				UpdateOffScreen = false;
				Audio.Play(Sfx.sfx_feather_reappear, Position);
			}
		}

		PointShadowAlpha = TCooldown <= 0 ? 1 : 0;

		Particles.SpawnParticle(
			Position + new Vec3(6 - World.Rng.Float() * 12, 6 - World.Rng.Float() * 12, 6 - World.Rng.Float() * 12),
			new Vec3(0, 0, 0), 1);
		Particles.Update(Time.Delta);
	}

	public virtual void Pickup(Player player)
	{
		if (TCooldown <= 0)
		{
			TCooldown = 1.5f;
			player.FeatherGet(this);
			UpdateOffScreen = true;
		}
	}

	public virtual void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		if (TCooldown <= 0)
		{
			Model.Transform =
				Matrix.CreateScale(2.0f) *
				Matrix.CreateTranslation(Vec3.UnitZ * MathF.Sin(World.GeneralTimer * 2.0f) * 2) *
				Matrix.CreateRotationZ(World.GeneralTimer * 3.0f);
			populate.Add((this, Model));
		}
	}
}
