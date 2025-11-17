using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Databasuppgift_2.Models


{
    // Enkel modellklass (entity) som EF Core mappa till en tabell "Category"
    public class Category
    {
        // Primärnyckel. Ef c0re ser "Categoryid" och gör det till PK
        public int CategoryID { get; set; }

        // Required == får inte vara null (Varken i C# eller i databasen)
        // MaxLength = genererar en kolumn med maxlängd 100 + används vid validering
        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = null!;

        // Optional (Nullable '?') text med max 250
        [MaxLength(250)]
        public string? CategoryDescription { get; set; }
    }
}
