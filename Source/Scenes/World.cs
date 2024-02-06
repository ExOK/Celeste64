using System.Diagnostics;
using ModelEntry = (Celeste64.Actor Actor, Celeste64.Model Model);

namespace Celeste64;

public class World : Scene
{
	public enum EntryReasons { Entered, Returned, Respawned }
	public readonly record struct EntryInfo(string Map, string CheckPoint, bool Submap, EntryReasons Reason);

	public Camera Camera = new();
	public Rng Rng = new(0);
	public float HitStun = 0;
	public bool Paused = false;
	public EntryInfo Entry = new();
	public readonly GridPartition<Solid> SolidGrid = new(200, 100);
	public float GeneralTimer = 0;
	public float DeathPlane = -100;

	public readonly List<Actor> Actors = [];
	private readonly List<Actor> adding = [];
	private readonly List<Actor> destroying = [];
	private readonly Dictionary<Type, List<Actor>> tracked = [];
	private readonly Dictionary<Type, Queue<Actor>> recycled = [];
	private readonly List<Type> trackedTypes = [];

	private readonly List<ModelEntry> models = [];
	private readonly List<Sprite> sprites = [];

	private Target? postTarget;
	private readonly Material postMaterial = new();
	private readonly Batcher batch = new();
	private readonly List<Skybox> skyboxes = [];
	private readonly SpriteRenderer spriteRenderer = new();

	// Pause Menu, only drawn when actually paused
	private readonly Menu pauseMenu = new();
	private AudioHandle pauseSnapshot;
	
	// makes the Strawberry UI wiggle when one is collected
	private float strawbCounterWiggle = 0;
	private float strawbCounterCooldown = 0;
	private float strawbCounterEase = 0;
	private int strawbCounterWas;

	private bool IsInEndingArea => Get<Player>() is {} player && Overlaps<EndingArea>(player.Position);
	private bool IsPauseEnabled
	{
		get
		{
			if (Game.Instance.IsMidTransition) return false;
			if (Get<Player>() is not Player player) return true;
			return player.IsAbleToPause;
		}
	}

	private readonly Stopwatch debugUpdTimer = new();
	private readonly Stopwatch debugRndTimer = new();
	private readonly Stopwatch debugFpsTimer = new();
	private TimeSpan lastDebugRndTime;
	private int debugUpdateCount;
	public static bool DebugDraw { get; private set; } = false;

	public World(EntryInfo entry)
	{
		Entry = entry;

		var stopwatch = Stopwatch.StartNew();
		var map = Assets.Maps[entry.Map];

		Camera.NearPlane = 20;
		Camera.FarPlane = 800;
		Camera.FOVMultiplier = 1;

		strawbCounterWas = Save.CurrentRecord.Strawberries.Count;
		strawbCounterWiggle = 0;

		// setup pause menu
		{
			Menu optionsMenu = new Menu();
			optionsMenu.Title = Loc.Str("OptionsTitle");
			optionsMenu.Add(new Menu.Toggle(Loc.Str("OptionsFullscreen"), Save.Instance.ToggleFullscreen, () => Save.Instance.Fullscreen));
			optionsMenu.Add(new Menu.Toggle(Loc.Str("OptionsZGuide"), Save.Instance.ToggleZGuide, () => Save.Instance.ZGuide));
			optionsMenu.Add(new Menu.Toggle(Loc.Str("OptionsTimer"), Save.Instance.ToggleTimer, () => Save.Instance.SpeedrunTimer));
			optionsMenu.Add(new Menu.MultiSelect<Save.InvertCameraOptions>(Loc.Str("OptionsInvertCamera"), Save.Instance.SetCameraInverted, () => Save.Instance.InvertCamera));
			optionsMenu.Add(new Menu.Spacer());
			optionsMenu.Add(new Menu.Slider(Loc.Str("OptionsBGM"), 0, 10, () => Save.Instance.MusicVolume, Save.Instance.SetMusicVolume));
			optionsMenu.Add(new Menu.Slider(Loc.Str("OptionsSFX"), 0, 10, () => Save.Instance.SfxVolume, Save.Instance.SetSfxVolume));

			pauseMenu.Title = Loc.Str("PauseTitle");
            pauseMenu.Add(new Menu.Option(Loc.Str("PauseResume"), () => SetPaused(false)));
			pauseMenu.Add(new Menu.Option(Loc.Str("PauseRetry"), () =>
			{
				SetPaused(false);
				Audio.StopBus(Sfx.bus_dialog, false);
				Get<Player>()?.Kill();
			}));
			pauseMenu.Add(new Menu.Submenu(Loc.Str("PauseOptions"), pauseMenu, optionsMenu));
			pauseMenu.Add(new Menu.Option(Loc.Str("PauseSaveQuit"), () => Game.Instance.Goto(new Transition()
			{
				Mode = Transition.Modes.Replace,
				Scene = () => new Overworld(true),
				FromPause = true,
				ToPause = true,
				ToBlack = new SlideWipe(),
				Saving = true
			})));
		}

		// environment
		{
			if (map.SnowAmount > 0)
				Add(new Snow(map.SnowAmount, map.SnowWind));

			if (!string.IsNullOrEmpty(map.Skybox))
			{
				// single skybox
				if (Assets.Textures.TryGetValue($"skyboxes/{map.Skybox}", out var skybox))
				{
					skyboxes.Add(new(skybox));
				}
				// group
				else
				{
					while (Assets.Textures.TryGetValue($"skyboxes/{map.Skybox}_{skyboxes.Count}", out var nextSkybox))
						skyboxes.Add(new(nextSkybox));
				}
			}

			Music = $"event:/music/{map.Music}";
			Ambience = $"event:/sfx/ambience/{map.Ambience}";
		}

		// load content
		map.Load(this);

		Log.Info($"Loaded Map '{Entry.Map}' in {stopwatch.ElapsedMilliseconds}ms");
	}

	public override void Disposed()
	{
		SetPaused(false);

		while (Actors.Count > 0)
		{
			foreach (var it in Actors)
				Destroy(it);
			ResolveChanges();
		}

		postTarget?.Dispose();
		postTarget = null;
	}

	public T Request<T>() where T : Actor, IRecycle, new()
	{
		if (recycled.TryGetValue(typeof(T), out var list) && list.Count > 0)
		{
			return Add((list.Dequeue() as T)!);
		}
		else
		{
			return Add(new T());
		}
	}

	public T Add<T>(T instance) where T : Actor
	{
		adding.Add(instance);
		instance.Destroying = false;
		instance.SetWorld(this);
		instance.Created();
		return instance;
	}

	public T? Get<T>() where T : class
	{
		var list = GetTypesOf<T>();
		if (list.Count > 0)
			return (list[0] as T)!;
		return null;
	}

	public T? Get<T>(Func<T, bool> predicate) where T : class
	{
		var list = GetTypesOf<T>();
		foreach (var it in list)
			if (predicate((it as T)!))
				return (it as T)!;
		return null;
	}

	public List<Actor> All<T>()
	{
		return GetTypesOf<T>();
	}

	public void Destroy(Actor actor)
	{
		Debug.Assert(actor.World == this);
		actor.Destroying = true;
		destroying.Add(actor);
	}

	private List<Actor> GetTypesOf<T>()
	{
		var type = typeof(T);
		if (!tracked.TryGetValue(type, out var list))
		{
			tracked[type] = list = new();
			foreach (var actor in Actors)
				if (actor is T)
					list.Add(actor);
			trackedTypes.Add(type);
		}
		return list;
	}

	private void ResolveChanges()
	{
		// resolve adding/removing actors
		while (adding.Count > 0 || destroying.Count > 0)
		{
			// first add group to world
			int addCount = adding.Count;
			for (int i = 0; i < addCount; i ++)
			{
				// sort into buckets
				var type = adding[i].GetType();

				// add to important tracked types
				foreach (var other in trackedTypes)
					if (type.IsAssignableTo(other))
						tracked[other].Add(adding[i]);

				// add it to the world
				Actors.Add(adding[i]);
			}

			// notify they're being added
			for (int i = 0; i < addCount; i ++)
				adding[i].Added();
			adding.RemoveRange(0, addCount);

			for (int i = 0; i < destroying.Count; i ++)
			{
				var it = destroying[i];
				it.Destroyed();

				// remove from buckets
				var type = it.GetType();
				foreach (var other in trackedTypes)
					if (type.IsAssignableTo(other))
						tracked[other].Remove(it);

				// remove from the world
				Actors.Remove(it);
				it.SetWorld(null);

				// recycled type
				if (it is IRecycle)
				{
					if (!recycled.TryGetValue(type, out var list))
						recycled[type] = list = new();
					list.Enqueue(it);
				}
			}
			destroying.Clear();
		}
	}

	public override void Update()
	{
		debugUpdTimer.Restart();

		// update audio
		Audio.SetListener(Camera);

		// increment playtime (if not in the ending area)
		if (!IsInEndingArea)
		{
			Save.CurrentRecord.Time += TimeSpan.FromSeconds(Time.Delta);
			Game.Instance.Music.Set("at_baddy", 0);
		}
		else
		{
			Game.Instance.Music.Set("at_baddy", 1);
		}

		// handle strawb counter
		{
			// wiggle when gained
			if (strawbCounterWas != Save.CurrentRecord.Strawberries.Count)
			{
				strawbCounterCooldown = 4.0f;
				strawbCounterWiggle = 1.0f;
				strawbCounterWas = Save.CurrentRecord.Strawberries.Count;
			}
			else
				Calc.Approach(ref strawbCounterWiggle, 0, Time.Delta / .6f);

			// hold stawb for a while
			if ((Get<Player>()?.IsStrawberryCounterVisible ?? false))
				strawbCounterCooldown = 2.0f;
			else
				strawbCounterCooldown -= Time.Delta;

			// ease strawb in/out
			if (IsInEndingArea || Paused || strawbCounterCooldown > 0 || (Get<Player>()?.IsStrawberryCounterVisible ?? false))
				strawbCounterEase = Calc.Approach(strawbCounterEase, 1, Time.Delta * 6.0f);
			else
				strawbCounterEase = Calc.Approach(strawbCounterEase, 0, Time.Delta * 6.0f);
		}

		// toggle debug draw
		if (Input.Keyboard.Pressed(Keys.F1))
			DebugDraw = !DebugDraw;
		
		// normal game loop
		if (!Paused)
		{
			// start pause menu
			if (Controls.Pause.ConsumePress() && IsPauseEnabled)
			{
				SetPaused(true);
				return;
			}

			// ONLY update the player when dead
			if (Get<Player>() is Player player && player.Dead)
			{
				player.Update();
				player.LateUpdate();
				ResolveChanges();
				return;
			}

			// ONLY update single cutscene object
			if (Get<Cutscene>((it) => it.FreezeGame) is Cutscene cs)
			{
				cs.Update();
				cs.LateUpdate();
				ResolveChanges();
				return;
			}

			// pause from hitstun
			if (HitStun > 0)
			{
				HitStun -= Time.Delta;
				return;
			}

			GeneralTimer += Time.Delta;

			// add / remove actors
			ResolveChanges();

			// update all actors
			var view = Camera.Frustum.GetBoundingBox().Inflate(10);
			debugUpdateCount = 0;
			foreach (var actor in Actors)
				if (actor.UpdateOffScreen || actor.WorldBounds.Intersects(view))
				{
					debugUpdateCount++;
					actor.Update();
				}
			foreach (var actor in Actors)
				if (actor.UpdateOffScreen || actor.WorldBounds.Intersects(view))
					actor.LateUpdate();
		}
		// unpause
		else
		{
			if (Controls.Pause.ConsumePress() || (pauseMenu.IsInMainMenu && Controls.Cancel.ConsumePress()))
			{
				pauseMenu.CloseSubMenus();
				SetPaused(false);
				Audio.Play(Sfx.ui_unpause);
			}
			else
			{
				pauseMenu.Update();
			}
		}

		debugUpdTimer.Stop();
	}

	public void SetPaused(bool paused)
	{
		if (paused != Paused)
		{
			Audio.SetBusPaused(Sfx.bus_gameplay, paused);
			Audio.SetBusPaused(Sfx.bus_bside_music, paused);

			if (paused)
			{
				Audio.Play(Sfx.ui_pause);
				pauseSnapshot = Audio.Play(Sfx.snapshot_pause);
			}
			else {
				pauseMenu.Index = 0;
				pauseSnapshot.Stop();
			}

			Controls.Consume();
			Paused = paused;
		}
	}

	public bool SolidRayCast(in Vec3 point, in Vec3 direction, float distance, out RayHit hit, bool ignoreBackfaces = true, bool ignoreTransparent = false)
	{
		hit = default;
		float? closest = null;

		var p0 = point;
		var p1 = point + direction * distance;
		var box = new BoundingBox(Vec3.Min(p0, p1), Vec3.Max(p0, p1)).Inflate(1);

		var solids = Pool.Get<List<Solid>>();
		SolidGrid.Query(solids, new Rect(box.Min.XY(), box.Max.XY()));

		foreach (var solid in solids)
		{
			if (!solid.Collidable || solid.Destroying)
				continue;

			if (solid.Transparent && ignoreTransparent)
				continue;

			if (!solid.WorldBounds.Intersects(box))
				continue;
				
			var verts = solid.WorldVertices;
			var faces = solid.WorldFaces;

			foreach (var face in faces)
			{
				// only do planes that are facing against us
				if (ignoreBackfaces && Vec3.Dot(face.Plane.Normal, direction) >= 0)
					continue;

				// ignore faces that are definitely too far away
				if (Utils.DistanceToPlane(point, face.Plane) > distance)
					continue;

				// check against each triangle in the face
				for (int i = 0; i < face.VertexCount - 2; i ++)
				{
					if (Utils.RayIntersectsTriangle(point, direction, 
						verts[face.VertexStart + 0],
						verts[face.VertexStart + i + 1],
						verts[face.VertexStart + i + 2], out float dist))
					{
						// too far away
						if (dist > distance)
							continue;

						hit.Intersections++;

						// we have a closer value
						if (closest.HasValue && dist > closest.Value)
							continue;

						// store as closest
						hit.Point = point + direction * dist;
						hit.Normal = face.Plane.Normal;
						hit.Distance = dist;
						hit.Actor = solid;
						closest = dist;
						break;
					}
				}
			}
		}

		Pool.Return(solids);
		return closest.HasValue;
	}

	public StackList8<WallHit> SolidWallCheck(in Vec3 point, float radius)
	{
		var radiusSquared = radius * radius;
		var flatPlane = new Plane(Vec3.UnitZ, point.Z);
		var flatPoint = point.XY();
		var hits = new StackList8<WallHit>();
		var solids = Pool.Get<List<Solid>>();
		SolidGrid.Query(solids, new Rect(point.X - radius, point.Y - radius, radius * 2, radius * 2));

		foreach (var solid in solids)
		{
			if (!solid.Collidable || solid.Destroying)
				continue;

			if (!solid.WorldBounds.Inflate(radius).Contains(point))
				continue;

			var verts = solid.WorldVertices;
			var faces = solid.WorldFaces;

			foreach (var face in faces)
			{
				// TODO: ignore planes that are less flat than this but still fairly floor-like?
				// ignore flat planes
				if (face.Plane.Normal.Z <= -1 || face.Plane.Normal.Z >= 1)
					continue;

				// igore planes that are definitely too far away
				var distanceToPlane = Utils.DistanceToPlane(point, face.Plane);
				if (distanceToPlane < 0 || distanceToPlane > radius)
					continue;

				WallHit? closestTriangleOnPlane = null;

				for (int i = 0; i < face.VertexCount - 2; i++)
				{
					if (Utils.PlaneTriangleIntersection(flatPlane,
						verts[face.VertexStart + 0], verts[face.VertexStart + i + 1], verts[face.VertexStart + i + 2],
						out var line0, out var line1))
					{
						var next = new Vec3(new Line(line0.XY(), line1.XY()).ClosestPoint(flatPoint), point.Z);
						var diff = (point - next);
						if (diff.LengthSquared() > radiusSquared)
							continue;

						var pushout = (radius - diff.Length()) * diff.Normalized();
						if (closestTriangleOnPlane.HasValue && pushout.LengthSquared() < 
							closestTriangleOnPlane.Value.Pushout.LengthSquared())
							continue;

						closestTriangleOnPlane = new WallHit()
						{
							Pushout = pushout,
							Point = next,
							Normal = face.Plane.Normal,
							Actor = solid
						};
					}
				}

				if (closestTriangleOnPlane.HasValue)
				{
					hits.Add(closestTriangleOnPlane.Value);
					if (hits.Count >= hits.Capacity)
						goto RESULT;
				}
			}
		}

	RESULT:
		Pool.Return(solids);
		return hits;
	}

	public bool SolidWallCheckNearest(in Vec3 point, float radius, out WallHit hit)
	{
		var hits = SolidWallCheck(point, radius);
		if (hits.Count > 0)
		{
			var closest = hits[0];
			for (int i = 1; i < hits.Count; i++)
			{
				if (hits[i].Pushout.LengthSquared() > closest.Pushout.LengthSquared()) // note reversed because we want the most pushout
					closest = hits[i];
			}
			hit = closest;
			return true;
		}
		else
		{
			hit = default;
			return false;
		}
	}

	public bool SolidWallCheckClosestToNormal(in Vec3 point, float radius, Vec3 normal, out WallHit hit)
	{
		var hits = SolidWallCheck(point, radius);
		if (hits.Count > 0)
		{
			hit = hits[0];
			for (int i = 1; i < hits.Count; i++)
			{
				var d0 = Vec3.Dot(hit.Normal, normal);
				var d1 = Vec3.Dot(hits[i].Normal, normal);
				if (d1 > d0)
					hit = hits[i];
			}
			return true;
		}
		else
		{
			hit = default;
			return false;
		}
	}

	public bool Overlaps<T>(Vec3 point, Func<T, bool>? predicate = null) where T : Actor
	{
		return OverlapsFirst(point, predicate) != null;
	}

	public T? OverlapsFirst<T>(Vec3 point, Func<T, bool>? predicate = null) where T : Actor
	{
		if (typeof(T).IsAssignableTo(typeof(Solid)))
		{
			var solids = Pool.Get<List<Solid>>();
			SolidGrid.Query(solids, new Rect(point.X - 1, point.Y - 1, 2, 2));

			foreach (var solid in solids)
			{
				if (solid is T instance)
					if (instance.WorldBounds.Contains(point) && (predicate == null || predicate(instance)))
					{
						Pool.Return(solids);
						return instance;
					}
			}

			Pool.Return(solids);
		}
		else
		{
			foreach (var actor in All<T>())
				if (actor.WorldBounds.Contains(point) && (predicate == null || predicate((actor as T)!)))
					return (actor as T)!;
		}
		return null;
	}

	public override void Render(Target target)
	{
		debugRndTimer.Restart();
		Camera.Target = target;
		target.Clear(0x444c83, 1, 0, ClearMask.All);

		// create render state
		RenderState state = new();
		{
			state.Camera = Camera;
			state.ModelMatrix = Matrix.Identity;
			state.SunDirection = new Vec3(0, -.7f, -1).Normalized();
			state.Silhouette = false;
			state.DepthCompare = DepthCompare.Less;
			state.DepthMask = true;
			state.VerticalFogColor = 0xdceaf0;
		}

		// collect renderable objects
		{
			sprites.Clear();
			models.Clear();

			// collect point shadows
			foreach (var actor in All<ICastPointShadow>())
			{
				var alpha = (actor as ICastPointShadow)!.PointShadowAlpha;
				if (alpha > 0 && 
					Camera.Frustum.Contains(actor.WorldBounds.Conflate(actor.WorldBounds - Vec3.UnitZ * 1000)))
					sprites.Add(Sprite.CreateShadowSprite(this, actor.Position + Vec3.UnitZ, alpha));
			}

			// collect models & sprites
			foreach (var actor in Actors)
			{
				if (!Camera.Frustum.Contains(actor.WorldBounds.Inflate(1)))
					continue;

				(actor as IHaveSprites)?.CollectSprites(sprites);
				(actor as IHaveModels)?.CollectModels(models);
			}

			// sort models by distance (for transparency)
			models.Sort((a, b) =>
				(int)((b.Actor.Position - Camera.Position).LengthSquared() -
				 (a.Actor.Position - Camera.Position).LengthSquared()));

			// perp all models
			foreach (var it in models)
				it.Model.Prepare();
		}

		// draw the skybox first
		{
			var shift = new Vec3(Camera.Position.X, Camera.Position.Y, Camera.Position.Z);
			for (int i = 0; i < skyboxes.Count; i++)
			{
				skyboxes[i].Render(Camera, 
				Matrix.CreateRotationZ(i * GeneralTimer * 0.01f) *
				Matrix.CreateScale(1, 1, 0.5f) *
				Matrix.CreateTranslation(shift), 300);
			}
		}

		// render solids
		RenderModels(ref state, models, ModelFlags.Terrain);

		// render silhouettes
		{
			var it = state;
			it.DepthCompare = DepthCompare.Greater;
			it.DepthMask = false;
			it.Silhouette = true;
			RenderModels(ref it, models, ModelFlags.Silhouette);
			state.Triangles = it.Triangles;
			state.Calls = it.Calls;
		}

		// render main models
		RenderModels(ref state, models, ModelFlags.Default);
		
		// perform post processing effects
		ApplyPostEffects();

		// render alpha threshold transparent stuff
		// {
		// 	state.CutoutMode = true;
		// 	RenderModels(ref state, models, ModelFlags.Cutout);
		// 	state.CutoutMode = false;
		// }

		// render 2d sprites
		{
			spriteRenderer.Render(ref state, sprites, false);
			spriteRenderer.Render(ref state, sprites, true);
		}

		// render partially transparent models... must be sorted etc
		{
			state.DepthMask = false;
			RenderModels(ref state, models, ModelFlags.Transparent);
			state.DepthMask = true;
		}

		// strawberry collect effect
		if (Camera.Target != null && models.Any((it) => it.Model.Flags.Has(ModelFlags.StrawberryGetEffect)))
		{
			var img = Assets.Subtextures["splash"];
			var orig = new Vec2(img.Width, img.Height) / 2;

			Camera.Target.Clear(Color.Black, 1, 0, ClearMask.Depth);

			batch.Rect(Camera.Target.Bounds, Color.Black * 0.90f);
			batch.Image(img, Camera.Target.Bounds.Center, orig, Vec2.One, 0, Color.White);
			batch.Render(Camera.Target);
			batch.Clear();

			RenderModels(ref state, models, ModelFlags.StrawberryGetEffect);

			ApplyPostEffects();
		}

		// ui
		{
			batch.SetSampler(new TextureSampler(TextureFilter.Linear, TextureWrap.ClampToEdge, TextureWrap.ClampToEdge));
			var bounds = new Rect(0, 0, target.Width, target.Height);
			var font = Language.Current.SpriteFont;

			foreach (var actor in All<IHaveUI>())
				(actor as IHaveUI)!.RenderUI(batch, bounds);

			// pause menu
			if (Paused)
			{
				batch.Rect(bounds, Color.Black * 0.70f);
				pauseMenu.Render(batch, bounds.Center);
			}

			// debug
			if (DebugDraw)
			{
				var updateMs = debugUpdTimer.Elapsed.TotalMilliseconds;
				var renderMs = lastDebugRndTime.TotalMilliseconds;
				var frameMs = debugFpsTimer.Elapsed.TotalMilliseconds;
				var fps = (int)(1000/frameMs);
				debugFpsTimer.Restart();

				batch.Text(font, $"Draws: {state.Calls}, Tris: {state.Triangles}, Upd: {debugUpdateCount}", bounds.BottomLeft, new Vec2(0, 1), Color.Red);
				batch.Text(font, $"u:{updateMs:0.00}ms | r:{renderMs:0.00}ms | f:{frameMs:0.00}ms / {fps}fps", bounds.BottomLeft - new Vec2(0, font.LineHeight), new Vec2(0, 1), Color.Red);
				batch.Text(font, $"m: {Entry.Map}, c: {Entry.CheckPoint}, s: {Entry.Submap}", bounds.BottomLeft - new Vec2(0, font.LineHeight * 2), new Vec2(0, 1), Color.Red);
			}

			// stats
			{
				var at = bounds.TopLeft + new Vec2(4, 8);
				if (IsInEndingArea || Save.Instance.SpeedrunTimer)
				{
					UI.Timer(batch, Save.CurrentRecord.Time, at);
					at.Y += UI.IconSize + 4;
				}

				if (strawbCounterEase > 0)
				{
					var wiggle = 1 + MathF.Sin(strawbCounterWiggle * MathF.Tau * 2) * strawbCounterWiggle * .3f;

					batch.PushMatrix(
						Matrix3x2.CreateTranslation(0, -UI.IconSize / 2) * 
						Matrix3x2.CreateScale(wiggle) * 
						Matrix3x2.CreateTranslation(at + new Vec2(-60 * (1 - Ease.Cube.Out(strawbCounterEase)), UI.IconSize / 2)));
					UI.Strawberries(batch, Save.CurrentRecord.Strawberries.Count, Vec2.Zero);
					batch.PopMatrix();
				}

				// show version number when paused / in ending area
				if (IsInEndingArea || Paused)
				{
                    UI.Text(batch, Game.VersionString, bounds.BottomLeft + new Vec2(4, -4) * Game.RelativeScale, new Vec2(0, 1), Color.White * 0.25f);
                }
			}

			// overlay
			{
				var scroll = -new Vec2(1.25f, 0.9f) * (float)(Time.Duration.TotalSeconds) * 0.05f;

				batch.PushBlend(BlendMode.Add);
				batch.Image(Assets.Textures["overworld/overlay"], 
					bounds.TopLeft, bounds.TopRight, bounds.BottomRight, bounds.BottomLeft,
					scroll + new Vec2(0, 0), scroll + new Vec2(1, 0), scroll + new Vec2(1, 1), scroll + new Vec2(0, 1),
					Color.White * 0.10f);
				batch.PopBlend();
			}

			batch.Render(Camera.Target);
			batch.Clear();
		}

		lastDebugRndTime = debugRndTimer.Elapsed;
		debugRndTimer.Stop();
	}

	private void ApplyPostEffects()
	{
		// perform post processing effects
		if (Camera.Target != null)
		{
			if (postTarget == null || postTarget.Width < Camera.Target.Width || postTarget.Height < Camera.Target.Height)
			{
				postTarget?.Dispose();
				postTarget = new(Camera.Target.Width, Camera.Target.Height);
			}
			postTarget.Clear(Color.Black);

			var postCam = Camera with { Target = postTarget };

			// apply post fx
			postMaterial.SetShader(Assets.Shaders["Edge"]);
            if (postMaterial.Shader?.Has("u_depth") ?? false)
			    postMaterial.Set("u_depth", Camera.Target.Attachments[1]);
            if (postMaterial.Shader?.Has("u_pixel") ?? false)
			    postMaterial.Set("u_pixel", new Vec2(1.0f / postCam.Target.Width, 1.0f / postCam.Target.Height));
            if (postMaterial.Shader?.Has("u_edge") ?? false)
			    postMaterial.Set("u_edge", new Color(0x110d33));
			batch.PushMaterial(postMaterial);
			batch.Image(Camera.Target.Attachments[0], Color.White);
			batch.Render(postTarget);
			batch.Clear();

			// draw post back to the gameplay
			batch.Image(postTarget, Color.White);
			batch.Render(Camera.Target);
			batch.Clear();
		}
	}

	private void RenderModels(ref RenderState state, List<ModelEntry> models, ModelFlags flags)
	{
		foreach (var it in models)
		{
			if (!it.Model.Flags.Has(flags))
				continue;

			state.ModelMatrix = it.Model.Transform * it.Actor.Matrix;
			it.Model.Render(ref state);
		}
	}
}
