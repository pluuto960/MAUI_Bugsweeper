namespace BugSweeper.Models
{
    public class Player
    {
        public string Name { get; private set; }
        public int Score { get; private set; }
        public int GamesPlayed { get; private set; }

        public Player(string name)
        {
            Name = name;
            Score = 0;
            GamesPlayed = 0;
        }

        public void AddScore(int points)
        {
            Score += points;
        }

        public void AddGame()
        {
            GamesPlayed++;
        }
    }
}