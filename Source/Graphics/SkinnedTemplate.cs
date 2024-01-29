using System.Runtime.InteropServices;

namespace Celeste64;

public class SkinnedTemplate
{
	public struct MeshPrimitive
	{
		public int Material;
		public int Index;
		public int Count;
	}

	public Mesh Mesh { get; set; } = null!;
	public Texture? Texture { get; private set; } = null;
	public readonly SharpGLTF.Schema2.ModelRoot Root;
	public readonly SharpGLTF.Runtime.SceneTemplate Template;
	public readonly List<Vertex> Vertices = [];
	public readonly List<int> Indices = [];
	public readonly List<MeshPrimitive>[] Parts;
	public readonly DefaultMaterial[] Materials;

	private readonly Image? image;

	public SkinnedTemplate(SharpGLTF.Schema2.ModelRoot model)
	{
		Root = model;
		Template = SharpGLTF.Runtime.SceneTemplate.Create(model.DefaultScene);
		Parts = new List<MeshPrimitive>[model.LogicalMeshes.Count];
		
		// Only load the first Texture, we only use one
		if (model.LogicalImages.Count > 0)
		{
			using var stream = new MemoryStream(model.LogicalImages[0].Content.Content.ToArray());
			image = new Image(stream);
		}

		// All Materials use the default Texture
		Materials = new DefaultMaterial[model.LogicalMaterials.Count];

		for (int i = 0; i < model.LogicalMeshes.Count; i ++)
		{
			var mesh = model.LogicalMeshes[i];
			var vertexCount = Vertices.Count;
			var indexStart = Indices.Count;
			var part = Parts[i] = new();

			foreach (var primitive in mesh.Primitives)
			{
				var verts = primitive.GetVertexAccessor("POSITION").AsVector3Array();
				var uvs = primitive.GetVertexAccessor("TEXCOORD_0").AsVector2Array();
				var normals = primitive.GetVertexAccessor("NORMAL").AsVector3Array();
				var weights = primitive.GetVertexAccessor("WEIGHTS_0");
				var joints = primitive.GetVertexAccessor("JOINTS_0");

				// not all primitives have weights/joints
				if (weights != null && joints != null)
				{
					var ws = weights.AsVector4Array();
					var js = joints.AsVector4Array();

					for (int j = 0; j < verts.Count; j++)
					{
						Vertices.Add(new Vertex(verts[j], uvs[j], Vec3.One, normals[j], js[j], ws[j]));
					}
				}
				else
				{
					for (int j = 0; j < verts.Count; j++)
					{
						Vertices.Add(new Vertex(verts[j], uvs[j], Vec3.One, normals[j], new(), Vec4.One));
					}
				}

				foreach (var index in primitive.GetIndices())
					Indices.Add(vertexCount + (int)index);
				
				part.Add(new()
				{
					Material = primitive.Material?.LogicalIndex ?? 0,
					Index = indexStart,
					Count = Indices.Count - indexStart
				});
			}
		}
	}

	public void ConstructResources()
	{
		if (image != null)
			Texture = new(image);

		for (int i = 0; i < Root.LogicalMaterials.Count; i++)
			Materials[i] = new DefaultMaterial(Texture) { Name = Root.LogicalMaterials[i].Name };

		Mesh = new();
		Mesh.SetVertices<Vertex>(CollectionsMarshal.AsSpan(Vertices));
		Mesh.SetIndices<int>(CollectionsMarshal.AsSpan(Indices));
	}
}