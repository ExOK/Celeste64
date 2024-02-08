namespace Celeste64.Mod;

public class ImGuiHandler
{
    public bool Active { get; set; } = true;
    public bool Visible { get; set; } = true;

    public virtual void Update() { }
    public virtual void Render() { }
}