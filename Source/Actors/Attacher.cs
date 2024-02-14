
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

	public virtual bool RidingPlatformCheck(Actor platform) => AttachedTo == platform;

	public virtual void RidingPlatformSetVelocity(in Vec3 value) { }

	public virtual void RidingPlatformMoved(in Vec3 delta) { Position += delta; }
}
