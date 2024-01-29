
namespace Celeste64;

public class FloatingDecoration : Actor, IHaveModels
{
	public readonly SimpleModel Model = new() { Flags = ModelFlags.Terrain };
	public float Rate;
	public float Offset;

    public override void Added()
    {
		Rate = World.Rng.Float(1, 3);
		Offset = World.Rng.Float(MathF.Tau);
		UpdateOffScreen = true;
    }

    public void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		var time = World.GeneralTimer * Rate * 0.25f + Offset;
		Model.Transform = Matrix.CreateTranslation(0, 0, MathF.Sin(time) * 12);
		populate.Add((this, Model));
	}
}
