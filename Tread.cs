using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace SpiralStairPlugin
{
    public class Tread : IGeometry
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            Entity[] treads = new Entity[parameters.NumTreads];
            double innerRadius = parameters.CenterPoleDia / 2;
            double outerRadius = parameters.OutsideDia / 2;
            double treadThickness = 0.25; // Thickness remains 0.25"
            double treadAngleRad = parameters.TreadAngle * Math.PI / 180; // Convert to radians

            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                for (int i = 0; i < parameters.NumTreads; i++)
                {
                    // Start at one riser height above Z=0, increment from there
                    double height = (i + 1) * parameters.RiserHeight;
                    double startAngle = i * treadAngleRad * (parameters.IsClockwise ? 1 : -1);
                    double endAngle = startAngle + treadAngleRad * (parameters.IsClockwise ? 1 : -1);

                    // Create inner and outer arcs in memory
                    using (CircularArc3d innerArc = new CircularArc3d(
                        Point3d.Origin, Vector3d.ZAxis, Vector3d.XAxis, innerRadius, startAngle, endAngle))
                    using (CircularArc3d outerArc = new CircularArc3d(
                        Point3d.Origin, Vector3d.ZAxis, Vector3d.XAxis, outerRadius, startAngle, endAngle))
                    {
                        // Convert arcs to Polyline for region creation
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
                                // Create region from the closed boundary
                                DBObjectCollection boundary = new DBObjectCollection { innerPoly, outerPoly, startLine, endLine };
                                using (DBObjectCollection regions = Region.CreateFromCurves(boundary))
                                {
                                    Region region = regions[0] as Region;

                                    // Extrude into a tread
                                    using (Solid3d tread = new Solid3d())
                                    {
                                        tread.CreateExtrudedSolid(region, new Vector3d(0, 0, treadThickness), new SweepOptions());
                                        tread.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, height)));

                                        btr.AppendEntity(tread);
                                        tr.AddNewlyCreatedDBObject(tread, true);

                                        treads[i] = tread;
                                        tread.DowngradeOpen();
                                    }
                                }
                            }
                        }
                    }
                }

                tr.Commit();
            }

            return treads;
        }
    }
}