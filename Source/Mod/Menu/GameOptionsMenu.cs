namespace Celeste64;

public class GameOptionsMenu : Menu
{
    public Menu FujiOptionsMenu;

    public GameOptionsMenu(Menu? rootMenu)
    {
        RootMenu = rootMenu;
        // Setup fuji options menu
        FujiOptionsMenu = new Menu();

        FujiOptionsMenu.Title = Loc.Str("FujiOptions");
		FujiOptionsMenu.Add(new Toggle("FujiEnableDebugMenu", Save.Instance.ToggleEnableDebugMenu, () => Save.Instance.EnableDebugMenu));
		FujiOptionsMenu.Add(new Toggle("FujiWriteLog", Save.Instance.ToggleWriteLog, () => Save.Instance.WriteLog));
		FujiOptionsMenu.Add(new Option("Exit", () =>
        {
            this.PopSubMenu();
        }));
        
        // Setup this menu
        this.Title = Loc.Str("OptionsTitle");
        this.Add(new Toggle("OptionsFullscreen", Save.Instance.ToggleFullscreen, () => Save.Instance.Fullscreen));
        this.Add(new Toggle("OptionsZGuide", Save.Instance.ToggleZGuide, () => Save.Instance.ZGuide));
        this.Add(new Toggle("OptionsTimer", Save.Instance.ToggleTimer, () => Save.Instance.SpeedrunTimer));
        this.Add(new MultiSelect<Save.InvertCameraOptions>("OptionsInvertCamera", Save.Instance.SetCameraInverted, () => Save.Instance.InvertCamera));
        this.Add(new Spacer());
        this.Add(new Slider("OptionsBGM", 0, 10, () => Save.Instance.MusicVolume, Save.Instance.SetMusicVolume));
        this.Add(new Slider("OptionsSFX", 0, 10, () => Save.Instance.SfxVolume, Save.Instance.SetSfxVolume));
        this.Add(new Spacer());
        this.Add(new Submenu("FujiOptions", this, FujiOptionsMenu)
            .Describe("FujiOptions.Description"));
    }
}