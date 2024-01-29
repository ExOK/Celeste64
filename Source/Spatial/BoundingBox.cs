
namespace Celeste64;

/// <summary>
/// Based on the BoundingBox from MonoGame:
/// https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/BoundingBox.cs
/// </summary>
public record struct BoundingBox(Vec3 Min, Vec3 Max)
{
	public readonly Vec3 Center => (Min + Max) / 2;
	public readonly Vec3 Size => Max - Min;

	public BoundingBox(Vec3 position, float size) 
		: this(position - Vec3.One * size / 2, position + Vec3.One * size) {}

	public readonly PlaneIntersectionType Intersects(in Plane plane)
	{
		// See https://web.archive.org/web/20161025133340/http://zach.in.tu-clausthal.de/teaching/cg_literatur/lighthouse3d_view_frustum_culling/index.html

		Vec3 positiveVertex;
		Vec3 negativeVertex;

		if (plane.Normal.X >= 0)
		{
			positiveVertex.X = Max.X;
			negativeVertex.X = Min.X;
		}
		else
		{
			positiveVertex.X = Min.X;
			negativeVertex.X = Max.X;
		}

		if (plane.Normal.Y >= 0)
		{
			positiveVertex.Y = Max.Y;
			negativeVertex.Y = Min.Y;
		}
		else
		{
			positiveVertex.Y = Min.Y;
			negativeVertex.Y = Max.Y;
		}

		if (plane.Normal.Z >= 0)
		{
			positiveVertex.Z = Max.Z;
			negativeVertex.Z = Min.Z;
		}
		else
		{
			positiveVertex.Z = Min.Z;
			negativeVertex.Z = Max.Z;
		}

		var distance = Vec3.Dot(plane.Normal, negativeVertex) + plane.D;
		if (distance > 0)
			return PlaneIntersectionType.Front;

		distance = Vec3.Dot(plane.Normal, positiveVertex) + plane.D;
		if (distance < 0)
			return PlaneIntersectionType.Back;

		return PlaneIntersectionType.Intersecting;
	}

	public readonly bool Contains(in Vec3 point)
	{
		return 
			point.X >= Min.X && point.Y >= Min.Y && point.Z >= Min.Z &&
			point.X <= Max.X && point.Y <= Max.Y && point.Z <= Max.Z;
	}

	public readonly bool Intersects(in BoundingBox box)
	{
		return 
			Max.X >= box.Min.X && Max.Y >= box.Min.Y && Max.Z >= box.Min.Z &&
			Min.X <= box.Max.X && Min.Y <= box.Max.Y && Min.Z <= box.Max.Z;
	}

	public readonly BoundingBox Inflate(float amount)
	{
		var size = Size + Vec3.One * amount * 2;
		var center = Center;
		return new(center - size / 2, center + size / 2);
	}

	public readonly BoundingBox Conflate(in BoundingBox other)
		=> new(Vec3.Min(Min, other.Min), Vec3.Max(Max, other.Max));

	public static BoundingBox operator+(BoundingBox a, Vec3 offset)
		=> new(a.Min + offset, a.Max + offset);
		
	public static BoundingBox operator-(BoundingBox a, Vec3 offset)
		=> new(a.Min - offset, a.Max - offset);

	public static BoundingBox Transform(in BoundingBox a, in Matrix matrix)
	{
		var corners = a.GetCorners();
		var min = Vec3.Transform(corners[0], matrix); var max = Vec3.Transform(corners[0], matrix);
		for (int i = 1; i < corners.Count; i ++)
		{
			var it = Vec3.Transform(corners[i], matrix);
			min = Vec3.Min(min, it);
			max = Vec3.Max(max, it);
		}
		return new BoundingBox(min, max);
	}

	public readonly StackList8<Vec3> GetCorners()
	{
		return [
			new Vec3(Min.X, Max.Y, Max.Z), 
			new Vec3(Max.X, Max.Y, Max.Z),
			new Vec3(Max.X, Min.Y, Max.Z), 
			new Vec3(Min.X, Min.Y, Max.Z), 
			new Vec3(Min.X, Max.Y, Min.Z),
			new Vec3(Max.X, Max.Y, Min.Z),
			new Vec3(Max.X, Min.Y, Min.Z),
			new Vec3(Min.X, Min.Y, Min.Z)
		];
	}
}