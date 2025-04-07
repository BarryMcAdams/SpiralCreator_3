using Autodesk.AutoCAD.ApplicationServices;
using System.Windows.Forms;

namespace SpiralStairPlugin
{
    public class Input : IInput
    {
        public StairInput GetInput(Document doc)
        {
            using (InputForm form = new InputForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return new StairInput
                    {
                        CenterPoleDia = form.CenterPoleDiameter,
                        OverallHeight = form.OverallHeight,
                        OutsideDia = form.OutsideDiameter,
                        RotationDeg = form.RotationDegrees,
                        IsClockwise = form.IsClockwise
                    };
                }
                return null;
            }
        }

        public void ShowRetryPrompt(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public StairInput GetAdjustedInput(Document doc, StairParameters parameters)
        {
            return GetInput(doc);
        }
    }
}