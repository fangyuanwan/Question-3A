using System;
using Microsoft.EntityFrameworkCore;

namespace Question3aConsume.Model
{
    public class taskItemContext: DbContext
    {
        public taskItemContext(DbContextOptions<taskItemContext> options) : base(options)
        {
        }

        public DbSet<taskItem> taskitems { get; set; }
    }
}
