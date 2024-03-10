using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MonoMod.Cil;
using MonoMod.Core;
using MonoMod.RuntimeDetour;
using Sledge;

namespace Celeste64.Mod.Patches;

/// <summary>
/// Hook into sledge to change how map data is loaded.
/// </summary>
internal static class SledgeHooks
{
	private static Hook? solidComputeVerticesHook;
	
	public static void Load()
	{
		solidComputeVerticesHook = new Hook(typeof(Sledge.Formats.Map.Objects.Solid).GetMethod(nameof(Sledge.Formats.Map.Objects.Solid.ComputeVertices), BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException(), On_Solid_ComputeVertices);
	}

	public static void Unload()
	{
		solidComputeVerticesHook?.Dispose();
	}

	public delegate void orig_Solid_ComputeVertices(Sledge.Formats.Map.Objects.Solid self);
	public static void On_Solid_ComputeVertices(orig_Solid_ComputeVertices orig, Sledge.Formats.Map.Objects.Solid self)
	{
		const float planeMatchEpsilon = 0.0075f; // Magic number that seems to match VHE

		if (self.Faces.Count < 4) return;

		var poly = new Sledge.Formats.Geometric.Precision.Polyhedrond(self.Faces.Select(x => x.Plane));

		foreach (var face in self.Faces)
		{
			face.Vertices.Clear();
			//In Sledge's original implementation, they only compared the planes normal. This caused issues if two planes had similar normal.
			//We are changing it to compare the distance as well for more accuracy.
			var pg = poly.Polygons.FirstOrDefault(x => x.Plane.Normal.EquivalentTo(face.Plane.Normal, planeMatchEpsilon) 
				&& Math.Abs(x.Plane.D - face.Plane.D) < 0.001f);
			if (pg != null)
			{
				face.Vertices.AddRange(pg.Vertices.Select(x => x.ToVector3()));
			}
		}
	}
}