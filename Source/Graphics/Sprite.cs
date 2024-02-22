using System.Runtime.CompilerServices;

namespace Celeste64;

public struct Sprite
{
	public Subtexture Subtexture;
	public Vec3 A;
	public Vec3 B;
	public Vec3 C;
	public Vec3 D;
	public Color Color;
	public bool Post;

	public static Sprite CreateShadowSprite(World world, Vec3 position, float strength = 1.0f)
	{
		if (world.SolidRayCast(position, -Vec3.UnitZ, 1000, out var hit))
		{
			var a = Vec3.Cross(hit.Normal, new Vec3(0, 1, 0));
			var b = Vec3.Cross(hit.Normal, new Vec3(1, 0, 0));
			var at = hit.Point + new Vec3(0, 0, 0.01f);
			var size = Calc.Map(hit.Distance, 0, 50, 3, 2);

			return new Sprite
			{
				Subtexture = Assets.Subtextures["circle"],
				A = at + (-a - b) * size,
				B = at + (a - b) * size,
				C = at + (a + b) * size,
				D = at + (-a + b) * size,
				Color = new Color(0x1d0b44) * 0.50f * strength,
			};
		}

		return default;
	}
	
	public static Sprite CreateFlat(World world, Vec3 at, string subtex, float size, Color color)
	{
		return new Sprite
		{
			Subtexture = Assets.Subtextures[subtex],
			A = at + new Vec3(-1, 1, 0) * size,
			B = at + new Vec3(1, 1, 0) * size,
			C = at + new Vec3(1, -1, 0) * size,
			D = at + new Vec3(-1, -1, 0) * size,
			Color = color
		};
	}

	public static Sprite CreateBillboard(World world, in Vec3 at, string subtex, float size, Color color)
	{
		var left = world.Camera.Left * size;
		var up = world.Camera.Up * size;
		return new Sprite
		{
			Subtexture = Assets.Subtextures[subtex],
			A = at + left + up,
			B = at - left + up,
			C = at - left - up,
			D = at + left - up,
			Color = color
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Sprite CreateBillboard(World world, in Vec3 at, in Subtexture subtex, float size, Color color)
	{
		var left = world.Camera.Left * size;
		var up = world.Camera.Up * size;
		return new Sprite
		{
			Subtexture = subtex,
			A = at + left + up,
			B = at - left + up,
			C = at - left - up,
			D = at + left - up,
			Color = color
		};
	}
}