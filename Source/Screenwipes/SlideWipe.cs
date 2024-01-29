
namespace Celeste64;

public class SlideWipe : ScreenWipe
{
	private const int Rows = 12;
	private const float AngleSize = 64;
	private const float Duration = 0.20f;

	public SlideWipe() : base(Duration) { }

	public override void Start() { }

	public override void Step(float percent) { }

	public override void Render(Batcher batch, Rect bounds)
	{
		if ((Percent <= 0 && IsFromBlack) || (Percent >= 1 && !IsFromBlack))
		{
			batch.Rect(bounds, Color.Black);
			return;
		}

		var shift = 64;
		var rect = bounds.Inflate(shift, 0);

		if (IsFromBlack)
			rect.X = Calc.Lerp(-shift, bounds.Right, Percent);
		else
			rect.X = Calc.Lerp(-bounds.Width, -shift, Percent);

		batch.Quad(
			rect.TopLeft + new Vec2(shift, 0),
			rect.TopRight,
			rect.BottomRight + new Vec2(-shift, 0),
			rect.BottomLeft,
			Color.Black);
	}
}