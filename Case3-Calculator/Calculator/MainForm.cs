using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Calculator
{
    public class MainForm : Form
    {
        private readonly CalculatorEngine _engine = new CalculatorEngine();

        private Label _expressionLabel;
        private TextBox _display;
        private TableLayoutPanel _keypad;
        private ListBox _historyList;
        private Button _saveButton;
        private Button _loadButton;
        private MenuStrip _menu;
        private ToolStripMenuItem _lightThemeItem;
        private ToolStripMenuItem _darkThemeItem;
        private ToolStripMenuItem[] _fontSizeItems;

        private float _fontSize = 14f;
        private bool _darkTheme;

        public MainForm()
        {
            Text = "Калькулятор";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(560, 460);
            MinimumSize = new Size(480, 420);
            KeyPreview = true; // чтобы можно было вводить с клавиатуры

            BuildMenu();
            BuildLayout();
            ApplyTheme();
            ApplyFontSize(_fontSize);
            UpdateDisplay();

            KeyPress += OnFormKeyPress;
            KeyDown += OnFormKeyDown;
            Resize += (s, e) => FitDisplayFont();
        }

        // ===== интерфейс =====

        private void BuildMenu()
        {
            _menu = new MenuStrip();

            var fileMenu = new ToolStripMenuItem("Файл");
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("Сохранить результаты...", null, OnSaveClick, Keys.Control | Keys.S));
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("Загрузить результаты...", null, OnLoadClick, Keys.Control | Keys.O));
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("Очистить историю", null, (s, e) => { _engine.History.Clear(); RefreshHistory(); }));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("Выход", null, (s, e) => Close()));

            var viewMenu = new ToolStripMenuItem("Вид");
            var themeMenu = new ToolStripMenuItem("Цветовая схема");
            _lightThemeItem = new ToolStripMenuItem("Светлая", null, (s, e) => { _darkTheme = false; ApplyTheme(); });
            _darkThemeItem = new ToolStripMenuItem("Тёмная", null, (s, e) => { _darkTheme = true; ApplyTheme(); });
            themeMenu.DropDownItems.Add(_lightThemeItem);
            themeMenu.DropDownItems.Add(_darkThemeItem);

            var fontMenu = new ToolStripMenuItem("Размер шрифта");
            float[] sizes = { 10f, 12f, 14f, 18f, 22f };
            _fontSizeItems = new ToolStripMenuItem[sizes.Length];
            for (int i = 0; i < sizes.Length; i++)
            {
                float size = sizes[i];
                _fontSizeItems[i] = new ToolStripMenuItem(size + " pt", null, (s, e) => ApplyFontSize(size));
                _fontSizeItems[i].Tag = size;
                fontMenu.DropDownItems.Add(_fontSizeItems[i]);
            }

            viewMenu.DropDownItems.Add(themeMenu);
            viewMenu.DropDownItems.Add(fontMenu);

            _menu.Items.Add(fileMenu);
            _menu.Items.Add(viewMenu);
            MainMenuStrip = _menu;
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(8)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));

            // слева дисплей и кнопки
            var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _expressionLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false,
                Height = 24
            };
            _display = new TextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                TabStop = false,
                TextAlign = HorizontalAlignment.Right,
                BorderStyle = BorderStyle.FixedSingle
            };

            _keypad = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 5 };
            for (int c = 0; c < 4; c++) _keypad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            for (int r = 0; r < 5; r++) _keypad.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

            string[,] layout =
            {
                { "C", "⌫", "±", "÷" },
                { "7", "8", "9", "×" },
                { "4", "5", "6", "-" },
                { "1", "2", "3", "+" },
                { "0", ".", "=", "=" }
            };
            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (r == 4 && c == 3) continue; // = на две клетки
                    var button = CreateButton(layout[r, c]);
                    _keypad.Controls.Add(button, c, r);
                    if (r == 4 && c == 2) _keypad.SetColumnSpan(button, 2);
                }
            }

            left.Controls.Add(_expressionLabel, 0, 0);
            left.Controls.Add(_display, 0, 1);
            left.Controls.Add(_keypad, 0, 2);

            // справа история
            var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var historyTitle = new Label { Text = "История", AutoSize = true, Dock = DockStyle.Fill };
            _historyList = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
            _historyList.DoubleClick += OnHistoryDoubleClick;
            new ToolTip().SetToolTip(_historyList, "Двойной щелчок по строке подставляет результат на дисплей");

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
            _saveButton = new Button { Text = "Сохранить", AutoSize = true, TabStop = false, FlatStyle = FlatStyle.Flat };
            _loadButton = new Button { Text = "Загрузить", AutoSize = true, TabStop = false, FlatStyle = FlatStyle.Flat };
            _saveButton.Click += OnSaveClick;
            _loadButton.Click += OnLoadClick;
            buttons.Controls.Add(_saveButton);
            buttons.Controls.Add(_loadButton);

            right.Controls.Add(historyTitle, 0, 0);
            right.Controls.Add(_historyList, 0, 1);
            right.Controls.Add(buttons, 0, 2);

            root.Controls.Add(left, 0, 0);
            root.Controls.Add(right, 1, 0);

            Controls.Add(root);
            Controls.Add(_menu);
        }

        private Button CreateButton(string text)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(3),
                FlatStyle = FlatStyle.Flat,
                TabStop = false
            };
            button.Click += OnButtonClick;
            return button;
        }

        // ===== обработчики кнопок =====

        private void OnButtonClick(object sender, EventArgs e)
        {
            var text = ((Button)sender).Text;
            HandleInput(text);
        }

        private void HandleInput(string key)
        {
            switch (key)
            {
                case "C": _engine.Clear(); break;
                case "⌫": _engine.Backspace(); break;
                case "±": _engine.ToggleSign(); break;
                case ".": _engine.InputDecimalPoint(); break;
                case "=": _engine.Calculate(); break;
                case "+": _engine.SetOperation('+'); break;
                case "-": _engine.SetOperation('-'); break;
                case "×": _engine.SetOperation('*'); break;
                case "÷": _engine.SetOperation('/'); break;
                default:
                    if (key.Length == 1 && char.IsDigit(key[0])) _engine.InputDigit(key[0]);
                    break;
            }
            UpdateDisplay();
        }

        private void OnFormKeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            string key = null;
            if (char.IsDigit(c)) key = c.ToString();
            else if (c == '.' || c == ',') key = ".";
            else if (c == '+') key = "+";
            else if (c == '-') key = "-";
            else if (c == '*') key = "×";
            else if (c == '/') key = "÷";
            else if (c == '=' || c == '\r') key = "=";
            else if (c == '\b') key = "⌫";
            else if (c == (char)27) key = "C";

            if (key != null)
            {
                HandleInput(key);
                e.Handled = true;
            }
        }

        private void OnFormKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                HandleInput("C");
                e.Handled = true;
            }
        }

        private void UpdateDisplay()
        {
            _display.Text = _engine.Display;
            FitDisplayFont();
            _expressionLabel.Text = _engine.Expression;
            _display.ForeColor = _engine.HasError ? Color.IndianRed : (_darkTheme ? Color.White : Color.Black);
            RefreshHistory();
        }

        // если текст не влезает в дисплей, уменьшаем шрифт
        private void FitDisplayFont()
        {
            float size = _fontSize * 1.8f;
            int available = Math.Max(50, _display.ClientSize.Width - 8);
            while (size > 8f)
            {
                using (var font = new Font("Segoe UI", size, FontStyle.Bold))
                {
                    if (TextRenderer.MeasureText(_display.Text, font).Width <= available) break;
                }
                size -= 1f;
            }
            if (_display.Font == null || Math.Abs(_display.Font.Size - size) > 0.1f)
                _display.Font = new Font("Segoe UI", size, FontStyle.Bold);
        }

        private void RefreshHistory()
        {
            if (_historyList.Items.Count == _engine.History.Count) return;
            _historyList.BeginUpdate();
            _historyList.Items.Clear();
            foreach (var line in _engine.History) _historyList.Items.Add(line);
            _historyList.EndUpdate();
            if (_historyList.Items.Count > 0) _historyList.TopIndex = _historyList.Items.Count - 1;
        }

        private void OnHistoryDoubleClick(object sender, EventArgs e)
        {
            var line = _historyList.SelectedItem as string;
            if (line == null) return;
            int eq = line.LastIndexOf('=');
            var value = eq >= 0 ? line.Substring(eq + 1) : line;
            _engine.LoadValue(value);
            UpdateDisplay();
        }

        // ===== сохранение и загрузка =====

        private void OnSaveClick(object sender, EventArgs e)
        {
            if (_engine.History.Count == 0)
            {
                MessageBox.Show(this, "История пустая, сохранять нечего.", "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Текстовый файл (*.txt)|*.txt";
                dialog.FileName = "calc_results_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".txt";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    File.WriteAllLines(dialog.FileName, _engine.History, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Не удалось сохранить файл: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnLoadClick(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Текстовый файл (*.txt)|*.txt|Все файлы (*.*)|*.*";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var lines = File.ReadAllLines(dialog.FileName, Encoding.UTF8)
                        .Where(l => l.Contains("="))
                        .ToList();
                    if (lines.Count == 0)
                    {
                        MessageBox.Show(this, "В файле нет сохранённых результатов.", "Загрузка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    _engine.History.Clear();
                    _engine.History.AddRange(lines);
                    _historyList.Items.Clear();
                    RefreshHistory();
                    // последний результат сразу на дисплей
                    var last = lines[lines.Count - 1];
                    _engine.LoadValue(last.Substring(last.LastIndexOf('=') + 1));
                    UpdateDisplay();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Не удалось загрузить файл: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ===== доп. опция: тема и размер шрифта =====

        private void ApplyTheme()
        {
            Color back = _darkTheme ? Color.FromArgb(32, 32, 36) : Color.FromArgb(243, 243, 243);
            Color fore = _darkTheme ? Color.White : Color.Black;
            Color digit = _darkTheme ? Color.FromArgb(58, 58, 64) : Color.White;
            Color op = _darkTheme ? Color.FromArgb(70, 70, 90) : Color.FromArgb(225, 225, 232);
            Color accent = _darkTheme ? Color.FromArgb(255, 149, 0) : Color.FromArgb(0, 103, 192);

            BackColor = back;
            ForeColor = fore;
            _menu.BackColor = back;
            _menu.ForeColor = fore;
            _display.BackColor = digit;
            _expressionLabel.ForeColor = _darkTheme ? Color.Silver : Color.DimGray;
            _historyList.BackColor = digit;
            _historyList.ForeColor = fore;

            foreach (Control control in _keypad.Controls)
            {
                var button = control as Button;
                if (button == null) continue;
                bool isDigit = button.Text.Length == 1 && (char.IsDigit(button.Text[0]) || button.Text == ".");
                if (button.Text == "=")
                {
                    button.BackColor = accent;
                    button.ForeColor = Color.White;
                }
                else
                {
                    button.BackColor = isDigit ? digit : op;
                    button.ForeColor = fore;
                }
                button.FlatAppearance.BorderColor = back;
            }

            foreach (var b in new[] { _saveButton, _loadButton })
            {
                b.BackColor = op;
                b.ForeColor = fore;
            }

            _lightThemeItem.Checked = !_darkTheme;
            _darkThemeItem.Checked = _darkTheme;
            UpdateDisplayColor();
        }

        private void UpdateDisplayColor()
        {
            if (_display == null) return;
            _display.ForeColor = _engine.HasError ? Color.IndianRed : (_darkTheme ? Color.White : Color.Black);
        }

        private void ApplyFontSize(float size)
        {
            _fontSize = size;
            var baseFont = new Font("Segoe UI", size);
            foreach (Control control in _keypad.Controls) control.Font = baseFont;
            _display.Font = new Font("Segoe UI", size * 1.8f, FontStyle.Bold);
            FitDisplayFont();
            _expressionLabel.Font = new Font("Segoe UI", Math.Max(9f, size * 0.8f));
            _historyList.Font = new Font("Segoe UI", Math.Max(9f, size * 0.8f));
            foreach (var item in _fontSizeItems) item.Checked = (float)item.Tag == size;
        }
    }
}
