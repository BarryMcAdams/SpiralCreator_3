using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace SpiralStairPlugin
{
    public class EntityCollection
    {
        private List<Entity> entities;

        public EntityCollection()
        {
            entities = new List<Entity>();
        }

        public void Add(Entity entity)
        {
            entities.Add(entity);
        }

        public int Count
        {
            get { return entities.Count; }
        }

        public Entity this[int index]
        {
            get { return entities[index]; }
        }
    }
}