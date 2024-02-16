namespace Celeste64.Mod;

/// <summary>
/// Base class for everything which want to draw something using Dear ImGui.
/// Instances need to be added to / removed from <see cref="ImGuiManager.Handlers"/> to register them. 
/// </summary>
public abstract class ImGuiHandler
{
    /// <summary>
    /// Whether this handler will be updated.
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>
    /// Whether this handler will be drawn.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Called once per frame and should be used to update state.
    /// </summary>
    public virtual void Update() { }
    
    /// <summary>
    /// Called an undefined amount per frame and should only be used to draw the current state.
    /// This should <b>not</b> update any state.
    /// </summary>
    public virtual void Render() { }
}