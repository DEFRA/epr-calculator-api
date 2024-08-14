﻿using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;
using EPR.Calculator.API.Data.DataSeeder;

namespace EPR.Calculator.API.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<DefaultParameterSettingMaster> DefaultParameterSettings { get; set; }

        public DbSet<DefaultParameterSettingDetail> DefaultParameterSettingDetail { get; set; }

        public DbSet<DefaultParameterTemplateMaster> DefaultParameterTemplateMasterList { get; set; }

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

            Seeder.Initialize(modelBuilder);
        }
    }
}
