using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace SpiralStairPlugin
{
    public class TopLanding : IGeometry
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                double innerRadius = parameters.CenterPoleDia / 2; // For the arc cutout
                double outerRadius = parameters.OutsideDia / 2;    // Full width from center to outside
                double landingLength = 50.0;                       // Long side of rectangle
                double landingThickness = 0.25;                    // Consistent with treads
                double height = parameters.OverallHeight - landingThickness; // Top at OverallHeight
                double treadAngleRad = parameters.TreadAngle * Math.PI / 180;
                double landingStartAngle = parameters.NumTreads * treadAngleRad * (parameters.IsClockwise ? 1 : -1);
                double landingEndAngle = landingStartAngle + treadAngleRad * (parameters.IsClockwise ? 1 : -1);

                // Create the outer rectangle (aligned along X-axis initially, short edge radial, from 0 to outerRadius)
                using (Polyline outerRect = new Polyline())
                {
                    outerRect.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);                    // Bottom-left (at pole center)
                    outerRect.AddVertexAt(1, new Point2d(outerRadius, 0), 0, 0, 0);          // Bottom-right
                    outerRect.AddVertexAt(2, new Point2d(outerRadius, landingLength), 0, 0, 0); // Top-right
                    outerRect.AddVertexAt(3, new Point2d(0, landingLength), 0, 0, 0);        // Top-left
                    outerRect.Closed = true;

                    // Create the inner arc to cut out the center pole
                    using (CircularArc3d innerArc = new CircularArc3d(
                        Point3d.Origin, Vector3d.ZAxis, Vector3d.XAxis, innerRadius, landingStartAngle, landingEndAngle))
                    {
                        // Approximate the arc with a Polyline
                        using (Polyline innerPoly = new Polyline())
                        {
                            int segments = 10;
                            double angleStep = (landingEndAngle - landingStartAngle) / segments;
                            for (int j = 0; j <= segments; j++)
                            {
                                double angle = landingStartAngle + j * angleStep;
                                innerPoly.AddVertexAt(j, new Point2d(innerRadius * Math.Cos(angle), innerRadius * Math.Sin(angle)), 0, 0, 0);
                            }
                            innerPoly.Closed = true;

                            // Create regions and subtract the arc from the rectangle
                            DBObjectCollection outerBoundary = new DBObjectCollection { outerRect };
                            DBObjectCollection innerBoundary = new DBObjectCollection { innerPoly };
                            using (DBObjectCollection outerRegions = Region.CreateFromCurves(outerBoundary))
                            using (DBObjectCollection innerRegions = Region.CreateFromCurves(innerBoundary))
                            {
                                Region outerRegion = outerRegions[0] as Region;
                                Region innerRegion = innerRegions[0] as Region;

                                outerRegion.BooleanOperation(BooleanOperationType.BoolSubtract, innerRegion);

                                // Extrude into a landing
                                using (Solid3d landing = new Solid3d())
                                {
                                    landing.CreateExtrudedSolid(outerRegion, new Vector3d(0, 0, landingThickness), new SweepOptions());

                                    // Rotate to match the 16th tread's start angle (no additional rotation)
                                    landing.TransformBy(Matrix3d.Rotation(landingStartAngle, Vector3d.ZAxis, Point3d.Origin));

                                    // Move so top is at OverallHeight
                                    landing.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, height)));

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