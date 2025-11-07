using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;

namespace UnitTestProject1
{
    [TestClass]
    public class MaterialCalculatorTests
    {
        private const string ConnectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=УПКузнецов1;Integrated Security=True";
        private (int productTypeId, int materialTypeId, decimal coefficient, decimal defectPercentage) GetTestData()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                string productQuery = "SELECT TOP 1 ProductTypeID, Coefficient FROM ProductTypes";
                using (SqlCommand productCmd = new SqlCommand(productQuery, conn))
                using (SqlDataReader productReader = productCmd.ExecuteReader())
                {
                    if (productReader.Read())
                    {
                        int productTypeId = productReader.GetInt32(0);
                        decimal coefficient = productReader.GetDecimal(1);
                        productReader.Close();
                        string materialQuery = "SELECT TOP 1 MaterialTypeID, DefectPercentage FROM MaterialTypes";
                        using (SqlCommand materialCmd = new SqlCommand(materialQuery, conn))
                        using (SqlDataReader materialReader = materialCmd.ExecuteReader())
                        {
                            if (materialReader.Read())
                            {
                                int materialTypeId = materialReader.GetInt32(0);
                                decimal defectPercentage = materialReader.GetDecimal(1);
                                return (productTypeId, materialTypeId, coefficient, defectPercentage);
                            }
                        }
                    }
                }
            }
            throw new Exception("Не удалось получить тестовые данные из базы");
        }

        [TestMethod]
        public void CalculateRequiredMaterial_NormalParameters_ReturnsPositiveValue()
        {
            // Arrange
            var testData = GetTestData();
            int productQuantity = 10;
            double rollWidth = 1.5;
            double length = 2.0;
            double stockQuantity = 5;

            // Act
            int result = WindowsFormsApp1.MaterialCalculator.CalculateRequiredMaterial(
                testData.productTypeId, testData.materialTypeId, productQuantity, rollWidth, length, stockQuantity);

            // Assert
            Assert.IsTrue(result >= 0, "Результат должен быть неотрицательным");
        }

        [TestMethod]
        public void CalculateRequiredMaterial_ZeroStock_ReturnsRequiredAmount()
        {
            // Arrange
            var testData = GetTestData();
            int productQuantity = 5;
            double rollWidth = 1.0;
            double length = 1.0;
            double stockQuantity = 0;

            // Act
            int result = WindowsFormsApp1.MaterialCalculator.CalculateRequiredMaterial(
                testData.productTypeId, testData.materialTypeId, productQuantity, rollWidth, length, stockQuantity);

            // Assert
            Assert.IsTrue(result > 0, "При нулевом запасе должен возвращаться положительный результат");
        }

        [TestMethod]
        public void CalculateRequiredMaterial_SufficientStock_ReturnsZero()
        {
            // Arrange
            var testData = GetTestData();
            int productQuantity = 1;
            double rollWidth = 1.0;
            double length = 1.0;
            double stockQuantity = 1000;

            // Act
            int result = WindowsFormsApp1.MaterialCalculator.CalculateRequiredMaterial(
                testData.productTypeId, testData.materialTypeId, productQuantity, rollWidth, length, stockQuantity);

            // Assert
            Assert.AreEqual(0, result, "При достаточном запасе должен возвращаться 0");
        }

        [TestMethod]
        public void CalculateRequiredMaterial_NonExistentProductType_ReturnsError()
        {
            // Arrange
            int nonExistentProductTypeId = 99999;
            var testData = GetTestData();
            int productQuantity = 1;
            double rollWidth = 1.0;
            double length = 1.0;
            double stockQuantity = 0;

            // Act
            int result = WindowsFormsApp1.MaterialCalculator.CalculateRequiredMaterial(
                nonExistentProductTypeId, testData.materialTypeId, productQuantity, rollWidth, length, stockQuantity);

            // Assert
            Assert.AreEqual(-1, result, "Для несуществующего типа продукции должен возвращаться -1");
        }

        [TestMethod]
        public void CalculateRequiredMaterial_NegativeProductQuantity_ReturnsError()
        {
            // Arrange
            var testData = GetTestData();
            int productQuantity = -5;
            double rollWidth = 1.0;
            double length = 1.0;
            double stockQuantity = 0;

            // Act
            int result = WindowsFormsApp1.MaterialCalculator.CalculateRequiredMaterial(
                testData.productTypeId, testData.materialTypeId, productQuantity, rollWidth, length, stockQuantity);

            // Assert
            Assert.AreEqual(-1, result, "При отрицательном количестве продукции должен возвращаться -1");
        }

        [TestMethod]
        public void CalculateRequiredMaterial_ZeroProductQuantity_ReturnsError()
        {
            // Arrange
            var testData = GetTestData();
            int productQuantity = 0;
            double rollWidth = 1.0;
            double length = 1.0;
            double stockQuantity = 0;

            // Act
            int result = WindowsFormsApp1.MaterialCalculator.CalculateRequiredMaterial(
                testData.productTypeId, testData.materialTypeId, productQuantity, rollWidth, length, stockQuantity);

            // Assert
            Assert.AreEqual(-1, result, "При нулевом количестве продукции должен возвращаться -1");
        }

        [TestMethod]
        public void CalculateRequiredMaterial_NegativeStock_ReturnsError()
        {
            // Arrange
            var testData = GetTestData();
            int productQuantity = 1;
            double rollWidth = 1.0;
            double length = 1.0;
            double stockQuantity = -10;

            // Act
            int result = WindowsFormsApp1.MaterialCalculator.CalculateRequiredMaterial(
                testData.productTypeId, testData.materialTypeId, productQuantity, rollWidth, length, stockQuantity);

            // Assert
            Assert.AreEqual(-1, result, "При отрицательном запасе должен возвращаться -1");
        }

        [TestMethod]
        public void CalculateRequiredMaterial_ZeroRollWidth_ReturnsError()
        {
            // Arrange
            var testData = GetTestData();
            int productQuantity = 1;
            double rollWidth = 0;
            double length = 1.0;
            double stockQuantity = 0;

            // Act
            int result = WindowsFormsApp1.MaterialCalculator.CalculateRequiredMaterial(
                testData.productTypeId, testData.materialTypeId, productQuantity, rollWidth, length, stockQuantity);

            // Assert
            Assert.AreEqual(-1, result, "При нулевой ширине рулона должен возвращаться -1");
        }

        [TestMethod]
        public void CalculateRequiredMaterial_ExactStockMatch_ReturnsZero()
        {
            // Arrange
            var testData = GetTestData();
            double materialPerUnit = 1.0 * 1.0 * (double)testData.coefficient;
            double totalMaterialNeeded = materialPerUnit * 1;
            double materialWithDefect = totalMaterialNeeded * (1 + (double)testData.defectPercentage);
            double exactStock = materialWithDefect;

            int productQuantity = 1;
            double rollWidth = 1.0;
            double length = 1.0;
            double stockQuantity = exactStock;

            // Act
            int result = WindowsFormsApp1.MaterialCalculator.CalculateRequiredMaterial(
                testData.productTypeId, testData.materialTypeId, productQuantity, rollWidth, length, stockQuantity);

            // Assert
            Assert.AreEqual(0, result, "При точном соответствии запаса и потребности должен возвращаться 0");
        }

        [TestMethod]
        public void CalculateRequiredMaterial_LargeValues_ReturnsCorrectCalculation()
        {
            // Arrange
            var testData = GetTestData();
            int productQuantity = 1000;
            double rollWidth = 2.5;
            double length = 3.0;
            double stockQuantity = 500;

            // Act
            int result = WindowsFormsApp1.MaterialCalculator.CalculateRequiredMaterial(
                testData.productTypeId, testData.materialTypeId, productQuantity, rollWidth, length, stockQuantity);

            // Assert
            Assert.IsTrue(result >= 0, "При больших значениях должен возвращаться корректный неотрицательный результат");
        }
    }
}