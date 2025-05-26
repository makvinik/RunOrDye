using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RunOrDye
{
    public partial class Form1 : Form
    {
        private Player player;
        private List<Monster> monsters;
        private int[,] grid;
        private Random random;
        private int cellSize = 30;
        private int gridWidth = 20;
        private int gridHeight = 15;
        private bool gameOver = false;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.ClientSize = new Size(gridWidth * cellSize, gridHeight * cellSize);
            this.Text = "Убеги от монстров!";
            random = new Random();
            InitializeGame();
        }

        private void InitializeGame()
        {
            grid = new int[gridWidth, gridHeight];
            for (int i = 0; i < 50; i++)
            {
                int x = random.Next(gridWidth);
                int y = random.Next(gridHeight);
                grid[x, y] = 1;
            }

            Point playerPos;
            do
            {
                playerPos = new Point(random.Next(gridWidth), random.Next(gridHeight));
            } while (grid[playerPos.X, playerPos.Y] == 1);
            player = new Player(playerPos);

            monsters = new List<Monster>();
            for (int i = 0; i < 3; i++)
            {
                Point monsterPos;
                do
                {
                    monsterPos = new Point(random.Next(gridWidth), random.Next(gridHeight));
                } while (grid[monsterPos.X, monsterPos.Y] == 1 ||
                         monsterPos == player.Position ||
                         monsters.Exists(m => m.Position == monsterPos));
                monsters.Add(new Monster(monsterPos));
            }
            gameOver = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Rectangle rect = new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize);
                    e.Graphics.FillRectangle(grid[x, y] == 1 ? Brushes.Brown : Brushes.LightGray, rect);
                    e.Graphics.DrawRectangle(Pens.Black, rect);
                }
            }

            Rectangle playerRect = new Rectangle(player.Position.X * cellSize, player.Position.Y * cellSize, cellSize, cellSize);
            e.Graphics.FillEllipse(Brushes.Green, playerRect);

            foreach (var monster in monsters)
            {
                Rectangle monsterRect = new Rectangle(monster.Position.X * cellSize, monster.Position.Y * cellSize, cellSize, cellSize);
                e.Graphics.FillEllipse(Brushes.Red, monsterRect);
            }

            if (gameOver)
            {
                string gameOverText = "Игра окончена! Нажмите R для рестарта";
                SizeF textSize = e.Graphics.MeasureString(gameOverText, this.Font);
                e.Graphics.DrawString(gameOverText, this.Font, Brushes.Black,
                    (this.Width - textSize.Width) / 2,
                    (this.Height - textSize.Height) / 2);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (gameOver && e.KeyCode == Keys.R)
            {
                InitializeGame();
                this.Invalidate();
                return;
            }

            Point newPosition = player.Position;
            switch (e.KeyCode)
            {
                case Keys.Up: newPosition.Y--; break;
                case Keys.Down: newPosition.Y++; break;
                case Keys.Left: newPosition.X--; break;
                case Keys.Right: newPosition.X++; break;
            }

            if (newPosition.X >= 0 && newPosition.X < gridWidth &&
                newPosition.Y >= 0 && newPosition.Y < gridHeight &&
                grid[newPosition.X, newPosition.Y] == 0)
            {
                player.Position = newPosition;
            }

            foreach (var monster in monsters)
            {
                Point nextStep = FindNextStepDijkstra(monster.Position, player.Position);
                monster.Position = nextStep;
            }

            foreach (var monster in monsters)
            {
                if (monster.Position == player.Position)
                {
                    gameOver = true;
                    break;
                }
            }
            this.Invalidate();
        }

        private Point FindNextStepDijkstra(Point start, Point target)
        {
            Dictionary<Point, int> distances = new Dictionary<Point, int>();
            Dictionary<Point, Point> previous = new Dictionary<Point, Point>();
            List<Point> unvisited = new List<Point>();

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y] == 0)
                    {
                        Point p = new Point(x, y);
                        distances[p] = int.MaxValue;
                        unvisited.Add(p);
                    }
                }
            }
            distances[start] = 0;

            while (unvisited.Count > 0)
            {
                Point current = unvisited[0];
                foreach (Point p in unvisited)
                {
                    if (distances[p] < distances[current]) current = p;
                }
                if (current == target) break;
                unvisited.Remove(current);

                foreach (Point neighbor in GetNeighbors(current))
                {
                    if (!unvisited.Contains(neighbor)) continue;
                    int alt = distances[current] + 1;
                    if (alt < distances[neighbor])
                    {
                        distances[neighbor] = alt;
                        previous[neighbor] = current;
                    }
                }
            }

            if (!previous.ContainsKey(target)) return start;
            Point step = target;
            while (previous.ContainsKey(step) && previous[step] != start) step = previous[step];
            return step;
        }

        private List<Point> GetNeighbors(Point p)
        {
            List<Point> neighbors = new List<Point>();
            if (p.X > 0 && grid[p.X - 1, p.Y] == 0) neighbors.Add(new Point(p.X - 1, p.Y));
            if (p.X < gridWidth - 1 && grid[p.X + 1, p.Y] == 0) neighbors.Add(new Point(p.X + 1, p.Y));
            if (p.Y > 0 && grid[p.X, p.Y - 1] == 0) neighbors.Add(new Point(p.X, p.Y - 1));
            if (p.Y < gridHeight - 1 && grid[p.X, p.Y + 1] == 0) neighbors.Add(new Point(p.X, p.Y + 1));
            return neighbors;
        }
    }

    public class Player
    {
        public Point Position { get; set; }
        public Player(Point position) => Position = position;
    }

    public class Monster
    {
        public Point Position { get; set; }
        public Monster(Point position) => Position = position;
    }
}