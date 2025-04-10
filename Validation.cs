namespace SpiralStairPlugin
{
    public class Validation : IValidation
    {
        public ValidatedStairInput Validate(StairInput input)
        {
            if (input.CenterPoleDia <= 0 || input.OverallHeight <= 0 || input.OutsideDia <= 0)
            {
                return null;
            }

            if (input.CenterPoleDia >= input.OutsideDia)
            {
                return null;
            }

            return new ValidatedStairInput(input.CenterPoleDia, input.OverallHeight, input.OutsideDia, input.RotationDeg, input.MidLandingAfterTread, input.IsClockwise);
        }
    }
}