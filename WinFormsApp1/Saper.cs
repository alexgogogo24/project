using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace WinFormsApp1
{
    public class SaperForm : Form
    {
        private const int GridSize = 10;
        private const int MineCount = 15;
        private const int ButtonSize = 30;

        private TableLayoutPanel gridPanel;
        private Label lblTime;
        private Label lblMines;
        private Label lblRecord;
        private Button btnBackToMenu;
        private Button btnRestart;
        private System.Windows.Forms.Timer gameTimer;

        private Button[,] buttons;
        private bool[,] isMine;
        private int[,] adjacentMines;
        private bool[,] isRevealed;
        private bool[,] isFlagged;

        private int timeElapsed;
        private int flagsPlaced;
        private bool isGameOver;
        private bool isFirstClick;
        private int bestTime = 0;

        public SaperForm()
        {
            this.Text = "Игра: Сапёр";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(GridSize * ButtonSize + 60, GridSize * ButtonSize + 140);

            InitializeUI();
            LoadRecordFromDb();
            InitializeGame();
        }

        private void InitializeUI()
        {
            lblTime = new Label
            {
                Text = "Время: 0",
                Location = new Point(20, 15),
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            lblMines = new Label
            {
                Text = $"Мины: {MineCount}",
                Location = new Point(120, 15),
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            lblRecord = new Label
            {
                Text = "Рекорд: нет",
                Location = new Point(220, 15),
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            gridPanel = new TableLayoutPanel
            {
                Location = new Point(30, 50),
                Size = new Size(GridSize * ButtonSize, GridSize * ButtonSize),
                ColumnCount = GridSize,
                RowCount = GridSize
            };

            for (int i = 0; i < GridSize; i++)
            {
                gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ButtonSize));
                gridPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, ButtonSize));
            }

            btnRestart = new Button
            {
                Text = "Играть снова",
                Location = new Point(30, GridSize * ButtonSize + 60),
                Size = new Size(145, 30),
                Visible = false
            };
            btnRestart.Click += BtnRestart_Click;

            btnBackToMenu = new Button
            {
                Text = "В главное меню",
                Location = new Point(185, GridSize * ButtonSize + 60),
                Size = new Size(145, 30),
                Visible = false
            };
            btnBackToMenu.Click += BtnBackToMenu_Click;

            this.Controls.Add(lblTime);
            this.Controls.Add(lblMines);
            this.Controls.Add(lblRecord);
            this.Controls.Add(gridPanel);
            this.Controls.Add(btnRestart);
            this.Controls.Add(btnBackToMenu);

            gameTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            gameTimer.Tick += GameTimer_Tick;
        }

        private void InitializeGame()
        {
            gameTimer.Stop();

            isFirstClick = true;
            isGameOver = false;
            timeElapsed = 0;
            flagsPlaced = 0;
            lblTime.Text = "Время: 0";
            lblMines.Text = $"Мины: {MineCount}";

            btnBackToMenu.Visible = false;
            btnRestart.Visible = false;

            buttons = new Button[GridSize, GridSize];
            isMine = new bool[GridSize, GridSize];
            adjacentMines = new int[GridSize, GridSize];
            isRevealed = new bool[GridSize, GridSize];
            isFlagged = new bool[GridSize, GridSize];

            gridPanel.Controls.Clear();

            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    Button btn = new Button
                    {
                        Dock = DockStyle.Fill,
                        Margin = new Padding(0),
                        Tag = new Point(x, y),
                        BackColor = Color.LightGray,
                        Font = new Font("Arial", 10, FontStyle.Bold)
                    };

                    btn.MouseDown += Cell_MouseDown;
                    buttons[x, y] = btn;
                    gridPanel.Controls.Add(btn, x, y);
                }
            }

            gameTimer.Start();
        }

        private void LoadRecordFromDb()
        {
            if (CurrentUser.Id == 0) return;
            try
            {
                string query = "SELECT Record_sap FROM saper WHERE Id_p = @Id_p";
                SqlParameter[] parameters = { new SqlParameter("@Id_p", CurrentUser.Id) };
                var dt = DbHelper.GetData(query, parameters);

                if (dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value)
                {
                    bestTime = Convert.ToInt32(dt.Rows[0][0]);
                    if (bestTime > 0)
                    {
                        lblRecord.Text = $"Рекорд: {bestTime} сек.";
                    }
                }
            }
            catch { }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (isGameOver) return;
            timeElapsed++;
            lblTime.Text = $"Время: {timeElapsed}";
        }

        private void Cell_MouseDown(object sender, MouseEventArgs e)
        {
            if (isGameOver) return;

            Button btn = sender as Button;
            Point pos = (Point)btn.Tag;
            int x = pos.X;
            int y = pos.Y;

            if (e.Button == MouseButtons.Left)
            {
                if (isFlagged[x, y]) return;

                if (isFirstClick)
                {
                    GenerateMines(x, y);
                    isFirstClick = false;
                }

                if (isMine[x, y])
                {
                    RevealAllMines();
                    GameOver(false);
                }
                else
                {
                    RevealCell(x, y);
                    CheckWinCondition();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (isRevealed[x, y]) return;

                isFlagged[x, y] = !isFlagged[x, y];
                btn.BackColor = isFlagged[x, y] ? Color.Yellow : Color.LightGray;
                flagsPlaced += isFlagged[x, y] ? 1 : -1;
                lblMines.Text = $"Мины: {MineCount - flagsPlaced}";
            }
        }

        private void GenerateMines(int safeX, int safeY)
        {
            Random rnd = new Random();
            int minesPlaced = 0;

            while (minesPlaced < MineCount)
            {
                int x = rnd.Next(GridSize);
                int y = rnd.Next(GridSize);

                if (Math.Abs(x - safeX) <= 1 && Math.Abs(y - safeY) <= 1) continue;

                if (!isMine[x, y])
                {
                    isMine[x, y] = true;
                    minesPlaced++;
                }
            }

            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    if (!isMine[x, y])
                    {
                        int count = 0;
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                int nx = x + dx, ny = y + dy;
                                if (nx >= 0 && nx < GridSize && ny >= 0 && ny < GridSize && isMine[nx, ny])
                                    count++;
                            }
                        }
                        adjacentMines[x, y] = count;
                    }
                }
            }
        }

        private void RevealCell(int x, int y)
        {
            if (x < 0 || x >= GridSize || y < 0 || y >= GridSize || isRevealed[x, y] || isFlagged[x, y]) return;

            isRevealed[x, y] = true;
            Button btn = buttons[x, y];
            btn.BackColor = Color.White;
            btn.Enabled = false;

            if (adjacentMines[x, y] > 0)
            {
                btn.Text = adjacentMines[x, y].ToString();
                switch (adjacentMines[x, y])
                {
                    case 1: btn.ForeColor = Color.Blue; break;
                    case 2: btn.ForeColor = Color.Green; break;
                    case 3: btn.ForeColor = Color.Red; break;
                    default: btn.ForeColor = Color.DarkMagenta; break;
                }
            }
            else
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        RevealCell(x + dx, y + dy);
                    }
                }
            }
        }

        private void RevealAllMines()
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    if (isMine[x, y])
                    {
                        buttons[x, y].BackColor = Color.Red;
                        buttons[x, y].Text = "💣";
                    }
                }
            }
        }

        private void CheckWinCondition()
        {
            int revealedCount = 0;
            for (int x = 0; x < GridSize; x++)
                for (int y = 0; y < GridSize; y++)
                    if (isRevealed[x, y]) revealedCount++;

            if (revealedCount == (GridSize * GridSize) - MineCount)
            {
                GameOver(true);
            }
        }

        private void GameOver(bool isWin)
        {
            isGameOver = true;
            gameTimer.Stop();

            btnBackToMenu.Visible = true;
            btnRestart.Visible = true;

            if (isWin)
            {
                MessageBox.Show($"Поле разминировано! Ваше время: {timeElapsed} сек.", "Победа!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SaveRecordToDb(timeElapsed);
            }
            else
            {
                MessageBox.Show("Вы подорвались на мине! Игра окончена.", "Поражение", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveRecordToDb(int time)
        {
            if (CurrentUser.Id == 0) return;

            try
            {
                string query = @"
                    IF EXISTS (SELECT 1 FROM saper WHERE Id_p = @playerId)
                    BEGIN
                        UPDATE saper 
                        SET Record_sap = @newRecord 
                        WHERE Id_p = @playerId AND (Record_sap > @newRecord OR Record_sap = 0)
                    END
                    ELSE
                    BEGIN
                        INSERT INTO saper (Id_p, Record_sap) VALUES (@playerId, @newRecord)
                    END";

                SqlParameter[] parameters = {
                    new SqlParameter("@playerId", CurrentUser.Id),
                    new SqlParameter("@newRecord", time)
                };

                int rows = DbHelper.Execute(query, parameters);
                if (rows > 0)
                {
                    bestTime = time;
                    lblRecord.Text = $"Рекорд: {bestTime} сек.";
                    MessageBox.Show($"Новый рекорд по времени сохранен в БД: {time} сек!", "Рекорд", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении рекорда: " + ex.Message);
            }
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            InitializeGame();
        }

        private void BtnBackToMenu_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}