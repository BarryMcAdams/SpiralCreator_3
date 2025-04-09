using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpiralStairPlugin
{
    public interface IGeometry
    {
        Entity[] Create(Document doc, Transaction tr, StairParameters parameters);
    }
}