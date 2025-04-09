using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using System.Windows.Forms;

namespace SpiralStairPlugin
{
    public class Input : IInput
    {
        public StairInput GetInput(Document doc)
        {
            using (InputForm form = new InputForm())
            {
                DialogResult result = form.ShowDialog();
                if (result != DialogResult.OK || form.IsCanceled)
                {
                    return null;
                }

                double centerPoleDia = form.CenterPoleDia;
                double overallHeight = form.OverallHeight;
                double outsideDia = form.OutsideDia;
                double rotationDeg = form.RotationDeg;

                // Prompt for mid-landing position if height exceeds 151"
                int? midLandingAfterTread = null;
                if (overallHeight > 151)
                {
                    // Calculate total treads to inform the user of the valid range
                    ICalc calc = new Calc();
                    ValidatedStairInput tempInput = new ValidatedStairInput(centerPoleDia, overallHeight, outsideDia, rotationDeg);
                    StairParameters parameters = calc.Calculate(tempInput);
                    if (parameters == null)
                    {
                        doc.Editor.WriteMessage("\nFailed to calculate parameters for mid-landing prompt.");
                        return null;
                    }
                    int totalTreads = parameters.NumTreads;

                    using (InputForm midLandingForm = new InputForm(totalTreads: totalTreads))
                    {
                        DialogResult midLandingResult = midLandingForm.ShowDialog();
                        if (midLandingResult != DialogResult.OK || midLandingForm.IsCanceled)
                        {
                            return null;
                        }

                        midLandingAfterTread = midLandingForm.MidLandingAfterTread;
                    }
                }

                return new StairInput(centerPoleDia, overallHeight, outsideDia, rotationDeg, midLandingAfterTread);
            }
        }

        public StairInput GetAdjustedInput(Document doc, ValidatedStairInput validInput, StairParameters parameters)
        {
            Editor ed = doc.Editor;

            // Display calculated parameters
            ed.WriteMessage($"\nCalculated Parameters:");
            ed.WriteMessage($"\nNumber of treads: {parameters.NumTreads}");
            ed.WriteMessage($"\nRiser height: {parameters.RiserHeight:F2} inches");
            ed.WriteMessage($"\nTread angle: {parameters.TreadAngle:F2} degrees");

            // Prompt if the user wants to adjust their input
            PromptKeywordOptions pko = new PromptKeywordOptions("\nDo you want to adjust your input? [Yes/No]: ")
            {
                AllowNone = true
            };
            pko.Keywords.Add("Yes");
            pko.Keywords.Add("No");
            pko.Keywords.Default = "No";
            PromptResult pr = ed.GetKeywords(pko);
            if (pr.Status != PromptStatus.OK || pr.StringResult == "No")
            {
                return null; // No adjustments, return null to proceed with current input
            }

            // Show the form with current values pre-filled
            using (InputForm form = new InputForm(
                initialCenterPoleDia: parameters.CenterPoleDia,
                initialOverallHeight: parameters.OverallHeight,
                initialOutsideDia: parameters.OutsideDia,
                initialRotationDeg: parameters.RotationDeg,
                initialMidLandingAfterTread: validInput.MidLandingAfterTread,
                totalTreads: parameters.OverallHeight > 151 ? (int?)parameters.NumTreads : null))
            {
                DialogResult result = form.ShowDialog();
                if (result != DialogResult.OK || form.IsCanceled)
                {
                    return null;
                }

                return new StairInput(form.CenterPoleDia, form.OverallHeight, form.OutsideDia, form.RotationDeg, form.MidLandingAfterTread);
            }
        }

        public void ShowRetryPrompt(string message)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            ed.WriteMessage($"\n{message} Please try again.");
        }
    }
}