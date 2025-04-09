﻿namespace SpiralStairPlugin
{
    public class StairInput
    {
        public double CenterPoleDia { get; set; }
        public double OverallHeight { get; set; }
        public double OutsideDia { get; set; }
        public double RotationDeg { get; set; }
        public int? MidLandingAfterTread { get; set; } // New property for user-specified mid-landing position

        public StairInput(double centerPoleDia, double overallHeight, double outsideDia, double rotationDeg, int? midLandingAfterTread = null)
        {
            CenterPoleDia = centerPoleDia;
            OverallHeight = overallHeight;
            OutsideDia = outsideDia;
            RotationDeg = rotationDeg;
            MidLandingAfterTread = midLandingAfterTread;
        }
    }
}