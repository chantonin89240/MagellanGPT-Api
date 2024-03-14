using API.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Data
{
    public class MongoDbContext : IMongoDbContext
    {
        //public DbSet<MyEntity> MyEntities { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseCosmos(
        //        "<your-connection-string>",
        //        "<your-database-name>",
        //        options => { options.UseMongoDb(); });
        //}

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.HasDefaultContainer("<your-collection-name>");
        //}
  
    }
}
