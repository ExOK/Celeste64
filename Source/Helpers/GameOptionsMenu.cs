namespace Celeste64;

public class GameOptionsMenu : Menu
{
    public Menu FujiOptionsMenu;

    public GameOptionsMenu()
    {
        // Setup fuji options menu
        FujiOptionsMenu = new Menu();

        FujiOptionsMenu.Title = Loc.Str("FujiOptions");
		FujiOptionsMenu.Add(new Menu.Toggle(Loc.Str("FujiWriteLog"), Save.Instance.ToggleWriteLog, () => Save.Instance.WriteLog));
		FujiOptionsMenu.Add(new Menu.Option(Loc.Str("Exit"), () => {
            this.PopSubMenu();
        }));
        
        // Setup this menu
        this.Title = Loc.Str("OptionsTitle");
        this.Add(new Menu.Toggle(Loc.Str("OptionsFullscreen"), Save.Instance.ToggleFullscreen, () => Save.Instance.Fullscreen));
        this.Add(new Menu.Toggle(Loc.Str("OptionsZGuide"), Save.Instance.ToggleZGuide, () => Save.Instance.ZGuide));
        this.Add(new Menu.Toggle(Loc.Str("OptionsTimer"), Save.Instance.ToggleTimer, () => Save.Instance.SpeedrunTimer));
        this.Add(new Menu.MultiSelect<Save.InvertCameraOptions>(Loc.Str("OptionsInvertCamera"), Save.Instance.SetCameraInverted, () => Save.Instance.InvertCamera));
        this.Add(new Menu.Spacer());
        this.Add(new Menu.Slider(Loc.Str("OptionsBGM"), 0, 10, () => Save.Instance.MusicVolume, Save.Instance.SetMusicVolume));
        this.Add(new Menu.Slider(Loc.Str("OptionsSFX"), 0, 10, () => Save.Instance.SfxVolume, Save.Instance.SetSfxVolume));
        this.Add(new Menu.Spacer());
        this.Add(new Menu.Submenu(Loc.Str("FujiOptions"), this, FujiOptionsMenu));
    }
}