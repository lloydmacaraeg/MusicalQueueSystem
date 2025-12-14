using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace QUEUESYSTEM
{
    public class TestConnectionForm : Form
    {
        private string connString = "Server=localhost;Database=MusicStoreQueue;Uid=root;Pwd=your_password;";
        private TextBox txtResults;

        public TestConnectionForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(600, 500);
            this.Text = "Database Connection Test";
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblTitle = new Label
            {
                Text = "Database Connection & Data Test",
                Location = new Point(20, 20),
                Size = new Size(560, 30),
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            txtResults = new TextBox
            {
                Location = new Point(20, 60),
                Size = new Size(560, 330),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9),
                ReadOnly = true
            };

            Button btnTest = new Button
            {
                Text = "Run Test",
                Location = new Point(250, 410),
                Size = new Size(100, 40),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnTest.Click += BtnTest_Click;

            this.Controls.AddRange(new Control[] { lblTitle, txtResults, btnTest });
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            txtResults.Clear();
            txtResults.AppendText("Starting Database Test...\r\n");
            txtResults.AppendText("================================\r\n\r\n");

            // Test 1: Connection
            txtResults.AppendText("TEST 1: Database Connection\r\n");
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    txtResults.AppendText("✓ Connection Successful!\r\n");
                    txtResults.AppendText($"  Server Version: {conn.ServerVersion}\r\n\r\n");
                }
                catch (Exception ex)
                {
                    txtResults.AppendText($"✗ Connection Failed!\r\n  Error: {ex.Message}\r\n\r\n");
                    return;
                }
            }

            // Test 2: Categories
            txtResults.AppendText("TEST 2: Categories Table\r\n");
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM Categories", conn);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    txtResults.AppendText($"✓ Found {count} categories\r\n\r\n");
                }
                catch (Exception ex)
                {
                    txtResults.AppendText($"✗ Error: {ex.Message}\r\n\r\n");
                }
            }

            // Test 3: Instruments
            txtResults.AppendText("TEST 3: Instruments Table\r\n");
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM Instruments", conn);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    txtResults.AppendText($"✓ Found {count} instruments\r\n\r\n");
                }
                catch (Exception ex)
                {
                    txtResults.AppendText($"✗ Error: {ex.Message}\r\n\r\n");
                }
            }

            // Test 4: Customers
            txtResults.AppendText("TEST 4: Customers Table\r\n");
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(@"
                        SELECT CustomerNumber, CustomerName, Status 
                        FROM Customers 
                        ORDER BY CustomerID DESC 
                        LIMIT 5", conn);

                    MySqlDataReader reader = cmd.ExecuteReader();
                    int count = 0;
                    while (reader.Read())
                    {
                        count++;
                        txtResults.AppendText($"  {reader["CustomerNumber"]} - {reader["CustomerName"]} [{reader["Status"]}]\r\n");
                    }

                    if (count == 0)
                        txtResults.AppendText("  No customers found\r\n");

                    txtResults.AppendText("\r\n");
                }
                catch (Exception ex)
                {
                    txtResults.AppendText($"✗ Error: {ex.Message}\r\n\r\n");
                }
            }

            // Test 5: Test Query for Customer 1
            txtResults.AppendText("TEST 5: Query Customer 1 Details\r\n");
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(@"
                        SELECT 
                            c.CustomerNumber,
                            c.CustomerName,
                            c.Status,
                            o.OrderDate,
                            o.TotalAmount
                        FROM Customers c
                        JOIN Orders o ON c.CustomerID = o.CustomerID
                        WHERE c.CustomerNumber = 'Customer 1'
                        LIMIT 1", conn);

                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtResults.AppendText($"✓ Found Customer 1\r\n");
                        txtResults.AppendText($"  Name: {reader["CustomerName"]}\r\n");
                        txtResults.AppendText($"  Status: {reader["Status"]}\r\n");
                        txtResults.AppendText($"  Total: ${reader["TotalAmount"]}\r\n");
                    }
                    else
                    {
                        txtResults.AppendText("✗ Customer 1 not found\r\n");
                    }
                    txtResults.AppendText("\r\n");
                }
                catch (Exception ex)
                {
                    txtResults.AppendText($"✗ Error: {ex.Message}\r\n\r\n");
                }
            }

            txtResults.AppendText("================================\r\n");
            txtResults.AppendText("Test Complete!\r\n");
        }
    }
}