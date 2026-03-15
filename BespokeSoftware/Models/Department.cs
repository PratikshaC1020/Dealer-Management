namespace BespokeSoftware.Models
{
    public class Department
    {
        public int DepID { get; set; }
        public string DepartmentName { get; set; }
        public bool IsDelete { get; set; }

        public List<Department> DepartmentList { get; set; }
    }
}
