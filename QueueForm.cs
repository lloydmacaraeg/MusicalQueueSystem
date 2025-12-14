using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QUEUESYSTEM
{
    public partial class QueueForm : Form
    {
        private readonly string connString;
        private Panel panelCategories;

        public QueueForm(string connectionString)
        {
            connString = connectionString;
            InitializeComponent();
            LoadQueueData();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 800);
            this.Text = "Queue Management - Musical Instruments by Category";
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblTitle = new Label
            {
                Text = "QUEUE SYSTEM - ORDERS BY CATEGORY",
                Location = new Point(20, 20),
                Size = new Size(1150, 30),
                Font = new Font("Arial", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            panelCategories = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(1150, 650),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };

            Button btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(950, 720),
                Size = new Size(100, 35),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnRefresh.Click += (s, e) => LoadQueueData();

            Button btnClose = new Button
            {
                Text = "Close",
                Location = new Point(1060, 720),
                Size = new Size(100, 35),
                BackColor = Color.LightCoral,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitle, panelCategories, btnRefresh, btnClose });
        }

        // ✅ Counts distinct customers per category (prevents inflated counts due to many order items)
        private (int totalOrders, int inQueue, int completed) GetCategoryStats(MySqlConnection conn, int categoryID)
        {
            using var cmd = new MySqlCommand(@"
                SELECT
                    COUNT(*) AS TotalOrders,
                    SUM(CASE WHEN Status='In Queue' THEN 1 ELSE 0 END) AS InQueue,
                    SUM(CASE WHEN Status='Completed' THEN 1 ELSE 0 END) AS Completed
                FROM (
                    SELECT DISTINCT c.CustomerID, c.Status
                    FROM Customers c
                    JOIN Orders o ON c.CustomerID = o.CustomerID
                    JOIN OrderItems oi ON o.OrderID = oi.OrderID
                    JOIN Instruments i ON oi.InstrumentID = i.InstrumentID
                    WHERE i.CategoryID = @catID
                ) x;", conn);

            cmd.Parameters.AddWithValue("@catID", categoryID);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return (0, 0, 0);

            return (
                r["TotalOrders"] == DBNull.Value ? 0 : Convert.ToInt32(r["TotalOrders"]),
                r["InQueue"] == DBNull.Value ? 0 : Convert.ToInt32(r["InQueue"]),
                r["Completed"] == DBNull.Value ? 0 : Convert.ToInt32(r["Completed"])
            );

        }

        private void LoadQueueData()
        {
            panelCategories.Controls.Clear();
            int yPos = 10;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // Get all categories
                    MySqlCommand cmdCat = new MySqlCommand(
                        "SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName", conn);

                    using var reader = cmdCat.ExecuteReader();
                    DataTable categories = new DataTable();
                    categories.Load(reader);

                    foreach (DataRow catRow in categories.Rows)
                    {
                        int categoryID = Convert.ToInt32(catRow["CategoryID"]);
                        string categoryName = catRow["CategoryName"].ToString();

                        // 1) Grid data: ONLY "In Queue" customers
                        MySqlCommand cmdQueue = new MySqlCommand(@"
                            SELECT 
                                c.CustomerNumber,
                                c.CustomerName,
                                c.Status,
                                COUNT(DISTINCT oi.OrderItemID) as ItemCount,
                                SUM(oi.Sales) as TotalSales,
                                o.OrderDate
                            FROM Customers c
                            JOIN Orders o ON c.CustomerID = o.CustomerID
                            JOIN OrderItems oi ON o.OrderID = oi.OrderID
                            JOIN Instruments i ON oi.InstrumentID = i.InstrumentID
                            WHERE i.CategoryID = @catID AND c.Status = 'In Queue'
                            GROUP BY c.CustomerID, c.CustomerNumber, c.CustomerName, c.Status, o.OrderDate
                            ORDER BY o.OrderDate;", conn);

                        cmdQueue.Parameters.AddWithValue("@catID", categoryID);

                        DataTable queueData = new DataTable();
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmdQueue))
                        {
                            adapter.Fill(queueData);
                        }

                        // 2) Stats: In Queue + Completed (from DB)
                        var stats = GetCategoryStats(conn, categoryID);

                        // Create category panel
                        Panel categoryPanel = CreateCategoryPanel(
                            categoryName,
                            queueData,
                            stats.totalOrders,
                            stats.inQueue,
                            stats.completed,
                            yPos
                        );

                        panelCategories.Controls.Add(categoryPanel);
                        yPos += categoryPanel.Height + 15;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading queue data: " + ex.Message);
                }
            }
        }

        private Panel CreateCategoryPanel(string categoryName, DataTable queueData,
            int totalOrders, int inQueue, int completed, int yPosition)
        {
            Panel panel = new Panel
            {
                Location = new Point(10, yPosition),
                Size = new Size(1110, 200),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };

            // Category Header
            Label lblCategory = new Label
            {
                Text = $"📦 {categoryName.ToUpper()}",
                Location = new Point(10, 10),
                Size = new Size(500, 25),
                Font = new Font("Courier", 12, FontStyle.Bold),
                ForeColor = Color.Black
            };

            // ✅ IMPORTANT: do NOT recount queueData here (that causes mismatch)
            Label lblStats = new Label
            {
                Text = $"Total Orders: {totalOrders} | In Queue: {inQueue} | Completed: {completed}",
                Location = new Point(520, 12),
                Size = new Size(580, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.DarkGreen,
                TextAlign = ContentAlignment.MiddleRight
            };

            // Queue DataGridView
            DataGridView dgvQueue = new DataGridView
            {
                Location = new Point(10, 45),
                Size = new Size(1085, 140),
                DataSource = queueData,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };

            // Format columns
            if (dgvQueue.Columns["TotalSales"] != null)
                dgvQueue.Columns["TotalSales"].DefaultCellStyle.Format = "C2";

            if (dgvQueue.Columns["OrderDate"] != null)
                dgvQueue.Columns["OrderDate"].DefaultCellStyle.Format = "MM/dd/yyyy HH:mm";

            // Color code status
            dgvQueue.CellFormatting += (s, e) =>
            {
                if (dgvQueue.Columns[e.ColumnIndex].Name == "Status")
                {
                    if (e.Value != null && e.Value.ToString() == "In Queue")
                        e.CellStyle.BackColor = Color.LightYellow;
                    else if (e.Value != null && e.Value.ToString() == "Completed")
                        e.CellStyle.BackColor = Color.LightGreen;
                }
            };

            // Double-click to view order details
            dgvQueue.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    string customerNumber = dgvQueue.Rows[e.RowIndex].Cells["CustomerNumber"].Value.ToString();
                    ShowOrderDetails(customerNumber);
                }
            };

            // Button to mark as complete
            Button btnComplete = new Button
            {
                Text = "Mark Selected as OUT",
                Location = new Point(920, 8),
                Size = new Size(170, 30),
                BackColor = Color.Orange,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            btnComplete.Click += (s, e) =>
            {
                try
                {
                    if (dgvQueue.SelectedRows.Count > 0)
                    {
                        if (dgvQueue.SelectedRows[0].Cells["CustomerNumber"] == null)
                        {
                            MessageBox.Show("CustomerNumber column not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        var cellValue = dgvQueue.SelectedRows[0].Cells["CustomerNumber"].Value;
                        if (cellValue == null)
                        {
                            MessageBox.Show("Customer number is empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        string custNo = cellValue.ToString();
                        string custName = dgvQueue.SelectedRows[0].Cells["CustomerName"].Value?.ToString() ?? "Unknown";

                        DialogResult result = MessageBox.Show(
                            $"Mark customer '{custName}' ({custNo}) as OUT/Completed?",
                            "Confirm",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            MarkCustomerAsComplete(custNo);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a customer from the queue!", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in button click:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            };

            panel.Controls.AddRange(new Control[] { lblCategory, lblStats, dgvQueue, btnComplete });
            return panel;
        }

        private void MarkCustomerAsComplete(string customerNumber)
        {
            if (string.IsNullOrEmpty(customerNumber))
            {
                MessageBox.Show("Invalid customer number!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // Get CustomerID
                    MySqlCommand cmdGetID = new MySqlCommand("SELECT CustomerID FROM Customers WHERE CustomerNumber = @custNo", conn);
                    cmdGetID.Parameters.AddWithValue("@custNo", customerNumber);
                    object result = cmdGetID.ExecuteScalar();

                    if (result == null)
                    {
                        MessageBox.Show($"Customer '{customerNumber}' not found in database!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    int customerID = Convert.ToInt32(result);

                    // Update customer + orders status
                    MySqlCommand cmdUpdate = new MySqlCommand(@"
                        UPDATE Customers SET Status = 'Completed' WHERE CustomerID = @custID;
                        UPDATE Orders SET Status = 'Completed' WHERE CustomerID = @custID;", conn);

                    cmdUpdate.Parameters.AddWithValue("@custID", customerID);

                    int rowsAffected = cmdUpdate.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        conn.Close();

                        MessageBox.Show($"Customer '{customerNumber}' marked as completed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        try
                        {
                            ShowOrderDetails(customerNumber);
                        }
                        catch (Exception detailEx)
                        {
                            MessageBox.Show($"Customer marked as OUT successfully, but could not show receipt:\n\n{detailEx.Message}",
                                "Partial Success",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }

                        LoadQueueData();
                    }
                    else
                    {
                        MessageBox.Show("No records were updated!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating customer status:\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowOrderDetails(string customerNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(customerNumber))
                {
                    MessageBox.Show("Invalid customer number!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrEmpty(connString))
                {
                    MessageBox.Show("Database connection string is not set!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                OrderDetailsForm detailsForm = new OrderDetailsForm(connString, customerNumber);
                detailsForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing order details:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
