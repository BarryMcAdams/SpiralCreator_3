using System;
using System.Collections.Generic;

namespace SpiralStairPlugin
{
    public class ComplianceChecker
    {
        public class Violation
        {
            public string Message { get; set; }
            public string SuggestedFix { get; set; }
        }

        public List<Violation> CheckCompliance(StairParameters parameters)
        {
            List<Violation> violations = new List<Violation>();

            // Rule 1: Clear Width >= 26 inches
            double clearWidth = (parameters.OutsideDia / 2 - 1.5) - (parameters.CenterPoleDia / 2);
            if (clearWidth < 26)
            {
                double requiredOutsideDia = (26 + parameters.CenterPoleDia / 2 + 1.5) * 2;
                violations.Add(new Violation
                {
                    Message = $"Clear Width is {clearWidth:F2} inches, must be at least 26 inches.",
                    SuggestedFix = $"Increase Outside Diameter to {requiredOutsideDia:F2} inches."
                });
            }

            // Rule 2: Walkline Radius <= 24.5 inches
            double walklineRadius = parameters.CenterPoleDia / 2 + 12;
            if (walklineRadius > 24.5)
            {
                double maxCenterPoleDia = (24.5 - 12) * 2;
                violations.Add(new Violation
                {
                    Message = $"Walkline radius is {walklineRadius:F2} inches, must be at most 24.5 inches.",
                    SuggestedFix = $"Decrease Center Pole Diameter to {maxCenterPoleDia:F2} inches."
                });
            }

            // Rule 3: Tread Depth at Walkline >= 6.75 inches
            // Calculate arc length at walkline (radius = CenterPoleDia / 2 + 12) over TreadAngle
            double treadAngleRad = parameters.TreadAngle * Math.PI / 180;
            double walklineDepth = (parameters.CenterPoleDia / 2 + 12) * treadAngleRad;
            if (walklineDepth < 6.75)
            {
                double requiredTreadAngle = 6.75 / (parameters.CenterPoleDia / 2 + 12) * 180 / Math.PI;
                double requiredRotationDeg = requiredTreadAngle * parameters.NumTreads;
                violations.Add(new Violation
                {
                    Message = $"Tread depth at walkline is {walklineDepth:F2} inches, must be at least 6.75 inches.",
                    SuggestedFix = $"Increase Rotation Degrees to {requiredRotationDeg:F2} degrees."
                });
            }

            // Rule 4: Treads Identical (already ensured by Tread.cs)

            // Rule 5: Rise <= 9.5 inches (already enforced in Calc.cs)
            if (parameters.RiserHeight > 9.5)
            {
                violations.Add(new Violation
                {
                    Message = $"Riser height is {parameters.RiserHeight:F2} inches, must be at most 9.5 inches.",
                    SuggestedFix = "Adjust Overall Height or add more treads."
                });
            }

            // Rule 6: Headroom >= 78 inches
            if (parameters.OverallHeight < 78)
            {
                violations.Add(new Violation
                {
                    Message = $"Headroom is {parameters.OverallHeight:F2} inches, must be at least 78 inches.",
                    SuggestedFix = "Increase Overall Height to at least 78 inches."
                });
            }

            return violations;
        }
    }
}