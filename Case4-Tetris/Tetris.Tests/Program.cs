using System;
using Tetris;

namespace Tetris.Tests
{
    // простые проверки логики тетриса без консоли
    // запуск: dotnet run --project Tetris.Tests
    internal static class Program
    {
        private static int _passed;
        private static int _failed;

        private static int Main()
        {
            // поворот фигуры
            var t = Figure.Create('T');
            var rotated = t.GetRotatedShape();
            Assert("Поворот T меняет размер 2x3 на 3x2", rotated.GetLength(0) == 3 && rotated.GetLength(1) == 2);
            var i = Figure.Create('I');
            var r1 = new Figure(i.GetRotatedShape(), 'I');
            var r2 = new Figure(r1.GetRotatedShape(), 'I');
            Assert("I после двух поворотов снова горизонтальная", r2.Height == 1 && r2.Width == 4);

            // движение и стенки
            var field = new GameField(20, 10);
            var o = Figure.Create('O');
            o.Row = 0; o.Col = 0;
            Assert("Нельзя уйти за левую стенку", !field.MoveLeft(o) && o.Col == 0);
            o.Col = 8;
            Assert("Нельзя уйти за правую стенку", !field.MoveRight(o) && o.Col == 8);
            o.Col = 4;
            int fell = field.Drop(o);
            Assert("Фигура падает до дна", fell == 18 && o.Row == 18);
            field.Lock(o);
            Assert("После фиксации клетки поля заняты", field.Cells[19, 4] == 1 && field.Cells[18, 5] == 1);

            // поворот у стенки со сдвигом
            var iv = new Figure(Figure.Create('I').GetRotatedShape(), 'I'); // вертикальная
            iv.Row = 5; iv.Col = 9;
            Assert("Поворот палки у правой стенки сдвигает ее внутрь", field.Rotate(iv) && iv.Col + iv.Width <= 10);

            // очистка линий
            var f2 = new GameField(4, 4);
            for (int c = 0; c < 4; c++) { f2.Cells[3, c] = 1; f2.Cells[2, c] = 1; }
            f2.Cells[1, 0] = 1;
            int cleared = f2.ClearFullLines();
            Assert("Убираются 2 полные линии", cleared == 2);
            Assert("Верхние блоки съезжают вниз", f2.Cells[3, 0] == 1 && f2.Cells[3, 1] == 0 && f2.Cells[1, 0] == 0);

            // игра: очки за линию
            var game = new Game(20, 4, seed: 1);
            for (int c = 0; c < 4; c++) game.Field.Cells[19, c] = 1;
            game.Field.Cells[19, 0] = 0;
            var stick = new Figure(Figure.Create('I').GetRotatedShape(), 'I');
            stick.Row = 0; stick.Col = 0;
            game.SetCurrent(stick);
            game.HandleAction(PlayerAction.HardDrop);
            Assert("За собранную линию начисляются очки", game.Lines == 1 && game.Score >= 100);

            // конец игры, когда фигуры дошли до верха
            var g2 = new Game(6, 4, seed: 2);
            int steps = 0;
            while (!g2.IsGameOver && steps < 1000) { g2.HandleAction(PlayerAction.HardDrop); steps++; }
            Assert("Игра заканчивается, когда поле заполнено до верха", g2.IsGameOver);
            g2.Reset();
            Assert("После Reset можно играть заново", !g2.IsGameOver && g2.Score == 0 && g2.Current != null);

            // пауза
            var g3 = new Game(seed: 3);
            int row = g3.Current.Row;
            g3.HandleAction(PlayerAction.Pause);
            g3.Tick();
            Assert("На паузе фигура не падает", g3.Current.Row == row);

            // отрисовка
            string frame = Renderer.BuildFrame(new Game(seed: 4));
            Assert("Кадр рисует 20 строк поля и рамку", frame.Split('\n').Length >= 22 && frame.Contains("│") && frame.Contains("└") && frame.Contains("[]"));
            Console.WriteLine();
            Console.WriteLine(frame);

            Console.WriteLine("Пройдено: " + _passed + ", ошибок: " + _failed);
            return _failed == 0 ? 0 : 1;
        }

        private static void Assert(string name, bool condition)
        {
            if (condition) { _passed++; Console.WriteLine("[OK]   " + name); }
            else { _failed++; Console.WriteLine("[FAIL] " + name); }
        }
    }
}
