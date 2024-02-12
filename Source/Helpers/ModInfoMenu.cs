namespace Celeste64;

public class ModInfoMenu : Menu
{
	public const int CardWidth = (int)(480 * Game.RelativeScale);
	public const int CardHeight = (int)(320 * Game.RelativeScale);
	public readonly Target Target;

	Subtexture postcardImage;
	Subtexture stampImage;
	Subtexture strawberryImage;

	internal GameMod? Mod;

	public Menu? RootMenu;

	internal ModInfoMenu()
	{
		postcardImage = new(Assets.Textures["postcards/back-empty"]);
		stampImage = Assets.Subtextures["stamp"];
		strawberryImage = Assets.Subtextures["icon_strawberry"];
		Target = new Target(CardWidth, CardHeight);

		Add(new Toggle(Loc.Str("PauseModEnabled"),
			() => {
				//If we are trying to disable the current mod, don't
				if (Mod != null && Mod != ModManager.Instance.CurrentLevelMod)
				{
					Save.Instance.GetOrMakeMod(Mod.ModInfo.Id).Enabled = !Save.Instance.GetOrMakeMod(Mod.ModInfo.Id).Enabled;

					if (Save.Instance.GetOrMakeMod(Mod.ModInfo.Id).Enabled)
					{
						Mod.OnModLoaded();
					}
					else
					{
						Mod.OnModUnloaded();
					}
					//if (Game.Instance.World != null)
					//{
						Game.Instance.NeedsReload = true;
					//}
				}
			},
			() => Mod != null ? Save.Instance.GetOrMakeMod(Mod.ModInfo.Id).Enabled : false));
		Add(new Option(Loc.Str("Back"), () =>
		{
			if(RootMenu != null)
			{
				RootMenu.PopSubMenu();
			}
		}));
	}

	public override void Initialized()
	{
		base.Initialized();
	}

	private static string GenerateModDescription(string str, int maxLength, int maxLines)
	{
		List<string> words = str.Split(' ').ToList();
		List<string> lines = [""];
		for(int wordIndex = 0; wordIndex < words.Count;)
		{
			if (lines.Count > maxLines) break;
			string word = words[wordIndex];
			if ((lines[lines.Count-1] + word).Length <= maxLength)
			{
				lines[lines.Count - 1] = lines[lines.Count - 1] + " " + word;
				wordIndex++;
				continue;
			}
			else if (lines.Last().Length == 0)
			{
				lines[lines.Count - 1] = word.Substring(0, maxLength);
				words.Insert(wordIndex+1, word.Substring(maxLength));
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

	protected virtual void RenderOptions(Batcher batch)
	{
		var font = Language.Current.SpriteFont;
		var size = Size;
		var position = Vec2.Zero;
		batch.PushMatrix(new Vec2(0, -size.Y / 2));

		if (!string.IsNullOrEmpty(Title))
		{
			var text = Title;
			var justify = new Vec2(0.5f, 0);
			var color = new Color(8421504);

			batch.PushMatrix(
				Matrix3x2.CreateScale(TitleScale) *
				Matrix3x2.CreateTranslation(position));
			UI.Text(batch, text, Vec2.Zero, justify, color);
			batch.PopMatrix();

			position.Y += font.LineHeight * TitleScale;
			position.Y += SpacerHeight + Spacing;
		}

		for (int i = 0; i < items.Count; i++)
		{
			if (string.IsNullOrEmpty(items[i].Label))
			{
				position.Y += SpacerHeight;
				continue;
			}

			var text = items[i].Label;
			var justify = new Vec2(0.5f, 0);
			var color = Index == i && Focused ? (Time.BetweenInterval(0.1f) ? 0x84FF54 : 0xFCFF59) : Foster.Framework.Color.White;

			UI.Text(batch, text, position, justify, color);

			position.Y += font.LineHeight;
			position.Y += Spacing;
		}
		batch.PopMatrix();
	}


	protected override void RenderItems(Batcher batch)
	{
		var font = Language.Current.SpriteFont;

		if (Mod != null && postcardImage.Texture != null)
		{
			// TODO: This is messy and probably should be cleaned up and rewritten, but I'm holding off for now because I think we might want to do a visual polish pass on it later anyways.
			var bounds = Target.Bounds;
			var scale = MathF.Max(bounds.Width / postcardImage.Width, bounds.Height / postcardImage.Height);
			var size = new Vec2(postcardImage.Width, postcardImage.Height);
			batch.Image(postcardImage, bounds.TopLeft, size / 2, Vec2.One * scale, 0, Color.White);
			batch.Text(font, $"{Mod.ModInfo.Name}\nBy: {Mod.ModInfo.ModAuthor ?? "Unknown"}\nv.{Mod.ModInfo.Version}", bounds.TopLeft - size / 4 + new Vec2(16, 12) * Game.RelativeScale, Color.Black * 0.7f);
			batch.PushMatrix(Matrix3x2.CreateScale(.7f) * Matrix3x2.CreateTranslation(bounds.TopLeft - size / 4 + new Vec2(14, 72) * Game.RelativeScale));
			batch.Text(font, GenerateModDescription(Mod.ModInfo.Description ?? "", 40, 15), Vec2.Zero, Color.Black);
			batch.PopMatrix();

			float imgScale = 0.9f;
			Subtexture image = Mod.Subtextures.TryGetValue(Mod.ModInfo.Icon ?? "", out Subtexture value) ? value : strawberryImage;
			float imgSizeMin = MathF.Min(postcardImage.Width, postcardImage.Height) / 6;
			Vec2 stampImageSize = new Vec2(imgSizeMin / stampImage.Width, imgSizeMin / stampImage.Height);
			Vec2 imageSize = new Vec2(imgSizeMin / image.Width, imgSizeMin / image.Height);
			Vec2 stampPos = bounds.TopLeft - (new Vec2(imgSizeMin, imgSizeMin) * imgScale) / 2 + new Vec2(size.X / 5.5f, -size.Y / 4.7f);
			Vec2 pos = bounds.TopLeft - (new Vec2(imgSizeMin, imgSizeMin) * imgScale) / 2 + new Vec2(size.X / 5.05f, -size.Y / 5.3f);
			batch.Image(stampImage, stampPos + new Vec2(imgSizeMin, imgSizeMin) * imgScale * 0.05f, stampImageSize * imgScale, stampImageSize * imgScale * 1.3f, 0, Color.White);
			batch.Image(image, pos + new Vec2(imgSizeMin, imgSizeMin) * imgScale * 0.05f, imageSize * imgScale, imageSize * imgScale, 0, Color.White);

			batch.PushMatrix(Matrix3x2.CreateScale(1.0f) * Matrix3x2.CreateTranslation(bounds.TopLeft + new Vec2(size.X / 6.8f, -size.Y/20)));
			RenderOptions(batch);
			batch.PopMatrix();
		}
	}
}