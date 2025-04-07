using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpiralStairPlugin
{
    public class Init : IInit
    {
        public AutoCADContext Initialize()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            return new AutoCADContext { Document = doc, Database = db };
        }

        public CenterPoleOptions GetCenterPoleOptions()
        {
            double[] diameters = new double[] { 4.0, 6.0, 8.0 };
            return new CenterPoleOptions { Diameters = diameters };
        }
    }
}