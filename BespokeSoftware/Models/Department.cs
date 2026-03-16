using System.ComponentModel.DataAnnotations;

namespace BespokeSoftware.Models
{
    public class Department
    {
        public int DepID { get; set; }
        [Required(ErrorMessage = "Department name is required")]
        public string DepartmentName { get; set; }

        public bool IsDelete { get; set; }

        public List<Department> DepartmentList { get; set; } = new List<Department>();
    }
}