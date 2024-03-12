namespace Celeste64.Mod;

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