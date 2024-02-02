
namespace Celeste64;

public unsafe sealed class StateMachine<TIndex, TEvent> 
	where TIndex : unmanaged, Enum
	where TEvent : unmanaged, Enum
{
	private static readonly int StateCount = Enum.GetValues<TIndex>().Length;
	private static readonly int EventCount = Enum.GetValues<TEvent>().Length;
	private static int StateToIndex(TIndex state) => *(int*)(&state);
	private static int EventToIndex(TEvent state) => *(int*)(&state);

	private readonly Action?[] update = new Action[StateCount];
	private readonly Action?[] enter = new Action[StateCount];
	private readonly Action?[] exit = new Action[StateCount];
	private readonly Func<CoEnumerator>?[] routine = new Func<CoEnumerator>[StateCount];
	private readonly Action?[][] events;

	private TIndex? state;
	private Routine running = new();

	public StateMachine()
	{
		events = new Action?[StateCount][];
		for (int i = 0; i < StateCount; i++)
			events[i] = new Action?[EventCount];
	}

	public void InitState(TIndex state, Action? update, Action? enter = null, Action? exit = null, Func<CoEnumerator>? routine = null)
	{
		int index = StateToIndex(state);
		this.update[index] = update;
		this.enter[index] = enter;
		this.exit[index] = exit;
		this.routine[index] = routine;

	}

	public void InitStateEvent(TIndex state, TEvent ev, Action? action)
	{
		events[StateToIndex(state)][EventToIndex(ev)] = action;
	}

	public TIndex? PreviousState { get; private set; }

	public TIndex? State
	{
		get => state;
		set
		{
			PreviousState = state;
			state = value;
			running.Clear();
			if (PreviousState.HasValue)
				exit[StateToIndex(PreviousState.Value)]?.Invoke();
			if (state.HasValue)
			{
				enter[StateToIndex(state.Value)]?.Invoke();
				if (routine[StateToIndex(state.Value)] is { } rt)
					running.Run(rt());
			}
		}
	}

	public void Update()
	{
		if (state.HasValue)
			update[StateToIndex(state.Value)]?.Invoke();
		running.Update();
	}

	public void CallEvent(TEvent ev)
	{
		if (state.HasValue)
			events[StateToIndex(state.Value)][EventToIndex(ev)]?.Invoke();
	}
}
