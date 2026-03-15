namespace BespokeSoftware.Models
{
    public class User
    {
        // ---------------- T_User ----------------
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public List<User> UserList { get; set; }
    }
}
