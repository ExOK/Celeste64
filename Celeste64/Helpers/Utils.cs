using System.Runtime.CompilerServices;

namespace Celeste64;

public static class Utils
{
	// based on "Computing the Barycentric Coordinates of a Projected Point"
	// by Wolfgang Heidrich:
	// projectedBarycentricCoord( Point p, Point q, Vector u, Vector v, float *b )
	// {
	// 	Vector n= cross( u, v );
	// 	float oneOver4ASquared= 1.0 / dot( n, n );
	// 	Vector w= p - q;
	// 	b[2]= dot( cross( u, w ), n ) * oneOver4ASquared;
	// 	b[1]= dot( cross( w, v ), n ) * oneOver4ASquared;
	// 	b[0]= 1.0 - b[1] - b[2];
	// }
	public static bool IsPointInTriangle(in Vec3 point, in Vec3 v0, in Vec3 v1, in Vec3 v2)
	{
		Vec3 u = v1 - v0;
		Vec3 v = v2 - v0;
		Vec3 n = Vec3.Cross(u, v);
		Vec3 w = point - v0;

		float oneOver4ASquared = 1.0f / Vec3.Dot(n, n);

		// Barycentric coordinates:
		float b2 = Vec3.Dot(Vec3.Cross(u, w), n) * oneOver4ASquared;
		float b1 = Vec3.Dot(Vec3.Cross(w, v), n) * oneOver4ASquared;
		float b0 = 1.0f - b1 - b2;

		return
			b0 >= 0 && b0 <= 1 &&
			b1 >= 0 && b1 <= 1 &&
			b2 >= 0 && b2 <= 1;
	}

	public static Vec3 ClosestPointOnTriangle(in Vec3 point, in Plane plane, in Vec3 v0, in Vec3 v1, in Vec3 v2)
	{
		if (IsPointInTriangle(point, v0, v1, v2))
		{
			return point - plane.Normal * DistanceToPlane(point, plane);
		}
		else
		{
			var a = ClosestPointOnLine(point, v0, v1);
			var b = ClosestPointOnLine(point, v1, v2);
			var c = ClosestPointOnLine(point, v2, v0);

			if (a.LengthSquared() < b.LengthSquared() && a.LengthSquared() < b.LengthSquared())
				return a;
			else if (b.LengthSquared() < c.LengthSquared())
				return b;
			return c;
		}
	}

    public static Vec3 ClosestPointOnLine(in Vec3 point, in Vec3 v0, in Vec3 v1)
    {
        Vec3 vector = v1 - v0;
        if (vector.X == 0f && vector.Y == 0f)
            return v0;

        float num = Vec3.Dot(v1 - v0, vector) / (vector.X * vector.X + vector.Y * vector.Y);
        if (num < 0f)
            num = 0f;
        else if (num > 1f)
            num = 1f;

        return vector * num + v0;
    }

	public static bool RayIntersectsTriangle(Vec3 origin, Vec3 direction, Vec3 v0, Vec3 v1, Vec3 v2, out float t)
	{
		t = 0f;

		// Calculate the normal of the triangle
		Vec3 edge1 = v1 - v0;
		Vec3 edge2 = v2 - v0;
		Vec3 normal = Vec3.Cross(edge1, edge2);

		// Check if the ray and triangle are parallel
		float dot = Vec3.Dot(normal, direction);
		if (Math.Abs(dot) < float.Epsilon)
			return false;

		// Calculate the intersection point
		Vec3 rayToVertex = v0 - origin;
		t = Vec3.Dot(rayToVertex, normal) / dot;

		// Check if the intersection point is behind the ray's origin
		if (t < 0)
			return false;

		// Calculate the barycentric coordinates
		Vec3 intersectionPoint = origin + t * direction;
		CalculateBarycentricCoordinates(intersectionPoint, v0, v1, v2, out float u, out float v, out float w);

		// Check if the intersection point is inside the triangle
		return u >= 0 && v >= 0 && w >= 0 && (u + v + w) <= 1;
	}

	private static void CalculateBarycentricCoordinates(Vec3 point, Vec3 v0, Vec3 v1, Vec3 v2, out float u, out float v, out float w)
	{
		Vec3 edge1 = v1 - v0;
		Vec3 edge2 = v2 - v0;
		Vec3 toPoint = point - v0;

		float dot11 = Vec3.Dot(edge1, edge1);
		float dot12 = Vec3.Dot(edge1, edge2);
		float dot22 = Vec3.Dot(edge2, edge2);
		float dot1p = Vec3.Dot(edge1, toPoint);
		float dot2p = Vec3.Dot(edge2, toPoint);

		float denominator = dot11 * dot22 - dot12 * dot12;
		
		u = (dot22 * dot1p - dot12 * dot2p) / denominator;
		v = (dot11 * dot2p - dot12 * dot1p) / denominator;
		w = 1 - u - v;
	}

	public static bool PlaneTriangleIntersection(in Plane plane, in Vec3 v0, in Vec3 v1, in Vec3 v2, out Vec3 line0, out Vec3 line1)
	{
		line0 = default;
		line1 = default;

		var index = 0;

		if (PlaneLineIntersection(plane, v0, v1, out var p0))
		{
			line0 = p0;
			index++;
		}
		if (PlaneLineIntersection(plane, v1, v2, out var p1))
		{
			if (index == 0) line0 = p1; else line1 = p1;
			index++;
		}
		if (PlaneLineIntersection(plane, v2, v0, out var p2))
		{
			if (index == 0) line0 = p2; else line1 = p2;
			index++;
		}

		return index > 0;
	}

	public static bool PlaneLineIntersection(in Plane plane, in Vec3 line0, in Vec3 line1, out Vec3 point)
	{
		point = default;

		var edge = line1 - line0;
		var rel = line0 - (plane.Normal * plane.D);
		var t = -Vec3.Dot(plane.Normal, rel) / Vec3.Dot(plane.Normal, edge);

		if (t >= 0 && t <= 1)
		{
			point = line0 + t * edge;
			return true;
		}

		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceToPlane(this Vec3 point, Plane plane)
		=> Vec3.Dot(plane.Normal, point) + plane.D;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vec2 XY(this Vec3 Vec3)
		=> new(Vec3.X, Vec3.Y);
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vec3 WithXY(this Vec3 Vec3, in Vec2 vec2)
		=> new(vec2.X, vec2.Y, Vec3.Z);


	public static Vec3 UpwardPerpendicularNormal(this Vec3 Vec3)
	{
		Vec3 = Vec3.Normalized();

		Vec3 arbitrary;
		if (MathF.Abs(Vec3.X) > MathF.Abs(Vec3.Y))
			arbitrary = Vec3.UnitY;
		else
			arbitrary = Vec3.UnitX;

		var result = Vec3.Cross(Vec3, arbitrary).Normalized();

		if (result.Z < 0)
			result = -result;

		return result;
	}

	/// <summary>
	/// Based on code from:
	/// https://danielsieger.com/blog/2021/03/27/generating-spheres.html
	/// </summary>
	public static void CreateSphere(List<Vertex> vertices, List<int> indices, int sliceCount, int stackCount)
	{
		static Vertex MakeVertex(in Vec3 pos)
			=> new(pos, Vec2.Zero, Vec3.One, pos.Normalized());

		// add top vertex
		var v0 = vertices.Count;
		vertices.Add(MakeVertex(new Vec3(0, 0, 1)));

		// generate vertices per stack / slice
		for (int i = 0; i < stackCount - 1; i++)
		{
			var phi = MathF.PI * (i + 1) / (float)(stackCount);
			for (int j = 0; j < sliceCount; j++)
			{
				var theta = 2.0f * MathF.PI * (j) / (float)(sliceCount);
				var x = MathF.Sin(phi) * MathF.Cos(theta);
				var y = MathF.Sin(phi) * MathF.Sin(theta);
				var z = MathF.Cos(phi);
				vertices.Add(MakeVertex(new Vec3(x, y, z)));
			}
		}

		// add bottom vertex
		var v1 = vertices.Count;
		vertices.Add(MakeVertex(new Vec3(0, 0, -1)));

		// add top / bottom triangles
		for (int i = 0; i < sliceCount; ++i)
		{
			var i0 = i + 1;
			var i1 = (i + 1) % sliceCount + 1;
			indices.Add(v0); indices.Add(i1); indices.Add(i0);
			i0 = i + sliceCount * (stackCount - 2) + 1;
			i1 = (i + 1) % sliceCount + sliceCount * (stackCount - 2) + 1;
			indices.Add(v1); indices.Add(i0); indices.Add(i1);
		}

		// add quads per stack / slice
		for (int j = 0; j < stackCount - 2; j++)
		{
			var j0 = j * sliceCount + 1;
			var j1 = (j + 1) * sliceCount + 1;
			for (int i = 0; i < sliceCount; i++)
			{
				var i0 = j0 + i;
				var i1 = j0 + (i + 1) % sliceCount;
				var i2 = j1 + (i + 1) % sliceCount;
				var i3 = j1 + i;
				indices.Add(i0);
				indices.Add(i1);
				indices.Add(i2);
				indices.Add(i0);
				indices.Add(i2);
				indices.Add(i3);
			}
		}
	}

	public static Vec3 Approach(in Vec3 from, in Vec3 target, float amount)
	{
		if (from == target)
		{
			return target;
		}

		Vec3 vector = target - from;
		if (vector.LengthSquared() <= amount * amount)
		{
			return target;
		}

		return from + vector.Normalized() * amount;
	}

	public static float Lerp2(float a, float b, float c, float t)
	{
		if (t < .5f)
			return float.Lerp(a, b, t * 2);
		else
			return float.Lerp(b, c, (t - .5f) * 2);
	}

	public static float Lerp3(float a, float b, float c, float d, float t)
	{
		if (t < 1f / 3)
			return float.Lerp(a, b, t * 3);
		else if (t < 2f / 3)
			return float.Lerp(b, c, (t - 1f / 3) * 3);
		else
			return float.Lerp(c, d, (t - 2f / 3) * 3);
	}

	public static Vec3 Bezier(Vec3 a, Vec3 b, Vec3 c, float t)
		=> Vec3.Lerp(Vec3.Lerp(a, b, t), Vec3.Lerp(b, c, t), t);

	public static float SineInOut(float t)
		=> -(MathF.Cos(MathF.PI * t) - 1f) / 2f;
}