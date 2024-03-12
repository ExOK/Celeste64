
namespace Celeste64;

public class Coin : Actor, IHaveModels, IHaveSprites, IPickup, ICastPointShadow
{
	public SkinnedModel Model;
	public float PickupRadius => 20;
	public bool Collected { get; private set; }
	public float PointShadowAlpha { get; set; }

	public virtual Color InactiveColor => 0x5fcde4;
	public virtual Color CollectedColor => 0xf141df;

	public Coin()
	{
		Model = new SkinnedModel(Assets.Models["coin"]);
		Model.Flags = ModelFlags.Default;
		Model.MakeMaterialsUnique();
		foreach (var mat in Model.Materials)
			mat.Color = InactiveColor;
		PointShadowAlpha = 1.0f;
		LocalBounds = new BoundingBox(Vec3.Zero, 16);
	}

	public virtual void CollectSprites(List<Sprite> populate)
	{
		if (!Collected)
		{
			//Particles.CollectSprites(Position, World, populate);
			var haloPos = Position + Vec3.UnitZ * 2 + Vec3.Transform(Vec3.Zero, Model.Transform);
			populate.Add(Sprite.CreateBillboard(World, haloPos, "gradient", 10, InactiveColor * 0.50f));
		}
	}

	public virtual void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		if (Collected)
		{
			Model.Flags = ModelFlags.Transparent;
			foreach (var mat in Model.Materials)
				mat.Color = CollectedColor * 0.50f;
		}

		Model.Transform =
			Matrix.CreateScale(6.0f) *
			Matrix.CreateTranslation(Vec3.UnitZ * MathF.Sin(World.GeneralTimer * 2.0f) * 2) *
			Matrix.CreateRotationZ(World.GeneralTimer * 3.0f);

		populate.Add((this, Model));
	}

	public virtual void Pickup(Player player)
	{
		if (!Collected)
		{
			Collected = true;
			if (!AnyRemaining(World))
				Audio.Play(Sfx.sfx_touch_switch_last, Position);
			else
				Audio.Play(Sfx.sfx_touch_switch_any, Position);
		}
	}

	public static bool AnyRemaining(World world)
	{
		foreach (var it in world.All<Coin>())
		{
			if (!(it as Coin)!.Collected)
				return true;
		}

		return false;
	}
}
