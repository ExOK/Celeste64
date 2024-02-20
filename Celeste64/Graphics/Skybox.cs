using System.Runtime.InteropServices;

namespace Celeste64;

public class Skybox
{
	public readonly Texture Texture;
	private readonly Mesh mesh = new();
	private readonly Material material = new(Assets.Shaders["Sprite"]);

	public Skybox(Texture texture)
	{
		Texture = texture;

		var block = Texture.Width / 4;
		
		var u = new Subtexture(Texture, new Rect(block * 0, block * 0, block, block));
		var d = new Subtexture(Texture, new Rect(block * 0, block * 2, block, block));
		var n = new Subtexture(Texture, new Rect(block * 0, block * 1, block, block));
		var e = new Subtexture(Texture, new Rect(block * 1, block * 1, block, block));
		var s = new Subtexture(Texture, new Rect(block * 2, block * 1, block, block));
		var w = new Subtexture(Texture, new Rect(block * 3, block * 1, block, block));

		var v0 = new Vec3(-1, -1, 1);
		var v1 = new Vec3(1, -1, 1);
		var v2 = new Vec3(1, 1, 1);
		var v3 = new Vec3(-1, 1, 1);
		var v4 = new Vec3(-1, -1, -1);
		var v5 = new Vec3(1, -1, -1);
		var v6 = new Vec3(1, 1, -1);
		var v7 = new Vec3(-1, 1, -1);
		
		var verts = new List<SpriteVertex>();
		var indices = new List<int>();

		AddFace(verts, indices, v0, v1, v2, v3, u.TexCoords3, u.TexCoords2, u.TexCoords1, u.TexCoords0);
		AddFace(verts, indices, v7, v6, v5, v4, d.TexCoords3, d.TexCoords2, d.TexCoords1, d.TexCoords0);
		AddFace(verts, indices, v4, v5, v1, v0, n.TexCoords2, n.TexCoords3, n.TexCoords0, n.TexCoords1);
		AddFace(verts, indices, v6, v7, v3, v2, s.TexCoords2, s.TexCoords3, s.TexCoords0, s.TexCoords1);
		AddFace(verts, indices, v0, v3, v7, v4, e.TexCoords0, e.TexCoords1, e.TexCoords2, e.TexCoords3);
		AddFace(verts, indices, v5, v6, v2, v1, w.TexCoords2, w.TexCoords3, w.TexCoords0, w.TexCoords1);

		mesh.SetVertices<SpriteVertex>(CollectionsMarshal.AsSpan(verts));
		mesh.SetIndices<int>(CollectionsMarshal.AsSpan(indices));
	}

	private static void AddFace(List<SpriteVertex> verts, List<int> indices, in Vec3 a, in Vec3 b, in Vec3 c, in Vec3 d, in Vec2 v0, in Vec2 v1, in Vec2 v2, in Vec2 v3)
	{
		int n = verts.Count;

		verts.Add(new SpriteVertex(a, v0, Color.White));
		verts.Add(new SpriteVertex(b, v1, Color.White));
		verts.Add(new SpriteVertex(c, v2, Color.White));
		verts.Add(new SpriteVertex(d, v3, Color.White));

		indices.Add(n + 0);indices.Add(n + 1);indices.Add(n + 2);
		indices.Add(n + 0);indices.Add(n + 2);indices.Add(n + 3);
	}

	public void Render(in Camera camera, in Matrix transform, float size)
	{
		var mat = Matrix.CreateScale(size) * transform * camera.ViewProjection;
        if (material.Shader?.Has("u_matrix") ?? false)
		    material.Set("u_matrix", mat);
        if (material.Shader?.Has("u_near") ?? false)
		    material.Set("u_near", camera.NearPlane);
        if (material.Shader?.Has("u_far") ?? false)
		    material.Set("u_far", camera.FarPlane);
        if (material.Shader?.Has("u_texture") ?? false)
		    material.Set("u_texture", Texture);

		DrawCommand cmd = new(camera.Target, mesh, material)
		{
			DepthMask = false,
			DepthCompare = DepthCompare.Always,
			CullMode = CullMode.Front
		};
		cmd.Submit();
	}
}