using System.Reflection;

namespace Celeste64.Mod;

/// <summary>
/// Used within Celeste64.HookGen to store the target method.
/// Should not be used by anything else.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class InternalHookGenTargetAttribute : Attribute
{
	internal MethodInfo Target = null!;
}