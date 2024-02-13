namespace Celeste64;

public class GameOptionsMenu 
{
    public Menu RootMenu = new Menu();

    public GameOptionsMenu()
    {
        RootMenu.Title = Loc.Str("OptionsTitle");
        RootMenu.Add(new Menu.Toggle(Loc.Str("OptionsFullscreen"), Save.Instance.ToggleFullscreen, () => Save.Instance.Fullscreen));
        RootMenu.Add(new Menu.Toggle(Loc.Str("OptionsZGuide"), Save.Instance.ToggleZGuide, () => Save.Instance.ZGuide));
        RootMenu.Add(new Menu.Toggle(Loc.Str("OptionsTimer"), Save.Instance.ToggleTimer, () => Save.Instance.SpeedrunTimer));
        RootMenu.Add(new Menu.MultiSelect<Save.InvertCameraOptions>(Loc.Str("OptionsInvertCamera"), Save.Instance.SetCameraInverted, () => Save.Instance.InvertCamera));
        RootMenu.Add(new Menu.Spacer());
        RootMenu.Add(new Menu.Slider(Loc.Str("OptionsBGM"), 0, 10, () => Save.Instance.MusicVolume, Save.Instance.SetMusicVolume));
        RootMenu.Add(new Menu.Slider(Loc.Str("OptionsSFX"), 0, 10, () => Save.Instance.SfxVolume, Save.Instance.SetSfxVolume));
    }
}