namespace SpiralStairPlugin
{
    public interface IInit
    {
        AutoCADContext Initialize();
        CenterPoleOptions GetCenterPoleOptions();
    }

    public class AutoCADContext
    {
        public Autodesk.AutoCAD.DatabaseServices.Database Database { get; set; }
        public Autodesk.AutoCAD.ApplicationServices.Document Document { get; set; }
    }

    public class CenterPoleOptions
    {
        public double[] Diameters { get; set; }
    }
}