using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpiralStairPlugin
{
    public interface ITweaks
    {
        EntityCollection ApplyTweaks(Document doc, EntityCollection entities, ValidatedStairInput input, StairParameters parameters);
    }
}