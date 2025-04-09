using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace SpiralStairPlugin
{
    public class MidLanding : IGeometry
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

            Entity[] landing = new Entity[1];
            double innerRadius = parameters.CenterPoleDia / 2;
            double outerRadius = parameters.OutsideDia / 2;
            double landingThickness = 0.25;
            double height = (parameters.NumTreads + 1) * parameters.RiserHeight;
            double treadAngleRad = parameters.TreadAngle * Math.PI / 180;
            double startAngle = parameters.NumTreads * treadAngleRad * (parameters.IsClockwise ? 1 : -1);
            double midLandingAngle = Math.PI / 2; // 90° span
            double endAngle = startAngle + midLandingAngle * (parameters.IsClockwise ? 1 : -1);

            Solid3d midLanding = new Solid3d();
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
                                    doc.Editor.WriteMessage("\nFailed to create region for mid-landing polyline.");
                                    throw new InvalidOperationException("Failed to create region for mid-landing polyline.");
                                }

                                using (Region region = regions[0] as Region)
                                {
                                    if (region == null)
                                    {
                                        doc.Editor.WriteMessage("\nRegion creation failed for mid-landing polyline.");
                                        throw new InvalidOperationException("Region creation failed for mid-landing polyline.");
                                    }

                                    // Create the mid-landing by extruding the region
                                    midLanding.CreateExtrudedSolid(region, new Vector3d(0, 0, landingThickness), new SweepOptions());
                                    doc.Editor.WriteMessage("\nSuccessfully created mid-landing solid.");
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

                if (midLanding == null || midLanding.Bounds == null)
                {
                    throw new InvalidOperationException("Mid-landing geometry is invalid after creation.");
                }
                doc.Editor.WriteMessage("\nMid-landing geometry validated successfully.");

                midLanding.TransformBy(Matrix3d.Rotation(startAngle, Vector3d.ZAxis, Point3d.Origin));
                doc.Editor.WriteMessage("\nSuccessfully applied rotation transformation to mid-landing.");
                midLanding.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, height)));
                doc.Editor.WriteMessage("\nSuccessfully applied height displacement transformation to mid-landing.");

                btr.AppendEntity(midLanding);
                doc.Editor.WriteMessage("\nSuccessfully appended mid-landing to BlockTableRecord.");
                tr.AddNewlyCreatedDBObject(midLanding, true);
                doc.Editor.WriteMessage("\nSuccessfully added mid-landing to transaction.");

                landing[0] = midLanding;
                midLanding.DowngradeOpen();
                doc.Editor.WriteMessage("\nSuccessfully downgraded mid-landing open state.");
            }
            catch (Exception ex)
            {
                doc.Editor.WriteMessage($"\nFailed to create mid-landing: {ex.Message}\nStackTrace: {ex.StackTrace}");
                landing[0] = null;
                throw;
            }

            return landing;
        }
    }
}