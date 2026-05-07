using System;
using System.Collections.Generic;
using System.Timers;

namespace BugSweeper.Models
{
    public class Game
    {
        public int Rows { get; }
        public int Columns { get; }
        public int Mines { get; }
        public Tile[,] Board { get; private set; }
        public bool IsRunning { get; private set; }
        public int RevealedCount { get; private set; }
        public int FlagsPlaced { get; private set; }
        public TimeSpan Elapsed => TimeSpan.FromMilliseconds(_elapsedMs);

        private int _elapsedMs = 0;
        private System.Timers.Timer _timer;

        public event Action<Tile, int, int>? TileUpdated;
        public event Action? GameWon;
        public event Action? GameLost;
        public event Action? TimerTick;

        public Game(int rows, int columns, int mines)
        {
            Rows = rows;
            Columns = columns;
            Mines = mines;
            Board = new Tile[rows, columns];
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (s, e) => { _elapsedMs += 1000; TimerTick?.Invoke(); };
            Reset();
        }

        public void Reset()
        {
            RevealedCount = 0;
            FlagsPlaced = 0;
            _elapsedMs = 0;
            IsRunning = false;
            _timer.Stop();

            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Columns; c++)
                    Board[r, c] = new Tile();
        }

        public void Start()
        {
            if (IsRunning) return;
            PlaceMines();
            CalculateAdjacency();
            IsRunning = true;
            _timer.Start();
        }

        private void PlaceMines()
        {
            var rand = new Random();
            int placed = 0;
            while (placed < Mines)
            {
                int r = rand.Next(Rows);
                int c = rand.Next(Columns);
                if (!Board[r, c].HasMine)
                {
                    Board[r, c].HasMine = true;
                    placed++;
                }
            }
        }

        private void CalculateAdjacency()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    if (Board[r, c].HasMine) continue;
                    int count = 0;
                    foreach (var n in GetNeighbors(r, c)) if (n.t.HasMine) count++;
                    Board[r, c].AdjacentMines = count;
                }
            }
        }

        private IEnumerable<(int r, int c, Tile t)> GetNeighbors(int row, int col)
        {
            for (int dr = -1; dr <= 1; dr++)
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    int nr = row + dr, nc = col + dc;
                    if (nr >= 0 && nr < Rows && nc >= 0 && nc < Columns)
                        yield return (nr, nc, Board[nr, nc]);
                }
        }

        public void Reveal(int row, int col)
        {
            if (!IsRunning) Start();
            var tile = Board[row, col];
            if (tile.Revealed || tile.Flagged) return;
            tile.Revealed = true;
            RevealedCount++;
            TileUpdated?.Invoke(tile, row, col);

            if (tile.HasMine)
            {
                // lost
                End(false);
                GameLost?.Invoke();
                return;
            }

            if (tile.AdjacentMines == 0)
            {
                // flood fill
                foreach (var (r, c, t) in GetNeighbors(row, col))
                {
                    if (!t.Revealed && !t.Flagged) Reveal(r, c);
                }
            }

            CheckWin();
        }

        public void ToggleFlag(int row, int col)
        {
            var tile = Board[row, col];
            if (tile.Revealed) return;
            tile.Flagged = !tile.Flagged;
            FlagsPlaced += tile.Flagged ? 1 : -1;
            TileUpdated?.Invoke(tile, row, col);
        }

        private void CheckWin()
        {
            int total = Rows * Columns;
            if (RevealedCount + Mines == total)
            {
                End(true);
                GameWon?.Invoke();
            }
        }

        private void End(bool won)
        {
            IsRunning = false;
            _timer.Stop();
        }
    }
}