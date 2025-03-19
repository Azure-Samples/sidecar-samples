using System.ComponentModel.DataAnnotations;

namespace devShopDNC.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = String.Empty;
    }
}
