
namespace Celeste64;

public abstract class Model
{
	public Matrix Transform = Matrix.Identity;
	public ModelFlags Flags;
	public readonly List<DefaultMaterial> Materials = [];
    public bool HasUniqueMaterials { get; private set; } = false;

    /// <summary>
    /// Makes our Materials unique, if they're not already
    /// </summary>
    public void MakeMaterialsUnique()
    {
        if (!HasUniqueMaterials)
        {
            HasUniqueMaterials = true;
            for (int i = 0; i < Materials.Count; i++)
                Materials[i] = Materials[i].Clone();
        }
    }

    public virtual void Prepare() { }
    public abstract void Render(ref RenderState state);
}