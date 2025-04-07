using System;

namespace SpiralStairPlugin
{
    public class Calc : ICalc
    {
        public StairParameters Calculate(ValidatedStairInput input)
        {
            // 16 total steps (15 treads + 1 landing), riser height = OverallHeight / total steps
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