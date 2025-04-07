using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpiralStairPlugin
{
    public class Output : IOutput
    {
        public void Finalize(Document doc, ValidatedStairInput input, StairParameters parameters, EntityCollection entities)
        {
            // No need to re-add entities since they were already added in Create methods
            // If we add new entities in the future, we can append them here
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                tr.Commit(); // Just commit to ensure any pending changes are saved
            }
        }
    }
}