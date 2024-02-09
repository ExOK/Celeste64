using ImGuiNET;

namespace Celeste64.Mod;

public class ImGuiManager
{
    private readonly ImGuiRenderer renderer;

    /// <summary>
    /// List of currently used <see cref="ImGuiHandler"/>s.
    /// </summary>
    public static List<ImGuiHandler> Handlers { get; } = [];

    /// <summary>
    /// Whether the keyboard input was consumed by Dear ImGui. 
    /// </summary>
    public bool WantCaptureKeyboard { get; private set; }
    
    /// <summary>
    /// Whether the mouse input  was consumed by Dear ImGui. 
    /// </summary>
    public bool WantCaptureMouse { get; private set; }

    internal ImGuiManager()
    {
        renderer = new ImGuiRenderer();
        renderer.RebuildFontAtlas();
    }

    public void UpdateHandlers()
    {
        renderer.Update();
        
        foreach (var handler in Handlers)
        {
            if (handler.Active) handler.Update();
        }
    }

    public void RenderHandlers()
    {
        renderer.BeforeRender();
        foreach (var handler in Handlers)
        {
            if (handler.Visible) handler.Render();
        }
        renderer.AfterRender();

        var io = ImGui.GetIO();
        WantCaptureKeyboard = io.WantCaptureKeyboard;
        WantCaptureMouse = io.WantCaptureMouse;
    }

    public void RenderTexture(Batcher batch)
    {
        if (renderer.target == null) return;
        batch.Image(renderer.target, Vec2.Zero, Color.White);
    }
}