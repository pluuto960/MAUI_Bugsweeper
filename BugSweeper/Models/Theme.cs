namespace BugSweeper.Models;

public class Theme
{
    public Color BackgroundColor { get; set; }
    public Color TextColor { get; set; }
    public string FontFamily { get; set; }

    public Theme(Color bg, Color text, string font)
    {
        BackgroundColor = bg;
        TextColor = text;
        FontFamily = font;
    }

    public void Apply(ContentPage page)
    {
        page.BackgroundColor = BackgroundColor;

        foreach (var element in GetAllChildren(page))
        {
            if (element is Label label)
            {
                label.TextColor = TextColor;
                label.FontFamily = FontFamily;
            }
            if (element is Button button)
            {
                button.TextColor = TextColor;
                button.FontFamily = FontFamily;
            }
        }
    }

    private IEnumerable<View> GetAllChildren(Element parent)
    {
        if (parent is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                yield return child;

                foreach (var sub in GetAllChildren(child))
                    yield return sub;
            }
        }
    }
}