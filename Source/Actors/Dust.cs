
namespace Celeste64;

public class Dust : Actor, IHaveSprites, IRecycle
{
	private static readonly string[] images = ["dust_0", "dust_1", "dust_2", "dust_3", "dust_4"];

	public Vec3 Velocity;
	public Subtexture Image;
	public Color Color;
	public float Percent;
	public float Duration;

	public void Init(Vec3 position, Vec3 velocity, Color? color = null)
	{
		Position = position;
		Velocity = velocity;
		Percent = 0.0f;
		Color = color ?? new Color(0.7f, 0.75f, 0.8f, 1.0f);
		Image = Assets.Subtextures[images[World.Rng.Int(images.Length)]];
		Duration = World.Rng.Float(0.5f, 1.0f);
		UpdateOffScreen = true;
	}

	public override void Update()
	{
		Position += Velocity * Time.Delta;
		Velocity.Z += 10 * Time.Delta;

		var vxy = Velocity.XY();
		vxy = Calc.Approach(vxy, Vec2.Zero, 200 * Time.Delta);
		Velocity = Velocity.WithXY(vxy);

		Percent += Time.Delta / Duration;
		if (Percent >= 1.0f)
			World.Destroy(this);
	}

	public void CollectSprites(List<Sprite> populate)
	{
		populate.Add(Sprite.CreateBillboard(World, Position, Image, 4 * (1.0f - Percent), Color));
	}
}
