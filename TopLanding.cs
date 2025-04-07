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

                double innerRadius = parameters.CenterPoleDia / 2;
                double outerRadius = parameters.OutsideDia / 2;
                double landingThickness = 0.25; // Updated to 0.25"
                double treadAngleRad = parameters.TreadAngle * Math.PI / 180;
                double height = parameters.NumTreads * parameters.RiserHeight; // Align with last tread's top
                double startAngle = parameters.NumTreads * treadAngleRad * (parameters.IsClockwise ? 1 : -1);
                double endAngle = startAngle + treadAngleRad * (parameters.IsClockwise ? 1 : -1);

                // Create inner and outer arcs for the landing
                using (CircularArc3d innerArc = new CircularArc3d(
                    Point3d.Origin, Vector3d.ZAxis, Vector3d.XAxis, innerRadius, startAngle, endAngle))
                using (CircularArc3d outerArc = new CircularArc3d(
                    Point3d.Origin, Vector3d.ZAxis, Vector3d.XAxis, outerRadius, startAngle, endAngle))
                {
                    // Convert arcs to Polyline
                    using (Polyline innerPoly = new Polyline())
                    using (Polyline outerPoly = new Polyline())
                    {
                        int segments = 10;
                        double angleStep = (endAngle - startAngle) / segments;
                        for (int j = 0; j <= segments; j++)
                        {
                            double angle = startAngle + j * angleStep;
                            innerPoly.AddVertexAt(j, new Point2d(innerRadius * Math.Cos(angle), innerRadius * Math.Sin(angle)), 0, 0, 0);
                            outerPoly.AddVertexAt(j, new Point2d(outerRadius * Math.Cos(angle), outerRadius * Math.Sin(angle)), 0, 0, 0);
                        }

                        // Create lines to close the sector
                        using (Line startLine = new Line(
                            new Point3d(innerRadius * Math.Cos(startAngle), innerRadius * Math.Sin(startAngle), 0),
                            new Point3d(outerRadius * Math.Cos(startAngle), outerRadius * Math.Sin(startAngle), 0)))
                        using (Line endLine = new Line(
                            new Point3d(outerRadius * Math.Cos(endAngle), outerRadius * Math.Sin(endAngle), 0),
                            new Point3d(innerRadius * Math.Cos(endAngle), innerRadius * Math.Sin(endAngle), 0)))
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