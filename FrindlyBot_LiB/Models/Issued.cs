using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FrindlyBot_LiB.Models
{
    public class Issued
    {
        [Key] public int IssueID { get; set; }
        [ForeignKey("AspNetUsers")] public string Email { get; set; }
        [ForeignKey("Files")] public int BookId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int Penalty { get; set; }
    }
}
