using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpiralStairPlugin
{
    public interface IGeometry
    {
        Entity[] Create(Document doc, StairParameters parameters);
    }

    public class EntityCollection
    {
        private System.Collections.Generic.List<Entity> entities;

        public EntityCollection()
        {
            entities = new System.Collections.Generic.List<Entity>();
        }

        public void Add(Entity entity)
        {
            entities.Add(entity);
        }

        public Entity[] ToArray()
        {
            return entities.ToArray();
        }
    }
}