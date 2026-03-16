using System.ComponentModel.DataAnnotations;

namespace BespokeSoftware.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        [Required(ErrorMessage = "Category name is required")]
        public string CategoryName { get; set; }
        public bool IsDelete { get; set; }

        public List<Category> CategoryList { get; set; } = new List<Category>();
    }
}
