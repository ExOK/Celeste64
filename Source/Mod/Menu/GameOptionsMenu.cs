namespace Celeste64;

public class GameOptionsMenu : Menu
{
	public Menu FujiOptionsMenu;


	public override void Closed()
	{
		base.Closed();
		Settings.SaveToFile();
	}

	public GameOptionsMenu(Menu? rootMenu)
	{
		RootMenu = rootMenu;
		// Setup fuji options menu
		FujiOptionsMenu = new Menu { Title = Loc.Str("FujiOptions") };

		FujiOptionsMenu.Add(new Toggle("FujiEnableDebugMenu", Settings.ToggleEnableDebugMenu, () => Settings.EnableDebugMenu));
		FujiOptionsMenu.Add(new Toggle("FujiWriteLog", Settings.ToggleWriteLog, () => Settings.WriteLog));
		FujiOptionsMenu.Add(new Toggle("FujiAdditionalLog", Settings.ToggleEnableAdditionalLogs, () => Settings.EnableAdditionalLogging));
		FujiOptionsMenu.Add(new Option("Exit", () =>
		{
			PopSubMenu();
		}));

		// Setup this menu
		Title = Loc.Str("OptionsTitle");
		Add(new Toggle("OptionsFullscreen", Settings.ToggleFullscreen, () => Settings.Fullscreen));
		Add(new Toggle("OptionsZGuide", Settings.ToggleZGuide, () => Settings.ZGuide));
		Add(new Toggle("OptionsTimer", Settings.ToggleTimer, () => Settings.SpeedrunTimer));
		if (Assets.Languages.Count > 1)
		{
			Add(new OptionList("OptionsLanguage",
				() => Assets.Languages.Select(l => l.Value.Label).ToList(),
				() => Language.Current.Label,
				(Language) =>
				{
					Language newLanguage = Assets.Languages.FirstOrDefault(la => la.Value.Label == Language).Value;
					Settings.SetLanguage(newLanguage.ID);
					newLanguage.Use();
				}));
		}
		Add(new MultiSelect<InvertCameraOptions>("OptionsInvertCamera", Settings.SetCameraInverted, () => Settings.InvertCamera));
		Add(new Spacer());
		Add(new Slider("OptionsBGM", 0, 10, () => Settings.MusicVolume, Settings.SetMusicVolume));
		Add(new Slider("OptionsSFX", 0, 10, () => Settings.SfxVolume, Settings.SetSfxVolume));
		Add(new Spacer());
		Add(new Submenu("FujiOptions", this, FujiOptionsMenu));
	}
}
