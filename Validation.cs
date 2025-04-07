namespace SpiralStairPlugin
{
    public class Validation : IValidation
    {
        public ValidatedStairInput Validate(StairInput input)
        {
            if (input.CenterPoleDia <= 0 || input.OverallHeight <= 0 || input.OutsideDia <= 0 || input.RotationDeg <= 0)
                return null;

            if (input.OutsideDia <= input.CenterPoleDia)
                return null;

            if (input.RotationDeg > 3600)
                return null;

            return new ValidatedStairInput
            {
                CenterPoleDia = input.CenterPoleDia,
                OverallHeight = input.OverallHeight,
                OutsideDia = input.OutsideDia,
                RotationDeg = input.RotationDeg,
                IsClockwise = input.IsClockwise
            };
        }
    }
}