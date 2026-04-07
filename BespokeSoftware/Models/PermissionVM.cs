using Microsoft.AspNetCore.Mvc.Rendering;

namespace BespokeSoftware.Models
{
    public class PermissionVM
    {
        public int RoleId { get; set; }
        public bool Add { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool List { get; set; }
        public bool View { get; set; }
        public List<SelectListItem>? RoleList { get; set; }
        public List<dynamic>? RolePermissionList { get; set; }
    }
}
