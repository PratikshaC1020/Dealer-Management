namespace BespokeSoftware.Models
{
    public class Role
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public bool IsDelete { get; set; }

        public List<Role> RoleList { get; set; }
    }
}
