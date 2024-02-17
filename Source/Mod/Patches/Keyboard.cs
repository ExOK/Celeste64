using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste64.Mod.Patches;

/// <summary>
/// <li>Disable input when <see cref="ImGuiManager.WantCaptureKeyboard"/> is true.</li>
/// </summary>
internal static class Keyboard 
{
    private static readonly List<ILHook> hooks = new();
    
    /// <summary>
    /// Whether keyboard input should be disabled.
    /// </summary>
    private static bool DisableKeyboard => ImGuiManager.WantCaptureKeyboard;
    
    public static void Load()
    {
        // Get all methods which should just return false when disabled
        hooks.AddRange(typeof(Foster.Framework.Keyboard)
            .GetMethods()
            .Where(m => m.Name is "Pressed" or "Down" or "Released" or "Repeated")
            .Select(m => new ILHook(m, IL_ReturnFalse)));
        // Get all methods which should just return null when disabled
        hooks.AddRange(typeof(Foster.Framework.Keyboard)
            .GetMethods()
            .Where(m => m.Name is "FirstDown" or "FirstPressed" or "FirstReleased")
            .Select(m => new ILHook(m, IL_ReturnNull)));
    }
    
    public static void Unload()
    {
        hooks.ForEach(hook => hook.Dispose());
    }
    
    private static void IL_ReturnFalse(ILContext il)
    {
        var cur = new ILCursor(il);
        // Return false when DisableKeyboard is true. Otherwise continue normally.
        // This inserts the following at the top:
        // if (DisableKeyboard) return false;
        cur.EmitCall(typeof(Keyboard).GetProperty(nameof(DisableKeyboard), BindingFlags.NonPublic | BindingFlags.Static)!.GetGetMethod(nonPublic: true)!);
        var skipReturnLabel = cur.DefineLabel();
        cur.EmitBrfalse(skipReturnLabel);
        cur.EmitLdcI4(0); // false
        cur.EmitRet();
        cur.MarkLabel(skipReturnLabel);
    }
    
    private static void IL_ReturnNull(ILContext il)
    {
        var cur = new ILCursor(il);
        // Return null when DisableKeyboard is true. Otherwise continue normally.
        // This inserts the following at the top:
        // if (DisableKeyboard) return null;
        cur.EmitCall(typeof(Keyboard).GetProperty(nameof(DisableKeyboard), BindingFlags.NonPublic | BindingFlags.Static)!.GetGetMethod(nonPublic: true)!);
        var skipReturnLabel = cur.DefineLabel();
        cur.EmitBrfalse(skipReturnLabel);
        cur.EmitLdnull();
        cur.EmitRet();
        cur.MarkLabel(skipReturnLabel);
    }
}
    