using System;

namespace Tetris
{
    // Фигура тетриса. Форма хранится как матрица битов: 1 - блок есть, 0 - пусто
    public class Figure
    {
        private static readonly int[][,] Shapes =
        {
            new int[,] { { 1, 1, 1, 1 } },              // I
            new int[,] { { 1, 1 }, { 1, 1 } },          // O
            new int[,] { { 0, 1, 0 }, { 1, 1, 1 } },    // T
            new int[,] { { 0, 1, 1 }, { 1, 1, 0 } },    // S
            new int[,] { { 1, 1, 0 }, { 0, 1, 1 } },    // Z
            new int[,] { { 1, 0, 0 }, { 1, 1, 1 } },    // J
            new int[,] { { 0, 0, 1 }, { 1, 1, 1 } }     // L
        };

        private static readonly char[] Names = { 'I', 'O', 'T', 'S', 'Z', 'J', 'L' };

        public int[,] Shape { get; private set; }
        public char Name { get; private set; }

        // позиция левого верхнего угла матрицы на поле
        public int Row { get; set; }
        public int Col { get; set; }

        public int Height { get { return Shape.GetLength(0); } }
        public int Width { get { return Shape.GetLength(1); } }

        public Figure(int[,] shape, char name)
        {
            Shape = (int[,])shape.Clone();
            Name = name;
        }

        public static Figure CreateRandom(Random random)
        {
            int i = random.Next(Shapes.Length);
            return new Figure(Shapes[i], Names[i]);
        }

        public static Figure Create(char name)
        {
            int i = Array.IndexOf(Names, name);
            if (i < 0) throw new ArgumentException("Нет такой фигуры: " + name);
            return new Figure(Shapes[i], Names[i]);
        }

        public bool IsBlock(int r, int c)
        {
            return Shape[r, c] == 1;
        }

        // поворот по часовой стрелке, возвращает новую матрицу (саму фигуру не меняет)
        public int[,] GetRotatedShape()
        {
            int h = Height, w = Width;
            var result = new int[w, h];
            for (int r = 0; r < h; r++)
                for (int c = 0; c < w; c++)
                    result[c, h - 1 - r] = Shape[r, c];
            return result;
        }

        public void SetShape(int[,] shape)
        {
            Shape = shape;
        }

        public Figure Copy()
        {
            var f = new Figure(Shape, Name);
            f.Row = Row;
            f.Col = Col;
            return f;
        }
    }
}
