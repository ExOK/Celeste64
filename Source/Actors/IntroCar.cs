
namespace Celeste64;

public class IntroCar : Solid
{
	private readonly SkinnedModel wheels;
	private readonly SkinnedModel body;
	private readonly float scale = 6;
	private Vec3 spawnPoint;
	private bool hasRider = false;

	public IntroCar(float scale)
	{
		this.scale = scale;

		wheels = new(Assets.Models["car_wheels"]);
		body = new(Assets.Models["car_top"]);

		// create solids out of body mesh ....?
		{
			var collider = new SkinnedModel(Assets.Models["car_collider"]);
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
		spawnPoint = Position;
    }

    public override void Update()
    {
        base.Update();

		if (!hasRider && HasPlayerRider())
		{
			hasRider = true;
			Audio.Play(Sfx.sfx_car_down, Position);
		}
		else if (hasRider && !HasPlayerRider())
		{
			hasRider = false;
			Audio.Play(Sfx.sfx_car_up, Position);
		}

		var target = (hasRider ? spawnPoint - Vec3.UnitZ * 1.5f : spawnPoint);
		var step = Utils.Approach(Position, target, 20 * Time.Delta);
		MoveTo(step);
    }

    public override void CollectModels(List<(Actor Actor, Model Model)> populate)
    {
		// hack: don't use actor translation for wheels....
		wheels.Transform = 
			Matrix.CreateTranslation((spawnPoint - Position) / scale) * 
			Matrix.CreateScale(scale);
		
		body.Transform = 
			Matrix.CreateScale(scale);

		populate.Add((this, wheels));
		populate.Add((this, body));
    }
}