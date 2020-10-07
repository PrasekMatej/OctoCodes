using Microsoft.EntityFrameworkCore;
using OctoCodes.Models;

namespace OctoCodes.Data
{
    public class OctoCodesContext : DbContext
    {
        public OctoCodesContext(DbContextOptions<OctoCodesContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}
