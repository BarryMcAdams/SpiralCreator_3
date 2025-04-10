using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace SpiralStairPlugin
{
    public class TopLanding : IGeometry
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
            double landingThickness = 0.25;
            double landingWidth = (parameters.OutsideDia - parameters.CenterPoleDia) / 2;
            double landingLength = landingWidth + 30; // Extend 30" beyond the outer radius
            double height = parameters.OverallHeight; // Top landing at overall height
            double treadAngleRad = parameters.TreadAngle * Math.PI / 180;
            double startAngle = parameters.NumTreads * treadAngleRad * (parameters.IsClockwise ? 1 : -1);

            Solid3d landingBox = new Solid3d();
            try
            {
                landingBox.CreateBox(landingWidth, landingLength, landingThickness);
                doc.Editor.WriteMessage("\nSuccessfully created landing box.");

                landingBox.TransformBy(Matrix3d.Displacement(new Vector3d(landingWidth / 2, landingLength / 2, 0)));
                doc.Editor.WriteMessage("\nSuccessfully positioned landing.");

                if (landingBox == null || landingBox.Bounds == null)
                {
                    throw new InvalidOperationException("Landing geometry is invalid after creation.");
                }
                doc.Editor.WriteMessage("\nLanding geometry validated successfully.");

                landingBox.TransformBy(Matrix3d.Rotation(startAngle, Vector3d.ZAxis, Point3d.Origin));
                doc.Editor.WriteMessage("\nSuccessfully applied rotation transformation to landing.");
                landingBox.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, height)));
                doc.Editor.WriteMessage("\nSuccessfully applied height displacement transformation to landing.");

                btr.AppendEntity(landingBox);
                doc.Editor.WriteMessage("\nSuccessfully appended landing to BlockTableRecord.");
                tr.AddNewlyCreatedDBObject(landingBox, true);
                doc.Editor.WriteMessage("\nSuccessfully added landing to transaction.");

                landing[0] = landingBox;
                landingBox.DowngradeOpen();
                doc.Editor.WriteMessage("\nSuccessfully downgraded landing open state.");
            }
            catch (Exception ex)
            {
                doc.Editor.WriteMessage($"\nFailed to create top landing: {ex.Message}\nStackTrace: {ex.StackTrace}");
                landing[0] = null;
                throw;
            }

            return landing;
        }
    }
}