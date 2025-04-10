using System;
using System.Windows.Forms;
using System.Drawing;

namespace SpiralStairPlugin
{
    public partial class InputForm : Form
    {
        public double CenterPoleDia { get; private set; }
        public double OverallHeight { get; private set; }
        public double OutsideDia { get; private set; }
        public double RotationDeg { get; private set; }
        public bool IsClockwise { get; private set; }
        public bool IsCanceled { get; private set; }

        private TextBox txtCenterPoleDia;
        private TextBox txtOverallHeight;
        private TextBox txtOutsideDia;
        private TextBox txtRotationDeg;
        private RadioButton rbClockwise;
        private RadioButton rbCounterClockwise;
        private Label lblCenterPoleDia;
        private Label lblOverallHeight;
        private Label lblOutsideDia;
        private Label lblRotationDeg;
        private Label lblRotationDirection;
        private Button btnOK;
        private Button btnCancel;

        public InputForm(double? initialCenterPoleDia = null, double? initialOverallHeight = null,
                        double? initialOutsideDia = null, double? initialRotationDeg = null,
                        bool initialIsClockwise = true)
        {
            InitializeComponents();

            // Pre-fill text boxes with initial values
            if (initialCenterPoleDia.HasValue)
                txtCenterPoleDia.Text = initialCenterPoleDia.Value.ToString();
            if (initialOverallHeight.HasValue)
                txtOverallHeight.Text = initialOverallHeight.Value.ToString();
            if (initialOutsideDia.HasValue)
                txtOutsideDia.Text = initialOutsideDia.Value.ToString();
            if (initialRotationDeg.HasValue)
                txtRotationDeg.Text = initialRotationDeg.Value.ToString();
            if (initialIsClockwise)
                rbClockwise.Checked = true;
            else
                rbCounterClockwise.Checked = true;
        }

        private void InitializeComponents()
        {
            this.Text = "Spiral Staircase Input";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(300, 220);

            // Labels and TextBoxes
            int yPos = 20;
            int labelWidth = 150;
            int textBoxWidth = 100;

            // Center Pole Diameter
            lblCenterPoleDia = new Label
            {
                Text = "Center Pole Diameter (in):",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, 20)
            };
            txtCenterPoleDia = new TextBox
            {
                Location = new Point(170, yPos),
                Size = new Size(textBoxWidth, 20)
            };
            yPos += 30;

            // Overall Height
            lblOverallHeight = new Label
            {
                Text = "Overall Height (in):",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, 20)
            };
            txtOverallHeight = new TextBox
            {
                Location = new Point(170, yPos),
                Size = new Size(textBoxWidth, 20)
            };
            yPos += 30;

            // Outside Diameter
            lblOutsideDia = new Label
            {
                Text = "Outside Diameter (in):",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, 20)
            };
            txtOutsideDia = new TextBox
            {
                Location = new Point(170, yPos),
                Size = new Size(textBoxWidth, 20)
            };
            yPos += 30;

            // Rotation Degrees
            lblRotationDeg = new Label
            {
                Text = "Rotation Degrees:",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, 20)
            };
            txtRotationDeg = new TextBox
            {
                Location = new Point(170, yPos),
                Size = new Size(textBoxWidth, 20)
            };
            yPos += 30;

            // Rotation Direction (Clockwise/Counterclockwise)
            lblRotationDirection = new Label
            {
                Text = "Rotation Direction:",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, 20)
            };
            rbClockwise = new RadioButton
            {
                Text = "Clockwise",
                Location = new Point(170, yPos),
                Size = new Size(80, 20),
                Enabled = false // Dead for now
            };
            rbCounterClockwise = new RadioButton
            {
                Text = "Counterclockwise",
                Location = new Point(250, yPos),
                Size = new Size(100, 20),
                Enabled = false // Dead for now
            };
            yPos += 30;

            // Buttons
            btnOK = new Button
            {
                Text = "OK",
                Location = new Point(50, yPos),
                Size = new Size(75, 30)
            };
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(150, yPos),
                Size = new Size(75, 30)
            };
            btnCancel.Click += BtnCancel_Click;

            // Enable Enter key to submit the form
            this.AcceptButton = btnOK;

            // Add controls to the form
            this.Controls.AddRange(new Control[] { lblCenterPoleDia, txtCenterPoleDia, lblOverallHeight, txtOverallHeight, lblOutsideDia, txtOutsideDia, lblRotationDeg, txtRotationDeg, lblRotationDirection, rbClockwise, rbCounterClockwise, btnOK, btnCancel });
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                CenterPoleDia = double.Parse(txtCenterPoleDia.Text);
                OverallHeight = double.Parse(txtOverallHeight.Text);
                OutsideDia = double.Parse(txtOutsideDia.Text);
                RotationDeg = double.Parse(txtRotationDeg.Text);
                IsClockwise = rbClockwise.Checked;

                // Basic validation
                if (CenterPoleDia <= 0 || OverallHeight <= 0 || OutsideDia <= 0)
                {
                    MessageBox.Show("All dimensions must be positive.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (CenterPoleDia >= OutsideDia)
                {
                    MessageBox.Show("Center pole diameter must be less than outside diameter.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter valid numeric values.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            IsCanceled = true;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}