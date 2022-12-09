using System.Data.Entity;
using System.Linq;
using Data;

namespace Helper
{
    public class EntityManager
    {
        public static void ContextClear(ALFAMEDIAEntities context)
        {
            var changedEntries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();
            foreach (var entry in changedEntries)
                entry.State = EntityState.Detached;
        }

        public static void EntityReload(ALFAMEDIAEntities context)
        {
            foreach (var entity in context.ChangeTracker.Entries())
            {
                if (entity.State != EntityState.Added || entity.State == EntityState.Modified)
                    entity.Reload();
                else 
                    entity.State = EntityState.Detached;
            }
        }
    }
}
