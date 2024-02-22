
namespace Celeste64;

public class AngledWipe : ScreenWipe
{
	private const int Rows = 12;
	private const float AngleSize = 64;
	private const float Duration = 0.50f;

	private readonly Vec2[] triangles;

	public AngledWipe() : base(Duration)
	{
		triangles = new Vec2[Rows * 6];
	}

	public override void Start()
	{
		
	}

	public override void Step(float percent)
	{

	}

	public override void Render(Batcher batch, Rect bounds)
	{
		if ((Percent <= 0 && IsFromBlack) || (Percent >= 1 && !IsFromBlack))
		{
			batch.Rect(bounds, Color.Black);
			return;
		}

		var rowHeight = (bounds.Height + 20) / Rows;
		var left = -AngleSize;
		var width = bounds.Width + AngleSize;

		for (var i = 0; i < Rows; i ++)
		{
			var v = i * 6;
			var x = left;
			var y = -10 + i * rowHeight;
			var e = 0f;

			// get delay based on Y
			var across = (i / (float)Rows);
			var delay = (IsFromBlack ? 1- across : across) * 0.3f;

			// get ease after delay
			if (Percent > delay)
				e = Math.Min(1.0f, (Percent - delay) / 0.7f);

			// start full, go to nothing, if we're wiping in
			if (IsFromBlack)
				e = 1 - e;

			// resulting width
			var w = width * e;
			
			triangles[v + 0] = new Vec2(x, y);
			triangles[v + 1] = new Vec2(x + w, y);
			triangles[v + 2] = new Vec2(x, y + rowHeight);

			triangles[v + 3] = new Vec2(x + w, y);
			triangles[v + 4] = new Vec2(x + w + +AngleSize, y + rowHeight);
			triangles[v + 5] = new Vec2(x, y + rowHeight);
		}

		// flip if we're wiping in
		if (IsFromBlack)
		{
			for (var i = 0; i < triangles.Length; i++)
			{
				triangles[i].X = bounds.Width - triangles[i].X;
				triangles[i].Y = bounds.Height - triangles[i].Y;
			}
		}

		batch.PushMatrix(bounds.TopLeft);
		for (int i = 0; i < triangles.Length; i += 3)
		{
			batch.Triangle(triangles[i], triangles[i + 1], triangles[i + 2], Color.Black);
		}
		batch.PopMatrix();
	}
}