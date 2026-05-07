using Microsoft.Maui;
using Microsoft.Maui.Controls;
using BugSweeper.Models;

namespace BugSweeper;

public partial class App : Application
{
    public static Theme CurrentTheme { get; private set; }

    public App()
    {
        // InitializeComponent(); // removed to avoid XAML-generated dependency issues

        // Ensure essential resources exist
        if (!Application.Current.Resources.ContainsKey("AppBackground"))
            Application.Current.Resources["AppBackground"] = Colors.White;
        if (!Application.Current.Resources.ContainsKey("AppText"))
            Application.Current.Resources["AppText"] = Colors.Black;
        if (!Application.Current.Resources.ContainsKey("AppFont"))
            Application.Current.Resources["AppFont"] = "Segoe UI";

        // Set default theme
        CurrentTheme = new Theme("Light", (Color)Application.Current.Resources["AppBackground"], (Color)Application.Current.Resources["AppText"], (string)Application.Current.Resources["AppFont"]);
        CurrentTheme.Apply(Application.Current);

        MainPage = new NavigationPage(new MainPage());
    }

    public static void SetTheme(Theme theme)
    {
        CurrentTheme = theme;
        theme.Apply(Application.Current);
    }
}