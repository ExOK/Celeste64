
namespace Celeste64;

[Flags]
public enum ModelFlags
{
	None		          = 0,
	Default	              = 1 << 0,
	Terrain	              = 1 << 1,
	Silhouette	          = 1 << 2,
	Transparent           = 1 << 3,
	Cutout		          = 1 << 4,
	StrawberryGetEffect   = 1 << 5,
}