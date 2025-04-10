namespace SpiralStairPlugin
{
    public class StairInput
    {
        public double CenterPoleDia { get; set; }
        public double OverallHeight { get; set; }
        public double OutsideDia { get; set; }
        public double RotationDeg { get; set; }
        public int? MidLandingAfterTread { get; set; }
        public bool IsClockwise { get; set; }

        public StairInput(double centerPoleDia, double overallHeight, double outsideDia, double rotationDeg, int? midLandingAfterTread = null, bool isClockwise = true)
        {
            CenterPoleDia = centerPoleDia;
            OverallHeight = overallHeight;
            OutsideDia = outsideDia;
            RotationDeg = rotationDeg;
            MidLandingAfterTread = midLandingAfterTread;
            IsClockwise = isClockwise;
        }
    }
}