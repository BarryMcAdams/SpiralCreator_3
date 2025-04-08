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

                // Define the treads array to store the landing entity
                Entity[] treads = new Entity[1]; // Create one landing

                double outerRadius = parameters.OutsideDia / 2;    // Full width from center to outside
                double landingLength = 50.0;                       // Long side of rectangle
                double landingThickness = 0.25;                    // Consistent with treads
                double height = parameters.OverallHeight - landingThickness; // Top at OverallHeight
                double treadAngleRad = parameters.TreadAngle * Math.PI / 180;
                double landingStartAngle = parameters.NumTreads * treadAngleRad * (parameters.IsClockwise ? 1 : -1);

                // Validate parameters
                if (outerRadius <= 0 || landingLength <= 0 || landingThickness <= 0)
                {
                    doc.Editor.WriteMessage($"\nInvalid parameters: outerRadius={outerRadius}, landingLength={landingLength}, landingThickness={landingThickness}. All must be positive.");
                    throw new ArgumentException("All dimensions must be positive.");
                }
                if (height < 0)
                {
                    doc.Editor.WriteMessage($"\nInvalid height: height={height}. Height must be non-negative.");
                    throw new ArgumentException("Height must be non-negative.");
                }

                // Create a simple rectangular landing (without arc cutout for now)
                Solid3d landing = new Solid3d();
                try
                {
                    // Create a rectangular Solid3d using CreateBox
                    landing.CreateBox(outerRadius, landingLength, landingThickness);
                    doc.Editor.WriteMessage("\nSuccessfully created landing box.");

                    // Position the box so the bottom-left corner is at (0, 0, 0)
                    // CreateBox centers the box at (0, 0, 0), so we need to shift it
                    landing.TransformBy(Matrix3d.Displacement(new Vector3d(outerRadius / 2, landingLength / 2, 0)));
                    doc.Editor.WriteMessage("\nSuccessfully positioned landing.");

                    // Validate the resulting geometry
                    if (landing == null || landing.Bounds == null)
                    {
                        throw new InvalidOperationException("Landing geometry is invalid after creation.");
                    }
                    doc.Editor.WriteMessage("\nLanding geometry validated successfully.");

                    // Apply transformations
                    landing.TransformBy(Matrix3d.Rotation(landingStartAngle, Vector3d.ZAxis, Point3d.Origin));
                    doc.Editor.WriteMessage("\nSuccessfully applied rotation transformation.");
                    landing.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, height)));
                    doc.Editor.WriteMessage("\nSuccessfully applied height displacement transformation.");

                    btr.AppendEntity(landing);
                    doc.Editor.WriteMessage("\nSuccessfully appended landing to BlockTableRecord.");
                    tr.AddNewlyCreatedDBObject(landing, true);
                    doc.Editor.WriteMessage("\nSuccessfully added landing to transaction.");

                    treads[0] = landing;
                    landing.DowngradeOpen();
                    doc.Editor.WriteMessage("\nSuccessfully downgraded landing open state.");
                }
                catch (Exception ex)
                {
                    doc.Editor.WriteMessage($"\nFailed to create landing: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    treads[0] = null; // Ensure treads[0] is explicitly null if creation fails
                    throw;
                }
                finally
                {
                    landing?.Dispose();
                }

                tr.Commit();
                doc.Editor.WriteMessage("\nSuccessfully committed transaction.");

                return treads;
            }
        }
    }
}