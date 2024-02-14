
namespace Celeste64;

public class Refill : Actor, IHaveSprites, IPickup, IHaveModels, ICastPointShadow
{
	public bool IsDouble;
	public SkinnedModel Model;
	public ParticleSystem Particles;
	public virtual float PointShadowAlpha { get; set; } = 1.0f;

	public Refill(bool isDouble)
	{
		IsDouble = isDouble;
		LocalBounds = new BoundingBox(Vec3.Zero, 3);
		Model = new(Assets.Models[isDouble ? "refill_gem_double" : "refill_gem"]);
		Model.Flags = ModelFlags.Default;
		Particles = new(32, new ParticleTheme()
		{
			Rate = 10.0f,
			Sprite = "particle-star",
			Life = 0.5f,
			Gravity = new Vec3(0, 0, 90),
			Size = 2.5f
		});
	}

	public float PickupRadius => 20;

	public float TCooldown;
	public float TCollect;

	public virtual void CollectSprites(List<Sprite> populate)
	{
		if (TCollect > 0)
		{
			var size = (1.0f - Ease.Cube.In(TCollect)) * 50;
			var alpha = TCollect;

			populate.Add(Sprite.CreateFlat(World, Position - Vec3.UnitZ * 3, "ring", size * 0.75f, Color.White * alpha * alpha));
			populate.Add(Sprite.CreateFlat(World, Position - Vec3.UnitZ * 3, "ring", size, Color.White * alpha));
		}

		Particles.CollectSprites(Position, World, populate);
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
			if (TCooldown <= 0.0f)
			{
				UpdateOffScreen = false;
				Audio.Play(IsDouble ? Sfx.sfx_dashcrystal_double_return : Sfx.sfx_dashcrystal_return, Position);
			}
		}

		if (TCollect > 0)
			TCollect -= Time.Delta * 3.0f;

		PointShadowAlpha = TCooldown <= 0 ? 1 : 0;

		Particles.SpawnParticle(
			Position + new Vec3(6 - World.Rng.Float() * 12, 6 - World.Rng.Float() * 12, 6 - World.Rng.Float() * 12),
			new Vec3(0, 0, 0), 1);
		Particles.Update(Time.Delta);
	}

	public virtual void Pickup(Player player)
	{
		int count = IsDouble ? 2 : 1;
		if (TCooldown <= 0 && player.Dashes < count)
		{
			UpdateOffScreen = true;
			player.RefillDash(count);
			TCooldown = 4;
			TCollect = 1.0f;
			World.HitStun = 0.05f;
			Audio.Play(IsDouble ? Sfx.sfx_dashcrystal_double : Sfx.sfx_dashcrystal, Position);
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
