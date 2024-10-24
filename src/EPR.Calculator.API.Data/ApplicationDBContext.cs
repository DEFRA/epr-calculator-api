﻿using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataSeeder;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext()
        {
        }

        public ApplicationDBContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer();
            }
        }

        public DbSet<DefaultParameterSettingMaster> DefaultParameterSettings { get; set; }

        public DbSet<DefaultParameterSettingDetail> DefaultParameterSettingDetail { get; set; }

        public DbSet<DefaultParameterTemplateMaster> DefaultParameterTemplateMasterList { get; set; }

        public DbSet<CalculatorRunClassification> CalculatorRunClassifications { get; set; }

        public DbSet<CalculatorRun> CalculatorRuns { get; set; }

        public DbSet<LapcapDataTemplateMaster> LapcapDataTemplateMaster { get; set; }

        public DbSet<LapcapDataMaster> LapcapDataMaster { get; set; }

        public DbSet<LapcapDataDetail> LapcapDataDetail { get; set; }

        public DbSet<CalculatorRunOrganisationDataDetail> CalculatorRunOrganisationDataDetails { get; set; }

        public DbSet<CalculatorRunOrganisationDataMaster> CalculatorRunOrganisationDataMaster {  get; set; }

        public DbSet<CalculatorRunPomDataDetail> CalculatorRunPomDataDetails { get; set; }

        public DbSet<CalculatorRunPomDataMaster> CalculatorRunPomDataMaster { get; set; }

        public DbSet<OrganisationData> OrganisationData { get; set; }

        public DbSet<PomData> PomData { get; set; }

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

            modelBuilder.Entity<CalculatorRunOrganisationDataDetail>();
            modelBuilder.Entity<CalculatorRunOrganisationDataMaster>()
            .HasMany(e => e.Details)
            .WithOne(e => e.CalculatorRunOrganisationDataMaster )
            .HasForeignKey(e => e.CalculatorRunOrganisationDataMasterId)
            .IsRequired(true);

            modelBuilder.Entity<CalculatorRunPomDataDetail>();
            modelBuilder.Entity<CalculatorRunPomDataMaster>()
            .HasMany(e => e.Details)
            .WithOne(e => e.CalculatorRunPomDataMaster)
            .HasForeignKey(e => e.CalculatorRunPomDataMasterId);

            modelBuilder.Entity<OrganisationData>().HasNoKey();
            modelBuilder.Entity<PomData>().HasNoKey();

            modelBuilder.Entity<CalculatorRun>().Property(e => e.CalculatorRunPomDataMasterId).IsRequired(false);
            modelBuilder.Entity<CalculatorRun>().Property(e => e.CalculatorRunOrganisationDataMasterId).IsRequired(false);
            modelBuilder.Entity<CalculatorRun>().Property(e => e.LapcapDataMasterId).IsRequired(false);
            modelBuilder.Entity<CalculatorRun>().Property(e => e.DefaultParameterSettingMasterId).IsRequired(false);

            modelBuilder.Entity<CalculatorRunPomDataMaster>()
            .HasMany(e => e.RunDetails)
            .WithOne(e => e.CalculatorRunPomDataMaster)
            .HasForeignKey(e => e.CalculatorRunPomDataMasterId);

            modelBuilder.Entity<CalculatorRunOrganisationDataMaster>()
            .HasMany(e => e.RunDetails)
            .WithOne(e => e.CalculatorRunOrganisationDataMaster)
            .HasForeignKey(e => e.CalculatorRunOrganisationDataMasterId);

            modelBuilder.Entity<LapcapDataMaster>()
            .HasMany(e => e.RunDetails)
            .WithOne(e => e.LapcapDataMaster)
            .HasForeignKey(e => e.LapcapDataMasterId);

            modelBuilder.Entity<DefaultParameterSettingMaster>()
            .HasMany(e => e.RunDetails)
            .WithOne(e => e.DefaultParameterSettingMaster)
            .HasForeignKey(e => e.DefaultParameterSettingMasterId);

            Seeder.Initialize(modelBuilder);
        }
    }
}
