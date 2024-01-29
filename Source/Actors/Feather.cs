
namespace Celeste64;

public class Feather : Actor, IHaveModels, IHaveSprites, IPickup, ICastPointShadow
{
	public SkinnedModel Model;
	public ParticleSystem Particles;
	public float PointShadowAlpha { get; set; } = 1.0f;
	public float PickupRadius => 16;

	private float tCooldown;

	public Feather()
	{
		Model = new(Assets.Models["feather"]);
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

	public void CollectSprites(List<Sprite> populate)
	{
		if (tCooldown <= 0)
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
		if (tCooldown > 0)
		{
			tCooldown -= Time.Delta;
			if (tCooldown <= 0)
			{
				UpdateOffScreen = false;
				Audio.Play(Sfx.sfx_feather_reappear, Position);
			}
		}
		
		PointShadowAlpha = tCooldown <= 0 ? 1 : 0;
		
		Particles.SpawnParticle(
			Position + new Vec3(6 - World.Rng.Float() * 12, 6 - World.Rng.Float() * 12, 6 - World.Rng.Float() * 12),
			new Vec3(0, 0, 0), 1);
		Particles.Update(Time.Delta);
	}

	public void Pickup(Player player)
	{
		if (tCooldown <= 0)
		{
			tCooldown = 1.5f;
			player.FeatherGet(this);
			UpdateOffScreen = true;
		}
	}

	public void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		if (tCooldown <= 0)
		{
			Model.Transform =
				Matrix.CreateScale(2.0f) *
				Matrix.CreateTranslation(Vec3.UnitZ * MathF.Sin(World.GeneralTimer * 2.0f) * 2) *
				Matrix.CreateRotationZ(World.GeneralTimer * 3.0f);
			populate.Add((this, Model));
		}
	}
}
