using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataSeeder;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext() { }
        public ApplicationDBContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<DefaultParameterSettingMaster> DefaultParameterSettings { get; set; }

        public DbSet<DefaultParameterSettingDetail> DefaultParameterSettingDetail { get; set; }

        public DbSet<DefaultParameterTemplateMaster> DefaultParameterTemplateMasterList { get; set; }

        public DbSet<CalculatorRunClassification> CalculatorRunClassifications { get; set; }

        public DbSet<CalculatorRun> CalculatorRuns { get; set; }

        public DbSet<LapcapDataTemplateMaster> LapcapDataTemplateMaster { get; set; }

        public DbSet<LapcapDataMaster> LapcapDataMaster { get; set; }

        public DbSet<LapcapDataDetail> LapcapDataDetail { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DefaultParameterTemplateMaster>().Property(x => x.ValidRangeFrom).HasPrecision(18, 3);
            modelBuilder.Entity<DefaultParameterTemplateMaster>().Property(x => x.ValidRangeTo).HasPrecision(18, 3);
            modelBuilder.Entity<DefaultParameterSettingDetail>();
            modelBuilder.Entity<DefaultParameterSettingMaster>();

            modelBuilder.Entity<DefaultParameterSettingMaster>()
            .HasMany(e => e.Details)
            .WithOne(e => e.DefaultParameterSettingMaster)
            .HasForeignKey(e => e.DefaultParameterSettingMasterId)
            .IsRequired(true);

            modelBuilder.Entity<LapcapDataTemplateMaster>();
            modelBuilder.Entity<LapcapDataMaster>();
            modelBuilder.Entity<LapcapDataDetail>();

            modelBuilder.Entity<LapcapDataMaster>()
            .HasMany(e => e.Details)
            .WithOne(e => e.LapcapDataMaster)
            .HasForeignKey(e => e.LapcapDataMasterId)
            .IsRequired(true);

            modelBuilder.Entity<LapcapDataTemplateMaster>()
            .HasMany(e => e.Details)
            .WithOne(e => e.LapcapDataTemplateMaster)
            .HasForeignKey(e => e.UniqueReference)
            .IsRequired(true);

            Seeder.Initialize(modelBuilder);
        }
    }
}
