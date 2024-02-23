using System.Runtime.InteropServices;
using ImGuiNET;
using Color = Foster.Framework.Color;
using Material = Foster.Framework.Material;
using Mesh = Foster.Framework.Mesh;
using Texture = Foster.Framework.Texture;

namespace Celeste64.Mod;

internal class ImGuiRenderer
{
    private readonly List<Mesh> meshes = [];

    // Textures
    internal Target? target = null;
    private Material? spriteMaterial = null;
    private readonly Dictionary<IntPtr, Texture> loadedTextures = new();

    private int textureId = 0;
    private IntPtr? fontTextureId;

    public ImGuiRenderer()
    {
        IntPtr context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);

        // Enable docking
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigDockingAlwaysTabBar = true;
        io.ConfigDockingTransparentPayload = true;

        io.Fonts.AddFontDefault();

        Input.OnTextEvent += chr => io.AddInputCharacter(chr);
    }

    public unsafe void RebuildFontAtlas() {
        // Get font texture from ImGui
        var io = ImGuiNET.ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);

        // Copy the data to a managed array
        byte[] pixels = new byte[width * height * bytesPerPixel];
        Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length);

        // Create and register the texture as an XNA texture
        var tex2d = new Texture(width, height);
        tex2d.SetData<byte>(pixels);

        // Should a texture already have been build previously, unbind it first so it can be deallocated
        if (fontTextureId.HasValue) UnbindTexture(fontTextureId.Value);

        // Bind the new texture to an ImGui-friendly id
        fontTextureId = BindTexture(tex2d);

        // Let ImGui know where to find the texture
        io.Fonts.SetTexID(fontTextureId.Value);
        io.Fonts.ClearTexData(); // Clears CPU side texture data
    }

    public IntPtr BindTexture(Texture texture) {
        IntPtr id = new(textureId++);
        loadedTextures.Add(id, texture);
        return id;
    }

    public void UnbindTexture(IntPtr id) {
        loadedTextures.Remove(id);
    }

    public void Update()
    {
        var io = ImGui.GetIO();
        io.DisplaySize = App.Size;
        io.DisplayFramebufferScale = App.Size / App.SizeInPixels;

        foreach (var key in Enum.GetValues<Keys>())
        {
            var imGuiKey = MapImGuiKey(key);
            if (imGuiKey is ImGuiKey.None or ImGuiKey.ModNone) continue;
            io.AddKeyEvent(imGuiKey, Input.Keyboard.Down(key));
        }

        io.AddMousePosEvent(Input.Mouse.Position.X, Input.Mouse.Position.Y);

        io.AddMouseButtonEvent(0, Input.Mouse.LeftDown);
        io.AddMouseButtonEvent(1, Input.Mouse.RightDown);
        io.AddMouseButtonEvent(2, Input.Mouse.MiddleDown);

        io.AddMouseWheelEvent(Input.Mouse.Wheel.X, Input.Mouse.Wheel.Y);
    }

    public void BeforeRender()
    {
        if (target == null || target.IsDisposed || target.Width != App.WidthInPixels ||
            target.Height != App.HeightInPixels)
        {
            target?.Dispose();
            target = new Target(App.WidthInPixels, App.HeightInPixels, [TextureFormat.Color, TextureFormat.Depth24Stencil8]);

            if (spriteMaterial != null && (spriteMaterial.Shader?.Has("u_matrix") ?? false))
                spriteMaterial.Set("u_matrix",
                    Matrix.CreateOrthographicOffCenter(0f, target!.Width, target.Height, 0f, -1.0f, 1.0f));
        }
        
        if (spriteMaterial == null)
        {
            spriteMaterial = new Material(Assets.Shaders["Sprite"]);

            if (spriteMaterial.Shader?.Has("u_matrix") ?? false)
                spriteMaterial.Set("u_matrix",
                    Matrix.CreateOrthographicOffCenter(0f, App.WidthInPixels, App.HeightInPixels, 0f, -1.0f, 1.0f));
            if (spriteMaterial.Shader?.Has("u_far") ?? false)
                spriteMaterial.Set("u_far", 1.0f);
            if (spriteMaterial.Shader?.Has("u_near") ?? false)
                spriteMaterial.Set("u_near", -1.0f);
        }

        target.Clear(Color.Transparent);
        
        var io = ImGui.GetIO();
        io.DeltaTime = Time.Delta;

        ImGui.NewFrame();
        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.NoDockingOverCentralNode);
    }

    public void AfterRender()
    {
        ImGui.Render();
        var data = ImGui.GetDrawData();

        RenderImGuiDrawData(data);
    }

    private void RenderImGuiDrawData(ImDrawDataPtr drawData)
    {
        // Handle cases of screen coordinates != from framebuffer coordinates (e.g. retina displays)
        drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

        UpdateBuffers(drawData);
        RenderCommandLists(drawData);
    }

    private void UpdateBuffers(ImDrawDataPtr drawData) {
        if (drawData.TotalVtxCount == 0) return;

        // Ensure there are enough meshes
        for (int i = meshes.Count; i < drawData.CmdListsCount; i++)
            meshes.Add(new Mesh());

        for (int i = 0; i < drawData.CmdListsCount; i++) {
            var cmdList = drawData.CmdLists[i];
            var mesh = meshes[i];

            mesh.SetVertices(cmdList.VtxBuffer.Data, cmdList.VtxBuffer.Size, ImGuiVertex.VertexFormat);
            mesh.SetIndices(cmdList.IdxBuffer.Data, cmdList.IdxBuffer.Size, IndexFormat.Sixteen);
        }
    }

    private void RenderCommandLists(ImDrawDataPtr drawData) {
        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            var cmdList = drawData.CmdLists[i];
            var mesh = meshes[i];

            for (int j = 0; j < cmdList.CmdBuffer.Size; j++)
            {
                var drawCmd = cmdList.CmdBuffer[j];
                if (drawCmd.ElemCount == 0) continue;

                if (!loadedTextures.ContainsKey(drawCmd.TextureId)) {
                    throw new InvalidOperationException($"Could not find a texture with id '{drawCmd.TextureId}', please check your bindings");
                }

                if (spriteMaterial != null && (spriteMaterial.Shader?.Has("u_texture") ?? false))
                    spriteMaterial.Set("u_texture", loadedTextures[drawCmd.TextureId]);

                DrawCommand cmd = new(target, mesh, spriteMaterial!)
                {
                    BlendMode = new BlendMode(BlendOp.Add, BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha),
                    Scissor = new(
                        (int)drawCmd.ClipRect.X,
                        (int)drawCmd.ClipRect.Y,
                        (int)(drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                        (int)(drawCmd.ClipRect.W - drawCmd.ClipRect.Y)
                    ),
                    MeshIndexStart = (int)drawCmd.IdxOffset,
                    MeshIndexCount = (int)drawCmd.ElemCount,
                };
                cmd.Submit();
            }
        }
    }

    private static ImGuiKey MapImGuiKey(Keys key) =>
        key switch
        {
            Keys.Backspace => ImGuiKey.Backspace,
            Keys.Tab => ImGuiKey.Tab,
            Keys.Enter => ImGuiKey.Enter,
            Keys.Capslock => ImGuiKey.CapsLock,
            Keys.Escape => ImGuiKey.Escape,
            Keys.Space => ImGuiKey.Space,
            Keys.PageUp => ImGuiKey.PageUp,
            Keys.PageDown => ImGuiKey.PageDown,
            Keys.End => ImGuiKey.End,
            Keys.Home => ImGuiKey.Home,
            Keys.Left => ImGuiKey.LeftArrow,
            Keys.Right => ImGuiKey.RightArrow,
            Keys.Up => ImGuiKey.UpArrow,
            Keys.Down => ImGuiKey.DownArrow,
            Keys.PrintScreen => ImGuiKey.PrintScreen,
            Keys.Insert => ImGuiKey.Insert,
            Keys.Delete => ImGuiKey.Delete,
            >= Keys.D1 and <= Keys.D0 => ImGuiKey._0 + (key - Keys.D1),
            >= Keys.A and <= Keys.Z => ImGuiKey.A + (key - Keys.A),
            >= Keys.Keypad1 and <= Keys.Keypad0 => ImGuiKey.Keypad0 + (key - Keys.Keypad1),
            Keys.KeypadMultiply => ImGuiKey.KeypadMultiply,
            Keys.KeypadPlus => ImGuiKey.KeypadAdd,
            Keys.KeypadMinus => ImGuiKey.KeypadSubtract,
            Keys.KeypadComma => ImGuiKey.KeypadDecimal,
            Keys.KeypadDivide => ImGuiKey.KeypadDivide,
            >= Keys.F1 and <= Keys.F12 => ImGuiKey.F1 + (key - Keys.F1),
            >= Keys.F13 and <= Keys.F24 => ImGuiKey.F13 + (key - Keys.F1),
            Keys.Numlock => ImGuiKey.NumLock,
            Keys.ScrollLock => ImGuiKey.ScrollLock,
            Keys.LeftShift => ImGuiKey.ModShift,
            Keys.LeftControl => ImGuiKey.ModCtrl,
            Keys.LeftAlt => ImGuiKey.ModAlt,
            Keys.Semicolon => ImGuiKey.Semicolon,
            Keys.Equals => ImGuiKey.Equal,
            Keys.Comma => ImGuiKey.Comma,
            Keys.Minus => ImGuiKey.Minus,
            Keys.Period => ImGuiKey.Period,
            Keys.Slash => ImGuiKey.Slash,
            Keys.Tilde => ImGuiKey.GraveAccent,
            Keys.LeftBracket => ImGuiKey.LeftBracket,
            Keys.RightBracket => ImGuiKey.RightBracket,
            Keys.Backslash => ImGuiKey.Backslash,
            Keys.Apostrophe => ImGuiKey.Apostrophe,
            _ => ImGuiKey.None,
        };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct ImGuiVertex(Vec2 position, Vec2 uv, Color color) : IVertex
    {
        public readonly Vec2 Pos = position;
        public readonly Vec2 UV = uv;
        public readonly Color Color = color;
        public VertexFormat Format => VertexFormat;

        public static readonly VertexFormat VertexFormat = VertexFormat.Create<ImGuiVertex>(
        [
            new (0, VertexType.Float2, normalized: false),
            new (1, VertexType.Float2, normalized: false),
            new (2, VertexType.UByte4, normalized: true)
        ]);
    }
}
