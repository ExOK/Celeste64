using System.Runtime.InteropServices;

namespace Celeste64;

public class SpriteRenderer
{
	private record struct SpriteBatch(Texture Texture, int Index, int Count);
	private readonly List<SpriteVertex> spriteVertices = [];
	private readonly List<int> spriteIndices = [];
	private readonly List<SpriteBatch> spriteBatches = [];
	private readonly Mesh spriteMesh = new();
	private readonly Material spriteMaterial = new(Assets.Shaders["Sprite"]);

	public void Render(ref RenderState state, List<Sprite> sprites, bool postEffects)
	{
		spriteVertices.Clear();
		spriteIndices.Clear();
		spriteBatches.Clear();

		SpriteBatch current = new();

		foreach (var board in sprites)
		{
			if (board.Subtexture.Texture == null)
				continue;

			if (board.Post != postEffects)
				continue;

			int i = spriteVertices.Count;

			if (board.Subtexture.Texture != current.Texture)
			{
				if (current.Count > 0)
					spriteBatches.Add(current);
				current.Texture = board.Subtexture.Texture;
				current.Index = i;
				current.Count = 0;
			}

			spriteVertices.Add(new(board.A, board.Subtexture.TexCoords0, board.Color));
			spriteVertices.Add(new(board.B, board.Subtexture.TexCoords1, board.Color));
			spriteVertices.Add(new(board.C, board.Subtexture.TexCoords2, board.Color));
			spriteVertices.Add(new(board.D, board.Subtexture.TexCoords3, board.Color));

			spriteIndices.Add(i + 0);
			spriteIndices.Add(i + 1);
			spriteIndices.Add(i + 2);
			spriteIndices.Add(i + 0);
			spriteIndices.Add(i + 2);
			spriteIndices.Add(i + 3);

			current.Count += 6;
		}

		if (current.Count > 0)
			spriteBatches.Add(current);

		spriteMesh.SetVertices<SpriteVertex>(CollectionsMarshal.AsSpan(spriteVertices));
		spriteMesh.SetIndices<int>(CollectionsMarshal.AsSpan(spriteIndices));
        if (spriteMaterial.Shader?.Has("u_matrix") ?? false)
		    spriteMaterial.Set("u_matrix", state.Camera.ViewProjection);
        if (spriteMaterial.Shader?.Has("u_far") ?? false)
		    spriteMaterial.Set("u_far", state.Camera.FarPlane);
        if (spriteMaterial.Shader?.Has("u_near") ?? false)
		    spriteMaterial.Set("u_near", state.Camera.NearPlane);

		foreach (var batch in spriteBatches)
		{
            if (spriteMaterial.Shader?.Has("u_texture") ?? false)
			    spriteMaterial.Set("u_texture", batch.Texture);

			var call = new DrawCommand(state.Camera.Target, spriteMesh, spriteMaterial)
			{
				BlendMode = BlendMode.Premultiply,
				DepthMask = false,
				DepthCompare = postEffects ? DepthCompare.Always : DepthCompare.Less,
				CullMode = CullMode.None,
				MeshIndexStart = batch.Index,
				MeshIndexCount = batch.Count
			};
			call.Submit();
			state.Calls++;
			state.Triangles += batch.Count / 3;
		}
	}
}