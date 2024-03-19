namespace Celeste64;

public class IntroCar : Solid
{
	public readonly SkinnedModel WheelsModel;
	public readonly SkinnedModel BodyModel;
	public readonly float Scale = 6;
	public Vec3 SpawnPoint;
	public bool HasRider = false;

	public IntroCar(float scale)
	{
		this.Scale = scale;

		WheelsModel = new(Assets.Models["car_wheels"]);
		BodyModel = new(Assets.Models["car_top"]);

		// create solids out of body mesh ....?
		{
			var collider = new SkinnedModel(Assets.Models["car_collider"]);
			var vertices = new List<Vec3>();
			var faces = new List<Face>();
			var meshVertices = collider.Template.Vertices;
			var meshIndices = collider.Template.Indices;
			var mat = SkinnedModel.BaseTranslation * collider.Transform * Matrix.CreateScale(scale);

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

		Transparent = true;
	}

	public override void Added()
	{
		base.Added();
		Position += -Vec3.UnitZ * 1.3f;
		SpawnPoint = Position;
	}

	public override void Update()
	{
		base.Update();

		if (!HasRider && HasPlayerRider())
		{
			HasRider = true;
			Audio.Play(Sfx.sfx_car_down, Position);
		}
		else if (HasRider && !HasPlayerRider())
		{
			HasRider = false;
			Audio.Play(Sfx.sfx_car_up, Position);
		}

		var target = (HasRider ? SpawnPoint - Vec3.UnitZ * 1.5f : SpawnPoint);
		var step = Utils.Approach(Position, target, 20 * Time.Delta);
		MoveTo(step);
	}

	public override void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		// hack: don't use actor translation for wheels....
		WheelsModel.Transform =
			Matrix.CreateTranslation((SpawnPoint - Position) / Scale) *
			Matrix.CreateScale(Scale);

		BodyModel.Transform =
			Matrix.CreateScale(Scale);

		populate.Add((this, WheelsModel));
		populate.Add((this, BodyModel));
	}
}
