using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }

        public DbSet<DefaultParameterSettingMaster> DefaultParameterSettings { get; set; }

        public DbSet<DefaultParameterSettingDetail> DefaultParameterSettingDetail { get; set; }

        public DbSet<DefaultParameterTemplateMaster> DefaultParameterTemplateMasterList { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DefaultParameterTemplateMaster>();
            modelBuilder.Entity<DefaultParameterSettingDetail>();
            modelBuilder.Entity<DefaultParameterSettingMaster>();
            Seeder.Initialize(modelBuilder);
        }
    }
}
