using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;


namespace WindowsFormsApp1
{
    public partial class ProductMaterialsForm : Form
    {
        private Button btnBack;
        private Button btnCalculate;
        private ComboBox cmbProductType;
        private ComboBox cmbMaterialType;
        private TextBox txtProductQuantity;
        private TextBox txtRollWidth;
        private TextBox txtLength;
        private TextBox txtStockQuantity;
        private Label lblResult;

        public ProductMaterialsForm()
        {
            InitializeComponent();
            SetupForm();
            LoadComboBoxData();
        }

        private void SetupForm()
        {
            this.Text = "Калькулятор материалов";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Icon = Properties.Resources.AppIcon;

            Label lblTitle = new Label
            {
                Text = "Калькулятор необходимых материалов",
                Font = new Font("Gabriola", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 96, 51),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            Label lblDescription = new Label
            {
                Text = "Расчет количества материала для производства продукции",
                Font = new Font("Gabriola", 12),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(20, 60)
            };
            GroupBox grpInput = new GroupBox
            {
                Text = "Параметры расчета",
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 96, 51),
                Location = new Point(20, 100),
                Size = new Size(640, 250),
                BackColor = Color.FromArgb(240, 245, 240)
            };
            Label lblProductType = new Label { Text = "Тип продукции:", Location = new Point(20, 40), AutoSize = true, Font = new Font("Gabriola", 12) };
            cmbProductType = new ComboBox { Location = new Point(150, 38), Size = new Size(200, 25), Font = new Font("Gabriola", 12), DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblMaterialType = new Label { Text = "Тип материала:", Location = new Point(20, 80), AutoSize = true, Font = new Font("Gabriola", 12) };
            cmbMaterialType = new ComboBox { Location = new Point(150, 78), Size = new Size(200, 25), Font = new Font("Gabriola", 12), DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblProductQuantity = new Label { Text = "Количество продукции:", Location = new Point(20, 120), AutoSize = true, Font = new Font("Gabriola", 12) };
            txtProductQuantity = new TextBox { Location = new Point(180, 118), Size = new Size(100, 25), Font = new Font("Gabriola", 12) };
            txtProductQuantity.Text = "1";

            Label lblRollWidth = new Label { Text = "Ширина рулона (м):", Location = new Point(300, 120), AutoSize = true, Font = new Font("Gabriola", 12) };
            txtRollWidth = new TextBox { Location = new Point(440, 118), Size = new Size(80, 25), Font = new Font("Gabriola", 12) };
            txtRollWidth.Text = "1,000";

            Label lblLength = new Label { Text = "Длина (м):", Location = new Point(20, 160), AutoSize = true, Font = new Font("Gabriola", 12) };
            txtLength = new TextBox { Location = new Point(100, 158), Size = new Size(80, 25), Font = new Font("Gabriola", 12) };
            txtLength.Text = "1,000";

            Label lblStockQuantity = new Label { Text = "Материал на складе:", Location = new Point(200, 160), AutoSize = true, Font = new Font("Gabriola", 12) };
            txtStockQuantity = new TextBox { Location = new Point(350, 158), Size = new Size(100, 25), Font = new Font("Gabriola", 12) };
            txtStockQuantity.Text = "0";

            btnCalculate = new Button
            {
                Text = "Рассчитать",
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 96, 51),
                ForeColor = Color.White,
                Size = new Size(150, 40),
                Location = new Point(480, 158),
                FlatStyle = FlatStyle.Flat
            };

            lblResult = new Label
            {
                Text = "Результат: ",
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 96, 51),
                Location = new Point(20, 200),
                Size = new Size(600, 40),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            grpInput.Controls.AddRange(new Control[]
            {
                lblProductType, cmbProductType,
                lblMaterialType, cmbMaterialType,
                lblProductQuantity, txtProductQuantity,
                lblRollWidth, txtRollWidth,
                lblLength, txtLength,
                lblStockQuantity, txtStockQuantity,
                btnCalculate,
                lblResult
            });

            btnBack = new Button
            {
                Text = "← Назад",
                Font = new Font("Gabriola", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 96, 51),
                ForeColor = Color.White,
                Size = new Size(120, 40),
                Location = new Point(540, 370),
                FlatStyle = FlatStyle.Flat
            };
            btnBack.Click += (s, e) => NavigationManager.GoBack(this);

            btnCalculate.Click += (s, e) => CalculateRequiredMaterial();
            var toolTip = new ToolTip();
            toolTip.SetToolTip(txtProductQuantity, "Количество единиц продукции для производства");
            toolTip.SetToolTip(txtRollWidth, "Ширина рулона в метрах (например: 1,500)");
            toolTip.SetToolTip(txtLength, "Длина в метрах (например: 2,000)");
            toolTip.SetToolTip(txtStockQuantity, "Количество материала уже имеющееся на складе");

            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblDescription,
                grpInput,
                btnBack
            });

            this.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    NavigationManager.GoBack(this);
                }
            };

            this.FormClosed += (s, e) => this.Dispose();
        }

        private void LoadComboBoxData()
        {
            try
            {
                string connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=УПКузнецов1;Integrated Security=True";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string productTypesQuery = "SELECT ProductTypeID, TypeName, Coefficient FROM ProductTypes";
                    DataTable productTypesTable = new DataTable();
                    new SqlDataAdapter(productTypesQuery, conn).Fill(productTypesTable);

                    cmbProductType.Items.Clear();
                    foreach (DataRow row in productTypesTable.Rows)
                    {
                        cmbProductType.Items.Add(new ComboBoxItem
                        {
                            Text = $"{row.Field<string>("TypeName")} (коэф: {row.Field<decimal>("Coefficient")})",
                            Value = row.Field<int>("ProductTypeID")
                        });
                    }
                    string materialTypesQuery = "SELECT MaterialTypeID, TypeName, DefectPercentage FROM MaterialTypes";
                    DataTable materialTypesTable = new DataTable();
                    new SqlDataAdapter(materialTypesQuery, conn).Fill(materialTypesTable);

                    cmbMaterialType.Items.Clear();
                    foreach (DataRow row in materialTypesTable.Rows)
                    {
                        cmbMaterialType.Items.Add(new ComboBoxItem
                        {
                            Text = $"{row.Field<string>("TypeName")} (брак: {row.Field<decimal>("DefectPercentage"):P1})",
                            Value = row.Field<int>("MaterialTypeID")
                        });
                    }
                    if (cmbProductType.Items.Count > 0)
                        cmbProductType.SelectedIndex = 0;
                    if (cmbMaterialType.Items.Count > 0)
                        cmbMaterialType.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateRequiredMaterial()
        {
            try
            {
                if (cmbProductType.SelectedItem == null || cmbMaterialType.SelectedItem == null)
                {
                    MessageBox.Show("Выберите тип продукции и тип материала!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtProductQuantity.Text) ||
                    string.IsNullOrWhiteSpace(txtRollWidth.Text) ||
                    string.IsNullOrWhiteSpace(txtLength.Text) ||
                    string.IsNullOrWhiteSpace(txtStockQuantity.Text))
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                int productTypeId = (int)((ComboBoxItem)cmbProductType.SelectedItem).Value;
                int materialTypeId = (int)((ComboBoxItem)cmbMaterialType.SelectedItem).Value;
                int productQuantity = int.Parse(txtProductQuantity.Text);
                double rollWidth = double.Parse(txtRollWidth.Text, CultureInfo.CurrentCulture);
                double length = double.Parse(txtLength.Text, CultureInfo.CurrentCulture);
                double stockQuantity = double.Parse(txtStockQuantity.Text, CultureInfo.CurrentCulture);
                if (productQuantity <= 0 || rollWidth <= 0 || length <= 0 || stockQuantity < 0)
                {
                    MessageBox.Show("Все числовые значения должны быть положительными!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                int result = MaterialCalculator.CalculateRequiredMaterial(
                    productTypeId, materialTypeId, productQuantity, rollWidth, length, stockQuantity);

                if (result == -1)
                {
                    lblResult.Text = "Ошибка расчета - проверьте корректность входных данных";
                    lblResult.ForeColor = Color.Red;
                }
                else if (result == 0)
                {
                    lblResult.Text = "Материала на складе достаточно, закупка не требуется";
                    lblResult.ForeColor = Color.FromArgb(0, 128, 0);
                }
                else
                {
                    lblResult.Text = $"Необходимо закупить {result} единиц материала";
                    lblResult.ForeColor = Color.FromArgb(0, 0, 128);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Проверьте правильность ввода числовых значений!\nИспользуйте запятую для дробных чисел.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка расчета: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public static class MaterialCalculator
    {
        /// <summary>
        /// Расчет количества необходимого материала для производства
        /// </summary>
        /// <param name="productTypeId">ID типа продукции</param>
        /// <param name="materialTypeId">ID типа материала</param>
        /// <param name="productQuantity">Количество получаемой продукции</param>
        /// <param name="rollWidth">Ширина рулона (параметр 1)</param>
        /// <param name="length">Длина (параметр 2)</param>
        /// <param name="stockQuantity">Количество материала на складе</param>
        /// <returns>Количество необходимого материала или -1 при ошибке</returns>
        public static int CalculateRequiredMaterial(int productTypeId, int materialTypeId,
            int productQuantity, double rollWidth, double length, double stockQuantity)
        {
            try
            {
                if (productQuantity <= 0 || rollWidth <= 0 || length <= 0 || stockQuantity < 0)
                    return -1;

                string connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=УПКузнецов1;Integrated Security=True";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string productTypeQuery = "SELECT Coefficient FROM ProductTypes WHERE ProductTypeID = @ProductTypeID";
                    SqlCommand productCmd = new SqlCommand(productTypeQuery, conn);
                    productCmd.Parameters.AddWithValue("@ProductTypeID", productTypeId);
                    object productCoefficientObj = productCmd.ExecuteScalar();

                    if (productCoefficientObj == null || productCoefficientObj == DBNull.Value)
                        return -1;

                    double productCoefficient = Convert.ToDouble(productCoefficientObj);
                    string materialTypeQuery = "SELECT DefectPercentage FROM MaterialTypes WHERE MaterialTypeID = @MaterialTypeID";
                    SqlCommand materialCmd = new SqlCommand(materialTypeQuery, conn);
                    materialCmd.Parameters.AddWithValue("@MaterialTypeID", materialTypeId);
                    object defectPercentageObj = materialCmd.ExecuteScalar();

                    if (defectPercentageObj == null || defectPercentageObj == DBNull.Value)
                        return -1;

                    double defectPercentage = Convert.ToDouble(defectPercentageObj);
                    double materialPerUnit = rollWidth * length * productCoefficient;
                    double totalMaterialNeeded = materialPerUnit * productQuantity;
                    double materialWithDefect = totalMaterialNeeded * (1 + defectPercentage);
                    double materialToPurchase = materialWithDefect - stockQuantity;
                    if (materialToPurchase <= 0)
                        return 0;
                    return (int)Math.Ceiling(materialToPurchase);
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }

    public class ComboBoxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

}