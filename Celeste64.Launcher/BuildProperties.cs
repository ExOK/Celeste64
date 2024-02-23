using EmbeddedBuildProperty;

namespace Celeste64.Launcher;

public partial class BuildProperties
{
	[BuildProperty]
	public static partial string ModVersion();
}