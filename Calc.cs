using System;

namespace SpiralStairPlugin
{
    public class Calc : ICalc
    {
        public StairParameters Calculate(ValidatedStairInput input)
        {
            // Calculate total steps (treads + landing) with riser height <= 9.5"
            int totalSteps = (int)Math.Ceiling(input.OverallHeight / 9.5); // 16 with 144"
            int numTreads = totalSteps - 1; // 15 treads, landing is the 16th
            double riserHeight = input.OverallHeight / totalSteps;
            double treadAngle = input.RotationDeg / numTreads; // Rotation over 15 treads

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

        public ComplianceRetryOption HandleComplianceFailure(StairParameters parameters)
        {
            if (parameters.RiserHeight > 9.5)
            {
                return new ComplianceRetryOption
                {
                    ShouldRetry = true,
                    Message = "Riser height must not exceed 9.5 inches."
                };
            }
            return new ComplianceRetryOption { ShouldRetry = false };
        }
    }
}