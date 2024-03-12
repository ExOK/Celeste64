
namespace Celeste64;

public class DoubleDashPuzzleBlock : Solid, IUnlockStrawberry, IHaveSprites
{
	public bool Satisfied { get; protected set; }

	public bool Ready = false;
	public float Pulse = 0;
	public Color PulseColor;

	public virtual void CollectSprites(List<Sprite> populate)
	{
		if (Ready && !Satisfied && Pulse > 0)
		{
			var haloPos = Position + Vec3.UnitZ * 2 + Vec3.Transform(Vec3.Zero, Model.Transform);
			populate.Add(Sprite.CreateBillboard(World, haloPos, "gradient", 50 * Pulse, PulseColor * 0.50f) with { Post = true });
			populate.Add(Sprite.CreateBillboard(World, Position, "ring", Pulse * Pulse * 40, PulseColor * .4f) with { Post = true });
			populate.Add(Sprite.CreateBillboard(World, Position, "ring", Pulse * 50, PulseColor * .4f) with { Post = true });
		}
	}

	public override void Update()
	{
		base.Update();

		if (!Satisfied && !Ready && World.Get<Player>() is { } player && player.Dashes >= 2 && HasPlayerRider())
		{
			PulseColor = player.Hair.Color;
			Ready = true;
			Pulse = 1;
			TShake = 1.0f;
			Audio.Play(Sfx.sfx_secret, Position);
		}

		if (Ready && Pulse > 0)
		{
			Pulse -= Time.Delta;
			if (Pulse <= 0)
				Satisfied = true;
		}
	}
}
