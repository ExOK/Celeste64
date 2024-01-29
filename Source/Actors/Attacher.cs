
namespace Celeste64;

public abstract class Attacher : Actor, IRidePlatforms
{
	public virtual Vec3 AttachNormal => -Vec3.UnitZ;
	public virtual Vec3 AttachOrigin => Position;

	public Actor? AttachedTo;

	public override void Added()
	{
		base.Added();

		if (World.SolidRayCast(AttachOrigin - AttachNormal, AttachNormal, 5f, out var hit)
		&& hit.Actor is Solid solid)
		{
			AttachedTo = hit.Actor;
			solid.Attachers.Add(this);
		}
	}

	public bool RidingPlatformCheck(Actor platform) => AttachedTo == platform;

	public void RidingPlatformSetVelocity(in Vec3 value) { }

	public void RidingPlatformMoved(in Vec3 delta) { Position += delta; }
}
