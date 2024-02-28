using System.Reflection;

namespace Celeste64.Mod;

// Used within Celeste64.HookGen to store the target method.
// Should not be used by anything else.

[AttributeUsage(AttributeTargets.Method)]
public class InternalOnHookGenTargetAttribute : Attribute
{
	internal MethodBase Target = null!;
}

[AttributeUsage(AttributeTargets.Method)]
public class InternalILHookGenTargetAttribute : Attribute
{
	internal MethodBase Target = null!;
}