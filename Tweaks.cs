using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpiralStairPlugin
{
    public class Tweaks : ITweaks
    {
        public EntityCollection ApplyTweaks(Document doc, EntityCollection entities, ValidatedStairInput input, StairParameters parameters)
        {
            return entities;
        }
    }
}