using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace SpiralStairPlugin
{
    public class Calc : ICalc
    {
        public StairParameters Calculate(ValidatedStairInput input)
        {
            try
            {
                // Initial calculation of riser height and number of treads
                double riserHeight = input.OverallHeight / (Math.Floor((input.OverallHeight - 9.0) / 9.0) + 1);
                int numTreads = (int)Math.Floor((input.OverallHeight - riserHeight) / riserHeight);
                double treadAngle = input.RotationDeg / numTreads;

                // Adjust the number of treads if a mid-landing is needed
                if (input.MidLandingAfterTread.HasValue)
                {
                    // Mid-landing replaces one tread, so reduce the tread count by 1
                    numTreads--;
                    treadAngle = input.RotationDeg / numTreads;
                }

                return new StairParameters
                {
                    CenterPoleDia = input.CenterPoleDia,
                    OverallHeight = input.OverallHeight,
                    OutsideDia = input.OutsideDia,
                    RotationDeg = input.RotationDeg,
                    IsClockwise = input.IsClockwise,
                    RiserHeight = riserHeight,
                    TreadAngle = treadAngle,
                    NumTreads = numTreads
                };
            }
            catch
            {
                return null;
            }
        }

        public void HandleComplianceFailure(StairParameters parameters)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            // Riser height should be between 4" and 7.75" (IBC R311.7.10.1)
            if (parameters.RiserHeight < 4.0 || parameters.RiserHeight > 7.75)
            {
                ed.WriteMessage($"\nCompliance Warning: Riser height ({parameters.RiserHeight:F2} inches) is outside the allowed range (4 to 7.75 inches) per IBC R311.7.10.1.");
            }

            // Clear width (tread width) should be at least 26" (outside diameter - center pole diameter)
            double clearWidth = (parameters.OutsideDia - parameters.CenterPoleDia) / 2;
            if (clearWidth < 26.0)
            {
                ed.WriteMessage($"\nCompliance Warning: Clear width ({clearWidth:F2} inches) is less than the minimum required (26 inches).");
            }
        }
    }
}