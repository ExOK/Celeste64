using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Celeste64;


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct SpriteVertex(Vec3 position, Vec2 texcoord, Color color) : IVertex
{
	public readonly Vec3 Pos = position;
	public readonly Vec2 Tex = texcoord;
	public readonly Color Color = color;
	public readonly VertexFormat Format => VertexFormat;

	private static readonly VertexFormat VertexFormat = VertexFormat.Create<SpriteVertex>(
	[
		new (0, VertexType.Float3, normalized: false),
		new (1, VertexType.Float2, normalized: false),
		new (2, VertexType.UByte4, normalized: true)
	]);
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Vertex(Vec3 position, Vec2 texcoord, Vec3 color, Vec3 normal, Vec4 joint, Vec4 weight) : IVertex
{
	public readonly Vec3 Pos = position;
	public readonly Vec2 Tex = texcoord;
	public readonly Vec3 Col = color;
	public readonly Vec3 Normal = normal;
	public readonly Vec4 Joint = joint;
	public readonly Vec4 Weight = weight;
	public readonly VertexFormat Format => VertexFormat;

	public Vertex(Vec3 position, Vec2 texcoord, Vec3 color, Vec3 normal)
		: this(position, texcoord, color, normal, Vec4.Zero, Vec4.One) { }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vertex Transform(in Matrix mat)
	{
		return new Vertex(
			Vec3.Transform(Pos, mat),
			Tex,
			Col,
			Vec3.TransformNormal(Normal, mat),
			Joint,
			Weight
		);
	}

	private static readonly VertexFormat VertexFormat = VertexFormat.Create<Vertex>(
	[
		new (0, VertexType.Float3, normalized: false),
		new (1, VertexType.Float2, normalized: false),
		new (2, VertexType.Float3, normalized: true),
		new (3, VertexType.Float3, normalized: false),
		new (4, VertexType.Float4, normalized: false),
		new (5, VertexType.Float4, normalized: false),
	]);
}