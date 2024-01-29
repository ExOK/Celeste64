using System.Runtime.InteropServices;

namespace Celeste64;

public class FogRing : Actor, IHaveModels
{
	private readonly SimpleModel model = new();
	private readonly float height;
	private readonly float radius;
	private readonly float speed;
	private float offset;

	public FogRing(Sledge.Formats.Map.Objects.Entity data)
	{
		var vertices = new List<Vertex>();
		var indices = new List<int>();
		var texture = data.GetStringProperty("texture", string.Empty);
		var alpha = data.GetFloatProperty("alpha", 1.0f);
		var color = Color.FromHexStringRGB(data.GetStringProperty("color", "ffffff"));
		height = data.GetFloatProperty("height", 1.0f);
		radius = data.GetFloatProperty("radius", 1.0f);
		speed = data.GetFloatProperty("speed", 1.0f);

		for (int i = 0; i <= 32; i ++)
		{
			var rot = Calc.AngleToVector((1 - (i / 32.0f)) * MathF.Tau);
			vertices.Add(new (new Vec3(rot, 1), new Vec2(i / 32.0f, 0), Color.White, Vec3.Zero));
			vertices.Add(new (new Vec3(rot, 0), new Vec2(i / 32.0f, 1), Color.White, Vec3.Zero));
		}

		for (int i = 0; i < 32; i ++)
		{
			indices.Add(i * 2 + 0);
			indices.Add(i * 2 + 2);
			indices.Add(i * 2 + 3);
			indices.Add(i * 2 + 0);
			indices.Add(i * 2 + 3);
			indices.Add(i * 2 + 1);
		}


		model.Mesh.SetVertices<Vertex>(CollectionsMarshal.AsSpan(vertices));
		model.Mesh.SetIndices<int>(CollectionsMarshal.AsSpan(indices));
		model.Materials.Add(new DefaultMaterial(Assets.Textures[texture]));
		model.Parts.Add(new(0, 0, indices.Count));
		model.CullMode = CullMode.None;
		model.Materials[0].Color = color * alpha;

		LocalBounds = new BoundingBox(-new Vec3(radius, radius, 0), new Vec3(radius, radius, height));
	}

    public override void Added()
    {
		offset = World.Rng.Float() * MathF.Tau;
    }

    public void CollectModels(List<(Actor Actor, Model Model)> populate)
    {
		model.Flags = ModelFlags.Cutout;
		model.CullMode = CullMode.None;
		model.Transform = 
			Matrix.CreateRotationZ(World.GeneralTimer * speed + offset) *
			Matrix.CreateScale(radius, radius, height);
		populate.Add((this, model));
    }
}