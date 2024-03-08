namespace Celeste64.Mod;

public class ModSelectionMenu : Menu
{
	public Target Target;

	private int currentPage = 0;
	private int currentRow = 0;
	private int currentColumn = 0;

	private const int rows = 2;
	private const int columns = 4;

	private int CurrentPageStart { get { return currentPage * columns * rows; } }
	private int CurrentIndex { get { return currentRow * columns + currentColumn; } }

	private Subtexture postcardImage;
	private Subtexture strawberryImage;

	private GameMod[] mods;
	private ModInfoMenu modInfoMenu;
	public Menu FailedToLoadModsMenu;
	public bool HandledFailedMods;

	internal ModSelectionMenu(Menu? rootMenu)
	{
		Target = new Target(Overworld.CardWidth, Overworld.CardHeight);
		Game.OnResolutionChanged += () => Target = new Target(Overworld.CardWidth, Overworld.CardHeight);
		
		RootMenu = rootMenu;
		postcardImage = new(Assets.Textures["postcards/back-empty"]);
		strawberryImage = Assets.Subtextures["icon_strawberry"];
		mods = ModManager.Instance.Mods.Where(mod => mod is not VanillaGameMod).ToArray();

		modInfoMenu = new ModInfoMenu(rootMenu);

		FailedToLoadModsMenu = new Menu(rootMenu);
		string unloadedMods = string.Join("\n", ModLoader.FailedToLoadMods);
		FailedToLoadModsMenu.Title = Loc.Str("FujiFailedToLoad") + "\n" + unloadedMods;
		FailedToLoadModsMenu.Add(new Option("Continue", () => {
			HandledFailedMods = true;
			if(RootMenu != null)
			{
				RootMenu.PopSubMenu();
			}
		}));
		FailedToLoadModsMenu.Add(new Option("FujiOpenLogFile", () => {
			Game.WriteToLog();
			Game.OpenLog();
		}));
	}

	public override void Initialized()
	{
		base.Initialized();
		currentColumn = 0;
		currentRow = 0;
		currentPage = 0;
		modInfoMenu = new ModInfoMenu(RootMenu);

		if (!HandledFailedMods && RootMenu != null && ModLoader.FailedToLoadMods.Any())
		{
			RootMenu.PushSubMenu(FailedToLoadModsMenu);
		}
	}

	private void RenderMod(Batcher batch, GameMod mod, Vec2 pos, Vec2 size)
	{
		float imgScale = 0.7f;
		Subtexture image = mod.Subtextures.TryGetValue(mod.ModInfo.Icon ?? "", out Subtexture value) ? value : strawberryImage;
		Vec2 imageSize = new Vec2(size.X / image.Width, size.Y / image.Height);
		batch.Rect((pos - (size * imgScale) / 2) * Game.RelativeScale, size * imgScale * Game.RelativeScale, Color.White);
		batch.Image(image, (pos - (size * imgScale) / 2) * Game.RelativeScale, imageSize * imgScale * Game.RelativeScale, imageSize * imgScale * Game.RelativeScale, 0, mod.Enabled ? Color.White : Color.Gray);
		batch.PushMatrix(Matrix3x2.CreateScale(.6f) * Matrix3x2.CreateTranslation((pos + new Vec2(0, size.Y * 0.4f)) * Game.RelativeScale));
		batch.Text(Language.Current.SpriteFont, GenerateModName(mod.ModInfo.Name ?? "", 16, 2), Vec2.Zero, new Vec2(0.5f, 0), Color.Black * 0.7f);
		batch.PopMatrix();
	}

	private void RenderCurrentMod(Batcher batch, GameMod mod, Vec2 pos, Vec2 size)
	{
		float imgScale = 0.8f;
		Subtexture image = mod.Subtextures.TryGetValue(mod.ModInfo.Icon ?? "", out Subtexture value) ? value : strawberryImage;
		Vec2 imageSize = new Vector2(size.X / image.Width, size.Y / image.Height);
		batch.Rect((pos - (size * imgScale) / 2) * Game.RelativeScale, size * imgScale * Game.RelativeScale, Color.LightGray);
		batch.Image(image, (pos - (size * imgScale) / 2) * Game.RelativeScale, imageSize * imgScale * Game.RelativeScale, imageSize * imgScale * Game.RelativeScale, 0, mod.Enabled ? Color.White : Color.Gray);
		batch.PushMatrix(Matrix3x2.CreateScale(.7f) * Matrix3x2.CreateTranslation((pos + new Vec2(0, size.Y * 0.4f)) * Game.RelativeScale));
		batch.Text(Language.Current.SpriteFont, GenerateModName(mod.ModInfo.Name ?? "", 16, 2), Vec2.Zero, new Vec2(0.5f, 0), mod.Enabled ? Color.Black : Color.Black * 0.7f);
		batch.PopMatrix();
	}

	private static string GenerateModName(string str, int maxLength, int maxLines)
	{
		List<string> words = str.Split(' ').ToList();
		List<string> lines = [""];
		for (int wordIndex = 0; wordIndex < words.Count;)
		{
			if (lines.Count > maxLines) break;
			string word = words[wordIndex];
			if ((lines[lines.Count - 1] + word).Length <= maxLength)
			{
				lines[lines.Count - 1] = lines[lines.Count - 1] + " " + word;
				wordIndex++;
				continue;
			}
			else if (lines.Last().Length == 0)
			{
				lines[lines.Count - 1] = word.Substring(0, maxLength);
				words.Insert(wordIndex + 1, word.Substring(maxLength));
				lines.Add("");
				wordIndex++;
				continue;
			}
			else
			{
				lines.Add("");
			}
		}

		return string.Join("\n", lines);
	}

	private void RenderMods(Batcher batch)
	{
		var bounds = Target.Bounds;
		var scale = MathF.Max(bounds.Width / postcardImage.Width, bounds.Height / postcardImage.Height);
		float sizeMin = MathF.Min(postcardImage.Width, postcardImage.Height) / 6;
		var size = new Vec2(sizeMin, sizeMin);

		var offset = new Vec2(postcardImage.Width * -0.19f, postcardImage.Height * 0.5f * -0.15f);

		int index =0;
		for (int i = 0; i < rows && CurrentPageStart + index < mods.Length; i++)
		{
			for (int j = 0; j < columns && CurrentPageStart + index < mods.Length; j++)
			{
				if(index == currentRow * columns + currentColumn)
				{
					RenderCurrentMod(batch, mods[CurrentPageStart + index], new Vec2(sizeMin * j * 1.1f, sizeMin * i * 1.1f) + offset, size);
				}
				else
				{
					RenderMod(batch, mods[CurrentPageStart + index], new Vec2(sizeMin * j * 1.1f, sizeMin * i * 1.1f) + offset, size);
				}
				index++;
			}
		}
	}

	protected override void HandleInput()
	{
		if (Controls.Menu.Horizontal.Positive.Pressed)
		{
			if(currentColumn == columns-1)
			{
				if(((currentPage+1) * columns * rows) + (currentRow * columns) < mods.Length)
				{
					currentPage++;
					currentColumn = 0;
				}
				else if((currentPage + 1) * columns * rows < mods.Length)
				{
					currentPage++;
					currentColumn = 0;
					currentRow = 0;
				}
			}
			else if (CurrentPageStart + CurrentIndex + 1 < mods.Length)
			{
				currentColumn += 1;
			}
		}
		if (Controls.Menu.Horizontal.Negative.Pressed)
		{
			if (currentColumn == 0)
			{
				if (currentPage > 0)
				{
					currentPage--;
					currentColumn = columns - 1;
				}
			}
			else
			{
				currentColumn -= 1;
			}
		}

		if (Controls.Menu.Vertical.Positive.Pressed && (currentRow + 1) < rows && CurrentPageStart+CurrentIndex+columns < mods.Length)
			currentRow += 1;
		if (Controls.Menu.Vertical.Negative.Pressed && (currentRow - 1) >= 0)
			currentRow -= 1;

		if (Controls.Confirm.Pressed)
		{
			modInfoMenu.SetMod(mods[CurrentPageStart + CurrentIndex]);
			Audio.Play(Sfx.ui_select);
			if (RootMenu != null)
			{
				RootMenu.PushSubMenu(modInfoMenu);
			}
			Controls.Consume();
		}
	}

	protected override void RenderItems(Batcher batch)
	{
		if (postcardImage.Texture != null)
		{
			var bounds = Target.Bounds;
			var scale = MathF.Max(bounds.Width / postcardImage.Width, bounds.Height / postcardImage.Height);
			var size = new Vec2(postcardImage.Width, postcardImage.Height);
			batch.Image(postcardImage, bounds.TopLeft, size / 2, Vec2.One * scale, 0, Color.White);
			RenderMods(batch);
		}
	}
}