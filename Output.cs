using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpiralStairPlugin
{
    public class Output : IOutput
    {
        public void Finalize(Document doc, ValidatedStairInput input, StairParameters parameters, EntityCollection entities)
        {
            if (doc == null || doc.Database == null || entities == null)
            {
                return;
            }

            // Entities are already added to the database by the geometry creators.
            // No additional action is needed here unless there are specific finalization steps.
            doc.Editor.WriteMessage("\nFinalized spiral staircase creation.");
        }
    }
}