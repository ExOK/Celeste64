using System.Runtime.InteropServices;

namespace Celeste64;

public class FogRing : Actor, IHaveModels
{
	public readonly SimpleModel Model = new();
	public readonly float Height;
	public readonly float Radius;
	public readonly float Speed;
	public float Offset;

	public FogRing(Sledge.Formats.Map.Objects.Entity data)
	{
		var vertices = new List<Vertex>();
		var indices = new List<int>();
		var texture = data.GetStringProperty("texture", string.Empty);
		var alpha = data.GetFloatProperty("alpha", 1.0f);
		var color = Color.FromHexStringRGB(data.GetStringProperty("color", "ffffff"));
		Height = data.GetFloatProperty("height", 1.0f);
		Radius = data.GetFloatProperty("radius", 1.0f);
		Speed = data.GetFloatProperty("speed", 1.0f);

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


		Model.Mesh.SetVertices<Vertex>(CollectionsMarshal.AsSpan(vertices));
		Model.Mesh.SetIndices<int>(CollectionsMarshal.AsSpan(indices));
		Model.Materials.Add(new DefaultMaterial(Assets.Textures[texture]));
		Model.Parts.Add(new(0, 0, indices.Count));
		Model.CullMode = CullMode.None;
		Model.Materials[0].Color = color * alpha;

		LocalBounds = new BoundingBox(-new Vec3(Radius, Radius, 0), new Vec3(Radius, Radius, Height));
	}

    public override void Added()
    {
		Offset = World.Rng.Float() * MathF.Tau;
    }

    public virtual void CollectModels(List<(Actor Actor, Model Model)> populate)
    {
		Model.Flags = ModelFlags.Cutout;
		Model.CullMode = CullMode.None;
		Model.Transform = 
			Matrix.CreateRotationZ(World.GeneralTimer * Speed + Offset) *
			Matrix.CreateScale(Radius, Radius, Height);
		populate.Add((this, Model));
    }
}