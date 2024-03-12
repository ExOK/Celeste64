namespace Celeste64.Mod;

public class ModOptionsMenu : Menu
{
	internal GameMod? Mod;

	internal ModOptionsMenu(Menu? rootMenu)
	{
		RootMenu = rootMenu;
	}

	public override void Closed()
	{
		base.Closed();
		if (Mod != null)
		{
			Mod.SaveSettings();
			Save.Instance.SaveToFile();
		}
	}

	//Items list always includes the Back item, so check if it's more than 1 to know if we should display it.
	internal bool ShouldDisplay => items.Count > 1;

	internal void SetMod(GameMod mod)
	{
		Mod = mod;
		items.Clear();
		Title = mod.ModInfo.Name + "\nOptions";

		mod.AddModSettings(this);

		if (RootMenu != null)
		{
			Add(new Option("Back", () =>
			{
				PopRootSubMenu();
			}));
		}
	}
}