namespace Celeste64;

public class GameOptionsMenu : Menu
{
    public GameOptionsMenu()
    {
        this.Title = Loc.Str("OptionsTitle");
        this.Add(new Menu.Toggle(Loc.Str("OptionsFullscreen"), Save.Instance.ToggleFullscreen, () => Save.Instance.Fullscreen));
        this.Add(new Menu.Toggle(Loc.Str("OptionsZGuide"), Save.Instance.ToggleZGuide, () => Save.Instance.ZGuide));
        this.Add(new Menu.Toggle(Loc.Str("OptionsTimer"), Save.Instance.ToggleTimer, () => Save.Instance.SpeedrunTimer));
        this.Add(new Menu.MultiSelect<Save.InvertCameraOptions>(Loc.Str("OptionsInvertCamera"), Save.Instance.SetCameraInverted, () => Save.Instance.InvertCamera));
        this.Add(new Menu.Spacer());
        this.Add(new Menu.Slider(Loc.Str("OptionsBGM"), 0, 10, () => Save.Instance.MusicVolume, Save.Instance.SetMusicVolume));
        this.Add(new Menu.Slider(Loc.Str("OptionsSFX"), 0, 10, () => Save.Instance.SfxVolume, Save.Instance.SetSfxVolume));
    }
}