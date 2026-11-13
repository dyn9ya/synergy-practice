using System;
using System.Text;

namespace Tetris
{
    // Вывод поля в консоль. [] - блок, " ." - пустая клетка
    public static class Renderer
    {
        private const string Block = "[]";
        private const string Empty = " .";

        // собирает кадр в строку (отдельно от вывода, так проще проверять)
        public static string BuildFrame(Game game)
        {
            var field = game.Field;
            var cur = game.Current;
            var sb = new StringBuilder();
            string[] side = BuildSidePanel(game);

            // верхняя граница
            sb.Append('┌').Append('─', field.Cols * 2).Append('┐').AppendLine();

            for (int r = 0; r < field.Rows; r++)
            {
                sb.Append('│');
                for (int c = 0; c < field.Cols; c++)
                {
                    bool filled = field.Cells[r, c] == 1;
                    if (!filled && cur != null && !game.IsGameOver)
                    {
                        int fr = r - cur.Row, fc = c - cur.Col;
                        if (fr >= 0 && fr < cur.Height && fc >= 0 && fc < cur.Width && cur.IsBlock(fr, fc))
                            filled = true;
                    }
                    sb.Append(filled ? Block : Empty);
                }
                sb.Append('│');
                string text = r < side.Length ? side[r] : "";
                sb.Append("   ").Append(text.PadRight(26)).AppendLine();
            }
            // нижняя граница
            sb.Append('└').Append('─', field.Cols * 2).Append('┘').AppendLine();
            return sb.ToString();
        }

        private static string[] BuildSidePanel(Game game)
        {
            var lines = new string[12];
            lines[0] = "ТЕТРИС";
            lines[1] = "";
            lines[2] = "Очки:   " + game.Score;
            lines[3] = "Линии:  " + game.Lines;
            lines[4] = "Уровень: " + game.Level;
            lines[5] = "";
            lines[6] = "Следующая:";
            for (int i = 0; i < 2; i++)
            {
                var sb = new StringBuilder("  ");
                var next = game.Next;
                if (next != null && i < next.Height)
                    for (int c = 0; c < next.Width; c++) sb.Append(next.IsBlock(i, c) ? Block : "  ");
                lines[7 + i] = sb.ToString();
            }
            lines[9] = "";
            lines[10] = game.IsPaused ? "ПАУЗА (P - продолжить)" : "";
            lines[11] = "";
            return lines;
        }

        public static void Draw(Game game)
        {
            Console.SetCursorPosition(0, 0);
            Console.Write(BuildFrame(game));
            Console.WriteLine();
            Console.WriteLine("Управление: <- -> или A/D - двигать, вверх/W - повернуть,");
            Console.WriteLine("вниз/S - быстрее, пробел - уронить, P - пауза, Esc - выход");
        }
    }
}
