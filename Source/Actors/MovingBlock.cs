
namespace Celeste64;

public class MovingBlock : Solid
{
	public Vec3 Start;
	public Vec3 End;

	private float lerp;
	private float target = 1;
	private bool goSlow;

	public MovingBlock(bool goSlow, Vec3 end)
	{
		this.goSlow = goSlow;
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

		Calc.Approach(ref lerp, target, Time.Delta / (goSlow ? 2 : 1));
		if (lerp == target)
			target = 1 + -(int)target;

		MoveTo(Vec3.Lerp(Start, End, Utils.SineInOut(lerp)));
	}
}
