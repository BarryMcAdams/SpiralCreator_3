using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace SpiralStairPlugin
{
    public class CenterPole : IGeometry
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

            Entity[] pole = new Entity[1];
            double radius = parameters.CenterPoleDia / 2;
            double height = parameters.OverallHeight;

            Solid3d centerPole = new Solid3d();
            try
            {
                centerPole.CreateFrustum(height, radius, radius, radius);
                doc.Editor.WriteMessage("\nSuccessfully created center pole.");

                if (centerPole == null || centerPole.Bounds == null)
                {
                    throw new InvalidOperationException("Center pole geometry is invalid after creation.");
                }
                doc.Editor.WriteMessage("\nCenter pole geometry validated successfully.");

                btr.AppendEntity(centerPole);
                doc.Editor.WriteMessage("\nSuccessfully appended center pole to BlockTableRecord.");
                tr.AddNewlyCreatedDBObject(centerPole, true);
                doc.Editor.WriteMessage("\nSuccessfully added center pole to transaction.");

                pole[0] = centerPole;
                centerPole.DowngradeOpen();
                doc.Editor.WriteMessage("\nSuccessfully downgraded center pole open state.");
            }
            catch (Exception ex)
            {
                doc.Editor.WriteMessage($"\nFailed to create center pole: {ex.Message}\nStackTrace: {ex.StackTrace}");
                pole[0] = null;
                throw;
            }
            finally
            {
                centerPole?.Dispose();
            }

            return pole;
        }
    }
}