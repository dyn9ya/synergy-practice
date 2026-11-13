using System;

namespace Tetris
{
    public enum PlayerAction
    {
        None,
        Left,
        Right,
        Rotate,
        SoftDrop,   // ускорить падение на 1 клетку
        HardDrop,   // уронить сразу вниз
        Pause,
        Quit
    }

    // Состояние одной игры: поле, текущая и следующая фигура, очки
    public class Game
    {
        private readonly Random _random;

        public GameField Field { get; private set; }
        public Figure Current { get; private set; }
        public Figure Next { get; private set; }
        public int Score { get; private set; }
        public int Lines { get; private set; }
        public int Level { get { return Lines / 10 + 1; } }
        public bool IsGameOver { get; private set; }
        public bool IsPaused { get; private set; }

        public Game(int rows = 20, int cols = 10, int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
            Field = new GameField(rows, cols);
            Reset();
        }

        public void Reset()
        {
            Field.Clear();
            Score = 0;
            Lines = 0;
            IsGameOver = false;
            IsPaused = false;
            Next = Figure.CreateRandom(_random);
            SpawnNext();
        }

        // скорость падения в мс, с уровнем растет
        public int FallInterval
        {
            get { return Math.Max(100, 600 - (Level - 1) * 50); }
        }

        // новая фигура появляется сверху по центру
        private void SpawnNext()
        {
            Current = Next;
            Current.Row = 0;
            Current.Col = (Field.Cols - Current.Width) / 2;
            Next = Figure.CreateRandom(_random);

            // если новой фигуре уже некуда встать, значит фигуры дошли до верха
            if (!Field.CanPlace(Current)) IsGameOver = true;
        }

        public void HandleAction(PlayerAction action)
        {
            if (IsGameOver) return;
            if (action == PlayerAction.Pause) { IsPaused = !IsPaused; return; }
            if (IsPaused) return;

            switch (action)
            {
                case PlayerAction.Left: Field.MoveLeft(Current); break;
                case PlayerAction.Right: Field.MoveRight(Current); break;
                case PlayerAction.Rotate: Field.Rotate(Current); break;
                case PlayerAction.SoftDrop:
                    if (Field.MoveDown(Current)) Score += 1;
                    else LockCurrent();
                    break;
                case PlayerAction.HardDrop:
                    Score += Field.Drop(Current) * 2;
                    LockCurrent();
                    break;
            }
        }

        // один шаг падения по таймеру
        public void Tick()
        {
            if (IsGameOver || IsPaused) return;
            if (!Field.MoveDown(Current)) LockCurrent();
        }

        private void LockCurrent()
        {
            bool inside = Field.Lock(Current);
            if (!inside)
            {
                IsGameOver = true;
                return;
            }

            int cleared = Field.ClearFullLines();
            if (cleared > 0)
            {
                int[] points = { 0, 100, 300, 500, 800 };
                Score += points[Math.Min(cleared, 4)] * Level;
                Lines += cleared;
            }
            SpawnNext();
        }

        // для тестов: поставить конкретную фигуру
        public void SetCurrent(Figure figure)
        {
            Current = figure;
        }
    }
}
