
namespace Celeste64;

public abstract class Scene
{
	public string Music = string.Empty;
	public string Ambience = string.Empty;

	public virtual void Entered() {}
	public virtual void Exited() {}
	public virtual void Disposed() {}
	public abstract void Update();
	public abstract void Render(Target target);
}