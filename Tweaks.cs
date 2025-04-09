using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using System; // Added to resolve Exception type

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

                // Find the top landing by type
                Entity topLanding = entities.GetByType("TopLanding");
                if (topLanding != null)
                {
                    try
                    {
                        topLanding.UpgradeOpen();
                        topLanding.Layer = layerName;
                        topLanding.DowngradeOpen();
                        doc.Editor.WriteMessage($"\nAssigned top landing to layer '{layerName}'.");
                    }
                    catch (Exception ex)
                    {
                        doc.Editor.WriteMessage($"\nFailed to assign top landing to layer '{layerName}': {ex.Message}\nStackTrace: {ex.StackTrace}");
                    }
                }
                else
                {
                    doc.Editor.WriteMessage("\nWarning: No top landing entity found in EntityCollection.");
                }

                tr.Commit();
            }

            return entities;
        }
    }
}