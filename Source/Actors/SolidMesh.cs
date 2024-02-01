
namespace Celeste64;

public class SolidMesh : Solid
{
	private readonly SkinnedModel model;
	private readonly float scale = 6;
	private Vec3 spawnPoint;

	public SolidMesh(SkinnedTemplate model, float scale)
	{
		this.scale = scale;

		this.model = new(model);

		// create solids out of mesh ....?
		{
			SkinnedModel collider = new(model);
			var vertices = new List<Vec3>();
			var faces = new List<Face>();
			var meshVertices = collider.Template.Vertices;
			var meshIndices = collider.Template.Indices;
			var mat = SkinnedModel.BaseTranslation * collider.Transform * Matrix.CreateScale(scale);

			for (int i = 0; i < collider.Instance.Count; i++)
			{
				var drawable = collider.Instance[i];
				if (drawable.Transform is not SharpGLTF.Transforms.RigidTransform statXform)
					continue;

				var meshPart = collider.Template.Parts[drawable.Template.LogicalMeshIndex];
				var meshMatrix = statXform.WorldMatrix * mat;

				foreach (var primitive in meshPart)
				{
					int v = vertices.Count;
					for (int n = 0; n < primitive.Count; n ++)
						vertices.Add(Vec3.Transform(meshVertices[meshIndices[primitive.Index + n + 0]].Pos, meshMatrix));
					for (int n = 0; n < primitive.Count; n += 3)
					{
						faces.Add(new Face()
						{
							Plane = Plane.CreateFromVertices(vertices[v + n + 0], vertices[v + n + 1], vertices[v + n + 2]), 
							Indices = [v + n + 0, v + n + 1, v + n + 2]
						});
					}
				}
			}

			LocalVertices = [.. vertices];
			LocalFaces = [.. faces];
			LocalBounds = new BoundingBox(
				vertices.Aggregate(Vec3.Min),
				vertices.Aggregate(Vec3.Max)
			);
		}

		Transparent = true;
	}

    public override void Added()
    {
        base.Added();
		Position += -Vec3.UnitZ * 1.3f;
		spawnPoint = Position;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void CollectModels(List<(Actor Actor, Model Model)> populate)
    {
		// hack: don't use actor translation for wheels....		
		model.Transform = 
			Matrix.CreateScale(scale);

		populate.Add((this, model));
    }
}