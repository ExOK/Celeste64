namespace Celeste64;

public class LoadingZone(string map, string checkpointName, bool isSubmap) : Actor
{
	public readonly string Map = map;
	public readonly string CheckpointName = checkpointName;
	public readonly bool IsSubmap = isSubmap;
}
