
namespace Celeste64;

public class Checkpoint : Actor, IHaveModels, IPickup, IHaveSprites
{
	public readonly string Name;
	public SkinnedModel ModelOff;
	public SkinnedModel ModelOn;

	private float tWiggle = 0.0f;

	public Checkpoint(string name)
	{
		Name = name;
		LocalBounds = new BoundingBox(Vec3.Zero, 8);
		ModelOff = new(Assets.Models["flag_off"]);
		ModelOff.Play("Idle");
		ModelOff.Transform = Matrix.CreateScale(0.2f);
		ModelOn = new(Assets.Models["flag_on"]);
		ModelOn.Play("Idle");
		ModelOn.Transform = Matrix.CreateScale(0.2f);
	}

	public float PickupRadius => 16;

	public bool IsCurrent => World.Entry.CheckPoint == Name;
	public SkinnedModel CurrentModel => (IsCurrent ? ModelOn : ModelOff);

	public override void Added()
	{
		// if we're the spawn checkpoint, shift us so the player isn't on top
		if (IsCurrent)
			Position -= Vec3.UnitY * 8;
	}

	public override void Update()
	{
		if (IsCurrent)
		{
			Calc.Approach(ref tWiggle, 0, Time.Delta / 0.7f);
			CurrentModel.Update();
		}
	}

	public void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		populate.Add((this, CurrentModel));
	}

	public void Pickup(Player player)
	{
		if (!IsCurrent)
		{
			Audio.Play(Sfx.sfx_checkpoint, Position);

			World.Entry = World.Entry with { CheckPoint = Name };
			if (!World.Entry.Submap)
				Save.CurrentRecord.Checkpoint = Name;

			tWiggle = 1;
		}
	}

	public void CollectSprites(List<Sprite> populate)
	{
		var haloPos = Position + Vec3.UnitZ * 16;
		var haloCol = new Color(IsCurrent ? 0x7fde46 : 0xdf5ab4) * .4f;
		populate.Add(Sprite.CreateBillboard(World, haloPos, "gradient", 12, haloCol * 0.40f));

		if (tWiggle > 0)
		{
			populate.Add(Sprite.CreateBillboard(World, haloPos, "ring", tWiggle * tWiggle * 40, haloCol) with { Post = true });
			populate.Add(Sprite.CreateBillboard(World, haloPos, "ring", tWiggle * 50, haloCol) with { Post = true });
		}
	}
}
