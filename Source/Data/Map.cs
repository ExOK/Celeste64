using System.Runtime.InteropServices;
using Sledge.Formats.Map.Formats;
using SledgeMapObject = Sledge.Formats.Map.Objects.MapObject;
using SledgeSolid = Sledge.Formats.Map.Objects.Solid;
using SledgeEntity = Sledge.Formats.Map.Objects.Entity;
using SledgeFace = Sledge.Formats.Map.Objects.Face;
using SledgeMap = Sledge.Formats.Map.Objects.MapFile;
using System.Runtime.CompilerServices;

namespace Celeste64;

public class Map
{
	public class ActorFactory(Func<Map, SledgeEntity, Actor?> create)
	{
		public bool UseSolidsAsBounds;
		public bool IsSolidGeometry;
		public Func<Map, SledgeEntity, Actor?> Create = create;
	}

	private const string StartCheckpoint = "Start";

	public readonly string Name;
	public readonly string Filename;
	public readonly string Folder;
	public readonly SledgeMap Data;
	public readonly string Skybox;
	public readonly float SnowAmount;
	public readonly Vec3 SnowWind;
	public readonly string Music;
	public readonly string Ambience;

	public static readonly Dictionary<string, ActorFactory> ActorFactories =  new()
	{
		["Strawberry"] = new((map, entity) =>
		{
			var id = $"{map.LoadWorld!.Entry.Map}/{map.LoadStrawberryCounter}";
			var lockedCondition = entity.GetStringProperty("targetname", string.Empty);
			var isLocked = entity.GetIntProperty("locked", 0) > 0;
			var playUnlockSound = entity.GetIntProperty("noUnlockSound", 0) == 0;
			Vec3? bubbleTo = null;
			if (map.FindTargetNode(entity.GetStringProperty("bubbleto", string.Empty), out var point))
				bubbleTo = point;
			map.LoadStrawberryCounter++;
			return new Strawberry(id, isLocked, lockedCondition, playUnlockSound, bubbleTo);
		}),
		["Refill"] = new((map, entity) => new Refill(entity.GetIntProperty("double", 0) > 0)),
		["Cassette"] = new((map, entity) => new Cassette(entity.GetStringProperty("map", string.Empty))),
		["Coin"] = new((map, entity) => new Coin()),
		["Feather"] = new((map, entity) => new Feather()),
		["MovingBlock"] = new((map, entity) =>
		{
			return new MovingBlock(
				entity.GetIntProperty("slow", 0) > 0,
				map.FindTargetNodeFromParam(entity, "target"));
		}) { IsSolidGeometry = true },
		["GateBlock"] = new((map, entity) => new GateBlock(map.FindTargetNodeFromParam(entity, "target"))) { IsSolidGeometry = true },
		["TrafficBlock"] = new((map, entity) => new TrafficBlock(map.FindTargetNodeFromParam(entity, "target"))) { IsSolidGeometry = true },
		["FallingBlock"] = new((map, entity) =>
		{
			return new FallingBlock() { Secret = (entity.GetIntProperty("secret", 0) != 0) };
		}) { IsSolidGeometry = true },
		["FloatyBlock"] = new((map, entity) => new FloatyBlock()) { IsSolidGeometry = true },
		["DeathBlock"] = new((map, entity) => new DeathBlock()) { UseSolidsAsBounds = true },
		["SpikeBlock"] = new((map, entity) => new SpikeBlock()) { UseSolidsAsBounds = true },
		["Spring"] = new((map, entity) => new Spring()),
		["Granny"] = new((map, entity) => new Granny()),
		["Badeline"] = new((map, entity) => new Badeline()),
		["Theo"] = new((map, entity) => new Theo()),
		["SignPost"] = new((map, entity) => new Signpost(entity.GetStringProperty("dialog", string.Empty))),
		["StaticProp"] = new((map, entity) =>
		{
			var prop = Path.GetFileNameWithoutExtension(entity.GetStringProperty("model", string.Empty));
			if (Assets.Models.TryGetValue(prop, out var model))
			{
				return new StaticProp(model,
					entity.GetIntProperty("radius", 6),
					entity.GetIntProperty("height", 10)
				);
			}
			return null;
		}),
		["BreakBlock"] = new((map, entity) =>
		{
			return new BreakBlock(
				entity.GetIntProperty("bounces", 0) != 0,
				entity.GetIntProperty("transparent", 0) != 0,
				entity.GetIntProperty("secret", 0) != 0);
		}) { IsSolidGeometry = true },
		["CassetteBlock"] = new((map, entity) => new CassetteBlock(entity.GetIntProperty("startOn", 1) != 0)) { IsSolidGeometry = true },
		["DoubleDashPuzzleBlock"] = new((map, entity) => new DoubleDashPuzzleBlock()) { IsSolidGeometry = true },
		["EndingArea"] = new((map, entity) => new EndingArea()) { UseSolidsAsBounds = true },
		["Fog"] = new((map, entity) => new FogRing(entity)),
		["FixedCamera"] = new((map, entity) => new FixedCamera(map.FindTargetNodeFromParam(entity, "target"))) { UseSolidsAsBounds = true },
		["IntroCar"] = new((map, entity) => new IntroCar(entity.GetFloatProperty("scale", 6)))
	};

	private readonly Dictionary<string, DefaultMaterial> currentMaterials = [];
	private readonly Dictionary<int, string> groupNames = [];
	private readonly List<(BoundingBox Bounds, SledgeSolid Solid)> staticSolids = [];
	private readonly List<SledgeEntity> staticDecorations = [];
	private readonly List<SledgeEntity> floatingDecorations = [];
	private readonly List<SledgeEntity> entities = [];
	private readonly HashSet<string> checkpoints = [];
	private readonly BoundingBox localStaticSolidsBounds;
	private readonly Matrix baseTransform = Matrix.CreateScale(0.2f);

	// kind of a hack, but assigned during load, unset after
	public World? LoadWorld;
	public int LoadStrawberryCounter = 0;

	public Map(string name, string filename)
	{
		Name = name;
		Filename = filename;
		Folder = Path.GetDirectoryName(filename) ?? string.Empty;

		var format = new QuakeMapFormat();
		Data = format.ReadFromFile(filename);

		Skybox = Data.Worldspawn.GetStringProperty("skybox", "city");
		SnowAmount = Data.Worldspawn.GetFloatProperty("snowAmount", 1);
		SnowWind = Data.Worldspawn.GetVectorProperty("snowDirection", -Vec3.UnitZ);
		Music = Data.Worldspawn.GetStringProperty("music", string.Empty);
		Ambience = Data.Worldspawn.GetStringProperty("ambience", string.Empty);

		void QueryObjects(SledgeMapObject obj)
		{
			foreach (var child in obj.Children)
			{
				if (child is SledgeEntity entity)
				{
					if (entity.ClassName == "func_group")
					{
						groupNames[entity.GetIntProperty("_tb_id", -1)] = entity.GetStringProperty("_tb_name", "");
						QueryObjects(child);
					}
					else if (entity.ClassName == "Decoration")
					{
						staticDecorations.Add(entity);
					}
					else if (entity.ClassName == "FloatingDecoration")
					{
						floatingDecorations.Add(entity);
					}
					else if (entity.ClassName == "PlayerSpawn")
					{
						checkpoints.Add(entity.GetStringProperty("name", StartCheckpoint));
						entities.Add(entity);
					}
					else
					{
						entities.Add(entity);
					}
				}
				else if (child is SledgeSolid solid)
				{
					staticSolids.Add((CalculateSolidBounds(solid), solid));
					QueryObjects(child);
				}
				else
				{
					QueryObjects(child);
				}
			}
		}

		QueryObjects(Data.Worldspawn);

		// figure out entire bounds of static solids (localized)
		if (staticSolids.Count > 0)
		{
			localStaticSolidsBounds = staticSolids[0].Bounds;
			for (int i = 1; i < staticSolids.Count; i++)
				localStaticSolidsBounds = localStaticSolidsBounds.Conflate(staticSolids[i].Bounds);
		}

		// shuffle floating decorations
		{
			var rng = new Rng();
			var n = floatingDecorations.Count;
			while (n > 1) 
			{
				int k = rng.Int(n--);
                (floatingDecorations[k], floatingDecorations[n]) = 
				(floatingDecorations[n], floatingDecorations[k]);
            }
        }

		// TODO:
		// A LOT more data could be cached here instead of done every time the map is Loaded into a World
		// ....
	}

	public void Load(World world)
	{
		LoadWorld = world;
		LoadStrawberryCounter = 0;

		// create materials for each texture type so they can be shared by each surface
		currentMaterials.Clear();
		foreach (var it in Assets.Textures)
			currentMaterials.Add(it.Key, new DefaultMaterial(it.Value));

		// load all static solids
		// group them in big chunks (this helps collision tests so we can cull entire objects based on their bounding box)
		if (staticSolids.Count > 0)
		{
			var combined = new List<SledgeSolid>();
			var available = new List<(BoundingBox Bounds, SledgeSolid Solid)>(); available.AddRange(staticSolids);
			var bounds = localStaticSolidsBounds;

			// split into a grid so we don't have one massive solid
			var chunk = new Vec3(1000, 1000, 1000);
			for (int x = 0; x < bounds.Size.X / chunk.X; x ++)
			for (int y = 0; y < bounds.Size.Y / chunk.Y; y ++)
			for (int z = 0; z < bounds.Size.Z / chunk.Z; z ++)
			{
				var box = new BoundingBox(bounds.Min, bounds.Min + chunk * new Vec3(1 + x, 1 + y, 1 + z));

				for (int i = available.Count - 1; i >= 0; i --)
					if (box.Contains(available[i].Bounds.Center))
					{
						combined.Add(available[i].Solid);
						available.RemoveAt(i);
					}

				var result = new Solid();
				GenerateSolid(result, combined);
				world.Add(result);
				combined.Clear();
			}
		}

		// load all decorations into one big model *shrug*
		{
			var decorations = new List<SledgeSolid>();
			var decoration = new Decoration();
			foreach (var entity in staticDecorations)
				CollectSolids(entity, decorations);
			decoration.LocalBounds = CalculateSolidBounds(decorations, baseTransform);
			GenerateModel(decoration.Model, decorations, baseTransform);
			world.Add(decoration);
		}

		// load floating decorations into 4-ish groups randomly
		if (floatingDecorations.Count > 0)
		{
			int from = 0;
			while (from < floatingDecorations.Count)
			{
				var decorations = new List<SledgeSolid>();
				var decoration = new FloatingDecoration();

				var to = Math.Min(from + floatingDecorations.Count / 4, floatingDecorations.Count);
				for (int j = from; j < to; j ++)
					CollectSolids(floatingDecorations[j], decorations);
				from = to;

				decoration.LocalBounds = CalculateSolidBounds(decorations, baseTransform);
				GenerateModel(decoration.Model, decorations, baseTransform);
				world.Add(decoration);
			}
		}

		// load actors
		foreach (var entity in entities)
			LoadActor(world, entity);

		Log.Info($"Strawb Count: {LoadStrawberryCounter}");
		LoadStrawberryCounter = 0;
		LoadWorld = null;
	}

	private void LoadActor(World world, SledgeEntity entity)
	{
		void HandleActorCreation(World world, SledgeEntity entity, Actor it, ActorFactory? factory)
		{
			if ((factory?.IsSolidGeometry ?? false) && it is Solid solid)
			{
				List<SledgeSolid> collection = [];
				CollectSolids(entity, collection);
				GenerateSolid(solid, collection);
			}

			if (entity.Properties.ContainsKey("origin"))
				it.Position = Vec3.Transform(entity.GetVectorProperty("origin", Vec3.Zero), baseTransform);

			if (entity.Properties.ContainsKey("_tb_group") && 
				groupNames.TryGetValue(entity.GetIntProperty("_tb_group", -1), out var groupName))
				it.GroupName = groupName;

			if (entity.Properties.ContainsKey("angle"))
				it.Facing = Calc.AngleToVector(entity.GetIntProperty("angle", 0) * Calc.DegToRad - MathF.PI / 2);

			if (factory?.UseSolidsAsBounds ?? false)
			{
				BoundingBox bounds = new();
				if (entity.Children.FirstOrDefault() is SledgeSolid sol)
					bounds = CalculateSolidBounds(sol, baseTransform);

				it.Position = bounds.Center;
				bounds.Min -= it.Position;
				bounds.Max -= it.Position;
				it.LocalBounds = bounds;
			}
			
			world.Add(it);
		}

		if (entity.ClassName == "PlayerSpawn")
		{
			var name = entity.GetStringProperty("name", StartCheckpoint);

			// spawns ther player if the world entry is this checkpoint
			// OR the world entry has no checkpoint and we're the start
			// OR the world entry checkpoint is misconfigured and we're the start
			var spawnsPlayer = 
				(world.Entry.CheckPoint == name) ||
				(string.IsNullOrEmpty(world.Entry.CheckPoint) && name == StartCheckpoint) ||
				(!checkpoints.Contains(world.Entry.CheckPoint) && name == StartCheckpoint);

			if (spawnsPlayer)
				HandleActorCreation(world, entity, new Player(), null);

			if (name != StartCheckpoint)
				HandleActorCreation(world, entity, new Checkpoint(name), null);

		}
		else if (ActorFactories.TryGetValue(entity.ClassName, out var factory))
		{
			var it = factory.Create(this, entity);
			if (it != null)
				HandleActorCreation(world, entity, it, factory);
		}
	}

	private SledgeEntity? FindTargetEntity(SledgeMapObject obj, string targetName)
	{
		if (string.IsNullOrEmpty(targetName))
			return null;

		if (obj is SledgeEntity en && en.GetStringProperty("targetname", string.Empty) == targetName)
			return en;

		foreach (var child in obj.Children)
		{
			if (FindTargetEntity(child, targetName) is {} it)
				return it;
		}

		return null;
	}

	public bool FindTargetNode(string name, out Vec3 pos)
	{
		if (FindTargetEntity(Data.Worldspawn, name) is { } target)
		{
			pos = Vec3.Transform(target.GetVectorProperty("origin", Vec3.Zero), baseTransform);
			return true;
		}
		pos = Vec3.Zero;
		return false;
	}

	public Vec3 FindTargetNodeFromParam(SledgeEntity en, string name)
	{
		if (FindTargetNode(en.GetStringProperty(name, string.Empty), out var pos))
			return pos;
		return Vec3.Zero;
	}

	private void CollectSolids(SledgeMapObject obj, List<SledgeSolid> into)
	{
		foreach (var child in obj.Children)
		{
			if (child is SledgeSolid solid)
				into.Add(solid);
			CollectSolids(child, into);
		}
	}

	private BoundingBox CalculateSolidBounds(SledgeSolid sol, in Matrix? transform = null)
	{
		Vec3 min = default, max = default;

		if (sol.Faces.Count > 0 && sol.Faces[0].Vertices.Count > 0)
			min = max = sol.Faces[0].Vertices[0];

		foreach (var face in sol.Faces)
			foreach (var vert in face.Vertices)
			{
				min = Vec3.Min(min, vert);
				max = Vec3.Max(max, vert);
			}

		if (transform.HasValue)
			return BoundingBox.Transform(new(min, max), transform.Value);
		else
			return new BoundingBox(min, max);
	}

	private BoundingBox CalculateSolidBounds(List<SledgeSolid> collection, in Matrix transform)
	{
		BoundingBox box = new();
		
		if (collection.Count > 0)
			box = CalculateSolidBounds(collection[0]);

		for (int i = 1; i < collection.Count; i++)
			box = box.Conflate(CalculateSolidBounds(collection[i]));

		return BoundingBox.Transform(box, transform);
	}

	private void GenerateModel(SimpleModel model, List<SledgeSolid> collection, in Matrix transform)
	{
		var used = Pool.Get<HashSet<string>>(); used.Clear();

		// find all used materials
		foreach (var solid in collection)
			foreach (var face in solid.Faces)
			{
				if (face.TextureName.StartsWith("__") || face.TextureName == "TB_empty" || face.TextureName == "invisible")
					continue;

				if (!used.Contains(face.TextureName))
				{
					used.Add(face.TextureName);
					model.Materials.Add(currentMaterials[face.TextureName]);
				}
			}

		if (used.Count <= 0)
		{
			Pool.Return(used);
			return;
		}

		var meshVertices = Pool.Get<List<Vertex>>();
		var meshIndices = Pool.Get<List<int>>();

		// merge all faces that share materials together
		for (int n = 0; n < model.Materials.Count; n++)
		{
			var mat = model.Materials[n];
			var start = meshIndices.Count;
			var texture = mat.Texture!;

			// add all faces with this material
			foreach (var solid in collection)
				foreach (var face in solid.Faces)
				{
					if (face.TextureName.StartsWith("__") || face.TextureName == "TB_empty" || face.TextureName == "invisible")
						continue;
					if (face.TextureName != texture.Name)
						continue;

					var vertexIndex = meshVertices.Count;
					var plane = Plane.Normalize(Plane.Transform(face.Plane, transform));
					CalculateRotatedUV(face, out var rotatedUAxis, out var rotatedVAxis);

					// add face vertices
					for (int i = 0; i < face.Vertices.Count; i++)
					{
						var pos = Vec3.Transform(face.Vertices[i], transform);
						var uv = CalculateUV(face, face.Vertices[i], texture.Size, rotatedUAxis, rotatedVAxis);
						meshVertices.Add(new Vertex(pos, uv, Color.White, plane.Normal));
					}

					// add mesh indices
					for (int i = 0; i < face.Vertices.Count - 2; i++)
					{
						meshIndices.Add(vertexIndex + 0);
						meshIndices.Add(vertexIndex + i + 1);
						meshIndices.Add(vertexIndex + i + 2);
					}
				}

			// add this part of the model
			model.Parts.Add(new()
			{
				MaterialIndex = n,
				IndexStart = start,
				IndexCount = meshIndices.Count - start
			});
		}

		model.Mesh.SetVertices<Vertex>(CollectionsMarshal.AsSpan(meshVertices));
		model.Mesh.SetIndices<int>(CollectionsMarshal.AsSpan(meshIndices));

		Pool.Return(meshVertices);
		Pool.Return(meshIndices);
		Pool.Return(used);
	}

	private void GenerateSolid(Solid into, List<SledgeSolid> collection)
	{
		if (collection.Count <= 0)
			return;

		// get bounds
		var transform = baseTransform;
		var bounds = CalculateSolidBounds(collection, transform);
		var center = bounds.Center;
		transform *= Matrix.CreateTranslation(-center);

		// create the model
		GenerateModel(into.Model, collection, transform);

		// get lists to build everything
		var colliderVertices = Pool.Get<List<Vec3>>();
		var colliderFaces = Pool.Get<List<Solid.Face>>();

		// find all used materials
		foreach (var solid in collection)
		foreach (var face in solid.Faces)
		{
			if (face.TextureName.StartsWith("__") || face.TextureName == "TB_empty")
				continue;

			// add collider vertices
			var vertexIndex = colliderVertices.Count;
			var last = Vec3.Zero;
			for (int i = 0; i < face.Vertices.Count; i++)
			{
				// skip collider vertices that are too close together ...
				var it = Vec3.Transform(face.Vertices[i], transform);
				if (i == 0 || (last - it).LengthSquared() > 1)
					colliderVertices.Add(last = it);
			}

			// add collider face
			if (colliderVertices.Count > vertexIndex)
			{
				colliderFaces.Add(new ()
				{
					Plane = Plane.Normalize(Plane.Transform(face.Plane, transform)),
					VertexStart = vertexIndex,
					VertexCount = colliderVertices.Count - vertexIndex
				});
			}
		}

		// set up values
		if (colliderVertices.Count > 0)
		{
			into.LocalBounds = new BoundingBox(
				colliderVertices.Aggregate(Vec3.Min),
				colliderVertices.Aggregate(Vec3.Max)
			);
			into.LocalVertices = [.. colliderVertices];
			into.LocalFaces = [.. colliderFaces];
			into.Position = center;
		}

		Pool.Return(colliderVertices);
		Pool.Return(colliderFaces);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void CalculateRotatedUV(in SledgeFace face, out Vec3 rotatedUAxis, out Vec3 rotatedVAxis)
	{
		// Determine the dominant axis of the normal vector
		static Vec3 GetRotationAxis(Vec3 normal)
		{
			var abs = Vec3.Abs(normal);
			if (abs.X > abs.Y && abs.X > abs.Z)
				return Vec3.UnitX;
			else if (abs.Y > abs.Z)
				return Vec3.UnitY;
			else
				return Vec3.UnitZ;
		}

        // Apply scaling to the axes
        var scaledUAxis = face.UAxis / face.XScale;
        var scaledVAxis = face.VAxis / face.YScale;

        // Determine the rotation axis based on the face normal
        var rotationAxis = GetRotationAxis(face.Plane.Normal);
        var rotationMatrix = Matrix.CreateFromAxisAngle(rotationAxis, face.Rotation * Calc.DegToRad);
        rotatedUAxis = Vec3.Transform(scaledUAxis, rotationMatrix);
        rotatedVAxis = Vec3.Transform(scaledVAxis, rotationMatrix);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vec2 CalculateUV(in SledgeFace face, in Vec3 vertex, in Vec2 textureSize, in Vec3 rotatedUAxis, in Vec3 rotatedVAxis)
    {
        Vec2 uv;
        uv.X = vertex.X * rotatedUAxis.X + vertex.Y * rotatedUAxis.Y + vertex.Z * rotatedUAxis.Z;
        uv.Y = vertex.X * rotatedVAxis.X + vertex.Y * rotatedVAxis.Y + vertex.Z * rotatedVAxis.Z;
        uv.X += face.XShift;
        uv.Y += face.YShift;
        uv.X /= textureSize.X;
        uv.Y /= textureSize.Y;
        return uv;
    }
}
