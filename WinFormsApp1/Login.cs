using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace WinFormsApp1
{
    public class LoginForm : Form
    {
        private Label lblLogin;
        private TextBox txtLogin;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnRegister;

        public LoginForm()
        {
            this.Text = "Авторизация";
            this.Size = new Size(300, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.FormClosed += (s, e) => Application.Exit();

            InitializeUI();
        }

        private void InitializeUI()
        {
            lblLogin = new Label { Text = "Логин:", Location = new Point(30, 30), AutoSize = true };
            txtLogin = new TextBox { Location = new Point(30, 55), Width = 220 };

            lblPassword = new Label { Text = "Пароль:", Location = new Point(30, 90), AutoSize = true };
            txtPassword = new TextBox
            {
                Location = new Point(30, 115),
                Width = 220,
                UseSystemPasswordChar = true
            };

            btnLogin = new Button
            {
                Text = "Войти",
                Location = new Point(30, 160),
                Width = 220,
                Height = 30
            };
            btnLogin.Click += BtnLogin_Click;

            btnRegister = new Button
            {
                Text = "Создать аккаунт",
                Location = new Point(30, 210),
                Width = 220,
                Height = 30,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnRegister.Click += BtnRegister_Click;

            this.Controls.Add(lblLogin);
            this.Controls.Add(txtLogin);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
            this.Controls.Add(btnRegister);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();

            try
            {
                string query = "SELECT Id_p FROM polzovately WHERE Login = @login AND password = @password";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@login", login),
                    new SqlParameter("@password", password)
                };

                var dt = DbHelper.GetData(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    CurrentUser.Id = Convert.ToInt32(dt.Rows[0]["Id_p"]);

                    MainForm mainMenu = new MainForm();
                    mainMenu.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            RegisterForm regForm = new RegisterForm();
            regForm.ShowDialog();
        }
    }
}