using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class AdminProductsForm : Form
    {
        private DataGridView dataGridView;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnSave;
        private Button btnCancel;
        private Button btnBack;
        private Button btnRefresh;
        private TextBox txtName;
        private TextBox txtArticle;
        private TextBox txtMinPrice;
        private TextBox txtRollWidth;
        private ComboBox cmbProductType;
        private ComboBox cmbSupplier;
        private Panel editPanel;
        private ToolTip toolTip;
        private ToolTip editPanelToolTip;

        private DataTable productsTable;
        private bool isEditMode = false;
        private int currentProductId = -1;

        public AdminProductsForm()
        {
            InitializeComponent();
            SetupForm();
            LoadProducts();
        }

        private void SetupForm()
        {
            this.Text = "Управление товарами - Администратор";
            this.Size = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Icon = Properties.Resources.AppIcon;

            Label lblTitle = new Label
            {
                Text = "Управление товарами",
                Font = new Font("Gabriola", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 96, 51),
                AutoSize = true,
                Location = new Point(20, 10)
            };

            dataGridView = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(1150, 350),
                BackgroundColor = Color.FromArgb(187, 217, 178),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Gabriola", 12),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(220, 235, 215)
                }
            };

            dataGridView.ColumnHeadersHeight = 50;
            dataGridView.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 96, 51),
                ForeColor = Color.White,
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
            dataGridView.RowTemplate.Height = 40;

            GroupBox grpControls = new GroupBox
            {
                Text = "Действия",
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 96, 51),
                Location = new Point(20, 430),
                Size = new Size(1150, 80),
                BackColor = Color.FromArgb(240, 245, 240)
            };

            btnAdd = new Button
            {
                Text = "➕ Добавить",
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 96, 51),
                ForeColor = Color.White,
                Size = new Size(150, 40),
                Location = new Point(20, 30),
                FlatStyle = FlatStyle.Flat
            };

            btnEdit = new Button
            {
                Text = "✏️ Редактировать",
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 96, 51),
                ForeColor = Color.White,
                Size = new Size(180, 40),
                Location = new Point(180, 30),
                FlatStyle = FlatStyle.Flat
            };

            btnDelete = new Button
            {
                Text = "🗑️ Удалить",
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                BackColor = Color.Red,
                ForeColor = Color.White,
                Size = new Size(150, 40),
                Location = new Point(370, 30),
                FlatStyle = FlatStyle.Flat
            };

            btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 96, 51),
                ForeColor = Color.White,
                Size = new Size(150, 40),
                Location = new Point(530, 30),
                FlatStyle = FlatStyle.Flat
            };

            btnBack = new Button
            {
                Text = "Назад",
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 96, 51),
                ForeColor = Color.White,
                Size = new Size(150, 40),
                Location = new Point(930, 30),
                FlatStyle = FlatStyle.Flat
            };
            btnBack.Click += (s, e) => NavigationManager.GoBack(this);

            editPanel = new Panel
            {
                Location = new Point(20, 530),
                Size = new Size(1150, 180),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 250),
                Visible = false
            };

            Label lblEditTitle = new Label
            {
                Text = "Редактирование товара:",
                Font = new Font("Gabriola", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 96, 51),
                Location = new Point(10, 0),
                AutoSize = true
            };
            Label lblId = new Label { Text = "ID товара:", Location = new Point(10, 40), AutoSize = true, Font = new Font("Gabriola", 12) };
            TextBox txtId = new TextBox
            {
                Location = new Point(100, 38),
                Size = new Size(100, 30),
                Font = new Font("Gabriola", 12),
                ReadOnly = true,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Gray
            };

            Label lblName = new Label { Text = "Название:", Location = new Point(220, 40), AutoSize = true, Font = new Font("Gabriola", 12) };
            txtName = new TextBox { Location = new Point(300, 38), Size = new Size(250, 30), Font = new Font("Gabriola", 12) };

            Label lblArticle = new Label { Text = "Артикул:", Location = new Point(570, 40), AutoSize = true, Font = new Font("Gabriola", 12) };
            txtArticle = new TextBox { Location = new Point(640, 38), Size = new Size(200, 30), Font = new Font("Gabriola", 12) };

            Label lblMinPrice = new Label { Text = "Цена:", Location = new Point(860, 40), AutoSize = true, Font = new Font("Gabriola", 12) };
            txtMinPrice = new TextBox { Location = new Point(910, 38), Size = new Size(120, 30), Font = new Font("Gabriola", 12) };

            Label lblRollWidth = new Label { Text = "Ширина:", Location = new Point(10, 85), AutoSize = true, Font = new Font("Gabriola", 12) };
            txtRollWidth = new TextBox { Location = new Point(80, 83), Size = new Size(100, 30), Font = new Font("Gabriola", 12) };

            Label lblProductType = new Label { Text = "Тип продукции:", Location = new Point(200, 85), AutoSize = true, Font = new Font("Gabriola", 12) };
            cmbProductType = new ComboBox { Location = new Point(320, 83), Size = new Size(250, 30), Font = new Font("Gabriola", 12), DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblSupplier = new Label { Text = "Поставщик:", Location = new Point(590, 85), AutoSize = true, Font = new Font("Gabriola", 12) };
            cmbSupplier = new ComboBox { Location = new Point(680, 83), Size = new Size(250, 30), Font = new Font("Gabriola", 12), DropDownStyle = ComboBoxStyle.DropDownList };

            btnSave = new Button
            {
                Text = "💾 Сохранить",
                Font = new Font("Gabriola", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 96, 51),
                ForeColor = Color.White,
                Size = new Size(120, 35),
                Location = new Point(800, 125),
                FlatStyle = FlatStyle.Flat
            };

            btnCancel = new Button
            {
                Text = "❌ Отмена",
                Font = new Font("Gabriola", 12, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(45, 96, 51),
                Size = new Size(120, 35),
                Location = new Point(930, 125),
                FlatStyle = FlatStyle.Flat
            };

            editPanel.Controls.AddRange(new Control[]
            {
        lblEditTitle,
        lblId, txtId,
        lblName, txtName,
        lblArticle, txtArticle,
        lblMinPrice, txtMinPrice,
        lblRollWidth, txtRollWidth,
        lblProductType, cmbProductType,
        lblSupplier, cmbSupplier,
        btnSave, btnCancel
            });

            grpControls.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh, btnBack });

            btnAdd.Click += (s, e) => ShowEditPanel(false);
            btnEdit.Click += (s, e) => ShowEditPanel(true);
            btnDelete.Click += (s, e) => DeleteProduct();
            btnRefresh.Click += (s, e) => LoadProducts();
            btnSave.Click += (s, e) => SaveProduct();
            btnCancel.Click += (s, e) => HideEditPanel();

            this.Controls.AddRange(new Control[]
            {
        lblTitle,
        dataGridView,
        grpControls,
        editPanel
            });
            this.FormClosed += (s, e) => this.Dispose();
            txtMinPrice.TextChanged += (s, e) => ValidateNumericField(txtMinPrice, "цена");
            txtRollWidth.TextChanged += (s, e) => ValidateNumericField(txtRollWidth, "ширина");
            this.FormClosed += (s, e) => this.Dispose();
            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 1000;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;
            editPanelToolTip = new ToolTip();
            editPanelToolTip.AutoPopDelay = 5000;
            editPanelToolTip.InitialDelay = 1000;
            editPanelToolTip.ReshowDelay = 500;
            editPanelToolTip.ShowAlways = true;

            SetupToolTips();

            txtMinPrice.TextChanged += (s, e) => ValidateNumericField(txtMinPrice, "цена");
            txtRollWidth.TextChanged += (s, e) => ValidateNumericField(txtRollWidth, "ширина");
            SetRequiredFieldStyle(txtName);
            SetRequiredFieldStyle(txtArticle);
            SetRequiredFieldStyle(txtMinPrice);
        }
        private void SetupToolTips()
        {
            toolTip.SetToolTip(btnAdd, "Добавить новый товар");
            toolTip.SetToolTip(btnEdit, "Редактировать выбранный товар");
            toolTip.SetToolTip(btnDelete, "Удалить выбранный товар\n• Будет удален вместе с материалами");
            toolTip.SetToolTip(btnRefresh, "Обновить список товаров");
            toolTip.SetToolTip(btnBack, "Вернуться на предыдущую страницу");
            editPanelToolTip.SetToolTip(txtName, "Введите название товара\n• Обязательное поле\n• Максимум 100 символов");
            editPanelToolTip.SetToolTip(txtArticle, "Введите артикул товара\n• Обязательное поле\n• Уникальный идентификатор");
            editPanelToolTip.SetToolTip(txtMinPrice, "Введите минимальную цену\n• Обязательное поле\n• Только положительные числа\n• Формат: 123.45");
            editPanelToolTip.SetToolTip(txtRollWidth, "Введите ширину рулона\n• Необязательное поле\n• Только положительные числа\n• Формат: 1.500");
            editPanelToolTip.SetToolTip(cmbProductType, "Выберите тип продукции\n• Обязательное поле");
            editPanelToolTip.SetToolTip(cmbSupplier, "Выберите поставщика\n• Необязательное поле");
            editPanelToolTip.SetToolTip(btnSave, "Сохранить изменения товара");
            editPanelToolTip.SetToolTip(btnCancel, "Отменить изменения и закрыть панель редактирования");
        }
        private void ValidateNumericField(TextBox textBox, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
                return;

            string text = textBox.Text.Replace(',', '.');

            if (!decimal.TryParse(text, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out decimal value) || value < 0)
            {
                textBox.BackColor = Color.LightPink;
                toolTip.SetToolTip(textBox, $"Некорректное значение {fieldName}\n• Используйте только числа и точку\n• Значение не может быть отрицательным");
            }
            else
            {
                textBox.BackColor = Color.LightGreen;
                toolTip.SetToolTip(textBox, $"Корректное значение {fieldName}");
            }
        }

        private void SetRequiredFieldStyle(TextBox textBox)
        {
            textBox.Enter += (s, e) =>
            {
                if (textBox.BackColor != Color.LightPink)
                    textBox.BackColor = Color.LightYellow;
            };

            textBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.BackColor = Color.LightPink;
                    toolTip.SetToolTip(textBox, "Это поле обязательно для заполнения");
                }
                else if (textBox.BackColor != Color.LightPink && textBox.BackColor != Color.LightGreen)
                {
                    textBox.BackColor = Color.White;
                }
            };
        }
        private void LoadProducts()
        {
            try
            {
                string connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=УПКузнецов1;Integrated Security=True";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                SELECT 
                    p.ProductID,
                    p.Name AS 'Название товара',
                    p.Article AS 'Артикул',
                    p.MinPartnerPrice AS 'Мин. цена',
                    p.RollWidth AS 'Ширина рулона',
                    pt.TypeName AS 'Тип продукции',
                    part.CompanyName AS 'Поставщик'
                FROM Products p
                INNER JOIN ProductTypes pt ON p.ProductTypeID = pt.ProductTypeID
                LEFT JOIN Partners part ON p.PartnerID = part.PartnerID
                ORDER BY p.ProductID";

                    productsTable = new DataTable();
                    new SqlDataAdapter(query, conn).Fill(productsTable);

                    dataGridView.DataSource = productsTable;

                    if (dataGridView.Columns["Мин. цена"] != null)
                    {
                        dataGridView.Columns["Мин. цена"].DefaultCellStyle.Format = "F2";
                        dataGridView.Columns["Мин. цена"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }

                    if (dataGridView.Columns["Ширина рулона"] != null)
                    {
                        dataGridView.Columns["Ширина рулона"].DefaultCellStyle.Format = "F3";
                        dataGridView.Columns["Ширина рулона"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }

                    LoadComboBoxData(conn);
                }

                HideEditPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadComboBoxData(SqlConnection conn)
        {
            string typesQuery = "SELECT ProductTypeID, TypeName FROM ProductTypes";
            DataTable typesTable = new DataTable();
            new SqlDataAdapter(typesQuery, conn).Fill(typesTable);

            cmbProductType.Items.Clear();
            foreach (DataRow row in typesTable.Rows)
            {
                cmbProductType.Items.Add(new ComboBoxItem
                {
                    Text = row.Field<string>("TypeName"),
                    Value = row.Field<int>("ProductTypeID")
                });
            }

            string suppliersQuery = "SELECT PartnerID, CompanyName FROM Partners";
            DataTable suppliersTable = new DataTable();
            new SqlDataAdapter(suppliersQuery, conn).Fill(suppliersTable);

            cmbSupplier.Items.Clear();
            foreach (DataRow row in suppliersTable.Rows)
            {
                cmbSupplier.Items.Add(new ComboBoxItem
                {
                    Text = row.Field<string>("CompanyName"),
                    Value = row.Field<int>("PartnerID")
                });
            }
        }

        private void ShowEditPanel(bool isEdit)
        {
            isEditMode = isEdit;
            TextBox txtId = editPanel.Controls.OfType<TextBox>()
                .FirstOrDefault(t => t.Name == "txtId" || t.ReadOnly);

            if (isEdit)
            {
                if (dataGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите товар для редактирования", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataGridViewRow selectedRow = dataGridView.SelectedRows[0];
                currentProductId = (int)selectedRow.Cells["ProductID"].Value;
                if (txtId != null)
                    txtId.Text = currentProductId.ToString();

                txtName.Text = selectedRow.Cells["Название товара"].Value.ToString();
                txtArticle.Text = selectedRow.Cells["Артикул"].Value.ToString();
                txtMinPrice.Text = selectedRow.Cells["Мин. цена"].Value.ToString();
                txtRollWidth.Text = selectedRow.Cells["Ширина рулона"].Value?.ToString() ?? "";

                string productType = selectedRow.Cells["Тип продукции"].Value.ToString();
                foreach (ComboBoxItem item in cmbProductType.Items)
                {
                    if (item.Text == productType)
                    {
                        cmbProductType.SelectedItem = item;
                        break;
                    }
                }

                if (selectedRow.Cells["Поставщик"].Value != DBNull.Value)
                {
                    string supplier = selectedRow.Cells["Поставщик"].Value.ToString();
                    foreach (ComboBoxItem item in cmbSupplier.Items)
                    {
                        if (item.Text == supplier)
                        {
                            cmbSupplier.SelectedItem = item;
                            break;
                        }
                    }
                }
                else
                {
                    cmbSupplier.SelectedIndex = -1;
                }
            }
            else
            {
                currentProductId = -1;
                if (txtId != null)
                    txtId.Text = "Авто";

                txtName.Text = "";
                txtArticle.Text = "";
                txtMinPrice.Text = "";
                txtRollWidth.Text = "";
                if (cmbProductType.Items.Count > 0) cmbProductType.SelectedIndex = 0;
                if (cmbSupplier.Items.Count > 0) cmbSupplier.SelectedIndex = 0;
            }

            editPanel.Visible = true;
        }

        private void HideEditPanel()
        {
            editPanel.Visible = false;
            isEditMode = false;
            currentProductId = -1;
        }

        private void SaveProduct()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название товара!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtArticle.Text))
            {
                MessageBox.Show("Введите артикул товара!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtArticle.Focus();
                return;
            }

            string priceText = txtMinPrice.Text.Trim();
            priceText = priceText.Replace(',', '.');

            if (!decimal.TryParse(priceText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out decimal minPrice))
            {
                MessageBox.Show("Введите корректное значение цены (например, 123.45)!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMinPrice.Focus();
                return;
            }
            if (minPrice < 0)
            {
                MessageBox.Show("Цена не может быть отрицательной!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMinPrice.Focus();
                return;
            }
            decimal? rollWidth = null;
            if (!string.IsNullOrWhiteSpace(txtRollWidth.Text))
            {
                string widthText = txtRollWidth.Text.Trim().Replace(',', '.');
                if (!decimal.TryParse(widthText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out decimal width) || width < 0)
                {
                    MessageBox.Show("Введите корректное значение ширины (например, 2.5)!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtRollWidth.Focus();
                    return;
                }
                rollWidth = width;
            }
            if (cmbProductType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип продукции!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbProductType.Focus();
                return;
            }

            object typeValue = ((ComboBoxItem)cmbProductType.SelectedItem).Value;
            int productTypeId;
            try
            {
                productTypeId = Convert.ToInt32(typeValue);
            }
            catch
            {
                MessageBox.Show("Неверное значение типа продукции.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            object partnerId = DBNull.Value;
            if (cmbSupplier.SelectedItem is ComboBoxItem sItem)
            {
                partnerId = sItem.Value ?? DBNull.Value;
            }

            string connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=УПКузнецов1;Integrated Security=True";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = isEditMode
                        ? @"UPDATE Products 
                    SET Name = @Name, Article = @Article, MinPartnerPrice = @MinPartnerPrice, 
                        RollWidth = @RollWidth, ProductTypeID = @ProductTypeID, PartnerID = @PartnerID
                    WHERE ProductID = @ProductID"
                        : @"INSERT INTO Products (Name, Article, MinPartnerPrice, RollWidth, ProductTypeID, PartnerID)
                    VALUES (@Name, @Article, @MinPartnerPrice, @RollWidth, @ProductTypeID, @PartnerID)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Article", txtArticle.Text.Trim());
                        cmd.Parameters.AddWithValue("@MinPartnerPrice", minPrice);
                        if (rollWidth.HasValue)
                            cmd.Parameters.AddWithValue("@RollWidth", rollWidth.Value);
                        else
                            cmd.Parameters.AddWithValue("@RollWidth", DBNull.Value);

                        cmd.Parameters.AddWithValue("@ProductTypeID", productTypeId);
                        if (partnerId != null && partnerId != DBNull.Value)
                            cmd.Parameters.AddWithValue("@PartnerID", partnerId);
                        else
                            cmd.Parameters.AddWithValue("@PartnerID", DBNull.Value);

                        if (isEditMode)
                            cmd.Parameters.AddWithValue("@ProductID", currentProductId);

                        int affected = cmd.ExecuteNonQuery();

                        if (affected > 0)
                        {
                            MessageBox.Show(
                                isEditMode ? "Товар успешно обновлён!" : "Товар успешно добавлен!",
                                "Успех",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );
                            LoadProducts();
                            HideEditPanel();
                        }
                        else
                        {
                            MessageBox.Show("Изменений не произошло. Проверьте введённые данные.", "Предупреждение",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Ошибка базы данных:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Неожиданная ошибка:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void DeleteProduct()
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите товар для удаления", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow selectedRow = dataGridView.SelectedRows[0];
            int productId = (int)selectedRow.Cells["ProductID"].Value;
            string productName = selectedRow.Cells["Название товара"].Value.ToString();

            if (MessageBox.Show($"Вы уверены, что хотите удалить товар '{productName}'?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                string connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=УПКузнецов1;Integrated Security=True";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string deleteMaterialsQuery = "DELETE FROM ProductMaterials WHERE ProductID = @ProductID";
                        using (SqlCommand materialsCmd = new SqlCommand(deleteMaterialsQuery, conn))
                        {
                            materialsCmd.Parameters.AddWithValue("@ProductID", productId);
                            int materialsDeleted = materialsCmd.ExecuteNonQuery();
                            if (materialsDeleted > 0)
                            {
                            }
                        }
                        string deleteProductQuery = "DELETE FROM Products WHERE ProductID = @ProductID";
                        using (SqlCommand productCmd = new SqlCommand(deleteProductQuery, conn))
                        {
                            productCmd.Parameters.AddWithValue("@ProductID", productId);
                            int result = productCmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                MessageBox.Show("Товар успешно удален", "Успех",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadProducts();
                            }
                            else
                            {
                                MessageBox.Show("Товар не был удален. Возможно, он уже был удален ранее.",
                                    "Информация",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 547)
                        {
                            MessageBox.Show($"Невозможно удалить товар '{productName}'.\n\n" +
                                "Возможные причины:\n" +
                                "• Товар используется в других таблицах базы данных\n" +
                                "• Есть связанные записи, которые не могут быть удалены\n\n" +
                                "Обратитесь к администратору базы данных для детальной информации.",
                                "Ошибка удаления",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show($"Ошибка базы данных: {ex.Message}",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Неожиданная ошибка: {ex.Message}",
                            "Ошибка",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
    public static class NavigationManager
    {
        private static Stack<Form> navigationStack = new Stack<Form>();

        public static void NavigateTo(Form currentForm, Form nextForm)
        {
            navigationStack.Push(currentForm);
            currentForm.Hide();
            nextForm.Show();
        }

        public static void GoBack(Form currentForm)
        {
            if (navigationStack.Count > 0)
            {
                Form previousForm = navigationStack.Pop();
                currentForm.Hide();
                previousForm.Show();
            }
            else
            {
                currentForm.Close();
            }
        }

        public static bool CanGoBack()
        {
            return navigationStack.Count > 0;
        }
    }

}