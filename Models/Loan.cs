using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Databasuppgift_2.Models
{
    public class Loan
    {
        // PK
        public int LoanID { get; set; }

        // FK
        public int MemberID { get; set; }

        public int BookID { get; set; }

        [Required]
        public DateTime LoanDate { get;set;  }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public List<Book> Books { get; set; } = new();

        // Tillgång till instanser
        public Member? Member { get; set; }
        public Book? Book { get; set; }
    }
}
