namespace Celeste64;

public class MouseAxisBinding : VirtualButton.IBinding
{
	public Vec2 Axis;
	public int Sign;

    public bool IsPressed => false;
    public bool IsDown => false;
    public bool IsReleased => false;
    public float Value => ValueNoDeadzone;

    public float ValueNoDeadzone
	{
		get
		{
			var normal = (Input.Mouse.Position - Input.LastState.Mouse.Position).Normalized();
			var dot = Calc.Clamp(Vec2.Dot(normal, Axis), 0, 1);
			Console.WriteLine($"{Axis}: {dot}");
			return dot;
		}
	}

    public VirtualButton.ConditionFn? Enabled { get; set; }
}