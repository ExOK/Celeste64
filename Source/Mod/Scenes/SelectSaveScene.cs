namespace Celeste64.Mod;

public class SelectSaveScene : Scene
{
    private readonly Batcher batch = new();
    private float easing = 0;

    private SaveSelectionMenu saveSelectionMenu;

    public SelectSaveScene()
    {
        Music = "event:/music/mus_title";
        saveSelectionMenu = new SaveSelectionMenu()
        {
            Title = "Select a Save",
        };
    }

    public override void Update()
    {
        easing = Calc.Approach(easing, 1, Time.Delta / 5.0f);
        saveSelectionMenu.Update();

        if (Controls.Cancel.Pressed)
        {
            Game.Instance.Goto(new Transition()
            {
                Mode = Transition.Modes.Replace,
                Scene = () => new Overworld(false),
                ToBlack = new SlideWipe(),
                ToPause = true
            });
        }
    }

    public override void Render(Target target)
    {
        target.Clear(Color.Black, 1, 0, ClearMask.All);
        batch.SetSampler(new TextureSampler(TextureFilter.Linear, TextureWrap.ClampToEdge, TextureWrap.ClampToEdge));
        var bounds = new Rect(0, 0, target.Width, target.Height);
        var scroll = -new Vec2(1.25f, 0.9f) * (float)(Time.Duration.TotalSeconds) * 0.05f;

        batch.PushBlend(BlendMode.Add);
        batch.PushSampler(new TextureSampler(TextureFilter.Linear, TextureWrap.Repeat, TextureWrap.Repeat));
        batch.Image(Assets.Textures["overworld/overlay"],
            bounds.TopLeft, bounds.TopRight, bounds.BottomRight, bounds.BottomLeft,
            scroll + new Vec2(0, 0), scroll + new Vec2(1, 0), scroll + new Vec2(1, 1), scroll + new Vec2(0, 1),
            Color.White * 0.10f);
        batch.PopSampler();
        batch.PopBlend();
        batch.Image(Assets.Textures["overworld/vignette"],
            bounds.TopLeft, bounds.TopRight, bounds.BottomRight, bounds.BottomLeft,
            new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1),
            Color.White * 0.30f);

        if (easing < 1)
        {
            batch.PushBlend(BlendMode.Subtract);
            batch.Rect(bounds, Color.White * (1 - Ease.Cube.Out(easing)));
            batch.PopBlend();
        }

        saveSelectionMenu.Render(batch, target.Bounds.Center);
        batch.Render(target);
        batch.Clear();
    }
}