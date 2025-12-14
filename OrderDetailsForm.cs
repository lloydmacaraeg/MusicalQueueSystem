using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace QUEUESYSTEM
{
    public partial class OrderDetailsForm : Form
    {
        private string connString;
        private string customerNumber;

        // Control references as class fields
        private Label lblCustomerNo;
        private Label lblCustomerName;
        private Label lblOrderDate;
        private Label lblStatus;
        private Label lblTotalAmount;
        private DataGridView dgvOrderItems;

        public OrderDetailsForm(string connectionString, string custNumber)
        {
            // Set connection info first
            connString = connectionString;
            customerNumber = custNumber;

            // Initialize all controls - this MUST complete first
            InitializeComponent();

            // Now it's safe to load data
            this.Load += OrderDetailsForm_Load;
        }

        private void OrderDetailsForm_Load(object sender, EventArgs e)
        {
            // Load data after form is fully initialized
            LoadOrderDetails();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(900, 700);
            this.Text = "Order Details - Customer Receipt";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Header Panel
            Panel headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(900, 80),
                BackColor = Color.Navy
            };

            Label lblTitle = new Label
            {
                Text = "🎵 MUSICAL INSTRUMENT STORE",
                Location = new Point(20, 10),
                Size = new Size(860, 30),
                Font = new Font("Arial", 18, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblSubtitle = new Label
            {
                Text = "ORDER RECEIPT",
                Location = new Point(20, 45),
                Size = new Size(860, 25),
                Font = new Font("Arial", 14, FontStyle.Regular),
                ForeColor = Color.LightGray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            headerPanel.Controls.AddRange(new Control[] { lblTitle, lblSubtitle });

            // Customer Info Panel
            Panel infoPanel = new Panel
            {
                Location = new Point(20, 100),
                Size = new Size(840, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.AliceBlue
            };

            // Initialize all label controls
            lblCustomerNo = new Label
            {
                Text = "Customer No: Loading...",
                Location = new Point(20, 15),
                Size = new Size(400, 25),
                Font = new Font("Arial", 11, FontStyle.Bold)
            };

            lblCustomerName = new Label
            {
                Text = "Customer Name: Loading...",
                Location = new Point(20, 45),
                Size = new Size(400, 25),
                Font = new Font("Arial", 11, FontStyle.Regular)
            };

            lblOrderDate = new Label
            {
                Text = "Order Date: ",
                Location = new Point(450, 15),
                Size = new Size(370, 25),
                Font = new Font("Arial", 11, FontStyle.Regular),
                TextAlign = ContentAlignment.TopRight
            };

            lblStatus = new Label
            {
                Text = "Status: ",
                Location = new Point(450, 45),
                Size = new Size(370, 25),
                Font = new Font("Arial", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.TopRight
            };

            infoPanel.Controls.AddRange(new Control[] { lblCustomerNo, lblCustomerName, lblOrderDate, lblStatus });

            // Items Header
            Label lblItemsHeader = new Label
            {
                Text = "ORDER ITEMS",
                Location = new Point(20, 195),
                Size = new Size(840, 25),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            // Initialize DataGridView for order items
            dgvOrderItems = new DataGridView
            {
                Location = new Point(20, 230),
                Size = new Size(840, 320),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                GridColor = Color.LightGray,
                Font = new Font("Arial", 10),
                RowHeadersVisible = false
            };

            // Totals Panel
            Panel totalsPanel = new Panel
            {
                Location = new Point(500, 565),
                Size = new Size(360, 60),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow
            };

            Label lblTotalLabel = new Label
            {
                Text = "TOTAL AMOUNT:",
                Location = new Point(10, 20),
                Size = new Size(180, 25),
                Font = new Font("Arial", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };

            lblTotalAmount = new Label
            {
                Text = "$0.00",
                Location = new Point(195, 20),
                Size = new Size(150, 25),
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                TextAlign = ContentAlignment.MiddleLeft
            };

            totalsPanel.Controls.AddRange(new Control[] { lblTotalLabel, lblTotalAmount });

            // Close Button
            Button btnClose = new Button
            {
                Text = "Close",
                Location = new Point(780, 635),
                Size = new Size(80, 35),
                BackColor = Color.LightCoral,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.Click += BtnClose_Click;

            // Add all controls to form
            this.Controls.AddRange(new Control[] {
                headerPanel,
                infoPanel,
                lblItemsHeader,
                dgvOrderItems,
                totalsPanel,
                btnClose
            });
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LoadOrderDetails()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // Get customer and order info
                    MySqlCommand cmdInfo = new MySqlCommand(@"
                        SELECT 
                            c.CustomerNumber,
                            c.CustomerName,
                            c.Status,
                            o.OrderDate,
                            o.TotalAmount
                        FROM Customers c
                        JOIN Orders o ON c.CustomerID = o.CustomerID
                        WHERE c.CustomerNumber = @custNo
                        LIMIT 1", conn);
                    cmdInfo.Parameters.AddWithValue("@custNo", customerNumber);

                    MySqlDataReader reader = cmdInfo.ExecuteReader();
                    if (reader.Read())
                    {
                        // Update all labels with customer info
                        lblCustomerNo.Text = $"Customer No: {reader["CustomerNumber"]}";
                        lblCustomerName.Text = $"Customer Name: {reader["CustomerName"]}";
                        lblOrderDate.Text = $"Order Date: {Convert.ToDateTime(reader["OrderDate"]).ToString("MM/dd/yyyy HH:mm")}";

                        string status = reader["Status"].ToString();
                        lblStatus.Text = $"Status: {status}";
                        lblStatus.ForeColor = status == "Completed" ? Color.Green : Color.Orange;

                        lblTotalAmount.Text = $"${Convert.ToDecimal(reader["TotalAmount"]):N2}";
                    }
                    else
                    {
                        MessageBox.Show("Customer order not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.Close();
                        return;
                    }
                    reader.Close();

                    // Get order items
                    MySqlCommand cmdItems = new MySqlCommand(@"
                        SELECT 
                            oi.Quantity,
                            i.InstrumentName as Item,
                            i.Description,
                            oi.Price,
                            oi.Sales
                        FROM Customers c
                        JOIN Orders o ON c.CustomerID = o.CustomerID
                        JOIN OrderItems oi ON o.OrderID = oi.OrderID
                        JOIN Instruments i ON oi.InstrumentID = i.InstrumentID
                        WHERE c.CustomerNumber = @custNo
                        ORDER BY i.InstrumentName", conn);
                    cmdItems.Parameters.AddWithValue("@custNo", customerNumber);

                    DataTable itemsData = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmdItems))
                    {
                        adapter.Fill(itemsData);
                    }

                    // Check if DataGridView is initialized
                    if (dgvOrderItems == null)
                    {
                        MessageBox.Show("DataGridView not initialized!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    dgvOrderItems.DataSource = itemsData;

                    // Format columns only if they exist
                    if (dgvOrderItems.Columns.Contains("Price"))
                    {
                        dgvOrderItems.Columns["Price"].DefaultCellStyle.Format = "C2";
                        dgvOrderItems.Columns["Price"].HeaderText = "Unit Price";
                    }

                    if (dgvOrderItems.Columns.Contains("Sales"))
                    {
                        dgvOrderItems.Columns["Sales"].DefaultCellStyle.Format = "C2";
                        dgvOrderItems.Columns["Sales"].HeaderText = "Total";
                    }

                    if (dgvOrderItems.Columns.Contains("Quantity"))
                    {
                        dgvOrderItems.Columns["Quantity"].Width = 80;
                    }

                    // Set alternate row colors
                    dgvOrderItems.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
                    dgvOrderItems.DefaultCellStyle.SelectionBackColor = Color.DarkBlue;
                    dgvOrderItems.DefaultCellStyle.SelectionForeColor = Color.White;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading order details:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                        "Database Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}