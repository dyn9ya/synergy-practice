namespace Tetris
{
    // Поле игры. Cells[r, c] = 0 если клетка пустая, 1 если занята
    public class GameField
    {
        public int Rows { get; private set; }
        public int Cols { get; private set; }
        public int[,] Cells { get; private set; }

        public GameField(int rows = 20, int cols = 10)
        {
            Rows = rows;
            Cols = cols;
            Cells = new int[rows, cols];
        }

        public void Clear()
        {
            Cells = new int[Rows, Cols];
        }

        // можно ли поставить фигуру с такой формой в позицию (row, col)
        public bool CanPlace(int[,] shape, int row, int col)
        {
            for (int r = 0; r < shape.GetLength(0); r++)
            {
                for (int c = 0; c < shape.GetLength(1); c++)
                {
                    if (shape[r, c] == 0) continue;
                    int fr = row + r;
                    int fc = col + c;
                    if (fc < 0 || fc >= Cols || fr >= Rows) return false;
                    if (fr < 0) continue; // выше поля можно
                    if (Cells[fr, fc] == 1) return false;
                }
            }
            return true;
        }

        public bool CanPlace(Figure figure)
        {
            return CanPlace(figure.Shape, figure.Row, figure.Col);
        }

        // ===== движение и вращение =====

        public bool MoveLeft(Figure figure) { return TryMove(figure, 0, -1); }
        public bool MoveRight(Figure figure) { return TryMove(figure, 0, 1); }
        public bool MoveDown(Figure figure) { return TryMove(figure, 1, 0); }

        public bool TryMove(Figure figure, int dRow, int dCol)
        {
            if (!CanPlace(figure.Shape, figure.Row + dRow, figure.Col + dCol)) return false;
            figure.Row += dRow;
            figure.Col += dCol;
            return true;
        }

        public bool Rotate(Figure figure)
        {
            var rotated = figure.GetRotatedShape();
            // если у стенки не влезает, пробуем чуть сдвинуть
            int[] shifts = { 0, -1, 1, -2, 2, -3, 3 };
            foreach (int s in shifts)
            {
                if (CanPlace(rotated, figure.Row, figure.Col + s))
                {
                    figure.SetShape(rotated);
                    figure.Col += s;
                    return true;
                }
            }
            return false;
        }

        // уронить вниз до упора, возвращает сколько клеток пролетела
        public int Drop(Figure figure)
        {
            int n = 0;
            while (MoveDown(figure)) n++;
            return n;
        }

        // записать фигуру в матрицу поля.
        // возвращает false если какой-то блок остался выше поля (значит конец игры)
        public bool Lock(Figure figure)
        {
            bool inside = true;
            for (int r = 0; r < figure.Height; r++)
            {
                for (int c = 0; c < figure.Width; c++)
                {
                    if (!figure.IsBlock(r, c)) continue;
                    int fr = figure.Row + r;
                    int fc = figure.Col + c;
                    if (fr < 0) { inside = false; continue; }
                    Cells[fr, fc] = 1;
                }
            }
            return inside;
        }

        // убирает заполненные строки, возвращает их количество
        public int ClearFullLines()
        {
            int cleared = 0;
            for (int r = Rows - 1; r >= 0; r--)
            {
                if (!IsLineFull(r)) continue;
                for (int y = r; y > 0; y--)
                    for (int c = 0; c < Cols; c++)
                        Cells[y, c] = Cells[y - 1, c];
                for (int c = 0; c < Cols; c++) Cells[0, c] = 0;
                cleared++;
                r++; // проверяем эту же строку еще раз, туда съехала верхняя
            }
            return cleared;
        }

        private bool IsLineFull(int r)
        {
            for (int c = 0; c < Cols; c++)
                if (Cells[r, c] == 0) return false;
            return true;
        }
    }
}
