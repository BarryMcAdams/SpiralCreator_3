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

            Entity[] midLanding = new Entity[1];
            double landingThickness = 0.25; // Same thickness as other landings
            double landingWidth = (parameters.OutsideDia - parameters.CenterPoleDia) / 2;
            double landingLength = landingWidth; // For a 90° sector, width and length are equal
            double height = parameters.OverallHeight; // Will be adjusted by caller
            double treadAngleRad = parameters.TreadAngle * Math.PI / 180;
            double startAngle = parameters.NumTreads * treadAngleRad * (parameters.IsClockwise ? 1 : -1);

            Solid3d landingSector = new Solid3d();
            try
            {
                // Create a 90° sector (25% of a circle)
                landingSector.CreateBox(landingWidth, landingLength, landingThickness);
                doc.Editor.WriteMessage("\nSuccessfully created mid-landing box.");

                // Position the sector
                landingSector.TransformBy(Matrix3d.Displacement(new Vector3d(landingWidth / 2, landingLength / 2, 0)));
                doc.Editor.WriteMessage("\nSuccessfully positioned mid-landing.");

                if (landingSector == null || landingSector.Bounds == null)
                {
                    throw new InvalidOperationException("Mid-landing geometry is invalid after creation.");
                }
                doc.Editor.WriteMessage("\nMid-landing geometry validated successfully.");

                // Rotate to form a 90° sector
                landingSector.TransformBy(Matrix3d.Rotation(startAngle, Vector3d.ZAxis, Point3d.Origin));
                doc.Editor.WriteMessage("\nSuccessfully applied rotation transformation to mid-landing.");
                landingSector.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, height)));
                doc.Editor.WriteMessage("\nSuccessfully applied height displacement transformation to mid-landing.");

                btr.AppendEntity(landingSector);
                doc.Editor.WriteMessage("\nSuccessfully appended mid-landing to BlockTableRecord.");
                tr.AddNewlyCreatedDBObject(landingSector, true);
                doc.Editor.WriteMessage("\nSuccessfully added mid-landing to transaction.");

                midLanding[0] = landingSector;
                landingSector.DowngradeOpen();
                doc.Editor.WriteMessage("\nSuccessfully downgraded mid-landing open state.");
            }
            catch (Exception ex)
            {
                doc.Editor.WriteMessage($"\nFailed to create mid-landing: {ex.Message}\nStackTrace: {ex.StackTrace}");
                midLanding[0] = null;
                throw;
            }

            return midLanding;
        }
    }
}