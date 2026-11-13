using System;
using System.Diagnostics;
using System.Threading;

namespace Tetris
{
    internal static class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Тетрис";
            try { Console.CursorVisible = false; } catch { }

            var game = new Game();
            bool playAgain = true;

            while (playAgain)
            {
                Console.Clear();
                bool quit = RunGame(game);
                if (quit) break;

                // конец игры, предлагаем начать заново
                Renderer.Draw(game);
                Console.WriteLine();
                Console.WriteLine("ИГРА ОКОНЧЕНА! Фигуры достигли верха поля. Очки: " + game.Score);
                Console.Write("Начать заново? (Y - да, N - нет): ");
                playAgain = AskYesNo();
                if (playAgain) game.Reset();
            }

            try { Console.CursorVisible = true; } catch { }
            Console.WriteLine();
            Console.WriteLine("Спасибо за игру!");
        }

        // основной цикл игры. возвращает true если игрок нажал Esc
        private static bool RunGame(Game game)
        {
            var timer = Stopwatch.StartNew();
            while (!game.IsGameOver)
            {
                while (Console.KeyAvailable)
                {
                    var action = ReadAction(Console.ReadKey(true).Key);
                    if (action == PlayerAction.Quit) return true;
                    game.HandleAction(action);
                }

                if (timer.ElapsedMilliseconds >= game.FallInterval)
                {
                    game.Tick();
                    timer.Restart();
                }

                Renderer.Draw(game);
                Thread.Sleep(20);
            }
            return false;
        }

        private static PlayerAction ReadAction(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A: return PlayerAction.Left;
                case ConsoleKey.RightArrow:
                case ConsoleKey.D: return PlayerAction.Right;
                case ConsoleKey.UpArrow:
                case ConsoleKey.W: return PlayerAction.Rotate;
                case ConsoleKey.DownArrow:
                case ConsoleKey.S: return PlayerAction.SoftDrop;
                case ConsoleKey.Spacebar: return PlayerAction.HardDrop;
                case ConsoleKey.P: return PlayerAction.Pause;
                case ConsoleKey.Escape: return PlayerAction.Quit;
                default: return PlayerAction.None;
            }
        }

        private static bool AskYesNo()
        {
            while (true)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Y) return true;
                if (key == ConsoleKey.N || key == ConsoleKey.Escape) return false;
            }
        }
    }
}
