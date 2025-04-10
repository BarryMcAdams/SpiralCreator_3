using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using System.Windows.Forms;

namespace SpiralStairPlugin
{
    public class Input : IInput
    {
        public StairInput GetInput(Document doc)
        {
            bool goBack;
            do
            {
                goBack = false;
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
                    bool isClockwise = form.IsClockwise;

                    // Calculate total treads to determine if a mid-landing is needed
                    ICalc calc = new Calc();
                    ValidatedStairInput tempInput = new ValidatedStairInput(centerPoleDia, overallHeight, outsideDia, rotationDeg, isClockwise: isClockwise);
                    StairParameters parameters = calc.Calculate(tempInput);
                    if (parameters == null)
                    {
                        doc.Editor.WriteMessage("\nFailed to calculate parameters for mid-landing check.");
                        return null;
                    }

                    // Check if a mid-landing is required (assuming residential for now; we can add a building type selector later)
                    bool isResidential = true; // Placeholder; can be made configurable
                    double heightLimit = isResidential ? 151 : 144;
                    int? midLandingAfterTread = null;
                    if (overallHeight > heightLimit)
                    {
                        using (MidLandingPrompt midLandingForm = new MidLandingPrompt(overallHeight, parameters.NumTreads, isResidential))
                        {
                            DialogResult midLandingResult = midLandingForm.ShowDialog();
                            if (midLandingResult == DialogResult.Cancel || midLandingForm.IsCanceled)
                            {
                                return null;
                            }
                            else if (midLandingResult == DialogResult.Retry || midLandingForm.GoBack)
                            {
                                goBack = true;
                                continue;
                            }
                            else if (midLandingForm.IgnoreMidLanding)
                            {
                                // Proceed without a mid-landing
                            }
                            else
                            {
                                midLandingAfterTread = midLandingForm.MidLandingTreadIndex;
                                if (midLandingAfterTread < 1 || midLandingAfterTread > parameters.NumTreads - 1)
                                {
                                    doc.Editor.WriteMessage($"\nInvalid mid-landing position. Must be between 1 and {parameters.NumTreads - 1}.");
                                    goBack = true;
                                    continue;
                                }
                            }
                        }
                    }

                    return new StairInput(centerPoleDia, overallHeight, outsideDia, rotationDeg, midLandingAfterTread, isClockwise);
                }
            } while (goBack);

            return null; // Should never reach here, but included for completeness
        }

        public void ShowRetryPrompt(string message)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            ed.WriteMessage($"\n{message} Please try again.");
        }
    }
}