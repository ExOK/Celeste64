
namespace Celeste64;

public class Cassette : Actor, IHaveModels, IPickup, IHaveSprites, ICastPointShadow
{
	public readonly string Map;
	public SkinnedModel CollectedModel;
	public SkinnedModel Model;
	public float PointShadowAlpha { get; set; } = 1.0f;

	public float TCooldown = 0.0f;
	public float TWiggle = 0.0f;

	public Cassette(string map)
	{
		Map = map;
		LocalBounds = new BoundingBox(Vec3.Zero, 3);
		Model = new(Assets.Models["tape_1"]);
		CollectedModel = new(Assets.Models["tape_2"]);
		CollectedModel.Flags = ModelFlags.Transparent;
		foreach (var mat in CollectedModel.Materials)
			mat.Color = Color.White * 0.50f;
	}

	public virtual float PickupRadius => 10;

	public virtual bool IsCollected => 
		!string.IsNullOrEmpty(Map) && 
		Save.CurrentRecord.CompletedSubMaps.Contains(Map);

	public override void Added()
	{
		LocalBounds = new BoundingBox(Vec3.Zero, 8);
		// in case you spawn on it
		TCooldown = 1.0f;
	}

	public virtual void SetCooldown()
	{
		TCooldown = 1.0f;
	}

	public override void Update()
	{
		PointShadowAlpha = IsCollected ? 0.5f : 1.0f;
		Calc.Approach(ref TCooldown, 0, Time.Delta);
		Calc.Approach(ref TWiggle, 0, Time.Delta / 0.7f);
	}

	public virtual void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		var wiggle = 1 + MathF.Sin(TWiggle * MathF.Tau * 2) * .8f * TWiggle;

		Model.Transform = CollectedModel.Transform =
			Matrix.CreateScale(Vec3.One * 2.5f * wiggle) *
			Matrix.CreateTranslation(Vec3.UnitZ * MathF.Sin(World.GeneralTimer * 2.0f) * 2) *
			Matrix.CreateRotationZ(World.GeneralTimer * 3.0f);

		populate.Add((this, IsCollected? CollectedModel : Model));
	}

	public virtual void Pickup(Player player)
	{
		if (!IsCollected && TCooldown <= 0.0f && !Game.Instance.IsMidTransition)
		{
			player.Stop();
			player.EnterCassette(this);
			TWiggle = 1.0f;
		}
	}

	public virtual void PlayerExit()
	{
		TWiggle = 1.0f;
	}

	public virtual void CollectSprites(List<Sprite> populate)
	{
		if (TWiggle > 0)
		{
			populate.Add(Sprite.CreateBillboard(World, Position, "ring", TWiggle * TWiggle * 40, Color.White * .4f) with { Post = true });
			populate.Add(Sprite.CreateBillboard(World, Position, "ring", TWiggle * 50, Color.White * .4f) with { Post = true });
		}
	}
}
