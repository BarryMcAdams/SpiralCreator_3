using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace SpiralStairPlugin
{
    public class CenterPole : IGeometry
    {
        public Entity[] Create(Document doc, StairParameters parameters)
        {
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a circle in memory
                using (Circle circle = new Circle())
                {
                    circle.Center = Point3d.Origin; // Base at (0,0,0)
                    circle.Radius = parameters.CenterPoleDia / 2;

                    // Create a region from the circle in memory
                    DBObjectCollection curves = new DBObjectCollection { circle };
                    using (Region region = Region.CreateFromCurves(curves)[0] as Region)
                    {
                        // Extrude the region into a cylinder
                        Solid3d pole = new Solid3d();
                        pole.CreateExtrudedSolid(region, new Vector3d(0, 0, parameters.OverallHeight), new SweepOptions());
                        // No displacement - bottom stays at Z=0

                        // Set color to darker grey (AutoCAD color index 251)
                        pole.ColorIndex = 251;

                        btr.AppendEntity(pole);
                        tr.AddNewlyCreatedDBObject(pole, true);

                        tr.Commit();
                        return new Entity[] { pole };
                    }
                }
            }
        }
    }
}