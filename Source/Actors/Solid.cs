
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

	public Vec3[] WorldVertices
	{
		get
		{
			ValidateTransformations();
			return worldVertices;
		}
	}
	public Face[] WorldFaces
	{
		get
		{
			ValidateTransformations();
			return worldFaces;
		}
	}

	public Vec3 Velocity = Vec3.Zero;

	public float TShake;

	private bool initialized = false;
	private Vec3[] worldVertices = [];
	private Face[] worldFaces = [];
	private BoundingBox lastWorldBounds;

	public override void Created()
	{
		worldVertices = new Vec3[LocalVertices.Length];
		worldFaces = new Face[LocalFaces.Length];
		lastWorldBounds = new();
		initialized = true;
		Transformed();
	}

    public override void Destroyed()
    {
		World.SolidGrid.Remove(this, new Rect(lastWorldBounds.Min.XY(), lastWorldBounds.Max.XY()));
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

	protected override void Transformed()
	{
		// realistically instead of transforming all the points, we could
		// inverse the matrix and test against that instead ... but *shrug*
		if (initialized)
		{
			var mat = Matrix;
			for (int i = 0; i < LocalVertices.Length; i ++)
				worldVertices[i] = Vec3.Transform(LocalVertices[i], mat);
			
			for (int i = 0; i < LocalFaces.Length; i ++)
			{
				worldFaces[i] = LocalFaces[i];
				worldFaces[i].Plane = Plane.Transform(LocalFaces[i].Plane, mat);
			}

			if (Alive)
			{
				World.SolidGrid.Remove(this, new Rect(lastWorldBounds.Min.XY(), lastWorldBounds.Max.XY()));
				World.SolidGrid.Insert(this, new Rect(WorldBounds.Min.XY(), WorldBounds.Max.XY()));
				lastWorldBounds = WorldBounds;
			}
		}
	}

	public bool HasPlayerRider()
	{
		return World.Get<Player>()?.RidingPlatformCheck(this) ?? false;
	}

	public void MoveTo(Vec3 target)
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