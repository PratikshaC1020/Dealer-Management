namespace BespokeSoftware.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public bool IsDelete { get; set; }

        public List<Category> CategoryList { get; set; } = new List<Category>();
    }
}
