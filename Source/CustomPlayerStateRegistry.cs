namespace Celeste64;

public static class CustomPlayerStateRegistry
{
    private static readonly List<CustomPlayerState> _customStates = [];

    private static readonly Dictionary<Type, Player.States> _stateTypesToId = [];

    public static IReadOnlyList<CustomPlayerState> RegisteredStates => _customStates;

    /// <summary>
    /// Id of the first custom state.
    /// </summary>
    public static Player.States BaseId { get; } = (Player.States)(Enum.GetValues<Player.States>().Length);
    
    /// <summary>
    /// Registers the provided type.
    /// Should not be called while there is a <see cref="Player"/> instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void Register<T>() where T : CustomPlayerState, new()
    {
        var state = new T();

        _stateTypesToId[typeof(T)] = BaseId + _customStates.Count;
        _customStates.Add(state);
    }
    
    /// <summary>
    /// Deregisters the provided type.
    /// Should not be called while there is a <see cref="Player"/> instance.
    /// </summary>
    public static void Deregister<T>() where T : CustomPlayerState, new()
    {
        if (RegisteredStates.OfType<T>().FirstOrDefault() is not { } registered)
            throw new UnregisteredStateUseException(typeof(T));

        _customStates.Remove(registered);
        _stateTypesToId.Remove(typeof(T));
    }

    /// <summary>
    /// Gets the custom state definition tied to the specified id.
    /// Returns null if the id is not associated with a custom state.
    /// </summary>
    public static CustomPlayerState? GetById(Player.States state)
    {
        var index = state - BaseId;
        if (index < 0 || index >= _customStates.Count)
            return null;

        return _customStates[index];
    }

    /// <summary>
    /// Gets the id of the specified custom state.
    /// Throws if the state is not registered.
    /// </summary>
    public static Player.States GetId<T>() where T : CustomPlayerState
    {
        if (_stateTypesToId.TryGetValue(typeof(T), out var id))
        {
            return id;
        }

        throw new UnregisteredStateUseException(typeof(T));
    }
}

public class UnregisteredStateUseException(Type type) : Exception
{
    public override string Message => $"Tried to use custom player state {type.FullName}, which is not registered!";
}