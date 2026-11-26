using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class MainForm : Form
    {
        public MainForm()
        {
            this.Text = "Главное меню - Мини-игры";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            this.FormClosed += (s, e) => Application.Exit();

            InitializeUI();
        }

        private void InitializeUI()
        {
            string[] gameNames = {
                "Тетрис", "Сапер",
                "Угадай слово", "Угадай число",
                "Шашки", "Шаматы",
                "Морской бой", "блок бласт",
                "Змейка", "казик"
            };

            int buttonWidth = 200;
            int buttonHeight = 50;
            int spacing = 20;
            int startX = 35;
            int startY = 20;

            for (int i = 0; i < gameNames.Length; i++)
            {
                Button btn = new Button
                {
                    Text = gameNames[i],
                    Size = new Size(buttonWidth, buttonHeight),
                    Location = new Point(
                        startX + (i % 2) * (buttonWidth + spacing),
                        startY + (i / 2) * (buttonHeight + spacing)
                    ),
                    Font = new Font("Arial", 11, FontStyle.Bold),
                    BackColor = Color.WhiteSmoke
                };

                if (gameNames[i] == "Змейка")
                {
                    btn.Click += BtnSnake_Click;
                }
                else if (gameNames[i] == "Сапер")
                {
                    btn.Click += BtnSaper_Click;
                }
                else
                {
                    btn.Click += (s, e) => MessageBox.Show($"Игра '{((Button)s).Text}' находится в разработке!", "В разработке", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.Controls.Add(btn);
            }

            Button btnLogout = new Button
            {
                Text = "Выйти из аккаунта",
                Size = new Size(420, 40),
                Location = new Point(35, 370),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.DarkRed
            };
            btnLogout.Click += BtnLogout_Click;
            this.Controls.Add(btnLogout);
        }

        private void BtnSnake_Click(object sender, EventArgs e)
        {
            this.Hide();
            SnakeForm snake = new SnakeForm();
            snake.ShowDialog();
            this.Show();
        }

        private void BtnSaper_Click(object sender, EventArgs e)
        {
            this.Hide();
            SaperForm saper = new SaperForm();
            saper.ShowDialog();
            this.Show();
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            CurrentUser.Id = 0;
            LoginForm login = new LoginForm();
            login.Show();
            this.Close();
        }
    }
}