using System;
using System.Collections.Generic;
using System.Globalization;

namespace Calculator
{
    // логика калькулятора, отдельно от формы (чтобы можно было проверить тестами)
    public class CalculatorEngine
    {
        public const int MaxDigits = 16;
        public const string DivideByZeroMessage = "Ошибка: деление на ноль";
        public const string OverflowMessage = "Ошибка: переполнение";
        public const string InvalidInputMessage = "Ошибка: некорректный ввод";

        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        private decimal? _accumulator;      // результат прошлой операции
        private char? _pendingOperation;    // операция, которая ждет второе число
        private bool _startNewNumber = true; // следующая цифра начнет новое число
        private char? _lastOperation;       // для повторного нажатия =
        private decimal _lastOperand;

        public CalculatorEngine()
        {
            History = new List<string>();
            Display = "0";
        }

        public string Display { get; private set; }

        // строка над дисплеем, например "12 +"
        public string Expression { get; private set; } = "";

        public bool HasError { get; private set; }

        // история вычислений
        public List<string> History { get; private set; }

        public void InputDigit(char digit)
        {
            if (digit < '0' || digit > '9')
            {
                SetError(InvalidInputMessage);
                return;
            }
            if (HasError) Clear();

            if (_startNewNumber)
            {
                Display = digit.ToString();
                _startNewNumber = false;
                return;
            }
            if (CountDigits(Display) >= MaxDigits) return; // не больше 16 цифр
            Display = Display == "0" ? digit.ToString() : Display + digit;
        }

        public void InputDecimalPoint()
        {
            if (HasError) Clear();
            if (_startNewNumber)
            {
                Display = "0.";
                _startNewNumber = false;
                return;
            }
            if (!Display.Contains(".")) Display += ".";
        }

        public void Backspace()
        {
            if (HasError) { Clear(); return; }
            if (_startNewNumber) return;
            Display = Display.Length > 1 ? Display.Substring(0, Display.Length - 1) : "0";
            if (Display == "-") Display = "0";
        }

        public void ToggleSign()
        {
            if (HasError || Display == "0") return;
            Display = Display.StartsWith("-") ? Display.Substring(1) : "-" + Display;
        }

        // нажали + - * /
        public void SetOperation(char op)
        {
            if (!IsOperation(op)) { SetError(InvalidInputMessage); return; }
            if (HasError) return;

            decimal current;
            if (!TryParseDisplay(out current)) return;

            if (_pendingOperation.HasValue && !_startNewNumber)
            {
                // если уже была операция, сначала считаем ее
                decimal result;
                if (!TryCompute(_accumulator.Value, current, _pendingOperation.Value, out result)) return;
                AddHistory(_accumulator.Value, _pendingOperation.Value, current, result);
                _accumulator = result;
                Display = Format(result);
            }
            else if (!_pendingOperation.HasValue || _accumulator == null)
            {
                _accumulator = current;
            }

            _pendingOperation = op;
            _lastOperation = null;
            _startNewNumber = true;
            Expression = Format(_accumulator.Value) + " " + Symbol(op);
        }

        // кнопка =, если нажать еще раз то повторяет последнюю операцию
        public void Calculate()
        {
            if (HasError) return;
            decimal current;
            if (!TryParseDisplay(out current)) return;

            char op;
            decimal left, right;
            if (_pendingOperation.HasValue)
            {
                op = _pendingOperation.Value;
                left = _accumulator ?? 0m;
                right = current;
            }
            else if (_lastOperation.HasValue)
            {
                op = _lastOperation.Value;
                left = current;
                right = _lastOperand;
            }
            else
            {
                return;
            }

            decimal result;
            if (!TryCompute(left, right, op, out result)) return;

            AddHistory(left, op, right, result);
            Expression = Format(left) + " " + Symbol(op) + " " + Format(right) + " =";
            Display = Format(result);
            _lastOperation = op;
            _lastOperand = right;
            _pendingOperation = null;
            _accumulator = null;
            _startNewNumber = true;
        }

        // кнопка C
        public void Clear()
        {
            Display = "0";
            Expression = "";
            _accumulator = null;
            _pendingOperation = null;
            _lastOperation = null;
            _startNewNumber = true;
            HasError = false;
        }

        // подставить число из истории или из файла
        public bool LoadValue(string text)
        {
            decimal value;
            if (!decimal.TryParse((text ?? "").Trim().Replace(',', '.'), NumberStyles.Number, Culture, out value))
            {
                SetError(InvalidInputMessage);
                return false;
            }
            if (HasError) Clear();
            Display = Format(value);
            _startNewNumber = true;
            return true;
        }

        public static bool IsOperation(char op)
        {
            return op == '+' || op == '-' || op == '*' || op == '/';
        }

        public static string Symbol(char op)
        {
            switch (op)
            {
                case '*': return "×";
                case '/': return "÷";
                case '-': return "-";
                default: return "+";
            }
        }

        public static string Format(decimal value)
        {
            // убираем лишние нули, 2.500 -> 2.5
            return value.ToString("0.############", Culture);
        }

        private bool TryCompute(decimal a, decimal b, char op, out decimal result)
        {
            result = 0;
            try
            {
                switch (op)
                {
                    case '+': result = a + b; break;
                    case '-': result = a - b; break;
                    case '*': result = a * b; break;
                    case '/':
                        if (b == 0m)
                        {
                            SetError(DivideByZeroMessage);
                            return false;
                        }
                        result = a / b;
                        break;
                    default:
                        SetError(InvalidInputMessage);
                        return false;
                }
                result = Math.Round(result, 12);
                return true;
            }
            catch (OverflowException)
            {
                SetError(OverflowMessage);
                return false;
            }
        }

        private bool TryParseDisplay(out decimal value)
        {
            if (decimal.TryParse(Display, NumberStyles.Number, Culture, out value)) return true;
            SetError(InvalidInputMessage);
            return false;
        }

        private void SetError(string message)
        {
            HasError = true;
            Display = message;
            Expression = "";
            _accumulator = null;
            _pendingOperation = null;
            _lastOperation = null;
            _startNewNumber = true;
        }

        private void AddHistory(decimal a, char op, decimal b, decimal result)
        {
            History.Add(Format(a) + " " + Symbol(op) + " " + Format(b) + " = " + Format(result));
        }

        private static int CountDigits(string s)
        {
            int n = 0;
            foreach (char c in s) if (char.IsDigit(c)) n++;
            return n;
        }
    }
}
