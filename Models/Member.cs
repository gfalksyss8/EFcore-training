using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Databasuppgift_2.Models
{
    public class Member
    {
        // PK
        public int MemberID { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null;

        [Required, MaxLength(100)]
        public string Email { get; set; } = null;

        // Navigation --> "En medlem kan ha många lån"
        public List<Loan> Loans { get; set; } = null;
    }
}
