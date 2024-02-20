using System;

namespace Celeste64.HookGen;

[AttributeUsage(AttributeTargets.Method)]
public class OnHookGenAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method)]
public class ILHookGenAttribute : Attribute;