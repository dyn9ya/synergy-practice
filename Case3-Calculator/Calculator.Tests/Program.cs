using System;
using Calculator;

namespace Calculator.Tests
{
    // проверка калькулятора на разных числах и операциях (пункт 7)
    // запуск: dotnet run --project Calculator.Tests
    internal static class Program
    {
        private static int _passed;
        private static int _failed;

        private static int Main()
        {
            Check("Сложение 2 + 3 = 5", "2+3=", "5");
            Check("Вычитание 10 - 4 = 6", "10-4=", "6");
            Check("Умножение 7 * 6 = 42", "7*6=", "42");
            Check("Деление 9 / 4 = 2.25", "9/4=", "2.25");
            Check("Последовательные операции 2 + 3 * 4 = 20", "2+3*4=", "20");
            Check("Результат используется дальше: 5 + 5 = , * 3 =", "5+5=*3=", "30");
            Check("Повтор '=' повторяет операцию: 2 + 3 = =", "2+3==", "8");
            Check("Дробные числа 0.1 + 0.2 = 0.3", "0.1+0.2=", "0.3");
            Check("Отрицательный результат 3 - 8 = -5", "3-8=", "-5");
            Check("Деление на ноль даёт ошибку", "5/0=", CalculatorEngine.DivideByZeroMessage);
            Check("После ошибки ввод цифры начинает заново", "5/0=7+1=", "8");
            Check("Кнопка C очищает", "123C", "0");
            Check("Вторая точка в числе игнорируется", "1..5", "1.5");
            Check("Ведущий ноль не дублируется", "0007", "7");
            Check("Смена операции до ввода второго числа", "6+*2=", "12");
            Check("Ограничение длины ввода 16 цифр", "12345678901234567890", "1234567890123456");
            Check("Переполнение обрабатывается", "9999999999999999*9999999999999999=", CalculatorEngine.OverflowMessage);

            var engine = new CalculatorEngine();
            Run(engine, "2+3=");
            Run(engine, "*4=");
            Assert("История сохраняет вычисления", engine.History.Count == 2 && engine.History[0] == "2 + 3 = 5" && engine.History[1] == "5 × 4 = 20");
            Assert("Загрузка числа из истории", engine.LoadValue(" 20 ") && engine.Display == "20");
            Assert("Некорректное значение при загрузке даёт ошибку", !engine.LoadValue("abc") && engine.HasError);

            Console.WriteLine();
            Console.WriteLine("Пройдено: " + _passed + ", ошибок: " + _failed);
            return _failed == 0 ? 0 : 1;
        }

        private static void Check(string name, string keys, string expected)
        {
            var engine = new CalculatorEngine();
            Run(engine, keys);
            bool ok = engine.Display == expected;
            Assert(name + (ok ? "" : " (получено: " + engine.Display + ")"), ok);
        }

        private static void Run(CalculatorEngine engine, string keys)
        {
            foreach (char k in keys)
            {
                if (char.IsDigit(k)) engine.InputDigit(k);
                else if (k == '.') engine.InputDecimalPoint();
                else if (k == '=') engine.Calculate();
                else if (k == 'C') engine.Clear();
                else engine.SetOperation(k);
            }
        }

        private static void Assert(string name, bool condition)
        {
            if (condition) { _passed++; Console.WriteLine("[OK]   " + name); }
            else { _failed++; Console.WriteLine("[FAIL] " + name); }
        }
    }
}
