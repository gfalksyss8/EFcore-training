using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Databasuppgift_2.Models


{
    // Enkel modellklass (entity) som EF Core mappa till en tabell "Product"
    public class Product
    {
        // Primärnyckel. Ef c0re ser "ProductID" och gör det till PK
        public int ProductID { get; set; }

        [Required]
        public int ProductPrice { get; set; }

        // Optional (Nullable '?') text med max 250
        [MaxLength(250)]
        public string? ProductDescription { get; set; }

        // Required text med max 100
        [Required, MaxLength(100)]
        public string ProductName { get; set; } = null!;

        // Foreign keys inherited from Category
        public int CategoryID { get; set; } 

        public Category Category { get; set; } = null!;
    }
}
