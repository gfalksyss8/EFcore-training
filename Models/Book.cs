using System.ComponentModel.DataAnnotations;

namespace Databasuppgift_2.Models

{
    // Enkel modellklass (entity) som EF Core mappar till en tabell "Book"
    public class Book
    {
        // Primärnyckel. Ef c0re ser "BookID" och gör det till PK
        public int BookID { get; set; }

        // Required == får inte vara null (Varken i C# eller i databasen)
        // MaxLength = genererar en kolumn med maxlängd 100 + används vid validering
        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public int Year { get; set; }

        // Foreign keys
        public int AuthorID { get; set; }
        public Author? Author { get; set; }
    }
}
