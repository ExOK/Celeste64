namespace Celeste64;

public class SolidMesh : Solid
{
	public readonly SkinnedModel ObjectModel;
	public readonly float Scale = 5;
	public Vec3 SpawnPoint;

	public SolidMesh(SkinnedTemplate model, float scale)
	{
		Scale = scale * 0.2f;

		ObjectModel = new(model)
		{
			Flags = ModelFlags.Terrain,
			Transform = Matrix.CreateScale(0.2f),
		};

		// create solids out of mesh ....?
		{
			SkinnedModel collider = new(model);
			var vertices = new List<Vec3>();
			var faces = new List<Face>();
			var meshVertices = collider.Template.Vertices;
			var meshIndices = collider.Template.Indices;
			var mat = SkinnedModel.BaseTranslation * collider.Transform * Matrix.CreateScale(Scale);

			foreach (var drawable in collider.Instance)
			{
				if (drawable.Transform is not SharpGLTF.Transforms.RigidTransform statXform)
					continue;

				var meshPart = collider.Template.Parts[drawable.Template.LogicalMeshIndex];
				var meshMatrix = statXform.WorldMatrix * mat;

				foreach (var primitive in meshPart)
				{
					int v = vertices.Count;
					for (int n = 0; n < primitive.Count; n++)
						vertices.Add(Vec3.Transform(meshVertices[meshIndices[primitive.Index + n + 0]].Pos, meshMatrix));
					for (int n = 0; n < primitive.Count; n += 3)
					{
						faces.Add(new Face()
						{
							Plane = Plane.CreateFromVertices(vertices[v + n + 0], vertices[v + n + 1], vertices[v + n + 2]),
							VertexStart = v + n,
							VertexCount = 3
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
	}

	public override void Added()
	{
		base.Added();
		SpawnPoint = Position;
	}

	public override void Update()
	{
		base.Update();
	}

	public override void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		ObjectModel.Transform = Matrix.CreateScale(Scale);

		populate.Add((this, ObjectModel));
	}
}
