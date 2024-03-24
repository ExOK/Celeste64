using System.Diagnostics;

namespace Celeste64;

public class GameOptionsMenu : Menu
{
	public Menu FujiOptionsMenu;

	public GameOptionsMenu(Menu? rootMenu)
	{
		RootMenu = rootMenu;
		// Setup fuji options menu
		FujiOptionsMenu = new Menu { Title = Loc.Str("FujiOptions") };

		FujiOptionsMenu.Add(new Toggle("FujiEnableDebugMenu", Save.Instance.ToggleEnableDebugMenu, () => Save.Instance.EnableDebugMenu));
		FujiOptionsMenu.Add(new Toggle("FujiWriteLog", Save.Instance.ToggleWriteLog, () => Save.Instance.WriteLog));
		FujiOptionsMenu.Add(new Slider("OptionsResolution", 1, 5, () => (int)Game.ResolutionScale, Game.Instance.SetResolutionScale));
		FujiOptionsMenu.Add(new Toggle("Quick Startup on CTRL", Save.Instance.ToggleQuickStart, () => Save.Instance.QuickStart));
		FujiOptionsMenu.Add(new Option("Exit", () =>
		{
			PopSubMenu();
		}));

		// Setup this menu
		Title = Loc.Str("OptionsTitle");
		Add(new Toggle("OptionsFullscreen", Save.Instance.ToggleFullscreen, () => Save.Instance.Fullscreen));
		Add(new Toggle("OptionsZGuide", Save.Instance.ToggleZGuide, () => Save.Instance.ZGuide));
		Add(new Toggle("OptionsTimer", Save.Instance.ToggleTimer, () => Save.Instance.SpeedrunTimer));
		if (Assets.Languages.Count > 1)
		{
			Add(new OptionList("OptionsLanguage",
				() => Assets.Languages.Select(l => l.Value.Label).ToList(),
				() => Language.Current.Label,
				(Language) =>
				{
					Language newLanguage = Assets.Languages.FirstOrDefault(la => la.Value.Label == Language).Value;
					Save.Instance.SetLanguage(newLanguage.ID);
					newLanguage.Use();
				}));
		}
		Add(new MultiSelect<Save.InvertCameraOptions>("OptionsInvertCamera", Save.Instance.SetCameraInverted, () => Save.Instance.InvertCamera));
		Add(new Spacer());
		Add(new Slider("OptionsBGM", 0, 10, () => Save.Instance.MusicVolume, Save.Instance.SetMusicVolume));
		Add(new Slider("OptionsSFX", 0, 10, () => Save.Instance.SfxVolume, Save.Instance.SetSfxVolume));
		Add(new Spacer());
		Add(new Submenu("FujiOptions", this, FujiOptionsMenu));
	}
}
