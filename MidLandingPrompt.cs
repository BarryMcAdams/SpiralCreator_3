using System;
using System.Windows.Forms;
using System.Drawing;

namespace SpiralStairPlugin
{
    public partial class MidLandingPrompt : Form
    {
        public int? MidLandingTreadIndex { get; private set; }
        public bool IgnoreMidLanding { get; private set; }
        public bool GoBack { get; private set; }
        public bool IsCanceled { get; private set; }

        private TextBox txtTreadIndex;
        private Label lblMessage;
        private Label lblTreadIndex;
        private Label lblExceptions;
        private Button btnProceed;
        private Button btnIgnore;
        private Button btnGoBack;
        private Button btnCancel;

        public MidLandingPrompt(double overallHeight, int totalTreads, bool isResidential)
        {
            InitializeComponents(overallHeight, totalTreads, isResidential);
        }

        private void InitializeComponents(double overallHeight, int totalTreads, bool isResidential)
        {
            this.Text = "Mid-Landing Required";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(400, 300);

            int yPos = 20;
            int labelWidth = 350;
            int textBoxWidth = 50;

            // Message about height exceeding the limit
            string codeRef = isResidential ? "IRC R311.7.3" : "IBC 1011.8";
            double heightLimit = isResidential ? 151 : 144;
            lblMessage = new Label
            {
                Text = $"The overall height ({overallHeight:F2}\") exceeds {heightLimit}\", requiring a mid-landing per {codeRef}.",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, 40),
                AutoSize = false
            };
            yPos += 50;

            // Exceptions (for IRC/residential only)
            if (isResidential)
            {
                lblExceptions = new Label
                {
                    Text = "Exceptions: (a) Stairways not within or serving a building, porch, or deck; (b) Stairways leading to non-habitable attics; (c) Stairways leading to crawl spaces.",
                    Location = new Point(20, yPos),
                    Size = new Size(labelWidth, 60),
                    AutoSize = false
                };
                yPos += 70;
            }
            else
            {
                yPos += 20;
            }

            // Tread Index Input
            lblTreadIndex = new Label
            {
                Text = $"Enter the tread index for the mid-landing (1 to {totalTreads - 1}):",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, 20)
            };
            yPos += 30;

            int defaultTreadIndex = (totalTreads - 1) / 2; // Default to middle tread
            txtTreadIndex = new TextBox
            {
                Text = defaultTreadIndex.ToString(),
                Location = new Point(20, yPos),
                Size = new Size(textBoxWidth, 20)
            };
            yPos += 40;

            // Buttons
            btnProceed = new Button
            {
                Text = "Proceed",
                Location = new Point(20, yPos),
                Size = new Size(75, 30)
            };
            btnProceed.Click += BtnProceed_Click;

            btnIgnore = new Button
            {
                Text = "Ignore and Continue",
                Location = new Point(110, yPos),
                Size = new Size(120, 30)
            };
            btnIgnore.Click += BtnIgnore_Click;

            btnGoBack = new Button
            {
                Text = "Go Back",
                Location = new Point(240, yPos),
                Size = new Size(75, 30)
            };
            btnGoBack.Click += BtnGoBack_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(320, yPos),
                Size = new Size(75, 30)
            };
            btnCancel.Click += BtnCancel_Click;

            // Enable Enter key to submit the form
            this.AcceptButton = btnProceed;

            // Add controls to the form
            if (isResidential)
            {
                this.Controls.AddRange(new Control[] { lblMessage, lblExceptions, lblTreadIndex, txtTreadIndex, btnProceed, btnIgnore, btnGoBack, btnCancel });
            }
            else
            {
                this.Controls.AddRange(new Control[] { lblMessage, lblTreadIndex, txtTreadIndex, btnProceed, btnIgnore, btnGoBack, btnCancel });
            }
        }

        private void BtnProceed_Click(object sender, EventArgs e)
        {
            try
            {
                MidLandingTreadIndex = int.Parse(txtTreadIndex.Text);
                IgnoreMidLanding = false;
                GoBack = false;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a valid numeric value for the tread index.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnIgnore_Click(object sender, EventArgs e)
        {
            IgnoreMidLanding = true;
            GoBack = false;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnGoBack_Click(object sender, EventArgs e)
        {
            GoBack = true;
            this.DialogResult = DialogResult.Retry;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            IsCanceled = true;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}