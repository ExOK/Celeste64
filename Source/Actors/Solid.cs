
namespace Celeste64;

public class Solid : Actor, IHaveModels
{
	/// <summary>
	/// If we're currently solid
	/// </summary>
	public bool Collidable = true;

	/// <summary>
	/// If the Camera should care about it
	/// </summary>
	public bool Transparent = false;

	/// <summary>
	/// Visual Model to Draw
	/// </summary>
	public readonly SimpleModel Model = new() { Flags = ModelFlags.Terrain };

	public readonly List<Attacher> Attachers = [];

	/// <summary>
	/// Collision Face
	/// </summary>
	public struct Face
	{
		public Plane Plane;
		public int VertexStart;
		public int VertexCount;
	}

	public Vec3[] LocalVertices = [];
	public Face[] LocalFaces = [];

	public virtual Vec3[] WorldVertices
	{
		get
		{
			ValidateTransformations();
			return WorldVerticesLocal;
		}
	}
	public virtual Face[] WorldFaces
	{
		get
		{
			ValidateTransformations();
			return WorldFacesLocal;
		}
	}

	public Vec3 Velocity = Vec3.Zero;

	public float TShake;

	public bool Initialized = false;
	public Vec3[] WorldVerticesLocal = [];
	public Face[] WorldFacesLocal = [];
	public BoundingBox LastWorldBounds;

	public override void Created()
	{
		WorldVerticesLocal = new Vec3[LocalVertices.Length];
		WorldFacesLocal = new Face[LocalFaces.Length];
		LastWorldBounds = new();
		Initialized = true;
		Transformed();
	}

    public override void Destroyed()
    {
		World.SolidGrid.Remove(this, new Rect(LastWorldBounds.Min.XY(), LastWorldBounds.Max.XY()));
    }

    public override void Update()
	{
		if (Velocity.LengthSquared() > .001f)
			MoveTo(Position + Velocity * Time.Delta);

		// virtual shaking
		if (TShake > 0)
		{
			TShake -= Time.Delta;
			if (TShake <= 0)
				Model.Transform = Matrix.Identity;
			else if (Time.OnInterval(.02f))
				Model.Transform = Matrix.CreateTranslation(World.Rng.Float(-1, 1), World.Rng.Float(-1, 1), 0);
		}
	}

	public override void Transformed()
	{
		// realistically instead of transforming all the points, we could
		// inverse the matrix and test against that instead ... but *shrug*
		if (Initialized)
		{
			var mat = Matrix;
			for (int i = 0; i < LocalVertices.Length; i ++)
				WorldVerticesLocal[i] = Vec3.Transform(LocalVertices[i], mat);
			
			for (int i = 0; i < LocalFaces.Length; i ++)
			{
				WorldFacesLocal[i] = LocalFaces[i];
				WorldFacesLocal[i].Plane = Plane.Transform(LocalFaces[i].Plane, mat);
			}

			if (Alive)
			{
				World.SolidGrid.Remove(this, new Rect(LastWorldBounds.Min.XY(), LastWorldBounds.Max.XY()));
				World.SolidGrid.Insert(this, new Rect(WorldBounds.Min.XY(), WorldBounds.Max.XY()));
				LastWorldBounds = WorldBounds;
			}
		}
	}

	public virtual bool HasPlayerRider()
	{
		return World.Get<Player>()?.RidingPlatformCheck(this) ?? false;
	}

	public virtual void MoveTo(Vec3 target)
	{
		var delta = (target - Position);

		if (Collidable)
		{
			if (delta.LengthSquared() > 0.001f)
			{
				foreach (var actor in World.All<IRidePlatforms>())
				{
					if (actor == this || actor is not IRidePlatforms rider)
						continue;
					
					if (rider.RidingPlatformCheck(this))
					{
						Collidable = false;
						rider.RidingPlatformSetVelocity(Velocity);
						rider.RidingPlatformMoved(delta);
						Collidable = true;
					}
				}

				Position += delta;
			}
		}
		else
		{
			Position += delta;
		}
	}

	public virtual void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		populate.Add((this, Model));
	}
}