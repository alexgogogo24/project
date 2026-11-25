using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace WinFormsApp1
{
    public class RegisterForm : Form
    {
        private Label lblNewLogin;
        private TextBox txtNewLogin;
        private Label lblNewPassword;
        private TextBox txtNewPassword;
        private Button btnCreate;

        public RegisterForm()
        {
            this.Text = "Регистрация";
            this.Size = new Size(300, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InitializeUI();
        }

        private void InitializeUI()
        {
            lblNewLogin = new Label { Text = "Придумайте логин:", Location = new Point(30, 30), AutoSize = true };
            txtNewLogin = new TextBox { Location = new Point(30, 55), Width = 220 };

            lblNewPassword = new Label { Text = "Придумайте пароль:", Location = new Point(30, 90), AutoSize = true };
            txtNewPassword = new TextBox
            {
                Location = new Point(30, 115),
                Width = 220,
                UseSystemPasswordChar = true
            };

            btnCreate = new Button
            {
                Text = "Зарегистрироваться",
                Location = new Point(30, 160),
                Width = 220,
                Height = 30
            };
            btnCreate.Click += BtnCreate_Click;

            this.Controls.Add(lblNewLogin);
            this.Controls.Add(txtNewLogin);
            this.Controls.Add(lblNewPassword);
            this.Controls.Add(txtNewPassword);
            this.Controls.Add(btnCreate);
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            string login = txtNewLogin.Text.Trim();
            string password = txtNewPassword.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Логин и пароль не могут быть пустыми.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string checkQuery = "SELECT COUNT(*) FROM polzovately WHERE Login = @login";
                SqlParameter[] checkParams = { new SqlParameter("@login", login) };
                var dt = DbHelper.GetData(checkQuery, checkParams);

                if (Convert.ToInt32(dt.Rows[0][0]) > 0)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string insertQuery = "INSERT INTO polzovately (Login, password) VALUES (@login, @password)";
                SqlParameter[] insertParams =
                {
                    new SqlParameter("@login", login),
                    new SqlParameter("@password", password)
                };

                DbHelper.Execute(insertQuery, insertParams);

                MessageBox.Show("Аккаунт успешно создан.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при регистрации: " + ex.Message);
            }
        }
    }
}