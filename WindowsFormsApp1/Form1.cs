using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private FlowLayoutPanel flpProducts;
        private TextBox txtSearch;
        private ComboBox cmbSort;
        private ComboBox cmbFilterType;
        private Label lblUser;
        private Button btnLogout;
        private Button btnHistory;
        private Label lblSort;
        private PictureBox userPhotoBox;
        private PictureBox logoBox;
        private bool _isGuest = false;
        private ComboBox cmbFilterSupplier;

        public bool IsGuest
        {
            get => _isGuest;
            set
            {
                _isGuest = value;
                UpdateControlsVisibility();
            }
        }

        public Form1()
        {
            InitializeComponent();
            SetupForm();
            LoadProducts();
        }

        private void SetupForm()
        {
            this.Text = "Продукция компании";
            this.Size = new Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Icon = Properties.Resources.AppIcon;
            logoBox = new PictureBox
            {
                Size = new Size(80, 80),
                Location = new Point(20, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Properties.Resources.logo
            };

            userPhotoBox = new PictureBox
            {
                Size = new Size(50, 50),
                Location = new Point(110, 15),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblUser = new Label
            {
                Text = GetUserInfoText(),
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 96, 51),
                Location = new Point(170, 20),
                AutoSize = true
            };

            btnLogout = new Button
            {
                Text = "Выход",
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(45, 96, 51),
                Size = new Size(100, 40),
                Location = new Point(850, 10),
                FlatStyle = FlatStyle.Flat
            };
            btnLogout.Click += (s, e) =>
            {
                if (MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        this.Hide();
                        LoginForm login = new LoginForm();
                        login.Show();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка выхода: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };
            btnHistory = new Button
            {
                Text = "История входов",
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(45, 96, 51),
                Size = new Size(150, 40),
                Location = new Point(680, 10),
                FlatStyle = FlatStyle.Flat
            };
            btnHistory.Click += (s, e) =>
            {
                HistoryForm historyForm = new HistoryForm();
                historyForm.ShowDialog();
            };

            Button btnManageProducts = new Button
            {
                Text = "Управление товарами",
                Font = new Font("Gabriola", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 96, 51),
                ForeColor = Color.White,
                Size = new Size(160, 35),
                Location = new Point(620, 150),
                FlatStyle = FlatStyle.Flat,
                Visible = UserSession.Role?.ToLower() == "администратор"
            };
            Button btnRefresh = new Button
            {
                Text = "Обновить",
                Font = new Font("Gabriola", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 96, 51),
                ForeColor = Color.White,
                Size = new Size(120, 35),
                Location = new Point(800, 150),
                FlatStyle = FlatStyle.Flat,
                Visible = UserSession.Role?.ToLower() == "администратор"
            };
            Button btnProductMaterials = new Button
            {
                Text = "Материалы продукции",
                Font = new Font("Gabriola", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 96, 51),
                ForeColor = Color.White,
                Size = new Size(160, 35),
                Location = new Point(450, 150),
                FlatStyle = FlatStyle.Flat,
                Visible = !IsGuest && UserSession.Role?.ToLower() != "гость"
            };

            btnProductMaterials.Click += (s, e) =>
            {
                ProductMaterialsForm materialsForm = new ProductMaterialsForm();
                NavigationManager.NavigateTo(this, materialsForm);
            };

            this.Controls.Add(btnProductMaterials);

            btnManageProducts.Click += (s, e) =>
            {
                AdminProductsForm adminForm = new AdminProductsForm();
                adminForm.ShowDialog();
            };
            btnRefresh.Click += (s, e) => LoadProducts();

            this.Controls.AddRange(new Control[] { btnManageProducts, btnRefresh });

            this.Controls.Add(btnManageProducts);
            Label lblSearch = new Label
            {
                Text = "Поиск:",
                Location = new Point(20, 100),
                Font = new Font("Gabriola", 14),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Location = new Point(80, 95),
                Size = new Size(200, 35),
                Font = new Font("Gabriola", 14)
            };
            txtSearch.TextChanged += (s, e) => LoadProducts();
            lblSort = new Label
            {
                Text = "Сортировка:",
                Location = new Point(300, 100),
                Font = new Font("Gabriola", 14),
                AutoSize = true
            };

            cmbSort = new ComboBox
            {
                Location = new Point(400, 95),
                Size = new Size(200, 35),
                Font = new Font("Gabriola", 14),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            if (!IsGuest)
            {
                cmbSort.Items.AddRange(new[] {
            "Без сортировки",
            "По возрастанию",
            "По убыванию"
        });
                cmbSort.SelectedIndex = 0;
                cmbSort.SelectedIndexChanged += (s, e) => LoadProducts();
            }
            Label lblFilterType = new Label
            {
                Text = "Тип продукции:",
                Location = new Point(620, 100),
                Font = new Font("Gabriola", 14),
                AutoSize = true
            };

            cmbFilterType = new ComboBox
            {
                Location = new Point(750, 95),
                Size = new Size(150, 35),
                Font = new Font("Gabriola", 14),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilterType.SelectedIndexChanged += (s, e) => LoadProducts();
            Label lblFilterSupplier = new Label
            {
                Text = "Поставщик:",
                Location = new Point(20, 150),
                Font = new Font("Gabriola", 14),
                AutoSize = true
            };

            cmbFilterSupplier = new ComboBox
            {
                Location = new Point(120, 145),
                Size = new Size(200, 35),
                Font = new Font("Gabriola", 14),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilterSupplier.SelectedIndexChanged += (s, e) => LoadProducts();

            flpProducts = new FlowLayoutPanel
            {
                Location = new Point(20, 200),
                Size = new Size(940, 500),
                AutoScroll = true,
                BackColor = Color.FromArgb(187, 217, 178),
                BorderStyle = BorderStyle.FixedSingle,
                WrapContents = true
            };

            this.Controls.AddRange(new Control[]
            {
        logoBox,
        userPhotoBox,
        lblUser,
        btnLogout,
        btnHistory,
        lblSearch,
        txtSearch,
        lblSort,
        cmbSort,
        lblFilterType,
        cmbFilterType,
        lblFilterSupplier,
        cmbFilterSupplier,
        flpProducts
            });

            SetUserPhoto();
            UpdateControlsVisibility();
            this.FormClosed += (s, e) => Application.Exit();
        }

        private string GetUserInfoText()
        {
            if (IsGuest)
            {
                return "Гость (Гость)";
            }
            else
            {
                return $"{UserSession.FullName} ({UserSession.Role})";
            }
        }

        private void SetUserPhoto()
        {
            if (IsGuest)
            {
                userPhotoBox.Image = Properties.Resources.GuestPhoto;
            }
            else
            {
                switch (UserSession.Role?.ToLower())
                {
                    case "администратор":
                    case "admin":
                        userPhotoBox.Image = Properties.Resources.AdminPhoto;
                        break;
                    case "менеджер":
                    case "manager":
                        userPhotoBox.Image = Properties.Resources.ManagerPhoto;
                        break;
                    case "клиент":
                    case "client":
                        userPhotoBox.Image = Properties.Resources.ClientPhoto;
                        break;
                    default:
                        userPhotoBox.Image = Properties.Resources.GuestPhoto;
                        break;
                }
            }
        }


        private void UpdateControlsVisibility()
        {
            if (lblSort != null) lblSort.Visible = !IsGuest;
            if (cmbSort != null) cmbSort.Visible = !IsGuest;
            if (btnHistory != null) btnHistory.Visible = !IsGuest;

            if (lblUser != null)
                lblUser.Text = GetUserInfoText();

            SetUserPhoto();
        }

        private void LoadProducts()
        {
            try
            {
                if (IsGuest && cmbSort != null)
                {
                    cmbSort.Visible = false;
                    if (lblSort != null) lblSort.Visible = false;
                }

                flpProducts.Controls.Clear();

                string connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=УПКузнецов1;Integrated Security=True";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string productsQuery = @"
                SELECT 
                    p.ProductID,
                    p.Name AS ProductName,
                    p.Article,
                    p.MinPartnerPrice,
                    p.RollWidth,
                    pt.TypeName AS ProductType,
                    pt.ProductTypeID,
                    pt.Coefficient,
                    part.CompanyName AS SupplierName
                FROM Products p
                INNER JOIN ProductTypes pt ON p.ProductTypeID = pt.ProductTypeID
                LEFT JOIN Partners part ON p.PartnerID = part.PartnerID";

                    DataTable productsTable = new DataTable();
                    new SqlDataAdapter(productsQuery, conn).Fill(productsTable);

                    string materialsQuery = @"
                SELECT 
                    pm.ProductID,
                    pm.RequiredQuantity,
                    m.UnitPrice,
                    m.StockQuantity
                FROM ProductMaterials pm
                INNER JOIN Materials m ON pm.MaterialID = m.MaterialID
                ORDER BY pm.ProductID";

                    DataTable materialsTable = new DataTable();
                    new SqlDataAdapter(materialsQuery, conn).Fill(materialsTable);

                    if (cmbFilterType.Items.Count == 0)
                    {
                        cmbFilterType.Items.Add("Все типы");
                        var types = productsTable.AsEnumerable()
                            .Select(r => r.Field<string>("ProductType"))
                            .Distinct();
                        foreach (var type in types)
                            cmbFilterType.Items.Add(type);
                        cmbFilterType.SelectedIndex = 0;
                    }

                    if (cmbFilterSupplier.Items.Count == 0)
                    {
                        cmbFilterSupplier.Items.Add("Все поставщики");
                        var suppliers = productsTable.AsEnumerable()
                            .Where(r => r["SupplierName"] != DBNull.Value)
                            .Select(r => r.Field<string>("SupplierName"))
                            .Distinct();
                        foreach (var supplier in suppliers)
                            cmbFilterSupplier.Items.Add(supplier);
                        cmbFilterSupplier.SelectedIndex = 0;
                    }

                    var filteredProducts = productsTable.AsEnumerable();
                    string search = txtSearch.Text.Trim().ToLower();

                    if (!string.IsNullOrEmpty(search))
                    {
                        filteredProducts = filteredProducts.Where(p =>
                            p.Field<string>("ProductName").ToLower().Contains(search) ||
                            p.Field<string>("Article").ToLower().Contains(search) ||
                            p.Field<string>("ProductType").ToLower().Contains(search) ||
                            (p["SupplierName"] != DBNull.Value && p.Field<string>("SupplierName").ToLower().Contains(search))
                        );
                    }

                    if (cmbFilterType.SelectedIndex > 0)
                    {
                        string selectedType = cmbFilterType.SelectedItem.ToString();
                        filteredProducts = filteredProducts.Where(p => p.Field<string>("ProductType") == selectedType);
                    }

                    if (cmbFilterSupplier.SelectedIndex > 0)
                    {
                        string selectedSupplier = cmbFilterSupplier.SelectedItem.ToString();
                        filteredProducts = filteredProducts.Where(p =>
                            p["SupplierName"] != DBNull.Value &&
                            p.Field<string>("SupplierName") == selectedSupplier);
                    }

                    var productsWithData = filteredProducts.Select(p =>
                    {
                        int productId = p.Field<int>("ProductID");
                        var productMaterials = materialsTable.AsEnumerable()
                            .Where(m => m.Field<int>("ProductID") == productId)
                            .ToList();

                        int availableCount = 0;

                        if (productMaterials.Any())
                        {
                            var counts = productMaterials.Select(m =>
                            {
                                object stockObj = m["StockQuantity"];
                                object requiredObj = m["RequiredQuantity"];

                                decimal stock = 0;
                                decimal required = 0;

                                if (stockObj != DBNull.Value)
                                    stock = Convert.ToDecimal(stockObj);

                                if (requiredObj != DBNull.Value)
                                    required = Convert.ToDecimal(requiredObj);

                                if (required <= 0)
                                    return int.MaxValue;

                                return (int)Math.Floor(stock / required);
                            });

                            availableCount = counts.Min();
                        }

                        decimal productCost = 0;
                        object priceObj = p["MinPartnerPrice"];
                        if (priceObj != DBNull.Value)
                            productCost = Convert.ToDecimal(priceObj);

                        productCost = Math.Max(0, Math.Round(productCost, 2));

                        return new
                        {
                            ProductRow = p,
                            ProductId = productId,
                            AvailableCount = availableCount,
                            Cost = productCost
                        };
                    }).ToList();
                    if (!IsGuest && cmbSort.SelectedIndex > 0)
                    {
                        try
                        {
                            if (cmbSort.SelectedIndex == 1)
                                productsWithData = productsWithData.OrderBy(p => p.AvailableCount).ToList();
                            else if (cmbSort.SelectedIndex == 2)
                                productsWithData = productsWithData.OrderByDescending(p => p.AvailableCount).ToList();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при сортировке: " + ex.Message,
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    if (!productsWithData.Any())
                    {
                        Label noResults = new Label
                        {
                            Text = "Ничего не найдено",
                            Font = new Font("Gabriola", 16, FontStyle.Bold),
                            ForeColor = Color.Red,
                            AutoSize = true
                        };

                        noResults.Location = new Point(
                            (flpProducts.Width - noResults.Width) / 2,
                            (flpProducts.Height - noResults.Height) / 2
                        );

                        flpProducts.Controls.Add(noResults);
                        return;
                    }

                    foreach (var productData in productsWithData)
                    {
                        var productRow = productData.ProductRow;
                        decimal productCost = productData.Cost;
                        int availableCount = productData.AvailableCount;

                        Panel card = new Panel
                        {
                            Size = new Size(900, 160),
                            BackColor = Color.White,
                            BorderStyle = BorderStyle.FixedSingle,
                            Margin = new Padding(10)
                        };

                        string article = productRow.Field<string>("Article") ?? "—";
                        string minPrice = productRow.Field<decimal>("MinPartnerPrice").ToString("F2");
                        string width = productRow.Field<decimal?>("RollWidth")?.ToString("F3") ?? "—";
                        string productType = productRow.Field<string>("ProductType");
                        string productName = productRow.Field<string>("ProductName");
                        string supplier = productRow["SupplierName"] != DBNull.Value ? productRow.Field<string>("SupplierName") : "—";

                        Label lblInfo = new Label
                        {
                            AutoSize = false,
                            Location = new Point(10, 10),
                            Size = new Size(600, 140),
                            Font = new Font("Gabriola", 14),
                            Text =
                                $"{productType} | {productName}\n" +
                                $"Артикул: {article}\n" +
                                $"Мин. цена партнёра: {minPrice} р\n" +
                                $"Ширина (м): {width}\n" +
                                $"Доступно для производства: {availableCount}\n" +
                                $"Поставщик: {supplier}"
                        };

                        Label lblCost = new Label
                        {
                            AutoSize = false,
                            Location = new Point(620, 40),
                            Size = new Size(260, 80),
                            Font = new Font("Gabriola", 16, FontStyle.Bold),
                            ForeColor = Color.FromArgb(45, 96, 51),
                            TextAlign = ContentAlignment.MiddleRight,
                            Text = $"{productCost:F2} р"
                        };

                        card.Controls.Add(lblInfo);
                        card.Controls.Add(lblCost);
                        flpProducts.Controls.Add(card);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
