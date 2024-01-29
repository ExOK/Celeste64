
namespace Celeste64;

public class StaticProp : Actor, IHaveModels, IHavePushout, IListenToAudioCallback
{
	public readonly SkinnedModel Model;

	public float PushoutHeight { get; set; } = 10;
	public float PushoutRadius { get; set; } = 6;

	private float scale = 1.0f;
	private float rotation = 0;

	public StaticProp(SkinnedTemplate model, float radius, float height)
	{
		Model = new(model);
		Model.Flags = ModelFlags.Terrain;
		Model.Transform = Matrix.CreateScale(0.2f);
		LocalBounds = new BoundingBox(new Vec3(-10, -10, 0), new Vec3(10, 10, 80));
		PushoutHeight = height;
		PushoutRadius = radius;
	}

    public override void Update()
    {	
		Calc.Approach(ref scale, 1.0f, Time.Delta);
    }

    public void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		Model.Transform = 
			Matrix.CreateRotationY(rotation) *
			Matrix.CreateScale(0.2f * scale);
		populate.Add((this, Model));
	}

    public void AudioCallbackEvent(int index)
    {
		if (World.Entry.Submap)
		{
			scale = 1.05f;
			rotation = 0.05f * ((index % 2) == 0 ? -1 : 1);
		}
    }
}
