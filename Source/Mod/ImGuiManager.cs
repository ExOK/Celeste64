using ImGuiNET;

namespace Celeste64.Mod;

public class ImGuiManager
{
    private readonly ImGuiRenderer renderer;

    public static List<ImGuiHandler> Handlers { get; } = [];

    public bool WantCaptureKeyboard { get; private set; }
    public bool WantCaptureMouse { get; private set; }

    public ImGuiManager()
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