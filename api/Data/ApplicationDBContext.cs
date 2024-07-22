using api.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

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

            modelBuilder.Entity<DefaultParameterSettingMaster>()
            .HasMany(e => e.Details)
            .WithOne(e => e.DefaultParameterSettingMaster)
            .HasForeignKey(e => e.DefaultParameterSettingMasterId)
            .IsRequired(true);

            Seeder.Initialize(modelBuilder);
        }
    }
}
