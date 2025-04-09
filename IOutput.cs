using Autodesk.AutoCAD.ApplicationServices;

namespace SpiralStairPlugin
{
    public interface IOutput
    {
        void Finalize(Document doc, ValidatedStairInput input, StairParameters parameters, EntityCollection entities);
    }
}