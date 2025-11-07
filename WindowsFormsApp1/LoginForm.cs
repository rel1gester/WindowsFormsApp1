using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static WindowsFormsApp1.Form1;

namespace WindowsFormsApp1
{
    public partial class LoginForm : Form
    {
        private int failedAttempts = 0;
        private bool captchaRequired = false;
        private string currentCaptcha = "";
        private DateTime? blockTime = null;
        private TextBox txtLogin;
        private TextBox txtPassword;
        private PictureBox captchaBox;
        private TextBox txtCaptcha;
        private Button btnRefreshCaptcha;
        private Label lblBlockTimer;
        private Timer blockTimer;

        public LoginForm()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "Вход в систему";
            this.Size = new Size(400, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(187, 217, 178);
            this.Icon = Properties.Resources.AppIcon;
            CreateControls();
            this.FormClosed += (s, e) => Application.Exit();
        }

        private void CreateControls()
        {
            PictureBox logo = new PictureBox
            {
                Size = new Size(120, 120),
                Location = new Point(140, 30),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Properties.Resources.logo
            };

            Label title = new Label
            {
                Text = "Вход в систему",
                Font = new Font("Gabriola", 20, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(120, 160),
                ForeColor = Color.Black,
                BackColor = Color.Transparent
            };

            Label lblLogin = new Label
            {
                Text = "Логин:",
                Location = new Point(50, 210),
                AutoSize = true,
                Font = new Font("Gabriola", 14),
                BackColor = Color.Transparent
            };

            txtLogin = new TextBox
            {
                Location = new Point(50, 250),
                Size = new Size(300, 35),
                Font = new Font("Gabriola", 14),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblPassword = new Label
            {
                Text = "Пароль:",
                Location = new Point(50, 290),
                AutoSize = true,
                Font = new Font("Gabriola", 14),
                BackColor = Color.Transparent
            };

            txtPassword = new TextBox
            {
                Location = new Point(50, 330),
                Size = new Size(300, 35),
                Font = new Font("Gabriola", 14),
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '*'
            };

            CheckBox chkShowPassword = new CheckBox
            {
                Text = "Показать пароль",
                Location = new Point(50, 370),
                AutoSize = true,
                Font = new Font("Gabriola", 14),
                BackColor = Color.Transparent
            };

            Button btnLogin = new Button
            {
                Text = "Войти",
                Location = new Point(50, 410),
                Size = new Size(300, 40),
                BackColor = Color.FromArgb(45, 96, 51),
                ForeColor = Color.White,
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            Button btnGuest = new Button
            {
                Text = "Войти как гость",
                Location = new Point(50, 460),
                Size = new Size(300, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(45, 96, 51)
            };

            lblBlockTimer = new Label
            {
                Location = new Point(50, 620),
                Size = new Size(300, 25),
                Font = new Font("Gabriola", 12, FontStyle.Bold),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '*';
            };
            btnLogin.Click += BtnLogin_Click;
            btnGuest.Click += BtnGuest_Click;

            this.Controls.AddRange(new Control[] {
                logo, title, lblLogin, txtLogin, lblPassword, txtPassword,
                chkShowPassword, btnLogin, btnGuest, lblBlockTimer
            });
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text;

            if (blockTime.HasValue && DateTime.Now < blockTime.Value)
            {
                return;
            }

            if (captchaRequired)
            {
                if (string.IsNullOrEmpty(txtCaptcha.Text))
                {
                    MessageBox.Show("Введите CAPTCHA!");
                    LoginHistory.Add(login, "", "Неудачная попытка", false);
                    return;
                }

                if (!ValidateCaptcha())
                {
                    MessageBox.Show("Неверная CAPTCHA!");
                    LoginHistory.Add(login, "", "Неудачная попытка", false);
                    failedAttempts++;
                    GenerateCaptcha();
                    captchaBox.Refresh();
                    txtCaptcha.Text = "";
                    txtCaptcha.Focus();
                    HandleFailedLogin();
                    return;
                }
            }

            if (AuthenticateUser(login, password))
            {
                failedAttempts = 0;
                captchaRequired = false;
                HideCaptchaControls();
                ShowMainForm(login);
            }
            else
            {
                LoginHistory.Add(login, "", "Неудачная попытка", false);
                failedAttempts++;
                HandleFailedLogin();
            }
        }

        private bool AuthenticateUser(string login, string password)
        {
            string connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=УПКузнецов1;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT UserID, Login, Role, FullName FROM Users WHERE Login = @Login AND Password = @Password";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Login", login);
                command.Parameters.AddWithValue("@Password", password);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        UserSession.UserId = (int)reader["UserID"];
                        UserSession.FullName = (string)reader["FullName"];
                        UserSession.Role = (string)reader["Role"];
                        UserSession.Photo = null;
                        LoginHistory.Add(login, UserSession.Role, UserSession.FullName, true);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка подключения к БД: " + ex.Message);
                }
            }
            return false;
        }

        private void BtnGuest_Click(object sender, EventArgs e)
        {
            LoginHistory.Add("Гость", "Гость", "Гость", true);
            ShowMainForm(null, true);
        }

        private void EnableForm(bool enabled)
        {
            foreach (Control control in this.Controls)
            {
                if (control != lblBlockTimer)
                    control.Enabled = enabled;
            }
        }

        private void StartBlockTimer()
        {
            blockTime = DateTime.Now.AddMinutes(0.01);
            EnableForm(false);
            lblBlockTimer.Visible = true;

            if (blockTimer == null)
            {
                blockTimer = new Timer();
                blockTimer.Interval = 1000;
                blockTimer.Tick += BlockTimer_Tick;
            }
            blockTimer.Start();

            UpdateBlockTimerDisplay();
        }

        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            UpdateBlockTimerDisplay();
        }

        private void UpdateBlockTimerDisplay()
        {
            if (blockTime.HasValue)
            {
                TimeSpan remaining = blockTime.Value - DateTime.Now;

                if (remaining.TotalSeconds <= 0)
                {
                    blockTimer.Stop();
                    blockTime = null;
                    EnableForm(true);
                    lblBlockTimer.Visible = false;
                    failedAttempts = 3;
                    captchaRequired = true;
                    MessageBox.Show("Система разблокирована. У вас есть 1 попытка. После неудачи приложение закроется.");
                }
                else
                {
                    lblBlockTimer.Text = $"Блокировка: {remaining.Minutes:00}:{remaining.Seconds:00}";
                    lblBlockTimer.Visible = true;
                }
            }
        }

        private void HandleFailedLogin()
        {
            if (failedAttempts == 1)
            {
                MessageBox.Show("Неверный логин или пароль!");
                ShowCaptchaControls();
                captchaRequired = true;
            }
            else if (failedAttempts >= 2)
            {
                if (failedAttempts == 2)
                {
                    StartBlockTimer();
                }
                else if (failedAttempts >= 3)
                {
                    MessageBox.Show("Неудачная попытка после разблокировки. Приложение будет закрыто.");
                    Application.Exit();
                }
            }
        }

        private void ShowCaptchaControls()
        {
            this.Height = 700;

            if (captchaBox == null)
            {
                captchaBox = new PictureBox
                {
                    Location = new Point(50, 520),
                    Size = new Size(200, 50),
                    BackColor = Color.White
                };
                captchaBox.Paint += CaptchaBox_Paint;
                this.Controls.Add(captchaBox);
            }
            else
            {
                captchaBox.Visible = true;
            }

            if (txtCaptcha == null)
            {
                txtCaptcha = new TextBox
                {
                    Location = new Point(50, 580),
                    Size = new Size(200, 30),
                    Font = new Font("Gabriola", 12),
                };
                this.Controls.Add(txtCaptcha);
            }
            else
            {
                txtCaptcha.Visible = true;
            }

            if (btnRefreshCaptcha == null)
            {
                btnRefreshCaptcha = new Button
                {
                    Text = "🔄",
                    Location = new Point(260, 520),
                    Size = new Size(40, 30),
                    Font = new Font("Gabriola", 10),
                    BackColor = Color.White
                };
                btnRefreshCaptcha.Click += (s, e) =>
                {
                    GenerateCaptcha();
                    captchaBox.Refresh();
                    txtCaptcha.Text = "";
                };
                this.Controls.Add(btnRefreshCaptcha);
            }
            else
            {
                btnRefreshCaptcha.Visible = true;
            }

            GenerateCaptcha();
            captchaBox.Refresh();
            txtCaptcha.Text = "";
            txtCaptcha.Focus();
        }

        private void HideCaptchaControls()
        {
            if (captchaBox != null) captchaBox.Visible = false;
            if (txtCaptcha != null) txtCaptcha.Visible = false;
            if (btnRefreshCaptcha != null) btnRefreshCaptcha.Visible = false;
            this.Height = 650;
        }

        private void GenerateCaptcha()
        {
            Random rand = new Random();
            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            currentCaptcha = "";

            for (int i = 0; i < 4; i++)
            {
                currentCaptcha += chars[rand.Next(chars.Length)];
            }
        }

        private void CaptchaBox_Paint(object sender, PaintEventArgs e)
        {
            if (string.IsNullOrEmpty(currentCaptcha)) return;

            PictureBox pb = sender as PictureBox;
            e.Graphics.Clear(Color.White);

            Random rand = new Random();

            for (int i = 0; i < 100; i++)
            {
                e.Graphics.DrawEllipse(Pens.LightGray,
                    rand.Next(pb.Width), rand.Next(pb.Height), 1, 1);
            }

            for (int i = 0; i < 5; i++)
            {
                e.Graphics.DrawLine(Pens.Gray,
                    rand.Next(pb.Width), rand.Next(pb.Height),
                    rand.Next(pb.Width), rand.Next(pb.Height));
            }

            Font font = new Font("Arial", 16, FontStyle.Bold);
            Brush brush = Brushes.Black;

            for (int i = 0; i < currentCaptcha.Length; i++)
            {
                float x = 20 + i * 35 + rand.Next(-8, 8);
                float y = 15 + rand.Next(-10, 10);
                float angle = rand.Next(-25, 25);

                e.Graphics.TranslateTransform(x, y);
                e.Graphics.RotateTransform(angle);
                e.Graphics.DrawString(currentCaptcha[i].ToString(), font, brush, -2, -2);
                e.Graphics.DrawString(currentCaptcha[i].ToString(), font, Brushes.LightGray, 1, 1);

                e.Graphics.ResetTransform();
            }
        }

        private bool ValidateCaptcha()
        {
            if (txtCaptcha == null || string.IsNullOrEmpty(txtCaptcha.Text))
                return false;

            return txtCaptcha.Text.Trim().ToUpper() == currentCaptcha.ToUpper();
        }

        private void ShowMainForm(string login, bool isGuest = false)
        {
            try
            {
                Form1 mainForm = new Form1();
                mainForm.IsGuest = isGuest || string.IsNullOrEmpty(login);
                mainForm.FormClosed += (s, e) => Application.Exit();
                mainForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка запуска главной формы: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }

    public static class LoginHistory
    {
        public static List<LoginAttempt> Logs = new List<LoginAttempt>();

        public class LoginAttempt
        {
            public DateTime Time { get; set; }
            public string Login { get; set; }
            public string Role { get; set; }
            public string FullName { get; set; }
            public bool IsSuccessful { get; set; }
        }
        public static void Add(string login, string role, string fullName, bool isSuccessful)
        {
            if (Logs == null)
                Logs = new List<LoginAttempt>();

            Logs.Add(new LoginAttempt
            {
                Time = DateTime.Now,
                Login = string.IsNullOrEmpty(login) ? "Гость" : login,
                Role = string.IsNullOrEmpty(role) ? "Гость" : role,
                FullName = string.IsNullOrEmpty(fullName) ? "Не указано" : fullName,
                IsSuccessful = isSuccessful
            });
        }
        public static void Add(string login, string role, string fullName)
        {
            Add(login, role, fullName, true);
        }
    }
    public static class UserSession
    {
        public static int UserId { get; set; }
        public static string FullName { get; set; }
        public static string Role { get; set; }
        public static byte[] Photo { get; set; }
        public static bool IsGuest => UserId == 0;
    }

}