using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace WinFormsApp1
{
    public class SnakeForm : Form
    {
        private const int CellSize = 20;
        private const int GridWidth = 30;
        private const int GridHeight = 30;
        private const int GameSpeed = 150;

        private Label lblScore;
        private Label lblRecord;
        private Label lblGameOver;
        private Button btnRestart;
        private Button btnBackToMenu;

        private List<Point> snake;
        private Point food;
        private Direction currentDirection;
        private Direction nextDirection;
        private int score;
        private int currentRecord = 0;
        private bool isGameOver;
        private Random rnd = new Random();

        private enum Direction { Up, Down, Left, Right }

        public SnakeForm()
        {
            this.Text = "Игра: Змейка";
            this.ClientSize = new Size(GridWidth * CellSize + 20, GridHeight * CellSize + 90);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.DoubleBuffered = true;

            InitializeUI();
            InitializeGame();
        }

        private void InitializeUI()
        {
            lblScore = new Label
            {
                Text = "Счет: 0",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Arial", 14, FontStyle.Bold)
            };

            lblRecord = new Label
            {
                Text = "Рекорд: 0",
                Location = new Point(150, 10),
                AutoSize = true,
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            lblGameOver = new Label
            {
                Text = "ИГРА ОКОНЧЕНА",
                Location = new Point(GridWidth * CellSize / 2 - 70, GridHeight * CellSize / 2 + 20),
                AutoSize = true,
                Font = new Font("Arial", 18, FontStyle.Bold),
                ForeColor = Color.Red,
                Visible = false
            };

            btnRestart = new Button
            {
                Text = "Начать заново",
                Location = new Point(10, GridHeight * CellSize + 50),
                Size = new Size(120, 30),
                Visible = false
            };
            btnRestart.Click += BtnRestart_Click;

            btnBackToMenu = new Button
            {
                Text = "В главное меню",
                Location = new Point(140, GridHeight * CellSize + 50),
                Size = new Size(120, 30),
                Visible = false
            };
            btnBackToMenu.Click += BtnBackToMenu_Click;

            this.Controls.Add(lblScore);
            this.Controls.Add(lblRecord);
            this.Controls.Add(lblGameOver);
            this.Controls.Add(btnRestart);
            this.Controls.Add(btnBackToMenu);

            this.Paint += SnakeForm_Paint;
            this.KeyDown += SnakeForm_KeyDown;
        }

        private void InitializeGame()
        {
            snake = new List<Point> { new Point(15, 15), new Point(14, 15), new Point(13, 15) };
            currentDirection = Direction.Right;
            nextDirection = Direction.Right;
            score = 0;
            isGameOver = false;
            lblScore.Text = "Счет: 0";
            lblGameOver.Visible = false;
            btnRestart.Visible = false;
            btnBackToMenu.Visible = false;
            GenerateFood();

            LoadRecordFromDb();
            _ = StartGameLoopAsync();
        }

        private void LoadRecordFromDb()
        {
            if (CurrentUser.Id == 0) return;

            try
            {
                string query = "SELECT Record_zmey FROM zmeyka WHERE Id_p = @Id_p";
                SqlParameter[] parameters = { new SqlParameter("@Id_p", CurrentUser.Id) };
                var dt = DbHelper.GetData(query, parameters);

                if (dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value)
                    currentRecord = Convert.ToInt32(dt.Rows[0][0]);
                else
                    currentRecord = 0;

                lblRecord.Text = $"Рекорд: {currentRecord}";
            }
            catch {}
        }

        private void GenerateFood()
        {
            while (true)
            {
                food = new Point(rnd.Next(0, GridWidth), rnd.Next(0, GridHeight));
                if (!snake.Contains(food)) break;
            }
        }

        private async Task StartGameLoopAsync()
        {
            while (!isGameOver)
            {
                currentDirection = nextDirection;
                Point head = snake[0];
                Point newHead = head;

                switch (currentDirection)
                {
                    case Direction.Up: newHead.Y--; break;
                    case Direction.Down: newHead.Y++; break;
                    case Direction.Left: newHead.X--; break;
                    case Direction.Right: newHead.X++; break;
                }

                if (newHead.X < 0 || newHead.X >= GridWidth || newHead.Y < 0 || newHead.Y >= GridHeight || snake.Contains(newHead))
                {
                    EndGame();
                    break;
                }

                snake.Insert(0, newHead);

                if (newHead == food)
                {
                    score += 10;
                    lblScore.Text = $"Счет: {score}";

                    if (score > currentRecord)
                    {
                        currentRecord = score;
                        lblRecord.Text = $"Рекорд: {currentRecord}";
                    }

                    GenerateFood();
                }
                else
                {
                    snake.RemoveAt(snake.Count - 1);
                }

                this.Invalidate();
                await Task.Delay(GameSpeed);
            }
        }

        private void SnakeForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W && currentDirection != Direction.Down) nextDirection = Direction.Up;
            else if (e.KeyCode == Keys.S && currentDirection != Direction.Up) nextDirection = Direction.Down;
            else if (e.KeyCode == Keys.A && currentDirection != Direction.Right) nextDirection = Direction.Left;
            else if (e.KeyCode == Keys.D && currentDirection != Direction.Left) nextDirection = Direction.Right;
        }

        private void SnakeForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int offsetY = 40;

            g.FillEllipse(Brushes.Red, food.X * CellSize, food.Y * CellSize + offsetY, CellSize, CellSize);

            for (int i = 0; i < snake.Count; i++)
            {
                Brush snakeBrush = (i == 0) ? Brushes.DarkGreen : Brushes.Green;
                g.FillRectangle(snakeBrush, snake[i].X * CellSize, snake[i].Y * CellSize + offsetY, CellSize - 1, CellSize - 1);
            }
        }

        private void EndGame()
        {
            isGameOver = true;
            lblGameOver.Visible = true;
            btnRestart.Visible = true;
            btnBackToMenu.Visible = true;

            if (CurrentUser.Id > 0)
            {
                try
                {
                    bool isNewRecord = DbHelper.SaveGameRecord("zmeyka", "Record_zmey", CurrentUser.Id, score);
                    if (isNewRecord)
                    {
                        MessageBox.Show($"Новый рекорд сохранен в БД: {score} очков!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении рекорда: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            InitializeGame();
            this.Focus();
        }

        private void BtnBackToMenu_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}