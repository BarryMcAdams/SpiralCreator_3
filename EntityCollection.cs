using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace SpiralStairPlugin
{
    public class EntityCollection
    {
        private List<(string Type, Entity Entity)> entities = new List<(string, Entity)>();

        public void Add(string type, Entity entity)
        {
            entities.Add((type, entity));
        }

        public int Count
        {
            get { return entities.Count; }
        }

        public Entity GetByType(string type)
        {
            return entities.LastOrDefault(e => e.Type == type).Entity;
        }

        public IEnumerable<Entity> GetAllByType(string type)
        {
            return entities.Where(e => e.Type == type).Select(e => e.Entity);
        }

        public IEnumerable<Entity> GetAllEntities()
        {
            return entities.Select(e => e.Entity);
        }
    }
}