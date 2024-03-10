namespace Celeste64.Mod;

/// <summary>
/// Prevents hook-gen to automatically generate hooks for the specific type / method.
/// This should used on almost every method inside the 'Celeste64' namespace for methods added by Fuji.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class DisallowHooksAttribute : Attribute;