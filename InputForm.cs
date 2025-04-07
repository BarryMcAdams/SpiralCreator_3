using System;
using System.Windows.Forms;

namespace SpiralStairPlugin
{
    public class InputForm : Form
    {
        private TextBox txtCenterPoleDia;
        private TextBox txtOverallHeight;
        private TextBox txtOutsideDia;
        private TextBox txtRotationDeg;
        private CheckBox chkClockwise;
        private Button btnOk;
        private Button btnCancel;

        public double CenterPoleDiameter { get; private set; }
        public double OverallHeight { get; private set; }
        public double OutsideDiameter { get; private set; }
        public double RotationDegrees { get; private set; }
        public bool IsClockwise { get; private set; }

        public InputForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Spiral Staircase Parameters";
            this.Width = 300;
            this.Height = 250;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.AcceptButton = btnOk; // Enter key triggers OK button

            Label lblCenterPole = new Label { Text = "Center Pole Dia (in):", Left = 20, Top = 20 };
            txtCenterPoleDia = new TextBox { Left = 150, Top = 20, Width = 100, Text = "6", TextAlign = HorizontalAlignment.Center };

            Label lblHeight = new Label { Text = "Overall Height (in):", Left = 20, Top = 50 };
            txtOverallHeight = new TextBox { Left = 150, Top = 50, Width = 100, Text = "144", TextAlign = HorizontalAlignment.Center };

            Label lblOutsideDia = new Label { Text = "Outside Dia (in):", Left = 20, Top = 80 };
            txtOutsideDia = new TextBox { Left = 150, Top = 80, Width = 100, Text = "60", TextAlign = HorizontalAlignment.Center };

            Label lblRotation = new Label { Text = "Rotation (deg):", Left = 20, Top = 110 };
            txtRotationDeg = new TextBox { Left = 150, Top = 110, Width = 100, Text = "450", TextAlign = HorizontalAlignment.Center };

            chkClockwise = new CheckBox { Text = "Clockwise", Left = 150, Top = 140, Checked = true };

            btnOk = new Button { Text = "OK", Left = 80, Top = 180, DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "Cancel", Left = 160, Top = 180, DialogResult = DialogResult.Cancel };

            btnOk.Click += (s, e) => ValidateAndSetValues();

            this.Controls.AddRange(new Control[] { lblCenterPole, txtCenterPoleDia, lblHeight, txtOverallHeight,
                lblOutsideDia, txtOutsideDia, lblRotation, txtRotationDeg, chkClockwise, btnOk, btnCancel });
        }

        private void ValidateAndSetValues()
        {
            if (double.TryParse(txtCenterPoleDia.Text, out double cpDia) && cpDia > 0 &&
                double.TryParse(txtOverallHeight.Text, out double height) && height > 0 &&
                double.TryParse(txtOutsideDia.Text, out double outDia) && outDia > 0 &&
                double.TryParse(txtRotationDeg.Text, out double rotDeg) && rotDeg > 0)
            {
                CenterPoleDiameter = cpDia;
                OverallHeight = height;
                OutsideDiameter = outDia;
                RotationDegrees = rotDeg;
                IsClockwise = chkClockwise.Checked;
            }
            else
            {
                MessageBox.Show("Please enter valid positive numbers.", "Invalid Input");
                DialogResult = DialogResult.None;
            }
        }
    }
}