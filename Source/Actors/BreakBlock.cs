
namespace Celeste64;

public class BreakBlock : Solid, IDashTrigger
{
	private static readonly string[] glassShards = ["shard_0", "shard_1", "shard_2"];
	private static readonly string[] woodShards = ["wood_shard_0", "wood_shard_1", "wood_shard_2"];

	public readonly bool Secret;

    public bool BouncesPlayer => bouncesPlayer;
    private readonly bool bouncesPlayer;

	public BreakBlock(bool bouncesPlayer_, bool transparent, bool secret)
	{
		bouncesPlayer = bouncesPlayer_;
		Transparent = transparent;
		Secret = secret;

		if (Transparent)
			Model.Flags = ModelFlags.Transparent;
	}

    public void HandleDash(Vec3 velocity)
	{
		var size = LocalBounds.Size;
		var amount = (size.X * size.Y * size.Z) / 200;
		var options = (Transparent ? glassShards : woodShards);

		if (Secret)
			Audio.Play(Sfx.sfx_secret, Position);

		if (Transparent)
			Audio.Play(Sfx.sfx_glassbreak, Position);
		else
			Audio.Play(Sfx.sfx_breakable_wall_wood, Position);

		for (int i = 0; i < amount; i++)
		{
			var offset = new Vec3(World.Rng.Float(size.X), World.Rng.Float(size.Y), World.Rng.Float(size.Z));
			var at = Vec3.Transform(offset - size / 2, Matrix);
			velocity = velocity.Normalized() * World.Rng.Float(100, 400);
			World.Request<Debris>().Init(at, velocity, options[World.Rng.Int(options.Length)]);
		}

		World.Destroy(this);
	}
}
