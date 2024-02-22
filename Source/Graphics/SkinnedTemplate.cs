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
	public readonly SharpGLTF.Schema2.ModelRoot Root;
	public readonly SharpGLTF.Runtime.SceneTemplate Template;
	public readonly List<Vertex> Vertices = [];
	public readonly List<int> Indices = [];
	public readonly List<MeshPrimitive>[] Parts;
	public readonly DefaultMaterial[] Materials;

	// only used while loading, cleared afterwards
	private readonly Dictionary<SharpGLTF.Memory.MemoryImage, Image> images = [];

	public SkinnedTemplate(SharpGLTF.Schema2.ModelRoot model)
	{
		Root = model;
		Template = SharpGLTF.Runtime.SceneTemplate.Create(model.DefaultScene);
		Parts = new List<MeshPrimitive>[model.LogicalMeshes.Count];
		
		// load the model's images
		foreach (var logicalImage in model.LogicalImages)
		{
			using var stream = new MemoryStream(logicalImage.Content.Content.ToArray());
			var img = new Image(stream);
			images.Add(logicalImage.Content, img);
		}

		// All Materials use the default material
		Materials = new DefaultMaterial[model.LogicalMaterials.Count];

		// create vertex/index array
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
		// create all the textures and clear the list of images we had loaded
		var textures = new Dictionary<SharpGLTF.Memory.MemoryImage, Texture>();
		foreach (var image in images)
			textures[image.Key] = new Texture(image.Value);
		images.Clear();

		// create all the materials, find their textures
		for (int i = 0; i < Root.LogicalMaterials.Count; i++)
		{
			var logicalMat = Root.LogicalMaterials[i];

			Materials[i] = new DefaultMaterial() { Name = logicalMat.Name };

			// figure out which texture to use by just using the first texture found
			foreach (var channel in logicalMat.Channels)
				if (channel.Texture != null && 
					channel.Texture.PrimaryImage != null && 
					textures.TryGetValue(channel.Texture.PrimaryImage.Content, out var texture))
				{
					Materials[i].Texture = texture;
					break;
				}
		}

		// upload verts to the mesh
		Mesh = new();
		Mesh.SetVertices<Vertex>(CollectionsMarshal.AsSpan(Vertices));
		Mesh.SetIndices<int>(CollectionsMarshal.AsSpan(Indices));
	}
}