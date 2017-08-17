using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UtilityDelta.EFCore.Entities;

namespace UtilityDelta.EFCore.Database
{
    public class FamilyContext : DbContext
    {
        public const string DatabasePath = "family.db";

        public DbSet<Grandparent> Grandparents { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Kid> Kids { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Grandparent>();
            modelBuilder.Entity<Parent>();
            modelBuilder.Entity<Kid>();
        }
    }
}
