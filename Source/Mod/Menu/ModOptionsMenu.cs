namespace Celeste64.Mod;

public class ModOptionsMenu : Menu
{
	public Target Target;

	Subtexture postcardImage;
	Subtexture stampImage;
	Subtexture strawberryImage;

	internal GameMod? Mod;

	public Menu? RootMenu;
	public Menu? depWarningMenu;
	public Menu? safeDisableErrorMenu;

	protected override int maxItemsCount { get; set; } = 8;

	internal ModOptionsMenu()
	{
		Target = new Target(Overworld.CardWidth, Overworld.CardHeight);
		Game.OnResolutionChanged += () => Target = new Target(Overworld.CardWidth, Overworld.CardHeight);
		
		postcardImage = new(Assets.Textures["postcards/back-empty"]);
		stampImage = Assets.Subtextures["stamp"];
		strawberryImage = Assets.Subtextures["icon_strawberry"];
	}

	public override void Initialized()
	{
		base.Initialized();
	}

	internal void SetMod(GameMod mod)
	{
		Mod = mod;
		items.Clear();
		mod.AddModOptions(this);

		Add(new Option(Loc.Str("Back"), () =>
		{
			if (RootMenu != null)
			{
				RootMenu.PopSubMenu();
			}
		}));
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
			batch.Text(font, $"{Mod.ModInfo.Name}\nBy: {Mod.ModInfo.ModAuthor ?? "Unknown"}\nv.{Mod.ModInfo.Version}", bounds.TopLeft + (-size / 4 + new Vec2(16, 12)) * Game.RelativeScale, Color.Black * 0.7f);

			float imgScale = 0.9f;
			Subtexture image = Mod.Subtextures.TryGetValue(Mod.ModInfo.Icon ?? "", out Subtexture value) ? value : strawberryImage;
			float imgSizeMin = MathF.Min(postcardImage.Width, postcardImage.Height) / 6;
			Vec2 stampImageSize = new Vec2(imgSizeMin / stampImage.Width, imgSizeMin / stampImage.Height);
			Vec2 imageSize = new Vec2(imgSizeMin / image.Width, imgSizeMin / image.Height);
			Vec2 stampPos = bounds.TopLeft - (new Vec2(imgSizeMin, imgSizeMin) * imgScale) / 2 + new Vec2(size.X / 5.5f, -size.Y / 4.7f);
			Vec2 pos = bounds.TopLeft - (new Vec2(imgSizeMin, imgSizeMin) * imgScale) / 2 + new Vec2(size.X / 5.05f, -size.Y / 5.3f);
			batch.Image(stampImage, (stampPos + new Vec2(imgSizeMin, imgSizeMin) * imgScale * 0.05f) * Game.RelativeScale, stampImageSize * imgScale * Game.RelativeScale, stampImageSize * imgScale * 1.3f * Game.RelativeScale, 0, Color.White);
			batch.Image(image, (pos + new Vec2(imgSizeMin, imgSizeMin) * imgScale * 0.05f) * Game.RelativeScale, imageSize * imgScale * Game.RelativeScale, imageSize * imgScale * Game.RelativeScale, 0, Color.White);

			base.RenderItems(batch);
		}
	}
}