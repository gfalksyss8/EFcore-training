using Databasuppgift_2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Databasuppgift_2
{
    // DbContext = "enheten" representerar databasen
    public class ShopContext : DbContext
    {
        // Db<Category> mappar till tabellen Category i databasen
        public DbSet<Category> Categories => Set<Category>();

        // Här berättar vi för EF Core att vi vill använda SQLite och var filen ska lämnas in
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "shop.db");

            optionsBuilder.UseSqlite($"Filename={dbPath}");
        }

        // OnModelCreating används för att finjustera modellen
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(e =>
            {
                // Sätter primärnyckel
                e.HasKey(x => x.CategoryID);
                
                // Säkerställer samma regler som data annotations (required + MaxLength)
                e.Property(x => x.CategoryName)
                    .IsRequired().HasMaxLength(100);

                e.Property(x => x.CategoryDescription)
                    .HasMaxLength(250);

                // Skapar ett UNIQUE-index på CategoryName
                // Databasen tillåter inte två rader med samma CategoryName. Kanske inte vill ha två categories per kategori
                e.HasIndex(x => x.CategoryName)
                    .IsUnique();
            });
        }

    }
}
