
namespace Celeste64;

/// <summary>
/// Draws any Models provided here, if the Actor's World Bounds is visible in the Camera
/// </summary>
public interface IHaveModels
{
	public void CollectModels(List<(Actor Actor, Model Model)> populate);
}

/// <summary>
/// Draws any Sprites provided here, if the Actor's World Bounds is visible in the Camera
/// </summary>
public interface IHaveSprites
{
	public void CollectSprites(List<Sprite> populate);
}

/// <summary>
/// Draws UI above the gameplay (regardless of where the Actor is)
/// </summary>
public interface IHaveUI
{
	public void RenderUI(Batcher batch, Rect bounds);
}

/// <summary>
/// Solid Platforms will search for any Actor implementing these, and move them with it
/// </summary>
public interface IRidePlatforms
{
	public void RidingPlatformSetVelocity(in Vec3 value);
	public void RidingPlatformMoved(in Vec3 delta);
	public bool RidingPlatformCheck(Actor platform);
}

/// <summary>
/// Player searches for these and calls Pickup when they're near it
/// </summary>
public interface IPickup
{
	public float PickupRadius { get; }
	public void Pickup(Player player);
}

/// <summary>
/// Player pushes out of these. Creates a Cylindar-shape from relative 0,0,0
/// </summary>
public interface IHavePushout
{
	public float PushoutHeight { get; set; }
	public float PushoutRadius { get; set; }
}

/// <summary>
/// Actor is notified of Audio Timeline Events
/// </summary>
public interface IListenToAudioCallback
{
	public void AudioCallbackEvent(int beatIndex);
}

/// <summary>
/// Actor is recycled instead of destroyed. Call World.Request<T> to get a new one.
/// </summary>
public interface IRecycle { }

/// <summary>
/// Strawberries search for any of these within their Target GroupName, and
/// waits until they're all satisfied or destroyed.
/// </summary>
public interface IUnlockStrawberry
{
	public bool Satisfied { get; }
}

/// <summary>
/// Actors with this interface will cast a small point shadow downwards
/// </summary>
public interface ICastPointShadow
{
	public float PointShadowAlpha { get; set; }
}