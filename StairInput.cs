namespace SpiralStairPlugin
{
    public class StairInput
    {
        public double CenterPoleDia { get; set; }
        public double OverallHeight { get; set; }
        public double OutsideDia { get; set; }
        public double RotationDeg { get; set; }
        public bool IsClockwise { get; set; }
    }

    public class ValidatedStairInput : StairInput
    {
    }
}