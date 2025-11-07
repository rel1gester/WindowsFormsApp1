using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class HistoryForm : Form
    {
        private DataGridView dataGridView;
        private TextBox txtFilter;
        private ComboBox cmbSort;
        private ComboBox cmbStatus;
        private Button btnClose;

        public HistoryForm()
        {
            InitializeComponent();
            SetupForm();
            LoadHistory();
        }

        private void SetupForm()
        {
            this.Text = "История входов в систему";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Icon = Properties.Resources.AppIcon;
            Label lblTitle = new Label
            {
                Text = "История входов",
                Font = new Font("Gabriola", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 96, 51),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            Label lblFilter = new Label
            {
                Text = "Фильтр по логину:",
                Font = new Font("Gabriola", 12),
                AutoSize = true,
                Location = new Point(20, 70)
            };

            txtFilter = new TextBox
            {
                Location = new Point(140, 65),
                Size = new Size(150, 25),
                Font = new Font("Gabriola", 12)
            };
            txtFilter.TextChanged += (s, e) => ApplyFilters();
            Label lblStatus = new Label
            {
                Text = "Статус:",
                Font = new Font("Gabriola", 12),
                AutoSize = true,
                Location = new Point(310, 70)
            };

            cmbStatus = new ComboBox
            {
                Location = new Point(370, 65),
                Size = new Size(150, 25),
                Font = new Font("Gabriola", 12),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new[] {
                "Все статусы",
                "Успешно",
                "Ошибка"
            });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += (s, e) => ApplyFilters();

            Label lblSort = new Label
            {
                Text = "Сортировка:",
                Font = new Font("Gabriola", 12),
                AutoSize = true,
                Location = new Point(540, 70)
            };

            cmbSort = new ComboBox
            {
                Location = new Point(630, 65),
                Size = new Size(150, 25),
                Font = new Font("Gabriola", 12),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSort.Items.AddRange(new[] {
                "Сначала новые",
                "Сначала старые"
            });
            cmbSort.SelectedIndex = 0;
            cmbSort.SelectedIndexChanged += (s, e) => ApplyFilters();

            dataGridView = new DataGridView
            {
                Location = new Point(20, 110),
                Size = new Size(940, 400),
                BackgroundColor = Color.FromArgb(187, 217, 178),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Gabriola", 11),
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(220, 235, 215)
                }
            };

            dataGridView.ColumnHeadersHeight = 50;
            dataGridView.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Gabriola", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 96, 51),
                ForeColor = Color.White,
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
            dataGridView.RowTemplate.Height = 35;

            dataGridView.Columns.Add("Time", "Время входа");
            dataGridView.Columns.Add("Login", "Логин");
            dataGridView.Columns.Add("FullName", "ФИО");
            dataGridView.Columns.Add("Role", "Роль");
            dataGridView.Columns.Add("Status", "Статус");

            dataGridView.Columns["Time"].Width = 150;
            dataGridView.Columns["Login"].Width = 120;
            dataGridView.Columns["FullName"].Width = 200;
            dataGridView.Columns["Role"].Width = 120;
            dataGridView.Columns["Status"].Width = 100;

            dataGridView.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == dataGridView.Columns["Status"].Index && e.Value != null)
                {
                    string status = e.Value.ToString();
                    if (status == "Успешно")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(0, 128, 0);
                        e.CellStyle.Font = new Font("Gabriola", 11, FontStyle.Bold);
                    }
                    else if (status == "Ошибка")
                    {
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = new Font("Gabriola", 11, FontStyle.Bold);
                    }
                }
            };

            btnClose = new Button
            {
                Text = "Назад",
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 96, 51),
                ForeColor = Color.White,
                Size = new Size(120, 40),
                Location = new Point(840, 520),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) =>
            {
                if (MessageBox.Show("Закрыть историю входов?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    this.Close();
                }
            };

            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblFilter, txtFilter,
                lblStatus, cmbStatus,
                lblSort, cmbSort,
                dataGridView,
                btnClose
            });
            this.FormClosed += (s, e) => this.Dispose();
        }

        private void LoadHistory()
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            dataGridView.Rows.Clear();

            if (LoginHistory.Logs == null || LoginHistory.Logs.Count == 0)
            {
                dataGridView.Rows.Add("Нет данных", "", "", "", "");
                return;
            }

            var filteredData = LoginHistory.Logs.AsEnumerable();

            string filterText = txtFilter.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(filterText))
            {
                filteredData = filteredData.Where(a =>
                    a.Login.ToLower().Contains(filterText) ||
                    a.FullName.ToLower().Contains(filterText) ||
                    a.Role.ToLower().Contains(filterText));
            }

            if (cmbStatus.SelectedIndex > 0)
            {
                string selectedStatus = cmbStatus.SelectedItem.ToString();
                bool isSuccessful = selectedStatus == "Успешно";
                filteredData = filteredData.Where(a => a.IsSuccessful == isSuccessful);
            }

            if (cmbSort.SelectedIndex == 0)
            {
                filteredData = filteredData.OrderByDescending(a => a.Time);
            }
            else
            {
                filteredData = filteredData.OrderBy(a => a.Time);
            }

            foreach (var attempt in filteredData)
            {
                string status = attempt.IsSuccessful ? "Успешно" : "Ошибка";
                dataGridView.Rows.Add(
                    attempt.Time.ToString("dd.MM.yyyy HH:mm:ss"),
                    attempt.Login,
                    attempt.FullName,
                    attempt.Role,
                    status
                );
            }
        }
        
    }
}