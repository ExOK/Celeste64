namespace Celeste64;

public static class CustomPlayerStateRegistry
{
    private static readonly List<CustomPlayerState> _customStates = [];

    private static readonly Dictionary<Type, Player.States> _stateTypesToId = [];

    public static IReadOnlyList<CustomPlayerState> RegisteredStates => _customStates;

    /// <summary>
    /// Id of the first custom state.
    /// </summary>
    internal static Player.States BaseId { get; } = (Player.States)(Enum.GetValues<Player.States>().Length);
    
    /// <summary>
    /// Registers the provided type.
    /// Should not be called while there is a <see cref="Player"/> instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static void Register<T>() where T : CustomPlayerState, new()
    {
        var state = new T();

        _stateTypesToId[typeof(T)] = BaseId + _customStates.Count;
        _customStates.Add(state);
    }
    
    /// <summary>
    /// Deregisters the provided type.
    /// Should not be called while there is a <see cref="Player"/> instance.
    /// </summary>
    internal static void Deregister<T>() where T : CustomPlayerState, new()
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
    internal static CustomPlayerState? GetById(Player.States state)
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
    internal static Player.States GetId<T>() where T : CustomPlayerState
    {
        if (_stateTypesToId.TryGetValue(typeof(T), out var id))
        {
            return id;
        }

        throw new UnregisteredStateUseException(typeof(T));
    }
}

/// <summary>
/// Defines a custom player state. Treated as a singleton.
/// </summary>
public abstract class CustomPlayerState
{
    /// <summary>
    /// Whether this state can control hair color.
    /// If this returns false, regular colors will be used based on dash count.
    /// </summary>
    public virtual bool ControlHairColor => false;

    /// <summary>
    /// Whether the player can pick up <see cref="IPickup"/>s.
    /// Defaults to true.
    /// </summary>
    public virtual bool IsAbleToPickup => true;
    
    /// <summary>
    /// Whether its possible to pause the game in this state.
    /// Defaults to true.
    /// </summary>
    public virtual bool IsAbleToPause => true;
    
    /// <summary>
    /// Called each frame when the player is in this state.
    /// </summary>
    public abstract void Update(Player player);

    /// <summary>
    /// Called the frame the player enters this state.
    /// </summary>
    public abstract void OnBegin(Player player);

    /// <summary>
    /// Called the frame the player leaves this state
    /// </summary>
    /// <param name="player"></param>
    public abstract void OnEnd(Player player);

    /// <summary>
    /// A routine that begins when the player enters this state.
    /// </summary>
    public abstract CoEnumerator Routine(Player player);
}

public class UnregisteredStateUseException(Type type) : Exception
{
    public override string Message => $"Tried to use custom player state {type.FullName}, which is not registered!";
}