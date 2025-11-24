using Databasuppgift_2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Databasuppgift_2
{
    // DbContext = "enheten" representerar databasen
    public class ShopContext : DbContext
    {
        // Db<Category> mappar till tabellen Category i databasen
        public DbSet<Category> Categories => Set<Category>();
        // Db<Product> mappar till tabellen Product i databasen
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Book> Books => Set<Book>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Loan> Loans => Set<Loan>();

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

                // Has many products
                e.HasMany(x => x.CategoryProducts);
            });

            modelBuilder.Entity<Product>(e =>
            {
                // Sätter primärnyckel
                e.HasKey(x => x.ProductID);

                // Säkerställer samma regler som data annotations (required + MaxLength)
                e.Property(x => x.ProductPrice).IsRequired();

                e.Property(x => x.ProductDescription).HasMaxLength(250);

                e.Property(x => x.ProductName).IsRequired().HasMaxLength(100);

                // Skapar ett UNIQUE-index på ProductName
                // Databasen tillåter inte två rader med samma ProductName.
                e.HasIndex(x => x.ProductName).IsUnique();

                // Foreign 
                e.HasOne(x => x.Category)
                                .WithMany(x => x.CategoryProducts)
                                .HasForeignKey(x => x.CategoryID)
                                .OnDelete(DeleteBehavior.Restrict)
                                .IsRequired();
            });

            modelBuilder.Entity<Book>(e =>
            {
                // Sätter primärnyckel
                e.HasKey(x => x.BookID);

                // Säkerställer regler för properties
                e.Property(x => x.Title).HasMaxLength(100).IsRequired();
                e.Property(x => x.Year).IsRequired();

                // Unique
                e.HasIndex(x => x.Title).IsUnique();

                // Foreign
                e.HasOne(x => x.Author)
                                .WithMany(x => x.Books)
                                .HasForeignKey(x => x.AuthorID)
                                .OnDelete(DeleteBehavior.Restrict)
                                .IsRequired();
            });

            modelBuilder.Entity<Author>(e =>
            {
                // sätter primärnyckel
                e.HasKey(x => x.AuthorID);

                // Säkerställer regler för properties
                e.Property(x => x.Name).HasMaxLength(100).IsRequired();
                e.Property(x => x.Country).HasMaxLength(100).IsRequired();

                // Has many books
                e.HasMany(x => x.Books);
            });
            modelBuilder.Entity<Member>(e =>
            {
                // sätter primärnyckel
                e.HasKey(x => x.MemberID);

                // Properties, regler
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
                e.Property(x=>x.Email).HasMaxLength(100).IsRequired();

                // Unik e-post
                e.HasIndex(x => x.Email).IsUnique();
            });
            modelBuilder.Entity<Loan>(e =>
            {
                // PK
                e.HasKey(x => x.LoanID);

                // Properties
                e.Property(x => x.LoanDate).IsRequired();
                e.Property(x => x.DueDate).IsRequired();

                // FK
                e.HasOne(x => x.Member)
                    .WithMany(x => x.Loans)
                    .HasForeignKey(x => x.MemberID)
                    .OnDelete(DeleteBehavior.Cascade); // Om ni tar bort en member, ta bort alla Loans
                e.HasOne(x => x.Book)
                    .WithMany()
                    .HasForeignKey(x => x.BookID)
                    .OnDelete(DeleteBehavior.Restrict); // Försöker du ta bort en bok
            });
        }
    }
}
