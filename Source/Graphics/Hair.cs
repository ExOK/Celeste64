using System.Runtime.InteropServices;

namespace Celeste64;

public class Hair : Model
{
	public int Nodes;
	public Color Color;
	public Vec3 Origin;
	public Vec3 OffsetPerNode;
	public Vec3 Squish = Vec3.One;
	public Vec3 Forward;
	public bool Grounded;
	public float Roundness = 0;
	public float ForwardOffsetPerNode = 0.50f;

	private readonly Mesh mesh = new();
	private readonly List<Vertex> sphereVertices = [];
	private readonly List<int> sphereIndices = [];
	private readonly List<Vertex> hairVertices = [];
	private readonly List<int> hairIndices = [];
	private readonly List<Vec3> nodes = [];
	private float wave = 0;
	private bool modified = true;

	public Hair()
	{
		Utils.CreateSphere(sphereVertices, sphereIndices, 6, 6);
		Materials.Add(new DefaultMaterial(Assets.Textures["white"]));
		Origin = new Vec3(0, .8f, -.6f);
		Nodes = 10;
	}

	public void CopyState(Hair other)
	{
		Nodes = other.Nodes;
		Color = other.Color;
		Origin = other.Origin;
		OffsetPerNode = other.OffsetPerNode;
		Squish = other.Squish;
		Forward = other.Forward;
		Grounded = other.Grounded;
		wave = other.wave;
		nodes.Clear();
		foreach (var node in other.nodes)
			nodes.Add(node);
		modified = true;

	}

	public void Update(in Matrix positionTransform)
	{
		modified = true;
		wave += Time.Delta * 4.0f;
		OffsetPerNode = Forward * ForwardOffsetPerNode + new Vec3(0, 0, -1 * (1 - Roundness));
		Origin = new Vec3(0, 1.0f, -.4f);

		while (nodes.Count > Nodes)
			nodes.RemoveAt(nodes.Count - 1);
		while (nodes.Count < Nodes)
			nodes.Add(new());

		// origin position
		var step = OffsetPerNode;

		// start hair offset
		nodes[0] = Vec3.Transform(Origin, positionTransform);

		// targets
		var target = nodes[0];
		var prev = nodes[0];
		var maxdist = 0.8f;
		var side = Vec3.TransformNormal(Forward, Matrix.CreateRotationZ(-MathF.PI / 2));
		var plane = Plane.Transform(new Plane(Forward, 0), Matrix.CreateTranslation(nodes[0] + Forward));

		for (int i = 1; i < nodes.Count; i++)
		{
			var p = (i / (float)nodes.Count);

			// wave target
			target += side * MathF.Sin(wave + i * 0.50f) * 0.50f * p;

			// approach target
			// TODO: use delta time
			nodes[i] += (target - nodes[i]) * 0.25f;
			//nodes[i] += (target - nodes[i]) * (1 - MathF.Pow(.0000001f, Time.Delta));

			// don't let the hair cross the forward boundary (intersects face)
			var dist = Utils.DistanceToPlane(nodes[i], plane);
			if (dist < 0)
				nodes[i] -= plane.Normal * dist;

			// don't go into the floor
			// TODO: use actual ground raycast distance :3
			if (Grounded)
			{
				var distanceToGround = Calc.Lerp(4, 6, p);
				if (nodes[i].Z < nodes[0].Z - distanceToGround)
					nodes[i] = nodes[i] with { Z = nodes[0].Z - distanceToGround };
			}

			// max dist from parent
			if ((nodes[i] - prev).Length() > maxdist)
				nodes[i] = prev + (nodes[i] - prev).Normalized() * maxdist;

			// set for next hair
			target = nodes[i] + step;
			prev = nodes[i];
		}
	}

	public override void Prepare()
	{
		if (!modified)
			return;
		modified = false;

		// update mesh on the CPU like true professionals
		// could use instanced meshes for this but I didn't add it to Foster yet

		hairVertices.Clear();
		hairIndices.Clear();

		var angle = Matrix.CreateRotationZ(Forward.XY().Angle());

		for (int i = 0; i < nodes.Count; i ++)
		{
			var lerp = i / (float)nodes.Count;
			var xzScale = Calc.Lerp(Calc.Lerp(3.2f, 3, Roundness), 1, lerp);
			var yScale = Calc.Lerp(Calc.Lerp(4, 3, Roundness), Calc.Lerp(2, 1, Roundness), lerp);

			var transform = 
				Matrix.CreateScale(new Vec3(xzScale, yScale, xzScale) * Squish) *
				angle *
				Matrix.CreateTranslation(nodes[i]);
			var index = hairVertices.Count;

			foreach (var vert in sphereVertices)
				hairVertices.Add(new(Vec3.Transform(vert.Pos, transform), Vec2.Zero, Vec3.One, vert.Normal));
			foreach (var ind in sphereIndices)
				hairIndices.Add(index + ind);
		}

		mesh.SetVertices<Vertex>(CollectionsMarshal.AsSpan(hairVertices));
		mesh.SetIndices<int>(CollectionsMarshal.AsSpan(hairIndices));
	}

	public override void Render(ref RenderState state)
	{
		// hack: don't use our model matrix
		var was = state.ModelMatrix;
		state.ModelMatrix = Matrix.Identity;
		state.ApplyToMaterial(Materials[0], Matrix.Identity);
		state.ModelMatrix = was;

		Materials[0].Color = Color;
		Materials[0].SilhouetteColor = Color;

		// draw hair
		var call = new DrawCommand(state.Camera.Target, mesh, Materials[0])
		{
			DepthCompare = state.DepthCompare,
			DepthMask = state.DepthMask,
			CullMode = CullMode.Front,
		};
		call.Submit();
		state.Calls++;
		state.Triangles += mesh.IndexCount / 3;
	}
}