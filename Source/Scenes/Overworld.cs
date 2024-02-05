
namespace Celeste64;

public class Overworld : Scene
{
	public const int CardWidth = (int)(480 * Game.RelativeScale);
	public const int CardHeight = (int)(320 * Game.RelativeScale);

	public class Entry
	{
		public readonly LevelInfo Level;
		public readonly Target Target;
		public readonly Subtexture Image;
		public readonly Menu Menu;
		public readonly bool Complete = false;

		public float HighlightEase;
		public float SelectionEase;

		public Entry(LevelInfo level)
		{
			Level = level;
			Target = new Target(CardWidth, CardHeight);
			Image = new(Assets.Textures[level.Preview]);
			Menu = new();
			Menu.UpSound = Sfx.main_menu_roll_up;
			Menu.DownSound = Sfx.main_menu_roll_down;

			if (Save.Instance.TryGetRecord(Level.ID) is {} record)
			{
				Menu.Add(new Menu.Option(Loc.Str("Continue")));
				Menu.Add(new Menu.Option(Loc.Str("Restart")));
				Complete = record.Strawberries.Count >= Level.Strawberries;
			}
			else
			{
				Menu.Add(new Menu.Option(Loc.Str("Start")));
			}
		}

		public void Redraw(Batcher batch, float shine)
		{
			const float Padding = 16 * Game.RelativeScale;

			Target.Clear(Color.Transparent);
			batch.SetSampler(new(TextureFilter.Linear, TextureWrap.ClampToEdge, TextureWrap.ClampToEdge));

			var bounds = Target.Bounds;
			var font = Language.Current.SpriteFont;
			var img = (SelectionEase < 0.50f ? Image : new(Assets.Textures["postcards/back"]));

			if (img.Texture != null)
			{
				var scale = MathF.Max(bounds.Width / img.Width, bounds.Height / img.Height);
				var size = new Vec2(img.Width, img.Height);
				batch.Image(img, bounds.Center, size / 2, Vec2.One * scale, 0, Color.White);
			}

			if (SelectionEase < 0.50f)
			{
				if (Complete && Assets.Textures.GetValueOrDefault("overworld/strawberry") is {} texture)
				{
					batch.Image(
						new Subtexture(texture), 
						bounds.BottomRight - new Vec2(50, 0), 
						new Vec2(texture.Width / 2, texture.Height), 
						Vec2.One * 0.50f, 0, Color.White);
				}

				batch.PushMatrix(Matrix3x2.CreateScale(2.0f) * Matrix3x2.CreateTranslation(bounds.BottomLeft + new Vec2(Padding, -Padding)));
				UI.Text(batch, Level.Name, Vec2.Zero, new Vec2(0, 1), Color.White);
				batch.PopMatrix();
			}
			else
			{
				batch.Rect(bounds, Color.Black * 0.25f);

				// info
				batch.Text(font, Level.Label, bounds.TopLeft + new Vec2(32, 24) * Game.RelativeScale, Color.Black * 0.7f);

				// stats
				batch.PushMatrix(Matrix3x2.CreateScale(1.3f) * Matrix3x2.CreateTranslation(bounds.Center + new Vec2(0, -Padding)));
				{
					int strawbs = 0, deaths = 0;
					TimeSpan time = new();

					if (Save.Instance.TryGetRecord(Level.ID) is { } record)
					{
						strawbs = record.Strawberries.Count;
						deaths = record.Deaths;
						time = record.Time;
					}

					UI.Strawberries(batch, strawbs, new Vec2(-8, -UI.IconSize / 2 - 4), 1);
					UI.Deaths(batch, deaths, new Vec2(8, -UI.IconSize / 2 - 4), 0);
					UI.Timer(batch, time, new Vec2(0, UI.IconSize / 2 + 4), 0.5f);
				}
				batch.PopMatrix();

				// options
				Menu.Render(batch, bounds.BottomCenter + new Vec2(0, -Menu.Size.Y - 8));
			}

			if (shine > 0)
			{
				batch.Line(
					bounds.BottomLeft + new Vec2(-50 + shine * 50, 50), 
					bounds.TopCenter + new Vec2(shine * 50, -50), 120, 
					Color.White * shine * 0.30f);

				batch.Line(
					bounds.BottomLeft + new Vec2(-50 + 100 + shine * 120, 50), 
					bounds.TopCenter + new Vec2(100 + shine * 120, -50), 70, 
					Color.White * shine * 0.30f);
			}

			batch.Render(Target);
		}
	}

	private enum States
	{
		Selecting,
		Selected,
		Entering,
		Restarting
	}

	private States state = States.Selecting;
	private int index = 0;
	private float slide = 0;
	private float selectedEase = 0;
	private float cameraCloseUpEase = 0;
	private Vec2 wobble = new Vec2(0, -1);
	private readonly List<Entry> entries = [];
	private readonly Batcher batch = new();
	private readonly Mesh mesh = new();
	private readonly Material material = new(Assets.Shaders["Sprite"]);
	private readonly Menu restartConfirmMenu = new();

	public Overworld(bool startOnLastSelected)
	{
		Music = "event:/music/mus_title";
		
		foreach (var level in Assets.Levels)
			entries.Add(new(level));

		var cardWidth = CardWidth / 6.0f / Game.RelativeScale;
		var cardHeight = CardHeight / 6.0f / Game.RelativeScale;

		mesh.SetVertices<SpriteVertex>([
			new(new Vec3(-cardWidth, 0, -cardHeight) / 2, new Vec2(0, 0), Color.White),
			new(new Vec3(cardWidth, 0, -cardHeight) / 2, new Vec2(1, 0), Color.White),
			new(new Vec3(cardWidth, 0, cardHeight) / 2, new Vec2(1, 1), Color.White),
			new(new Vec3(-cardWidth, 0, cardHeight) / 2, new Vec2(0, 1), Color.White),
		]);
		mesh.SetIndices<int>([0, 1, 2, 0, 2, 3]);

		restartConfirmMenu.Add(new Menu.Option(Loc.Str("Cancel")));
		restartConfirmMenu.Add(new Menu.Option(Loc.Str("RestartLevel")));
		restartConfirmMenu.UpSound = Sfx.main_menu_roll_up;
		restartConfirmMenu.DownSound = Sfx.main_menu_roll_down;

		if (startOnLastSelected)
		{
			var exitedFrom = entries.FindIndex((e) => e.Level.ID == Save.Instance.LevelID);
			if (exitedFrom >= 0)
			{
				index = exitedFrom;
				state = States.Selected;
				selectedEase = 1.0f;
				entries[index].HighlightEase = entries[index].SelectionEase = 1.0f;
			}
		}

		cameraCloseUpEase = 1.0f;
	}

	public override void Update()
	{
		slide += (index - slide) * (1 - MathF.Pow(.001f, Time.Delta));
		wobble += (Controls.Camera.Value - wobble) * (1 - MathF.Pow(.1f, Time.Delta));
		Calc.Approach(ref cameraCloseUpEase, state == States.Entering ? 1 : 0, Time.Delta);
		Calc.Approach(ref selectedEase, state != States.Selecting ? 1 : 0, 8 * Time.Delta);

		for (int i = 0; i < entries.Count; i++)
		{
			var it = entries[i];
			Calc.Approach(ref it.HighlightEase, index == i ? 1.0f : 0.0f, Time.Delta * 8.0f); 
			Calc.Approach(ref it.SelectionEase, index == i && (state == States.Selected || state == States.Restarting) ? 1.0f : 0.0f, Time.Delta * 4.0f);

			if (it.SelectionEase >= 0.50f && state == States.Selected)
				it.Menu.Update();
			it.Menu.Focused = state == States.Selected;
		}

		if (Game.Instance.IsMidTransition)
			return;
		
		if (state == States.Selecting)
		{
			var was = index;
			if (Controls.Menu.Horizontal.Negative.Pressed)
			{
				Controls.Menu.ConsumePress();
				index--;
			}
			if (Controls.Menu.Horizontal.Positive.Pressed)
			{
				Controls.Menu.ConsumePress();
				index++;
			}
			index = Calc.Clamp(index, 0, entries.Count - 1);

			if (was != index)
				Audio.Play(Sfx.ui_move);

			if (Controls.Confirm.ConsumePress())
			{
				state = States.Selected;
				entries[index].Menu.Index = 0;
				Audio.Play(Sfx.main_menu_postcard_flip);
			}

			if (Controls.Cancel.ConsumePress())
			{
				Game.Instance.Goto(new Transition()
				{
					Mode = Transition.Modes.Replace,
					Scene = () => new Titlescreen(),
					ToBlack = new AngledWipe(),
					ToPause = true
				});
			}
		}
		else if (state == States.Selected)
		{
			if (Controls.Confirm.ConsumePress() && entries[index].SelectionEase > 0.50f)
			{
				if (entries[index].Menu.Index == 1)
				{
					Audio.Play(Sfx.main_menu_restart_confirm_popup);
					restartConfirmMenu.Index = 0;
					state = States.Restarting;
				}
				else
				{
					Audio.Play(Sfx.main_menu_start_game);
					Game.Instance.Music.Stop();
					state = States.Entering;
				}
			}
			else if (Controls.Cancel.ConsumePress())
			{
				Audio.Play(Sfx.main_menu_postcard_flip_back);
				state = States.Selecting;
			}
		}
		else if (state == States.Restarting)
		{
			restartConfirmMenu.Update();

			if (Controls.Confirm.ConsumePress())
			{
				if (restartConfirmMenu.Index == 1)
				{
					Audio.Play(Sfx.main_menu_start_game);
					Game.Instance.Music.Stop();
					Save.Instance.EraseRecord(entries[index].Level.ID);
					state = States.Entering;
				}
				else
				{
					Audio.Play(Sfx.main_menu_restart_cancel);
					state = States.Selected;
				}
			}
			else if (Controls.Cancel.ConsumePress())
			{
				Audio.Play(Sfx.main_menu_restart_cancel);
				state = States.Selected;
			}
		}
		else if (state == States.Entering)
		{
			if (cameraCloseUpEase >= 1.0f)
			{
				entries[index].Level.Enter(new SlideWipe(), 1.5f);
			}
		}
	}

	public override void Render(Target target)
	{
		target.Clear(0x0b090d, 1, 0, ClearMask.All);

		// update entry textures
		foreach (var entry in entries)
		{
			var flip = (entry.SelectionEase > 0.50f ? 1 : -1);
			var shine = MathF.Max(0, MathF.Max(-wobble.X, flip * wobble.Y)) * entry.HighlightEase;
			entry.Redraw(batch, shine);
			batch.Clear();
		}

		// draw each entry to the screen
		var camera = new Camera
		{
			Target = target,
			Position = new Vec3(0, -100 + 30 * Ease.Cube.In(cameraCloseUpEase), 0),
			LookAt = new Vec3(0, 0, 0),
			NearPlane = 1,
			FarPlane = 1000
		};

		for (int i = 0; i < entries.Count; i ++)
		{
			var it = entries[i];
			var shift = Ease.Cube.In(1.0f - it.HighlightEase) * 30 - Ease.Cube.In(it.SelectionEase) * 20;
			if (i != index)
				shift += Ease.Cube.InOut(selectedEase) * 50;
			var position = new Vec3((i - slide) * 60, shift, 0);
			var rotation = Ease.Cube.InOut(it.SelectionEase);
			var matrix = 
				Matrix.CreateScale(new Vec3(it.SelectionEase >= 0.50f ? -1 : 1, 1, 1)) *
				Matrix.CreateRotationX(wobble.Y * it.HighlightEase) *
				Matrix.CreateRotationZ(wobble.X * it.HighlightEase) *
				Matrix.CreateRotationZ((state == States.Entering ? -1 : 1) * rotation * MathF.PI) *
				Matrix.CreateTranslation(position);

            if (material.Shader?.Has("u_matrix") ?? false)
			    material.Set("u_matrix", matrix * camera.ViewProjection);
            if (material.Shader?.Has("u_near") ?? false)
			    material.Set("u_near", camera.NearPlane);
            if (material.Shader?.Has("u_far") ?? false)
			    material.Set("u_far", camera.FarPlane);
            if (material.Shader?.Has("u_texture") ?? false)
			    material.Set("u_texture", it.Target);
            if (material.Shader?.Has("u_texture_sampler") ?? false)
			    material.Set("u_texture_sampler", new TextureSampler(TextureFilter.Linear, TextureWrap.ClampToEdge, TextureWrap.ClampToEdge));

			var cmd = new DrawCommand(target, mesh, material)
			{
				DepthMask = true,
				DepthCompare = DepthCompare.Less,
				CullMode = CullMode.None
			};
			cmd.Submit();
		}

		// overlay
		{
			batch.SetSampler(new TextureSampler(TextureFilter.Linear, TextureWrap.ClampToEdge, TextureWrap.ClampToEdge));
			var bounds = new Rect(0, 0, target.Width, target.Height);
			var scroll = -new Vec2(1.25f, 0.9f) * (float)(Time.Duration.TotalSeconds) * 0.05f;

			// confirmation
			if (state == States.Restarting)
			{
				batch.Rect(bounds, Color.Black * 0.90f);
				restartConfirmMenu.Render(batch, bounds.Center);
			}

			batch.PushBlend(BlendMode.Add);
			batch.PushSampler(new TextureSampler(TextureFilter.Linear, TextureWrap.Repeat, TextureWrap.Repeat));
			batch.Image(Assets.Textures["overworld/overlay"], 
				bounds.TopLeft, bounds.TopRight, bounds.BottomRight, bounds.BottomLeft,
				scroll + new Vec2(0, 0), scroll + new Vec2(1, 0), scroll + new Vec2(1, 1), scroll + new Vec2(0, 1),
				Color.White * 0.10f);
			batch.PopBlend();
			batch.PopSampler();
			batch.Image(Assets.Textures["overworld/vignette"], 
				bounds.TopLeft, bounds.TopRight, bounds.BottomRight, bounds.BottomLeft,
				new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1),
				Color.White * 0.30f);

			// button prompts
			if (state != States.Entering)
			{
				var cancelPrompt = Loc.Str(state == States.Selecting ? "back" : "cancel");
				var at = bounds.BottomRight + new Vec2(-16, -4) * Game.RelativeScale + new Vec2(0, -UI.PromptSize);
				var width = 0.0f;
				UI.Prompt(batch, Controls.Cancel, cancelPrompt, at, out width, 1.0f);
				at.X -= width + 8 * Game.RelativeScale;
				UI.Prompt(batch, Controls.Confirm, Loc.Str("confirm"), at, out _, 1.0f);

				// show version number on Overworld as well
                UI.Text(batch, Game.VersionString, bounds.BottomLeft + new Vec2(4, -4) * Game.RelativeScale, new Vec2(0, 1), Color.White * 0.25f);
            }

			if (cameraCloseUpEase > 0)
			{
				batch.PushBlend(BlendMode.Subtract);
				batch.Rect(bounds, Color.White * Ease.Cube.In(cameraCloseUpEase));
				batch.PopBlend();
			}
			batch.Render(target);
			batch.Clear();
		}
	}
}