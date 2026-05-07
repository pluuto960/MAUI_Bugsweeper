using Microsoft.Maui.Controls;
using BugSweeper.Models;
using System.Linq;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using System;

namespace BugSweeper;

public class MainPage : ContentPage
{
    private Game _game;
    private int _rows = 8, _cols = 8, _mines = 10;
    private bool _flagMode = false;

    // UI fields
    private Picker ThemePicker;
    private Button NewGameButton;
    private Button FlagModeButton;
    private Label MinesLabel, TimeLabel, FlagsLabel;
    private Grid BoardGrid;

    public MainPage()
    {
        // Build UI in code to avoid XAML linking problems
        // Use DynamicResource so theme changes update immediately
        this.SetDynamicResource(BackgroundColorProperty, "AppBackground");

        ThemePicker = new Picker { Title = "Theme", HorizontalOptions = LayoutOptions.Start };
        NewGameButton = new Button { Text = "New Game", HorizontalOptions = LayoutOptions.End };
        FlagModeButton = new Button { Text = "Flag Mode" };

        MinesLabel = new Label();
        TimeLabel = new Label();
        FlagsLabel = new Label();
        MinesLabel.SetDynamicResource(Label.TextColorProperty, "AppText");
        TimeLabel.SetDynamicResource(Label.TextColorProperty, "AppText");
        FlagsLabel.SetDynamicResource(Label.TextColorProperty, "AppText");

        BoardGrid = new Grid { RowSpacing = 2, ColumnSpacing = 2, HorizontalOptions = LayoutOptions.Center };

        var topRow = new StackLayout { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.Fill };
        topRow.Children.Add(ThemePicker);
        topRow.Children.Add(NewGameButton);

        var stats = new StackLayout { Orientation = StackOrientation.Horizontal, Spacing = 15 };
        var lblMines = new Label { Text = "Mines:", VerticalOptions = LayoutOptions.Center };
        lblMines.SetDynamicResource(Label.TextColorProperty, "AppText");
        stats.Children.Add(lblMines);
        stats.Children.Add(MinesLabel);
        var lblTime = new Label { Text = "Time:", VerticalOptions = LayoutOptions.Center };
        lblTime.SetDynamicResource(Label.TextColorProperty, "AppText");
        stats.Children.Add(lblTime);
        stats.Children.Add(TimeLabel);
        var lblFlags = new Label { Text = "Flags:", VerticalOptions = LayoutOptions.Center };
        lblFlags.SetDynamicResource(Label.TextColorProperty, "AppText");
        stats.Children.Add(lblFlags);
        stats.Children.Add(FlagsLabel);

        var flagRow = new StackLayout { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.Center };
        flagRow.Children.Add(FlagModeButton);

        var main = new StackLayout { Padding = 10, Spacing = 10 };
        // make main layout background follow theme as well
        main.SetDynamicResource(BackgroundColorProperty, "AppBackground");
        main.Children.Add(topRow);
        main.Children.Add(stats);
        main.Children.Add(new ScrollView { Content = BoardGrid });
        main.Children.Add(flagRow);

        Content = main;

        // Wire events
        ThemePicker.Items.Add("Light");
        ThemePicker.Items.Add("Dark");
        ThemePicker.Items.Add("Colorful");
        ThemePicker.SelectedIndex = 0;
        ThemePicker.SelectedIndexChanged += ThemePicker_SelectedIndexChanged;

        NewGameButton.Clicked += NewGameButton_Clicked;
        FlagModeButton.Clicked += (s, e) => { _flagMode = !_flagMode; FlagModeButton.Text = _flagMode ? "Flag: ON" : "Flag: OFF"; };

        StartNewGame();
    }

    private void ThemePicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        Theme theme = ThemePicker.SelectedIndex switch
        {
            1 => new Theme("Dark", Colors.Black, Colors.White, "Segoe UI"),
            2 => new Theme("Colorful", Colors.LightGreen, Colors.DarkBlue, "Comic Sans MS"),
            _ => new Theme("Light", Colors.White, Colors.Black, "Segoe UI"),
        };
        App.SetTheme(theme);
    }

    private void NewGameButton_Clicked(object? sender, EventArgs e) => StartNewGame();

    private void StartNewGame()
    {
        _game = new Game(_rows, _cols, _mines);
        _game.TileUpdated += OnTileUpdated;
        _game.GameWon += OnGameWon;
        _game.GameLost += OnGameLost;
        _game.TimerTick += OnTimerTick;

        BuildGrid();

        MinesLabel.Text = _mines.ToString();
        FlagsLabel.Text = "0";
        TimeLabel.Text = "0s";
    }

    private void OnTimerTick()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            TimeLabel.Text = $"{(int)_game.Elapsed.TotalSeconds}s";
        });
    }

    private void BuildGrid()
    {
        BoardGrid.RowDefinitions.Clear();
        BoardGrid.ColumnDefinitions.Clear();
        BoardGrid.Children.Clear();

        for (int r = 0; r < _rows; r++)
            BoardGrid.RowDefinitions.Add(new RowDefinition { Height = 40 });
        for (int c = 0; c < _cols; c++)
            BoardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 40 });

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                var btn = new Button
                {
                    BackgroundColor = Colors.LightGray,
                    FontSize = 14,
                    CornerRadius = 4,
                    Padding = 0,
                    Text = "",
                };
                // apply dynamic font resource so theme changes update font
                btn.SetDynamicResource(Button.FontFamilyProperty, "AppFont");
                int rr = r, cc = c;
                btn.BindingContext = (rr, cc); // store coordinates
                btn.Clicked += (s, e) => OnTileClicked(rr, cc, btn);
                BoardGrid.Add(btn, c, r);
            }
        }
    }

    private void OnTileClicked(int row, int col, Button btn)
    {
        if (_flagMode)
        {
            _game.ToggleFlag(row, col);
            UpdateButtonForTile(row, col, btn);
            FlagsLabel.Text = _game.FlagsPlaced.ToString();
            return;
        }

        _game.Reveal(row, col);
    }

    private void OnTileUpdated(Tile tile, int row, int col)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var btn = BoardGrid.Children.OfType<Button>().FirstOrDefault(b =>
            {
                if (b.BindingContext is ValueTuple<int, int> pos)
                    return pos.Item1 == row && pos.Item2 == col;
                return false;
            });
            if (btn != null) UpdateButtonForTile(row, col, btn);
        });
    }

    private void UpdateButtonForTile(int row, int col, Button btn)
    {
        var tile = _game.Board[row, col];
        if (tile.Flagged)
        {
            btn.Text = "⚑";
            btn.BackgroundColor = Colors.Orange;
        }
        else if (!tile.Revealed)
        {
            btn.Text = "";
            btn.BackgroundColor = Colors.LightGray;
        }
        else
        {
            if (tile.HasMine)
            {
                btn.Text = "💣";
                btn.BackgroundColor = Colors.Red;
                _ = PlayExplosionAnimation(btn);
            }
            else if (tile.AdjacentMines > 0)
            {
                btn.Text = tile.AdjacentMines.ToString();
                btn.BackgroundColor = Colors.White;
                // use dynamic text color so theme changes update numbers
                btn.SetDynamicResource(Button.TextColorProperty, "AppText");
            }
            else
            {
                btn.Text = "";
                btn.BackgroundColor = Colors.White;
            }
        }
    }

    private async Task PlayExplosionAnimation(Button btn)
    {
        if (btn == null) return;
        try
        {
            btn.AnchorX = 0.5;
            btn.AnchorY = 0.5;
            await Task.WhenAll(btn.ScaleToAsync(1.6, 180), btn.RotateToAsync(18, 180));
            await btn.RotateToAsync(-18, 140);
            await btn.RotateToAsync(0, 140);

            btn.Text = "💥";
            btn.TextColor = Colors.White;
            await Task.WhenAll(btn.ScaleToAsync(1.3, 150), btn.FadeToAsync(0.25, 350));
            await Task.Delay(300);
            await btn.FadeToAsync(1, 200);
            await btn.ScaleToAsync(1, 120);

            btn.BackgroundColor = Colors.DarkRed;
        }
        catch { }
    }

    private async void OnGameWon()
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            int seconds = (int)_game.Elapsed.TotalSeconds;
            string msg = $"You won in {seconds}s!";
            SaveScore(seconds);
            await DisplayAlertAsync("Victory", msg, "OK");
        });
    }

    private async void OnGameLost()
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await DisplayAlertAsync("Game Over", "You hit a mine.", "OK");
        });
    }

    private void SaveScore(int seconds)
    {
        try
        {
            var best = Preferences.Get("best_time", int.MaxValue);
            if (seconds < best) Preferences.Set("best_time", seconds);
        }
        catch { }
    }
}