using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpiralStairPlugin
{
    public interface IOutput
    {
        void Finalize(Document doc, ValidatedStairInput input, StairParameters parameters, EntityCollection entities);
    }
}