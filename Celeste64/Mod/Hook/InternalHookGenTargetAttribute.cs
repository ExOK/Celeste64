using System;

namespace Celeste64.Mod;

[AttributeUsage(AttributeTargets.Method)]
public class InternalHookGenTargetAttribute : Attribute
{
	internal string TargetType = null!;
	internal string TargetMemberName = null!;
	internal string[]? TargetParameters = null;
}