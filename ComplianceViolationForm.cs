using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace SpiralStairPlugin
{
    public class ComplianceViolationForm : Form
    {
        private TextBox txtViolations;
        private Button btnTryAgain;
        private Button btnIgnore;
        private Button btnCancel;

        public enum Result
        {
            TryAgain,
            Ignore,
            Cancel
        }

        public Result UserChoice { get; private set; }

        public ComplianceViolationForm(List<ComplianceChecker.Violation> violations)
        {
            InitializeComponents(violations);
        }

        private void InitializeComponents(List<ComplianceChecker.Violation> violations)
        {
            this.Text = "Compliance Violations Detected";
            this.Width = 500;
            this.Height = 350;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblInstructions = new Label
            {
                Text = "The following violations were detected. Please choose an option:",
                Left = 20,
                Top = 20,
                Width = 460
            };

            // Check if there's a Clear Width violation and add educational message
            StringBuilder messageBuilder = new StringBuilder();
            bool hasClearWidthViolation = false;
            foreach (var violation in violations)
            {
                if (violation.Message.Contains("Clear Width"))
                {
                    hasClearWidthViolation = true;
                    break;
                }
            }
            if (hasClearWidthViolation)
            {
                messageBuilder.AppendLine("Note: Clear Width is the actual, unobstructed, usable width of an opening or passageway, measured horizontally between the narrowest points. It represents the space available for passage, free from encroachments or obstacles.");
                messageBuilder.AppendLine();
            }

            // Add violation messages
            foreach (var violation in violations)
            {
                messageBuilder.AppendLine($"{violation.Message} Suggested fix: {violation.SuggestedFix}");
                messageBuilder.AppendLine();
            }

            txtViolations = new TextBox
            {
                Left = 20,
                Top = 50,
                Width = 460,
                Height = 180,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = true,
                Text = messageBuilder.ToString()
            };

            btnTryAgain = new Button
            {
                Text = "Try Again",
                Left = 100,
                Top = 240,
                Width = 100,
                DialogResult = DialogResult.Retry
            };
            btnTryAgain.Click += (s, e) => { UserChoice = Result.TryAgain; Close(); };

            btnIgnore = new Button
            {
                Text = "Ignore and Continue",
                Left = 210,
                Top = 240,
                Width = 120,
                DialogResult = DialogResult.Ignore
            };
            btnIgnore.Click += (s, e) => { UserChoice = Result.Ignore; Close(); };

            btnCancel = new Button
            {
                Text = "Cancel",
                Left = 340,
                Top = 240,
                Width = 100,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.Click += (s, e) => { UserChoice = Result.Cancel; Close(); };

            this.Controls.AddRange(new Control[] { lblInstructions, txtViolations, btnTryAgain, btnIgnore, btnCancel });
        }
    }
}