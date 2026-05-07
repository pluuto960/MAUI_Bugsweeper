namespace BugSweeper.Models
{
    public class Tile
    {
        public bool HasMine { get; set; }
        public bool Revealed { get; set; }
        public bool Flagged { get; set; }
        public int AdjacentMines { get; set; }

        public Tile()
        {
            HasMine = false;
            Revealed = false;
            Flagged = false;
            AdjacentMines = 0;
        }
    }
}