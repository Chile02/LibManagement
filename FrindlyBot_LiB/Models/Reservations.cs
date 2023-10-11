using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FrindlyBot_LiB.Models
{
    public class Reservations
    {
        [Key] public int ReservationID { get; set; }
        [ForeignKey("AspNetUsers")] public string Email { get; set; }
        [ForeignKey("Files")] public int BookID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime endDate { get; set; }
        public string Status { get; set; }
    }
}
