
namespace Celeste64;

public struct Camera()
{
	private Target? target = null;
	private Vec3 position =  Vec3.Zero;
	private Vec3 lookAt = new(0, 1, 0);
	private float nearPlane = 0.01f;
	private float farPlane = 100f;
	private float fovMultiplier = 1.0f;

	private Vec3? normal;
	private Matrix? view;
	private Matrix? projection;
	private Matrix? viewProjection;
	private Quaternion? rotation;
	private Vec3? upwards;
	private Vec3? leftwards;
	private BoundingFrustum? frustum;

	public Target? Target
	{
		readonly get => target;
		set
		{
			if (target != value)
			{
				target = value;
				MakeDirty();
			}
		}
	}

	public Vec3 Position
	{
		readonly get => position;
		set
		{
			if (position != value)
			{
				position = value;
				MakeDirty();
			}
		}
	}

	public float FOVMultiplier
	{
		readonly get => fovMultiplier;
		set
		{
			if (fovMultiplier != value)
			{
				fovMultiplier = value;
				MakeDirty();
			}
		}
	}

	public Vec3 LookAt
	{
		readonly get => lookAt;
		set
		{
			if (lookAt != value)
			{
				lookAt = value;
				MakeDirty();
			}
		}
	}

	public float NearPlane
	{
		readonly get => nearPlane;
		set
		{
			if (nearPlane != value)
			{
				nearPlane = value;
				MakeDirty();
			}
		}
	}

	public float FarPlane
	{
		readonly get => farPlane;
		set
		{
			if (farPlane != value)
			{
				farPlane = value;
				MakeDirty();
			}
		}
	}

	public Vec3 Normal
	{
		get
		{
			if (!normal.HasValue)
				normal = (LookAt - Position).Normalized();
			return normal.Value;
		}
	}

	public Matrix View
	{
		get
		{
			if (!view.HasValue)
				view = Matrix.CreateLookAt(Position, LookAt, Vec3.UnitZ);
			return view.Value;
		}
	}

	public Matrix Projection
	{
		get
		{
			if (!projection.HasValue)
			{
				var width = target?.Width ?? App.WidthInPixels;
				var height = target?.Height ?? App.HeightInPixels;
				projection = Matrix.CreatePerspectiveFieldOfView(MathF.PI / 4.0f * fovMultiplier, width / (float)height, nearPlane, farPlane);
			}
			return projection.Value;
		}
	}

	public Matrix ViewProjection
	{
		get
		{
			if (!viewProjection.HasValue)
				viewProjection = View * Projection;
			return viewProjection.Value;
		}
	}

	public Quaternion Rotation
	{
		get
		{
			if (!rotation.HasValue)
				rotation = Quaternion.CreateFromRotationMatrix(View);
			return rotation.Value;
		}
	}

	public BoundingFrustum Frustum
	{
		get
		{
			if (!frustum.HasValue)
				frustum = new BoundingFrustum(ViewProjection);
			return frustum.Value;
		}
	}

	public Vec3 Forward => Normal;

	public Vec3 Up
	{
		get
		{
			if (!upwards.HasValue)
				upwards = Vec3.Transform(Vec3.UnitY, Rotation.Conjugated());
			return upwards.Value;
		}
	}

	public Vec3 Left
	{
		get
		{
			if (!leftwards.HasValue)
				leftwards = Vec3.Transform(-Vec3.UnitX, Rotation.Conjugated());
			return leftwards.Value;
		}
	}

	private void MakeDirty()
	{
		normal = null;
		view = null;
		projection = null;
		viewProjection = null;
		rotation = null;
		upwards = null;
		leftwards = null;
		frustum = null;
	}
}
