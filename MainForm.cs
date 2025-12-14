using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace QUEUESYSTEM
{
    public partial class MainForm : Form
    {
        private string connString = "Server=localhost;Database=MusicStoreQueue;Uid=root;Pwd=12Bernerslee23@;";
        private DataTable cartItems;
        private int currentCustomerID = -1;

        public MainForm()
        {
            InitializeComponent();
            InitializeCart();
            LoadInstruments();
            GenerateCustomerNumber();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(900, 700);
            this.Text = "Musical Instrument Store - Order Entry";
            this.StartPosition = FormStartPosition.CenterScreen;

            // Customer Number
            Label lblCustNo = new Label { Text = "Customer No:", Location = new Point(20, 20), Size = new Size(100, 20) };
            TextBox txtCustNo = new TextBox { Name = "txtCustomerNo", Location = new Point(130, 18), Size = new Size(150, 25), ReadOnly = true };

            // Customer Name
            Label lblCustName = new Label { Text = "Customer Name:", Location = new Point(20, 55), Size = new Size(100, 20) };
            TextBox txtCustName = new TextBox { Name = "txtCustomerName", Location = new Point(130, 53), Size = new Size(250, 25) };

            // Instrument Selection
            Label lblCategory = new Label { Text = "Category:", Location = new Point(20, 95), Size = new Size(100, 20) };
            ComboBox cmbCategory = new ComboBox { Name = "cmbCategory", Location = new Point(130, 93), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged;

            Label lblInstrument = new Label { Text = "Instrument:", Location = new Point(20, 130), Size = new Size(100, 20) };
            ComboBox cmbInstrument = new ComboBox { Name = "cmbInstrument", Location = new Point(130, 128), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbInstrument.SelectedIndexChanged += CmbInstrument_SelectedIndexChanged;

            Label lblPrice = new Label { Name = "lblPrice", Text = "Price: $0.00", Location = new Point(400, 130), Size = new Size(150, 20), Font = new Font("Arial", 10, FontStyle.Bold) };

            Label lblQty = new Label { Text = "Quantity:", Location = new Point(20, 165), Size = new Size(100, 20) };
            NumericUpDown numQty = new NumericUpDown { Name = "numQuantity", Location = new Point(130, 163), Size = new Size(80, 25), Minimum = 1, Maximum = 100, Value = 1 };

            Button btnAdd = new Button { Text = "Add to Cart", Location = new Point(220, 160), Size = new Size(120, 30), BackColor = Color.LightGreen };
            btnAdd.Click += BtnAdd_Click;

            // Cart Items DataGridView
            Label lblCart = new Label { Text = "Items in Cart:", Location = new Point(20, 210), Size = new Size(150, 20), Font = new Font("Arial", 10, FontStyle.Bold) };
            DataGridView dgvCart = new DataGridView
            {
                Name = "dgvCart",
                Location = new Point(20, 240),
                Size = new Size(840, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            Button btnRemove = new Button { Text = "Remove Selected", Location = new Point(20, 550), Size = new Size(120, 30), BackColor = Color.LightCoral };
            btnRemove.Click += BtnRemove_Click;

            // Total Label
            Label lblTotal = new Label { Name = "lblTotal", Text = "Total: $0.00", Location = new Point(700, 550), Size = new Size(160, 25), Font = new Font("Arial", 12, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight };

            // Action Buttons
            Button btnSubmit = new Button { Text = "Submit Order", Location = new Point(620, 600), Size = new Size(120, 40), BackColor = Color.DodgerBlue, ForeColor = Color.White, Font = new Font("Arial", 10, FontStyle.Bold) };
            btnSubmit.Click += BtnSubmit_Click;

            Button btnQueue = new Button { Text = "View Queue", Location = new Point(750, 600), Size = new Size(110, 40), BackColor = Color.Orange, Font = new Font("Arial", 10, FontStyle.Bold) };
            btnQueue.Click += BtnQueue_Click;

            Button btnClear = new Button { Text = "Clear Cart", Location = new Point(480, 600), Size = new Size(120, 40), BackColor = Color.Gray, ForeColor = Color.White };
            btnClear.Click += BtnClear_Click;

            this.Controls.AddRange(new Control[] { lblCustNo, txtCustNo, lblCustName, txtCustName, lblCategory, cmbCategory, lblInstrument, cmbInstrument, lblPrice, lblQty, numQty, btnAdd, lblCart, dgvCart, btnRemove, lblTotal, btnSubmit, btnQueue, btnClear });
        }

        private void InitializeCart()
        {
            cartItems = new DataTable();
            cartItems.Columns.Add("InstrumentID", typeof(int));
            cartItems.Columns.Add("Instrument", typeof(string));
            cartItems.Columns.Add("Description", typeof(string));
            cartItems.Columns.Add("Quantity", typeof(int));
            cartItems.Columns.Add("Price", typeof(decimal));
            cartItems.Columns.Add("Sales", typeof(decimal));

            DataGridView dgv = (DataGridView)this.Controls["dgvCart"];
            dgv.DataSource = cartItems;
            dgv.Columns["InstrumentID"].Visible = false;
        }

        private void LoadInstruments()
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT CategoryID, CategoryName FROM Categories ORDER BY CategoryName", conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    ComboBox cmb = (ComboBox)this.Controls["cmbCategory"];
                    cmb.Items.Clear();
                    cmb.Items.Add(new { CategoryID = 0, CategoryName = "-- Select Category --" });

                    while (reader.Read())
                    {
                        cmb.Items.Add(new { CategoryID = reader.GetInt32(0), CategoryName = reader.GetString(1) });
                    }

                    cmb.DisplayMember = "CategoryName";
                    cmb.ValueMember = "CategoryID";
                    cmb.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading categories: " + ex.Message);
                }
            }
        }

        private void GenerateCustomerNumber()
        {
            TextBox txt = (TextBox)this.Controls["txtCustomerNo"];

            // Get the next customer number from database
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) + 1 FROM Customers", conn);
                    int nextNumber = Convert.ToInt32(cmd.ExecuteScalar());
                    txt.Text = "Customer " + nextNumber;
                }
                catch (Exception)
                {
                    // Fallback if database query fails
                    txt.Text = "Customer 1";
                }
            }
        }

        private void CmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmbCat = (ComboBox)sender;
            ComboBox cmbInst = (ComboBox)this.Controls["cmbInstrument"];

            if (cmbCat.SelectedIndex <= 0)
            {
                cmbInst.Items.Clear();
                return;
            }

            dynamic selected = cmbCat.SelectedItem;
            int categoryID = selected.CategoryID;

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT InstrumentID, InstrumentName, Description, Price FROM Instruments WHERE CategoryID = @catID ORDER BY InstrumentName", conn);
                    cmd.Parameters.AddWithValue("@catID", categoryID);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    cmbInst.Items.Clear();
                    cmbInst.Items.Add(new { InstrumentID = 0, InstrumentName = "-- Select Instrument --", Description = "", Price = 0m });

                    while (reader.Read())
                    {
                        cmbInst.Items.Add(new
                        {
                            InstrumentID = reader.GetInt32(0),
                            InstrumentName = reader.GetString(1),
                            Description = reader.GetString(2),
                            Price = reader.GetDecimal(3)
                        });
                    }

                    cmbInst.DisplayMember = "InstrumentName";
                    cmbInst.ValueMember = "InstrumentID";
                    cmbInst.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading instruments: " + ex.Message);
                }
            }
        }

        private void CmbInstrument_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            Label lblPrice = (Label)this.Controls["lblPrice"];

            if (cmb.SelectedIndex <= 0)
            {
                lblPrice.Text = "Price: $0.00";
                return;
            }

            dynamic selected = cmb.SelectedItem;
            lblPrice.Text = $"Price: ${selected.Price:N2}";
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            ComboBox cmbInst = (ComboBox)this.Controls["cmbInstrument"];
            NumericUpDown numQty = (NumericUpDown)this.Controls["numQuantity"];

            if (cmbInst.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select an instrument!");
                return;
            }

            dynamic selected = cmbInst.SelectedItem;
            int qty = (int)numQty.Value;
            decimal price = selected.Price;
            decimal sales = qty * price;

            cartItems.Rows.Add(selected.InstrumentID, selected.InstrumentName, selected.Description, qty, price, sales);
            UpdateTotal();

            MessageBox.Show("Item added to cart!");
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            DataGridView dgv = (DataGridView)this.Controls["dgvCart"];
            if (dgv.SelectedRows.Count > 0)
            {
                dgv.Rows.Remove(dgv.SelectedRows[0]);
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (DataRow row in cartItems.Rows)
            {
                total += Convert.ToDecimal(row["Sales"]);
            }

            Label lbl = (Label)this.Controls["lblTotal"];
            lbl.Text = $"Total: ${total:N2}";
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            TextBox txtName = (TextBox)this.Controls["txtCustomerName"];
            TextBox txtNo = (TextBox)this.Controls["txtCustomerNo"];

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter customer name!");
                return;
            }

            if (cartItems.Rows.Count == 0)
            {
                MessageBox.Show("Cart is empty!");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                MySqlTransaction trans = null;
                try
                {
                    conn.Open();
                    trans = conn.BeginTransaction();

                    // Insert Customer
                    MySqlCommand cmdCust = new MySqlCommand("INSERT INTO Customers (CustomerNumber, CustomerName, Status) VALUES (@no, @name, 'In Queue'); SELECT LAST_INSERT_ID();", conn, trans);
                    cmdCust.Parameters.AddWithValue("@no", txtNo.Text);
                    cmdCust.Parameters.AddWithValue("@name", txtName.Text);
                    currentCustomerID = Convert.ToInt32(cmdCust.ExecuteScalar());

                    // Calculate total
                    decimal total = 0;
                    foreach (DataRow row in cartItems.Rows)
                        total += Convert.ToDecimal(row["Sales"]);

                    // Insert Order
                    MySqlCommand cmdOrder = new MySqlCommand("INSERT INTO Orders (CustomerID, TotalAmount, Status) VALUES (@custID, @total, 'Pending'); SELECT LAST_INSERT_ID();", conn, trans);
                    cmdOrder.Parameters.AddWithValue("@custID", currentCustomerID);
                    cmdOrder.Parameters.AddWithValue("@total", total);
                    int orderID = Convert.ToInt32(cmdOrder.ExecuteScalar());

                    // Insert Order Items
                    foreach (DataRow row in cartItems.Rows)
                    {
                        MySqlCommand cmdItem = new MySqlCommand("INSERT INTO OrderItems (OrderID, InstrumentID, Quantity, Price, Sales) VALUES (@ordID, @instID, @qty, @price, @sales)", conn, trans);
                        cmdItem.Parameters.AddWithValue("@ordID", orderID);
                        cmdItem.Parameters.AddWithValue("@instID", row["InstrumentID"]);
                        cmdItem.Parameters.AddWithValue("@qty", row["Quantity"]);
                        cmdItem.Parameters.AddWithValue("@price", row["Price"]);
                        cmdItem.Parameters.AddWithValue("@sales", row["Sales"]);
                        cmdItem.ExecuteNonQuery();
                    }

                    trans.Commit();
                    MessageBox.Show($"Order submitted successfully!\nCustomer: {txtName.Text}\nTotal: ${total:N2}", "Success");
                    BtnClear_Click(null, null);
                    GenerateCustomerNumber();
                }
                catch (Exception ex)
                {
                    trans?.Rollback();
                    MessageBox.Show("Error submitting order: " + ex.Message);
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            cartItems.Clear();
            ((TextBox)this.Controls["txtCustomerName"]).Clear();
            ((ComboBox)this.Controls["cmbCategory"]).SelectedIndex = 0;
            ((ComboBox)this.Controls["cmbInstrument"]).Items.Clear();
            ((NumericUpDown)this.Controls["numQuantity"]).Value = 1;
            UpdateTotal();
        }

        private void BtnQueue_Click(object sender, EventArgs e)
        {
            QueueForm queueForm = new QueueForm(connString);
            queueForm.ShowDialog();
        }
    }
}