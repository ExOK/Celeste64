
namespace Celeste64;

public class NonClimbableBlock : Solid {
	public override bool IsClimbable
	{
		get
		{
			return false;
		}
	}
}
