
namespace Celeste64;

public struct RenderState
{
	public Camera Camera;
	public Matrix ModelMatrix;
	public bool Silhouette;
	public Vec3 SunDirection;
	public Color VerticalFogColor;
	public DepthCompare DepthCompare;
	public bool DepthMask;
	public bool CutoutMode;
	public int Calls;
	public int Triangles;

	public void ApplyToMaterial(DefaultMaterial mat, in Matrix localTransformation)
	{
		if (mat.Shader == null)
			return;

		mat.Model = localTransformation * ModelMatrix;
		mat.MVP = mat.Model * Camera.ViewProjection;
		mat.NearPlane = Camera.NearPlane;
		mat.FarPlane = Camera.FarPlane;
		mat.Silhouette = Silhouette;
		mat.Time = (float)Time.Duration.TotalSeconds;
		mat.SunDirection = SunDirection;
		mat.VerticalFogColor = VerticalFogColor;
		mat.Cutout = CutoutMode;
	}
}