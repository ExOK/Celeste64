using SharpGLTF.Transforms;

using BlendInput = (int TrackIdx, float Time, float Weight);

namespace Celeste64;

public class SkinnedModel : Model
{
	public readonly SkinnedTemplate Template;
	public readonly SharpGLTF.Runtime.SceneInstance Instance;
	public static readonly Matrix BaseTranslation = Matrix.CreateRotationX(MathF.PI / 2);

    public int AnimationIndex;
	public float Rate = 1.0f;

	private const int SkinMatrixCount = 32;
	private readonly Matrix[] transformSkin = new Matrix[SkinMatrixCount];

	private struct Playing
	{
		public int Index;
		public float Time;
		public float Duration;
		public float Blend;
		public float BlendDuration;
		public bool Loops;
	}

	private readonly List<BlendInput[]> blendedInput = [];
	private readonly Dictionary<(int, int), float> blendDurations = [];
	private readonly Dictionary<int, bool> loops = [];
	private readonly Dictionary<int, float> durations = [];
	private readonly Dictionary<string, int> indexof = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Playing> playing = [];

	public SkinnedModel(SkinnedTemplate template)
	{
		Template = template;
		Instance = Template.Template.CreateInstance();
		Flags = ModelFlags.Default;
		
		// by default just use the template's materials
		for (int i = 0; i < Template.Materials.Length; i ++)
			Materials.Add(Template.Materials[i]);

		foreach (var it in Template.Root.LogicalAnimations)
		{
			indexof[it.Name] = it.LogicalIndex;
			durations.Add(it.LogicalIndex, it.Duration);
		}
	}

	public void SetBlendDuration(string from, string to, float duration, bool twoWay = true)
	{
		var a = IndexOf(from);
		var b = IndexOf(to);

		if (a >= 0 && b >= 0)
		{
			blendDurations[(a, b)] = duration;
			if (twoWay)
                blendDurations[(b, a)] = duration;
        }
    }

    public void SetLooping(string name, bool looping)
    {
		var index = IndexOf(name);
		if (index >= 0)
			loops[index] = looping;
    }

	public int IndexOf(string name)
	{
		if (indexof.TryGetValue(name, out var index))
			return index;
		return -1;
    }

    public void Play(string name, bool restart = false)
    {
		var index = IndexOf(name);
		if (index >= 0)
			Play(index, restart);
    }

    public void Play(int index, bool restart = false)
	{
		var it = RequestPlayingStruct(index);

		// snap if we have nothing to blend from
		if (playing.Count <= 0)
			it.Blend = 1;

		// reset time if we're not blended at all
		if (restart || it.Blend <= 0)
			it.Time = 0;

		// set blend duration
		var last = (playing.Count > 0 ? playing[^1].Index : -1);
		it.BlendDuration = blendDurations.GetValueOrDefault((index, last), 0.01f);

		playing.Add(it);
	}

	public void Play(string nameA, string nameB, float blendAmount)
	{
		var indexA = IndexOf(nameA);
		var indexB = IndexOf(nameB);

		if (indexA < 0 || indexB < 0)
			return;

		var a = RequestPlayingStruct(indexA);
		var b = RequestPlayingStruct(indexB);

		a.BlendDuration = 0;
		b.BlendDuration = 0;
		a.Blend = 1.0f - blendAmount;
		b.Blend = blendAmount;

		playing.Clear();
		playing.Add(a);
		playing.Add(b);
	}

	public void Clear()
	{
		playing.Clear();
	}

	/// <summary>
	/// Gets the current playing struct and removes it from the list.
	/// Must be added back after being requested
	/// </summary>
	private Playing RequestPlayingStruct(int index)
	{
		Playing it = new() { Index = index, Time = 0, Blend = 0 };

		for (int i = 0; i < playing.Count; i ++)
		{
			if (playing[i].Index == index)
			{
				it = playing[i];
				playing.RemoveAt(i);
				break;
			}
		}

		it.Duration = durations[index];
		it.Loops = loops.GetValueOrDefault(index, true);
		return it;
	}

	public void Update()
	{
		// get blend duration
		var blendDuration = 0.0f;
		if (playing.Count > 0)
			blendDuration = playing[^1].BlendDuration;

		// blend between them
		for (int i = 0; i < playing.Count; i++)
		{
			var it = playing[i];

			it.Time += Rate * Time.Delta;
			if (it.Time >= it.Duration)
			{
				if (it.Loops)
					it.Time -= it.Duration;
				else
					it.Time = it.Duration;
			}

			if (blendDuration > 0)
				it.Blend = Calc.Approach(it.Blend, (i == playing.Count - 1 ? 1 : 0), Time.Delta / blendDuration);
			
			playing[i] = it;
		}

		// set blended weights
		SetBlendedWeights(GetBlendedWeights());
    }

	public StackList8<BlendInput> GetBlendedWeights()
	{
		StackList8<BlendInput> result = new();

		for (int i = playing.Count - 1; i >= 0; i--)
		{
			if (playing[i].Blend > 0)
			{
				result.Add((
					playing[i].Index,
					playing[i].Time % playing[i].Duration,
					 1.0f - playing[i].Blend // why inverse?
				));

				if (result.Count >= result.Capacity)
					break;
			}
		}

		return result;
    }

	public void SetBlendedWeights(in StackList8<BlendInput> values)
	{
        // this ugly thing is here because 'SetAnimationFrame' can't take a Span
		// and I don't want to allocate a new array every frame
        while (blendedInput.Count <= values.Count)
			blendedInput.Add(new BlendInput[blendedInput.Count]);

		var input = blendedInput[values.Count];
		for (int i = 0; i < values.Count; i++)
			input[i] = values[i];

        Instance.Armature.SetAnimationFrame(input);
    }

    public override void Render(ref RenderState state)
	{
		for (int i = 0; i < Instance.Count; i ++)
		{
			var drawable = Instance[i];
			var meshPart = Template.Parts[drawable.Template.LogicalMeshIndex];

			if (drawable.Transform is RigidTransform statXform)
			{
				foreach (var primitive in meshPart)
				{
					var mat = Materials[primitive.Material];

					state.ApplyToMaterial(mat, statXform.WorldMatrix * BaseTranslation);
					
					if (mat.Shader != null && 
						mat.Shader.Has("u_jointMult"))
						mat.Set("u_jointMult", 0.0f);

                    DrawCommand cmd = new(state.Camera.Target, Template.Mesh, mat)
                    {
                        MeshIndexStart = primitive.Index,
                        MeshIndexCount = primitive.Count,
						DepthMask = state.DepthMask,
                        DepthCompare = state.DepthCompare,
                        CullMode = CullMode.Back
                    };
                    cmd.Submit();
					state.Calls++;
					state.Triangles += primitive.Count / 3;
				}
			}

			if (drawable.Transform is SkinnedTransform skinXform)
			{
				foreach (var primitive in meshPart)
				{
					var mat = Materials[primitive.Material];

					state.ApplyToMaterial(mat, BaseTranslation);
					
					if (mat.Shader != null && 
						mat.Shader.Has("u_jointMat"))
					{
						for (int j = 0, n = Math.Min(SkinMatrixCount, skinXform.SkinMatrices.Count); j < n; j ++)
							transformSkin[j] = skinXform.SkinMatrices[j];
						mat.Set("u_jointMult", 1.0f);
						mat.Set("u_jointMat", transformSkin.AsSpan());
					}

                    DrawCommand cmd = new(state.Camera.Target, Template.Mesh, mat)
                    {
                        MeshIndexStart = primitive.Index,
                        MeshIndexCount = primitive.Count,
						DepthMask = state.DepthMask,
                        DepthCompare = state.DepthCompare,
                        CullMode = CullMode.Back
                    };
                    cmd.Submit();
					state.Calls++;
					state.Triangles += primitive.Count / 3;
				}
			}
		}
	}
}