namespace SpiralStairPlugin
{
    public class Calc : ICalc
    {
        public StairParameters Calculate(ValidatedStairInput input)
        {
            int numTreads = (int)(input.OverallHeight / 7.5);
            if (numTreads < 1) numTreads = 1;

            double riserHeight = input.OverallHeight / numTreads;
            double treadAngle = input.RotationDeg / numTreads;

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
            if (parameters.RiserHeight < 6 || parameters.RiserHeight > 8)
            {
                return new ComplianceRetryOption
                {
                    ShouldRetry = true,
                    Message = "Riser height must be between 6 and 8 inches."
                };
            }
            return new ComplianceRetryOption { ShouldRetry = false };
        }
    }
}