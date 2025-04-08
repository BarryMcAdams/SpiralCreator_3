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
            if (doc == null || doc.Database == null)
            {
                throw new ArgumentNullException(nameof(doc), "Document or its database is null.");
            }

            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
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

                // Define the treads array to store the tread entity
                Entity[] treads = new Entity[1]; // Create one tread

                double outerRadius = parameters.OutsideDia / 2;    // Full width from center to outside
                double treadLength = 50.0;                         // Approximate length (same as landing for simplicity)
                double treadThickness = 0.25;                      // Consistent with landing
                double height = (parameters.NumTreads + 1) * parameters.RiserHeight; // Height of this tread
                double treadAngleRad = parameters.TreadAngle * Math.PI / 180;
                double startAngle = parameters.NumTreads * treadAngleRad * (parameters.IsClockwise ? 1 : -1);

                // Create a simple rectangular tread (without arc cutout for now)
                Solid3d tread = new Solid3d();
                try
                {
                    // Create a rectangular Solid3d using CreateBox
                    tread.CreateBox(outerRadius, treadLength, treadThickness);
                    doc.Editor.WriteMessage("\nSuccessfully created tread box.");

                    // Position the box so the bottom-left corner is at (0, 0, 0)
                    // CreateBox centers the box at (0, 0, 0), so we need to shift it
                    tread.TransformBy(Matrix3d.Displacement(new Vector3d(outerRadius / 2, treadLength / 2, 0)));
                    doc.Editor.WriteMessage("\nSuccessfully positioned tread.");

                    // Validate the resulting geometry
                    if (tread == null || tread.Bounds == null)
                    {
                        throw new InvalidOperationException("Tread geometry is invalid after creation.");
                    }
                    doc.Editor.WriteMessage("\nTread geometry validated successfully.");

                    // Apply transformations
                    tread.TransformBy(Matrix3d.Rotation(startAngle, Vector3d.ZAxis, Point3d.Origin));
                    doc.Editor.WriteMessage("\nSuccessfully applied rotation transformation to tread.");
                    tread.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, height)));
                    doc.Editor.WriteMessage("\nSuccessfully applied height displacement transformation to tread.");

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
                finally
                {
                    tread?.Dispose();
                }

                tr.Commit();
                doc.Editor.WriteMessage("\nSuccessfully committed transaction for tread.");

                return treads;
            }
        }
    }
}