
namespace Celeste64;

public struct RayHit
{
	public Vec3 Point;
	public Vec3 Normal;
	public float Distance;
	public Actor? Actor;
	public int Intersections;
}

public struct WallHit
{
	public Vec3 Pushout;
	public Vec3 Normal;
	public Vec3 Point;
	public Actor? Actor;
}