using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace BugSweeper.Models
{
    public class Theme
    {
        public string Name { get; }
        private Color Background { get; }
        private Color TextColor { get; }
        private string FontFamily { get; }

        public Theme(string name, Color background, Color textColor, string fontFamily)
        {
            Name = name;
            Background = background;
            TextColor = textColor;
            FontFamily = fontFamily;
        }

        public void Apply(Application app)
        {
            if (app == null) return;
            app.Resources["AppBackground"] = Background;
            app.Resources["AppText"] = TextColor;
            app.Resources["AppFont"] = FontFamily;
        }
    }
}