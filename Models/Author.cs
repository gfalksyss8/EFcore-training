using System.ComponentModel.DataAnnotations;

namespace Databasuppgift_2.Models

{
    // Enkel modellklass (entity) som EF Core mappar till en tabell "Author"
    public class Author
    {
        // Primärnyckel. Ef c0re ser "AuthorID" och gör det till PK
        public int AuthorID { get; set; }

        // Required == får inte vara null (Varken i C# eller i databasen)
        // MaxLength = genererar en kolumn med maxlängd 100 + används vid validering
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required, MaxLength(100)]
        public string? Country { get; set; }

        // Authors have many books
        public List<Book> Books { get; set; } = new List<Book>();
    }
}
