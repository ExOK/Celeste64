namespace Celeste64;

public class MovingBlock : Solid
{
	public Vec3 Start;
	public Vec3 End;

	public float Lerp;
	public float Target = 1;
	public bool GoSlow;

	public MovingBlock(bool goSlow, Vec3 end)
	{
		this.GoSlow = goSlow;
		UpdateOffScreen = true;
		End = end;
	}

	public override void Added()
	{
		Start = Position;
		base.Added();
	}

	public override void Update()
	{
		base.Update();

		Calc.Approach(ref Lerp, Target, Time.Delta / (GoSlow ? 2 : 1));
		if (Lerp == Target)
			Target = 1 + -(int)Target;

		MoveTo(Vec3.Lerp(Start, End, Utils.SineInOut(Lerp)));
	}
}
