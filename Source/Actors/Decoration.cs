
namespace Celeste64;

public class Decoration : Actor, IHaveModels
{
	public readonly SimpleModel Model = new() { Flags = ModelFlags.Terrain };

	public void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		populate.Add((this, Model));
	}
}
