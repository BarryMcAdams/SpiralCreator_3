using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace SpiralStairPlugin
{
    public class Tread : IGeometry
    {
        public Entity[] Create(Document doc, Transaction tr, StairParameters parameters)
        {
            if (doc == null || doc.Database == null)
            {
                throw new ArgumentNullException(nameof(doc), "Document or its database is null.");
            }
            if (tr == null)
            {
                throw new ArgumentNullException(nameof(tr), "Transaction is null.");
            }

            BlockTable bt = tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
            if (bt == null)
            {
                throw new InvalidOperationException("Failed to access BlockTable.");
            }

            BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            if (btr == null)
            {
                throw new InvalidOperationException("Failed to access BlockTableRecord for ModelSpace.");
            }

            Entity[] treads = new Entity[1]; // Create one tread at a time
            double innerRadius = parameters.CenterPoleDia / 2;
            double outerRadius = parameters.OutsideDia / 2;
            double treadThickness = 0.25;
            double treadAngleRad = parameters.TreadAngle * Math.PI / 180;

            // Use parameters.NumTreads as the step index
            int stepIndex = parameters.NumTreads;
            double height = (stepIndex + 1) * parameters.RiserHeight; // Start at one riser height above Z=0
            double startAngle = stepIndex * treadAngleRad * (parameters.IsClockwise ? 1 : -1);
            double endAngle = startAngle + treadAngleRad * (parameters.IsClockwise ? 1 : -1);

            Solid3d tread = new Solid3d();
            try
            {
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

                        // Add the polylines to the database
                        btr.AppendEntity(innerPoly);
                        tr.AddNewlyCreatedDBObject(innerPoly, true);
                        btr.AppendEntity(outerPoly);
                        tr.AddNewlyCreatedDBObject(outerPoly, true);

                        // Create lines to close the sector
                        using (Line startLine = new Line(
                            new Point3d(innerRadius * Math.Cos(startAngle), innerRadius * Math.Sin(startAngle), 0),
                            new Point3d(outerRadius * Math.Cos(startAngle), outerRadius * Math.Sin(startAngle), 0)))
                        using (Line endLine = new Line(
                            new Point3d(outerRadius * Math.Cos(endAngle), outerRadius * Math.Sin(endAngle), 0),
                            new Point3d(innerRadius * Math.Cos(endAngle), innerRadius * Math.Sin(endAngle), 0)))
                        {
                            // Add the lines to the database
                            btr.AppendEntity(startLine);
                            tr.AddNewlyCreatedDBObject(startLine, true);
                            btr.AppendEntity(endLine);
                            tr.AddNewlyCreatedDBObject(endLine, true);

                            // Create region from the closed boundary
                            DBObjectCollection boundary = new DBObjectCollection { innerPoly, outerPoly, startLine, endLine };
                            using (DBObjectCollection regions = Region.CreateFromCurves(boundary))
                            {
                                if (regions == null || regions.Count == 0)
                                {
                                    doc.Editor.WriteMessage("\nFailed to create region for tread polyline.");
                                    throw new InvalidOperationException("Failed to create region for tread polyline.");
                                }

                                using (Region region = regions[0] as Region)
                                {
                                    if (region == null)
                                    {
                                        doc.Editor.WriteMessage("\nRegion creation failed for tread polyline.");
                                        throw new InvalidOperationException("Region creation failed for tread polyline.");
                                    }

                                    // Create the tread by extruding the region
                                    tread.CreateExtrudedSolid(region, new Vector3d(0, 0, treadThickness), new SweepOptions());
                                    doc.Editor.WriteMessage("\nSuccessfully created tread solid.");
                                }
                            }

                            // Clean up temporary entities
                            startLine.Erase();
                            endLine.Erase();
                        }

                        innerPoly.Erase();
                        outerPoly.Erase();
                    }
                }

                // Validate the resulting geometry
                if (tread == null || tread.Bounds == null)
                {
                    doc.Editor.WriteMessage("\nTread geometry is invalid after extrusion.");
                    throw new InvalidOperationException("Tread geometry is invalid after extrusion.");
                }

                // Apply height transformation
                tread.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, height)));
                doc.Editor.WriteMessage("\nSuccessfully applied height transformation to tread.");

                btr.AppendEntity(tread);
                doc.Editor.WriteMessage("\nSuccessfully appended tread to BlockTableRecord.");
                tr.AddNewlyCreatedDBObject(tread, true);
                doc.Editor.WriteMessage("\nSuccessfully added tread to transaction.");

                treads[0] = tread;
                tread.DowngradeOpen();
                doc.Editor.WriteMessage("\nSuccessfully downgraded tread open state.");
            }
            catch (Exception ex)
            {
                doc.Editor.WriteMessage($"\nFailed to create tread: {ex.Message}\nStackTrace: {ex.StackTrace}");
                treads[0] = null; // Ensure treads[0] is explicitly null if creation fails
                throw;
            }

            return treads;
        }
    }
}