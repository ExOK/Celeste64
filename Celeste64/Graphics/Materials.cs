using System.Diagnostics;

namespace Celeste64;

public class DefaultMaterial : Material
{
	public const string MatrixUniformName = "u_mvp";

	public string Name = string.Empty;

    private Texture? texture = null;
	private Matrix matrix;
	private Matrix model;
    private Color color;
    private float effects;
    private float near;
    private float far;
    private bool cutout;
    private bool silhouette;
	private Color silhouetteColor;
    private float time;
    private Vec3 sun;
    private Color verticalFogColor;

    public DefaultMaterial(Texture? texture = null)
		: base(Assets.Shaders["Default"])
	{
        if (!(Shader?.Has(MatrixUniformName) ?? false))
        {
            Log.Warning($"Shader '{Shader?.Name}' is missing '{MatrixUniformName}' uniform");
        }
        
        Texture = texture;
		Color = Color.White;
		Effects = 1.0f;
    }

    public Matrix MVP
    {
        get => matrix;
        set
        {
            matrix = value;
            if (Shader?.Has(MatrixUniformName) ?? false)
                Set(MatrixUniformName, value);
        }
    }

    public Matrix Model
    {
        get => model;
        set
        {
            model = value;
            if (Shader?.Has("u_model") ?? false)
                Set("u_model", value);
        }
    }

    public Texture? Texture
	{
		get => texture;
		set
		{
			if (texture != value)
			{
				texture = value;
				if (Shader?.Has("u_texture") ?? false)
					Set("u_texture", value);
			}
		}
    }

    public Color Color
    {
        get => color;
		set
		{
            if (color != value)
            {
                color = value;
                if (Shader?.Has("u_color") ?? false)
                    Set("u_color", value);
            }
        }
    }

    public float NearPlane
    {
        get => near;
        set
        {
            if (near != value)
            {
                near = value;
                if (Shader?.Has("u_near") ?? false)
                    Set("u_near", value);
            }
        }
    }

    public float FarPlane
    {
        get => far;
        set
        {
            if (far != value)
            {
                far = value;
                if (Shader?.Has("u_far") ?? false)
                    Set("u_far", value);
            }
        }
    }

    public float Effects
    {
        get => effects;
		set
		{
            if (effects != value)
            {
                effects = value;
                if (Shader?.Has("u_effects") ?? false)
                    Set("u_effects", value);
            }
        }
    }

    public bool Cutout
    {
        get => cutout;
        set
        {
            if (cutout != value)
            {
                cutout = value;
                if (Shader?.Has("u_cutout") ?? false)
                    Set("u_cutout", value ? 0.50f : 0.0f);
            }
        }
    }

    public bool Silhouette
    {
        get => silhouette;
        set
        {
            if (silhouette != value)
            {
                silhouette = value;
                if (Shader?.Has("u_silhouette") ?? false)
                    Set("u_silhouette", value ? 1.0f : 0.0f);
            }
        }
    }

    public Color SilhouetteColor
    {
        get => silhouetteColor;
        set
        {
            if (silhouetteColor != value)
            {
                silhouetteColor = value;
                if (Shader?.Has("u_silhouette_color") ?? false)
                    Set("u_silhouette_color", value);
            }
        }
    }

    public Color VerticalFogColor
    {
        get => verticalFogColor;
        set
        {
            if (verticalFogColor != value)
            {
                verticalFogColor = value;
                if (Shader?.Has("u_vertical_fog_color") ?? false)
                    Set("u_vertical_fog_color", value);
            }
        }
    }

    public Vec3 SunDirection
    {
        get => sun;
        set
        {
            if (sun != value)
            {
                sun = value;
                if (Shader?.Has("u_sun") ?? false)
                    Set("u_sun", value);
            }
        }
    }

    public float Time
    {
        get => time;
        set
        {
            if (time != value)
            {
                time = value;
                if (Shader?.Has("u_time") ?? false)
                    Set("u_time", value);
            }
        }
    }

    public virtual DefaultMaterial Clone()
	{
		var copy = new DefaultMaterial(Texture);
		CopyTo(copy);
        copy.MVP = MVP;
        copy.Model = model;
        copy.Texture = Texture;
		copy.Color = Color;
        copy.Silhouette = Silhouette;
        copy.SilhouetteColor = SilhouetteColor;
        copy.Time = Time;
        copy.SunDirection = SunDirection;
        copy.NearPlane = NearPlane;
        copy.FarPlane = FarPlane;
        return copy;
	}
}