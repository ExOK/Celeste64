using SharpGLTF.Transforms;
using System.Runtime.InteropServices;

namespace Celeste64;

public class SimpleModel : Model
{
	public record struct Part(int MaterialIndex, int IndexStart, int IndexCount);
	public readonly Mesh Mesh = new();
	public readonly List<Part> Parts = [];
	public CullMode CullMode = CullMode.Back;

	public SimpleModel() { }

	public SimpleModel(List<SimpleModel> combine)
	{
		throw new NotImplementedException();
	}
	
	public SimpleModel(List<SkinnedModel> combine)
	{
		var vertices = new List<Vertex>();
		var indices = new List<int>();
		
		foreach (var it in combine)
		{
			for (int i = 0; i < it.Instance.Count; i++)
			{
				var drawable = it.Instance[i];

				if (drawable.Transform is not RigidTransform statXform)
					continue;

				var meshPart = it.Template.Parts[drawable.Template.LogicalMeshIndex];
				var meshMatrix = statXform.WorldMatrix * it.Transform;
				var meshVertices = it.Template.Vertices;
				var meshIndices = it.Template.Indices;
				var vertexOffset = vertices.Count;
				var indexOffset = indices.Count;

				for (int n = 0; n < meshVertices.Count; n++)
					vertices.Add(meshVertices[n].Transform(meshMatrix));

				for (int n = 0; n < meshIndices.Count; n++)
					indices.Add(vertexOffset + meshIndices[n]);

				foreach (var primitive in meshPart)
				{
					var mat = it.Materials[primitive.Material];
					var matIndex = Materials.IndexOf(mat);
					if (matIndex < 0)
					{
						matIndex = Materials.Count;
						Materials.Add(mat);
					}

					var next = new Part()
					{
						MaterialIndex = matIndex,
						IndexStart = primitive.Index + indexOffset,
						IndexCount = primitive.Count
					};

					if (Parts.Count > 0)
					{
						var last = Parts[^1];
						if (last.MaterialIndex == next.MaterialIndex)
						{
							var end = last.IndexStart + last.IndexCount;
							if (end == next.IndexStart)
							{
								last.IndexCount += next.IndexCount;
								Parts[^1] = last;
								continue;
							}
						}
					}

					Parts.Add(next);
				}
			}
		}

		Mesh.SetVertices<Vertex>(CollectionsMarshal.AsSpan(vertices));
		Mesh.SetIndices<int>(CollectionsMarshal.AsSpan(indices));
		Transform = Matrix.Identity;
		Flags = ModelFlags.Terrain;
	}

	public override void Render(ref RenderState state)
	{
		foreach (var mat in Materials)
		{
			state.ApplyToMaterial(mat, Matrix.Identity);

			if (mat.Shader != null &&
				mat.Shader.Has("u_jointMult"))
				mat.Set("u_jointMult", 0.0f);
		}

		foreach (var segment in Parts)
		{
			if (segment.IndexCount <= 0 || segment.MaterialIndex < 0)
				continue;

			var call = new DrawCommand(state.Camera.Target, Mesh, Materials[segment.MaterialIndex])
			{
				DepthCompare = state.DepthCompare,
				DepthMask = state.DepthMask,
				CullMode = CullMode,
				MeshIndexStart = segment.IndexStart,
				MeshIndexCount = segment.IndexCount
			};
			call.Submit();
			state.Calls++;
			state.Triangles += segment.IndexCount / 3;
		}
	}
}
