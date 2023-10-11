using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrindlyBot_LiB.Models
{
    public class BookModel
    {
        [Key] public int BookID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string BookCover { get; set; }

        [NotMapped]
        public IFormFile ImagePath { get; set; }
    }
}
