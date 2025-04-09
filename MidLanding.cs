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

            Solid3d midLanding = new Solid3d();
            try
            {
                midLanding.CreateFrustum(landingThickness, outerRadius, outerRadius, outerRadius);
                doc.Editor.WriteMessage("\nSuccessfully created mid-landing.");

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
            finally
            {
                midLanding?.Dispose();
            }

            return landing;
        }
    }
}