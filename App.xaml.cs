using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace MultiHubApp;

public partial class App : Application
{
    const string PrefKeyDark = "is_dark_theme";

    public App()
    {
        InitializeComponent();

        bool isDark = Preferences.Get(PrefKeyDark, false);
        Application.Current.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;

        MainPage = new AppShell();
    }

    public static void SetUserTheme(bool dark)
    {
        Preferences.Set(PrefKeyDark, dark);
        Application.Current.UserAppTheme = dark ? AppTheme.Dark : AppTheme.Light;
    }
}

