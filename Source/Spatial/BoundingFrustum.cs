using System.Runtime.CompilerServices;

namespace Celeste64;

/// <summary>
/// Based on the BoundingFrustum from MonoGame:
/// https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/BoundingFrustum.cs
/// </summary>
public struct BoundingFrustum
{
	public const int PlaneCount = 6;
	public const int CornerCount = 8;

	[InlineArray(PlaneCount)]
	private struct PlaneArray { private Plane _element0; }
	[InlineArray(CornerCount)]
	private struct CornerArray { private Vec3 _element0; }

	private Matrix matrix;
	private CornerArray corners;
	private PlaneArray planes;

	public Matrix Matrix
	{
		readonly get => matrix;
		set
		{
			matrix = value;
			CreatePlanes();
			CreateCorners();
		}
	}

	public readonly Plane Near => planes[0];
	public readonly Plane Far => planes[1];
	public readonly Plane Left => planes[2];
	public readonly Plane Right => planes[3];
	public readonly Plane Top => planes[4];
	public readonly Plane Bottom => planes[5];

	public BoundingFrustum(Matrix value)
	{
		matrix = value;
		CreatePlanes();
		CreateCorners();
	}

	public readonly bool Contains(in BoundingBox box)
	{
		for (var i = 0; i < PlaneCount; ++i)
		{
			if (box.Intersects(planes[i]) == PlaneIntersectionType.Front)
				return false;
		}

		return true;
	}

	public readonly BoundingBox GetBoundingBox()
	{
		var min = corners[0];
		var max = corners[0];
		for (int i = 1; i < CornerCount; i ++)
		{
			min = Vec3.Min(min, corners[i]);
			max = Vec3.Max(max, corners[i]);
		}
		return new BoundingBox(min, max);
	}

	private void CreateCorners()
	{
		IntersectionPoint(planes[0], planes[2], planes[4], out corners[0]);
		IntersectionPoint(planes[0], planes[3], planes[4], out corners[1]);
		IntersectionPoint(planes[0], planes[3], planes[5], out corners[2]);
		IntersectionPoint(planes[0], planes[2], planes[5], out corners[3]);
		IntersectionPoint(planes[1], planes[2], planes[4], out corners[4]);
		IntersectionPoint(planes[1], planes[3], planes[4], out corners[5]);
		IntersectionPoint(planes[1], planes[3], planes[5], out corners[6]);
		IntersectionPoint(planes[1], planes[2], planes[5], out corners[7]);
	}

	private void CreatePlanes()
	{            
		planes[0] = new Plane(-matrix.M13, -matrix.M23, -matrix.M33, -matrix.M43);
		planes[1] = new Plane(matrix.M13 - matrix.M14, matrix.M23 - matrix.M24, matrix.M33 - matrix.M34, matrix.M43 - matrix.M44);
		planes[2] = new Plane(-matrix.M14 - matrix.M11, -matrix.M24 - matrix.M21, -matrix.M34 - matrix.M31, -matrix.M44 - matrix.M41);
		planes[3] = new Plane(matrix.M11 - matrix.M14, matrix.M21 - matrix.M24, matrix.M31 - matrix.M34, matrix.M41 - matrix.M44);
		planes[4] = new Plane(matrix.M12 - matrix.M14, matrix.M22 - matrix.M24, matrix.M32 - matrix.M34, matrix.M42 - matrix.M44);
		planes[5] = new Plane(-matrix.M14 - matrix.M12, -matrix.M24 - matrix.M22, -matrix.M34 - matrix.M32, -matrix.M44 - matrix.M42);

        NormalizePlane(ref planes[0]);
        NormalizePlane(ref planes[1]);
        NormalizePlane(ref planes[2]);
        NormalizePlane(ref planes[3]);
        NormalizePlane(ref planes[4]);
        NormalizePlane(ref planes[5]);
	}

	private static void IntersectionPoint(in Plane a, in Plane b, in Plane c, out Vec3 result)
	{
		// Formula used
		//                d1 ( N2 * N3 ) + d2 ( N3 * N1 ) + d3 ( N1 * N2 )
		//P =   -------------------------------------------------------------------------
		//                             N1 . ( N2 * N3 )
		//
		// Note: N refers to the normal, d refers to the displacement. '.' means dot product. '*' means cross product
		
		var f = -Vec3.Dot(a.Normal, Vec3.Cross(b.Normal, c.Normal));
		var v1 = Vec3.Cross(b.Normal, c.Normal) * a.D;
		var v2 = Vec3.Cross(c.Normal, a.Normal) * b.D;
		var v3 = Vec3.Cross(a.Normal, b.Normal) * c.D;
		
		result.X = (v1.X + v2.X + v3.X) / f;
		result.Y = (v1.Y + v2.Y + v3.Y) / f;
		result.Z = (v1.Z + v2.Z + v3.Z) / f;
	}
	
	private static void NormalizePlane(ref Plane p)
	{
		float factor = 1f / p.Normal.Length();
		p.Normal.X *= factor;
		p.Normal.Y *= factor;
		p.Normal.Z *= factor;
		p.D *= factor;
	}
}


