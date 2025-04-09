using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpiralStairPlugin
{
    public class Tweaks : ITweaks
    {
        public EntityCollection ApplyTweaks(Document doc, EntityCollection entities, ValidatedStairInput validInput, StairParameters parameters)
        {
            if (doc == null || doc.Database == null)
            {
                return entities;
            }

            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                // Open the LayerTable for write
                LayerTable lt = tr.GetObject(doc.Database.LayerTableId, OpenMode.ForWrite) as LayerTable;
                if (lt == null)
                {
                    doc.Editor.WriteMessage("\nFailed to access LayerTable.");
                    return entities;
                }

                // Create the TopLanding layer if it doesn't exist
                string layerName = "TopLanding";
                if (!lt.Has(layerName))
                {
                    LayerTableRecord ltr = new LayerTableRecord
                    {
                        Name = layerName,
                        Color = Color.FromRgb(0, 255, 255) // Set color to RGB (0, 255, 255) - Cyan
                    };
                    lt.Add(ltr);
                    tr.AddNewlyCreatedDBObject(ltr, true);
                    doc.Editor.WriteMessage($"\nCreated layer '{layerName}' with color RGB(0, 255, 255).");
                }

                // The top landing is the last entity in the EntityCollection (as per Command.cs)
                if (entities != null && entities.Count > 0)
                {
                    Entity topLanding = entities[entities.Count - 1];
                    if (topLanding != null && !topLanding.IsDisposed && topLanding.IsWriteEnabled == false)
                    {
                        topLanding.UpgradeOpen();
                        topLanding.Layer = layerName;
                        topLanding.DowngradeOpen();
                        doc.Editor.WriteMessage($"\nAssigned top landing to layer '{layerName}'.");
                    }
                    else
                    {
                        doc.Editor.WriteMessage("\nWarning: Top landing entity is null, disposed, or not readable and cannot be assigned to layer.");
                    }
                }
                else
                {
                    doc.Editor.WriteMessage("\nWarning: EntityCollection is empty or null.");
                }

                tr.Commit();
            }

            return entities;
        }
    }
}