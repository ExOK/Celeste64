global using CoEnumerator = System.Collections.Generic.IEnumerator<Celeste64.Co>;

namespace Celeste64;

public readonly struct Co
{
	public static readonly Co Continue = new(Types.Continue);
	public static readonly Co SingleFrame = new(Types.SingleFrame);
	public static Co Run(CoEnumerator routine) => new(routine);
	public static Co Until(Func<bool> condition) => new (condition); 

	public enum Types
	{
		Continue,
		SingleFrame,
		Wait,
		Routine,
		Until
	}

	public readonly Types Type;
	public readonly float Time;
	public readonly CoEnumerator? Routine;
	public readonly Func<bool>? Condition;
	
	public Co(Types type)
		=> Type = type;

	public Co(float time) 
		: this(Types.Wait) => Time = time;

	public Co(CoEnumerator subroutine)
		: this(Types.Routine) => Routine = subroutine;

	public Co(Func<bool> condition)
		: this(Types.Until) => Condition = condition;

	public static implicit operator Co(float time) => new(time);
	public static implicit operator Co(Func<bool> condition) => new(condition);
}

public class Routine
{
	private readonly List<CoEnumerator> running = new();
	private float waiting;
	private Func<bool>? condition;
	private uint runningID;

	public bool IsRunning => running.Count > 0;

	public void Run(CoEnumerator routine)
	{
		Clear();

		runningID++;
		running.Add(routine);

		// step in immediately when Run is called
		Update();
	}

	public void Run(Co node)
	{
		Clear();

		if (node.Routine != null)
			running.Add(node.Routine);
	}

	public void Queue(CoEnumerator routine)
	{
		if (running.Count <= 0)
		{
			Run(routine);
		}
		else
		{
			running.Insert(0, routine);
		}
	}

	public void Queue(Co node)
	{
		if (running.Count <= 0)
		{
			Run(node);
		}
		else
		{
			if (node.Routine != null)
				running.Insert(0, node.Routine);
		}
	}

	public void Clear()
	{
		running.Clear();
		waiting = 0;
		runningID++;
		condition = null;
	}

	public void Update()
	{
	RunAgain:

		if (waiting > 0)
		{
			waiting -= Time.Delta;
			return;
		}

		if (condition != null && !condition.Invoke())
			return;

		if (running.Count <= 0)
			return;

		var it = running[^1];
		var id = runningID;
		var next = it.MoveNext();

		// make sure Run/Clear weren't called from within MoveNext()
		if (id == runningID)
		{
			if (next)
			{
				var value = it.Current;

				switch (value.Type)
				{
					case Co.Types.Continue:
						goto RunAgain;
					case Co.Types.SingleFrame:
						break;
					case Co.Types.Wait:
						waiting = value.Time;
						break;
					case Co.Types.Routine:
						if (value.Routine != null)
							running.Add(value.Routine);
						goto RunAgain;
					case Co.Types.Until:
						condition = value.Condition;
						break;
				}
			}
			else
			{
				running.RemoveAt(running.Count - 1);
				goto RunAgain;
			}
		}
	}
}
