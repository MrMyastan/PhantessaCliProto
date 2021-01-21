using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PhantessaCliProto
{
    public class InventoryContext : DbContext
    {
        public DbSet<Record> Records { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=inventory.db");
    }

    public class Record
    {
        public int RecordId { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Shelf { get; set; }
    }

}