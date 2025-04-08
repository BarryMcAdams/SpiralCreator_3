using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace SpiralStairPlugin
{
    public class MidLanding : IGeometry
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                double innerRadius = parameters.CenterPoleDia / 2; // For the arc cutout
                double outerRadius = parameters.OutsideDia / 2;    // Full width from center to outside
                double landingThickness = 0.25;                    // Consistent with treads
                double treadAngleRad = parameters.TreadAngle * Math.PI / 180;

                // Calculate mid-landing position (halfway point)
                int totalSteps = parameters.NumTreads + 1; // Treads + top landing
                int midStep = totalSteps / 2; // Place mid-landing after this step
                double height = midStep * parameters.RiserHeight; // Z position of mid-landing
                double landingStartAngle = midStep * treadAngleRad * (parameters.IsClockwise ? 1 : -1);
                double landingEndAngle = landingStartAngle + (Math.PI / 2) * (parameters.IsClockwise ? 1 : -1); // Span 90°

                // Create inner and outer arcs for the landing
                using (CircularArc3d innerArc = new CircularArc3d(
                    Point3d.Origin, Vector3d.ZAxis, Vector3d.XAxis, innerRadius, landingStartAngle, landingEndAngle))
                using (CircularArc3d outerArc = new CircularArc3d(
                    Point3d.Origin, Vector3d.ZAxis, Vector3d.XAxis, outerRadius, landingStartAngle, landingEndAngle))
                {
                    // Convert arcs to Polyline
                    using (Polyline innerPoly = new Polyline())
                    using (Polyline outerPoly = new Polyline())
                    {
                        int segments = 10;
                        double angleStep = (landingEndAngle - landingStartAngle) / segments;
                        for (int j = 0; j <= segments; j++)
                        {
                            double angle = landingStartAngle + j * angleStep;
                            innerPoly.AddVertexAt(j, new Point2d(innerRadius * Math.Cos(angle), innerRadius * Math.Sin(angle)), 0, 0, 0);
                            outerPoly.AddVertexAt(j, new Point2d(outerRadius * Math.Cos(angle), outerRadius * Math.Sin(angle)), 0, 0, 0);
                        }

                        // Create lines to close the sector
                        using (Line startLine = new Line(
                            new Point3d(innerRadius * Math.Cos(landingStartAngle), innerRadius * Math.Sin(landingStartAngle), 0),
                            new Point3d(outerRadius * Math.Cos(landingStartAngle), outerRadius * Math.Sin(landingStartAngle), 0)))
                        using (Line endLine = new Line(
                            new Point3d(outerRadius * Math.Cos(landingEndAngle), outerRadius * Math.Sin(landingEndAngle), 0),
                            new Point3d(innerRadius * Math.Cos(landingEndAngle), innerRadius * Math.Sin(landingEndAngle), 0)))
                        {
                            // Create region
                            DBObjectCollection boundary = new DBObjectCollection { innerPoly, outerPoly, startLine, endLine };
                            using (DBObjectCollection regions = Region.CreateFromCurves(boundary))
                            {
                                Region region = regions[0] as Region;

                                // Extrude into a landing
                                using (Solid3d landing = new Solid3d())
                                {
                                    landing.CreateExtrudedSolid(region, new Vector3d(0, 0, landingThickness), new SweepOptions());
                                    landing.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, height)));

                                    // Set color to green (RGB: 0, 255, 0)
                                    landing.ColorIndex = 3; // AutoCAD color index for green

                                    btr.AppendEntity(landing);
                                    tr.AddNewlyCreatedDBObject(landing, true);

                                    tr.Commit();
                                    return new Entity[] { landing };
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}