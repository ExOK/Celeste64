namespace Celeste64.Mod;

public abstract class GameModSettings
{

}

/// <summary>
/// The dialog key / name for the settings option.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class SettingNameAttribute(string name) : Attribute
{
	public string Name = name;
}

/// <summary>
/// Add a description shown when the setting is selected
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SettingDescriptionAttribute(string description) : Attribute
{
	public string Description = description;
}

/// <summary>
/// The integer option range.
/// The Max must be greater than the Min for this to work
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SettingRangeAttribute(int min, int max) : Attribute
{
	public int Min = min;
	public int Max = max;
}

/// <summary>
/// Any options with this attribute will reload the game after being changed.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SettingNeedsReloadAttribute : Attribute
{
	public SettingNeedsReloadAttribute()
	{
	}
}

/// <summary>
/// Ignore the setting in the default mod options menu handler.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SettingIgnoreAttribute : Attribute
{
	public SettingIgnoreAttribute()
	{
	}
}

/// <summary>
/// Insert a spacer before the setting.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SettingSpacerAttribute : Attribute
{
	public SettingSpacerAttribute()
	{
	}
}

/// <summary>
/// Insert a subheader before the setting.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SettingSubHeaderAttribute(string subheader) : Attribute
{
	public string SubHeader = subheader;
}

/// <summary>
/// Create a submenu based on this type.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SettingSubMenuAttribute : Attribute { }
