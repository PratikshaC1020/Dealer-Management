using BespokeSoftware.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System;

namespace BespokeSoftware.Controllers
{

    [Authorize(AuthenticationSchemes = "MyCookieAuth")]
    public class MasterDataController : Controller
    {
        private readonly string _connectionString;

        public MasterDataController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        [HttpGet]
        public IActionResult Department(int? id)
        {
            Department model = new Department();
            model.DepartmentList = new List<Department>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                if (id != null)
                {
                    SqlCommand cmd = new SqlCommand("SELECT DepID,Department,IsDelete FROM T_Department WHERE DepID=@id", con);
                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        model.DepID = Convert.ToInt32(dr["DepID"]);
                        model.DepartmentName = dr["Department"].ToString();
                        model.IsDelete = Convert.ToBoolean(dr["IsDelete"]);
                    }

                    dr.Close();
                }

                SqlCommand cmd2 = new SqlCommand("SELECT DepID,Department,IsDelete FROM T_Department  WHERE IsDelete = 0", con);

                SqlDataReader dr2 = cmd2.ExecuteReader();

                while (dr2.Read())
                {
                    model.DepartmentList.Add(new Department
                    {
                        DepID = Convert.ToInt32(dr2["DepID"]),
                        DepartmentName = dr2["Department"].ToString(),
                        IsDelete = Convert.ToBoolean(dr2["IsDelete"])
                    });
                }
            }

            return View(model);
        }

        // SAVE / UPDATE
        [HttpPost]
        public IActionResult Department(Department model)
        {
            if (!ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("SELECT DepID,Department,IsDelete FROM T_Department", con);
                    SqlDataReader dr = cmd.ExecuteReader();

                    model.DepartmentList = new List<Department>();

                    while (dr.Read())
                    {
                        model.DepartmentList.Add(new Department
                        {
                            DepID = Convert.ToInt32(dr["DepID"]),
                            DepartmentName = dr["Department"].ToString(),
                            IsDelete = Convert.ToBoolean(dr["IsDelete"])
                        });
                    }
                }

                return View(model);
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                // ✅ DUPLICATE CHECK
                SqlCommand checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM T_Department WHERE LOWER(LTRIM(RTRIM(Department))) = LOWER(LTRIM(RTRIM(@Department))) AND DepID != @DepID",
                con);

                checkCmd.Parameters.AddWithValue("@Department", model.DepartmentName.Trim());
                checkCmd.Parameters.AddWithValue("@DepID", model.DepID);

                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    TempData["SweetAlertMessage"] = "Department already exists";
                    TempData["SweetAlertOptions"] = "error";

                    return RedirectToAction("Department");
                }

                if (model.DepID == 0)
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO T_Department(Department,IsDelete) VALUES(@Department,@IsDelete)", con);

                    cmd.Parameters.AddWithValue("@Department", model.DepartmentName.Trim());
                    cmd.Parameters.AddWithValue("@IsDelete", model.IsDelete);

                    cmd.ExecuteNonQuery();

                    TempData["SweetAlertMessage"] = "Department Saved Successfully";
                    TempData["SweetAlertOptions"] = "success";
                }
                else
                {
                    SqlCommand cmd = new SqlCommand("UPDATE T_Department SET Department=@Department,IsDelete=@IsDelete WHERE DepID=@DepID", con);

                    cmd.Parameters.AddWithValue("@Department", model.DepartmentName.Trim());
                    cmd.Parameters.AddWithValue("@IsDelete", model.IsDelete);
                    cmd.Parameters.AddWithValue("@DepID", model.DepID);

                    cmd.ExecuteNonQuery();

                    TempData["SweetAlertMessage"] = "Department Updated Successfully";
                    TempData["SweetAlertOptions"] = "success";
                }
            }

            return RedirectToAction("Department");
        }

        public IActionResult DeleteDepartment(int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(
                "UPDATE T_Department SET IsDelete = 1 WHERE DepID=@id", con);

                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }

            TempData["SweetAlertMessage"] = "Department Deleted Successfully";
            TempData["SweetAlertOptions"] = "success";

            return RedirectToAction("Department");
        }
        [HttpGet]
        public IActionResult Category(int? id)
        {
            Category model = new Category();
            model.CategoryList = new List<Category>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                // Edit data load
                if (id != null)
                {
                    SqlCommand cmd = new SqlCommand("SELECT ID,Category,IsDelete FROM T_Category WHERE ID=@id", con);
                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        model.CategoryID = Convert.ToInt32(dr["ID"]);
                        model.CategoryName = dr["Category"].ToString();
                        model.IsDelete = Convert.ToBoolean(dr["IsDelete"]);
                    }

                    dr.Close();
                }

                // Category list load
                SqlCommand cmd2 = new SqlCommand("SELECT ID,Category,IsDelete FROM T_Category WHERE IsDelete = 0", con);
                SqlDataReader dr2 = cmd2.ExecuteReader();

                while (dr2.Read())
                {
                    model.CategoryList.Add(new Category
                    {
                        CategoryID = Convert.ToInt32(dr2["ID"]),
                        CategoryName = dr2["Category"].ToString(),
                        IsDelete = Convert.ToBoolean(dr2["IsDelete"])
                    });
                }
            }

            return View(model);
        }


        // SAVE / UPDATE
        [HttpPost]
        public IActionResult Category(Category model)
        {
            if (!ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("SELECT ID,Category,IsDelete FROM T_Category", con);
                    SqlDataReader dr = cmd.ExecuteReader();

                    model.CategoryList = new List<Category>();

                    while (dr.Read())
                    {
                        model.CategoryList.Add(new Category
                        {
                            CategoryID = Convert.ToInt32(dr["ID"]),
                            CategoryName = dr["Category"].ToString(),
                            IsDelete = Convert.ToBoolean(dr["IsDelete"])
                        });
                    }
                }

                return View(model);
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM T_Category WHERE LOWER(LTRIM(RTRIM(Category))) = LOWER(LTRIM(RTRIM(@Category))) AND ID != @CategoryID", con);

                checkCmd.Parameters.AddWithValue("@Category", model.CategoryName.Trim());
                checkCmd.Parameters.AddWithValue("@CategoryID", model.CategoryID);

                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    TempData["SweetAlertMessage"] = "Category already exists";
                    TempData["SweetAlertOptions"] = "error";
                    return RedirectToAction("Category");
                }
                if (model.CategoryID == 0)
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO T_Category(Category,IsDelete) VALUES(@Category,@IsDelete)", con);

                    cmd.Parameters.AddWithValue("@Category", model.CategoryName.Trim());
                    cmd.Parameters.AddWithValue("@IsDelete", model.IsDelete);

                    cmd.ExecuteNonQuery();

                    TempData["SweetAlertMessage"] = "Category Saved Successfully";
                    TempData["SweetAlertOptions"] = "success";
                }
                else
                {
                    SqlCommand cmd = new SqlCommand("UPDATE T_Category SET Category=@Category,IsDelete=@IsDelete WHERE ID=@CategoryID", con);

                    cmd.Parameters.AddWithValue("@Category", model.CategoryName.Trim());
                    cmd.Parameters.AddWithValue("@IsDelete", model.IsDelete);
                    cmd.Parameters.AddWithValue("@CategoryID", model.CategoryID);

                    cmd.ExecuteNonQuery();

                    TempData["SweetAlertMessage"] = "Category Updated Successfully";
                    TempData["SweetAlertOptions"] = "success";
                }
            }

            return RedirectToAction("Category", new { id = (int?)null });
        }

        public IActionResult DeleteCategory(int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(
                "UPDATE T_Category SET IsDelete = 1 WHERE ID=@id", con);

                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }

            TempData["SweetAlertMessage"] = "Category Deleted Successfully";
            TempData["SweetAlertOptions"] = "success";

            return RedirectToAction("Category");
        }
        [HttpGet]
        public IActionResult Payment(int? id)
        {
            PaymentMode model = new PaymentMode();
            model.PaymentModeList = new List<PaymentMode>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                if (id != null)
                {
                    SqlCommand cmd = new SqlCommand("SELECT ID,PaymentMode,IsDelete FROM T_PaymentMode WHERE ID=@id", con);
                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        model.PaymentModeID = Convert.ToInt32(dr["ID"]);
                        model.Payment = dr["PaymentMode"].ToString();
                        model.IsDelete = Convert.ToBoolean(dr["IsDelete"]);
                    }

                    dr.Close();
                }

                SqlCommand cmd2 = new SqlCommand("SELECT ID,PaymentMode,IsDelete FROM T_PaymentMode WHERE IsDelete = 0", con);

                SqlDataReader dr2 = cmd2.ExecuteReader();

                while (dr2.Read())
                {
                    model.PaymentModeList.Add(new PaymentMode
                    {
                        PaymentModeID = Convert.ToInt32(dr2["ID"]),
                        Payment = dr2["PaymentMode"].ToString(),
                        IsDelete = Convert.ToBoolean(dr2["IsDelete"])
                    });
                }
            }

            return View(model);
        }


        // SAVE / UPDATE
        [HttpPost]
        public IActionResult Payment(PaymentMode model)
        {
            if (!ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("SELECT ID,PaymentMode,IsDelete FROM T_PaymentMode", con);
                    SqlDataReader dr = cmd.ExecuteReader();

                    model.PaymentModeList = new List<PaymentMode>();

                    while (dr.Read())
                    {
                        model.PaymentModeList.Add(new PaymentMode
                        {
                            PaymentModeID = Convert.ToInt32(dr["ID"]),
                            Payment = dr["PaymentMode"].ToString(),
                            IsDelete = Convert.ToBoolean(dr["IsDelete"])
                        });
                    }
                }

                return View(model);
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM T_PaymentMode WHERE LOWER(LTRIM(RTRIM(PaymentMode))) = LOWER(LTRIM(RTRIM(@PaymentMode))) AND ID != @PaymentModeID", con);

                checkCmd.Parameters.AddWithValue("@PaymentMode", model.Payment.Trim());
                checkCmd.Parameters.AddWithValue("@PaymentModeID", model.PaymentModeID);

                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    TempData["SweetAlertMessage"] = "Payment Mode already exists";
                    TempData["SweetAlertOptions"] = "error";
                    return RedirectToAction("Payment");
                }
                if (model.PaymentModeID == 0)
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO T_PaymentMode(PaymentMode,IsDelete) VALUES(@PaymentMode,@IsDelete)", con);

                    cmd.Parameters.AddWithValue("@PaymentMode", model.Payment.Trim());
                    cmd.Parameters.AddWithValue("@IsDelete", model.IsDelete);

                    cmd.ExecuteNonQuery();

                    TempData["SweetAlertMessage"] = "Payment Mode Saved Successfully";
                    TempData["SweetAlertOptions"] = "success";
                }
                else
                {
                    SqlCommand cmd = new SqlCommand("UPDATE T_PaymentMode SET PaymentMode=@PaymentMode,IsDelete=@IsDelete WHERE ID=@PaymentModeID", con);

                    cmd.Parameters.AddWithValue("@PaymentMode", model.Payment.Trim());
                    cmd.Parameters.AddWithValue("@IsDelete", model.IsDelete);
                    cmd.Parameters.AddWithValue("@PaymentModeID", model.PaymentModeID);

                    cmd.ExecuteNonQuery();

                    TempData["SweetAlertMessage"] = "Payment Mode Updated Successfully";
                    TempData["SweetAlertOptions"] = "success";
                }
            }

            return RedirectToAction("Payment", new { id = (int?)null });
        }
        public IActionResult DeletePaymentMode(int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(
                "UPDATE T_PaymentMode SET IsDelete = 1 WHERE ID=@id", con);

                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }

            TempData["SweetAlertMessage"] = "Payment Mode Deleted Successfully";
            TempData["SweetAlertOptions"] = "success";

            return RedirectToAction("Payment");
        }
        [HttpGet]
        public IActionResult User(int? id)
        {
            User model = new User();
            if (id == null)
            {
                model.IsActive = true;   // default Active
            }
            // dropdown load
            ViewBag.DepartmentList = GetDepartmentList();
            ViewBag.RoleList = GetRoleList();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                if (id != null)
                {
                    SqlCommand cmd = new SqlCommand("SELECT * FROM T_User WHERE UserID=@id", con);
                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        model.UserID = Convert.ToInt32(dr["UserID"]);
                        model.Name = dr["Name"].ToString();
                        model.Email = dr["Email"].ToString();
                        model.MobileNo = dr["MobileNo"].ToString();
                        model.Password = dr["Password"].ToString();
                        model.RoleId = Convert.ToInt32(dr["RoleId"]);
                        model.DepID = Convert.ToInt32(dr["DepID"]);
                        model.IsActive = Convert.ToBoolean(dr["IsActive"]);
                    }

                    dr.Close();
                }

                // LIST QUERY
                SqlCommand cmd2;

                if (id != null)
                {
                    cmd2 = new SqlCommand(@"
                            SELECT * 
                            FROM T_User u 
                            LEFT JOIN T_Role r ON r.RoleID = u.RoleId 
                            LEFT JOIN T_Department d ON d.DepID = u.DepID
                            WHERE u.UserID=@id", con);

                    cmd2.Parameters.AddWithValue("@id", id);
                }
                else
                {
                    cmd2 = new SqlCommand(@"
                        SELECT * 
                        FROM T_User u 
                        LEFT JOIN T_Role r ON r.RoleID = u.RoleId 
                        LEFT JOIN T_Department d ON d.DepID = u.DepID", con);
                }

                SqlDataReader dr2 = cmd2.ExecuteReader();

                while (dr2.Read())
                {
                    model.UserList.Add(new User
                    {
                        UserID = Convert.ToInt32(dr2["UserID"]),
                        Name = dr2["Name"].ToString(),
                        Email = dr2["Email"].ToString(),
                        MobileNo = dr2["MobileNo"].ToString(),
                        Password = dr2["Password"].ToString(),
                        RoleId = Convert.ToInt32(dr2["RoleId"]),
                        Role = dr2["RoleName"].ToString(),
                        DepID = Convert.ToInt32(dr2["DepID"]),
                        Department = dr2["Department"].ToString(),
                        IsActive = Convert.ToBoolean(dr2["IsActive"])
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult User(User model)
        {
            
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                if (model.UserID == 0)
                {
                    string generatedPassword = GeneratePassword();

                    SqlCommand cmd = new SqlCommand(
                    "INSERT INTO T_User(Name,Email,MobileNo,DepID,Password,RoleId,IsActive) VALUES(@Name,@Email,@MobileNo,@DepID,@Password,@RoleId,@IsActive)", con);

                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);
                    cmd.Parameters.AddWithValue("@Password", generatedPassword);
                    cmd.Parameters.AddWithValue("@DepID", model.DepID);
                    cmd.Parameters.AddWithValue("@RoleId", model.RoleId);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                    cmd.ExecuteNonQuery();

                    TempData["SweetAlertMessage"] = "User Created Password : " + generatedPassword;
                }
                else
                {
                    SqlCommand cmd = new SqlCommand(
                    "UPDATE T_User SET Name=@Name,Email=@Email,MobileNo=@MobileNo,DepID=@DepID ,RoleId=@RoleId,IsActive=@IsActive WHERE UserID=@UserID", con);

                    cmd.Parameters.AddWithValue("@Name", model.Name.Trim());
                    cmd.Parameters.AddWithValue("@Email", model.Email.Trim());
                    cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo.Trim());
                    cmd.Parameters.AddWithValue("@DepID", (object?)model.DepID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoleId", (object?)model.RoleId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                    cmd.Parameters.AddWithValue("@UserID", model.UserID);

                    cmd.ExecuteNonQuery();

                    TempData["SweetAlertMessage"] = "User Updated Successfully";
                    TempData["SweetAlertOptions"] = "success";
                }
            }

            return RedirectToAction("User", new { id = (int?)null });
        }
        public IActionResult DeleteUser(int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(
                "DELETE FROM T_User WHERE UserID=@id", con);

                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }

            TempData["SweetAlertMessage"] = "User Deleted Successfully";
            TempData["SweetAlertOptions"] = "success";

            return RedirectToAction("User");
        }
        private List<SelectListItem> GetDepartmentList()
        {
            List<SelectListItem> deptList = new List<SelectListItem>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT DepID,Department FROM T_Department WHERE IsDelete=0", con);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    deptList.Add(new SelectListItem
                    {
                        Value = dr["DepID"].ToString(),
                        Text = dr["Department"].ToString()
                    });
                }
            }

            return deptList;
        }
        private List<SelectListItem> GetRoleList()
        {
            List<SelectListItem> roleList = new List<SelectListItem>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT RoleId,RoleName FROM T_Role", con);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    roleList.Add(new SelectListItem
                    {
                        Value = dr["RoleId"].ToString(),
                        Text = dr["RoleName"].ToString()
                    });
                }
            }

            return roleList;
        }

        public string GeneratePassword()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();

            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpGet]
        public IActionResult ChangePassword(int id)
        {
            User model = new User();
            model.UserID = id;
            return View(model);
        }

        [HttpPost]
        public IActionResult UpdatePassword(int userId, string password)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(
                "UPDATE T_User SET Password=@Password WHERE UserID=@UserID", con);

                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@UserID", userId);

                cmd.ExecuteNonQuery();
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Role(int? id)
        {
            Role model = new Role();
            model.RoleList = new List<Role>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

              
                if (id != null)
                {
                    SqlCommand cmd = new SqlCommand("SELECT RoleID,RoleName,IsDelete FROM T_Role WHERE RoleID=@id", con);
                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        model.RoleID = Convert.ToInt32(dr["RoleID"]);
                        model.RoleName = dr["RoleName"].ToString();
                        model.IsDelete = Convert.ToBoolean(dr["IsDelete"]);
                    }

                    dr.Close();
                }

              
                SqlCommand cmd2 = new SqlCommand("SELECT RoleID,RoleName,IsDelete FROM T_Role WHERE IsDelete = 0", con);
                SqlDataReader dr2 = cmd2.ExecuteReader();

                while (dr2.Read())
                {
                    model.RoleList.Add(new Role
                    {
                        RoleID = Convert.ToInt32(dr2["RoleID"]),
                        RoleName = dr2["RoleName"].ToString(),
                        IsDelete = Convert.ToBoolean(dr2["IsDelete"])
                    });
                }
            }

            return View(model);
        }
        [HttpPost]
        public IActionResult Role(Role model)
        {
            ModelState.Remove("RoleList");

            if (!ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("SELECT RoleID,RoleName,IsDelete FROM T_Role", con);
                    SqlDataReader dr = cmd.ExecuteReader();

                    model.RoleList = new List<Role>();

                    while (dr.Read())
                    {
                        model.RoleList.Add(new Role
                        {
                            RoleID = Convert.ToInt32(dr["RoleID"]),
                            RoleName = dr["RoleName"].ToString(),
                            IsDelete = Convert.ToBoolean(dr["IsDelete"])
                        });
                    }
                }

                return View(model);
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                
                SqlCommand checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM T_Role WHERE LOWER(LTRIM(RTRIM(RoleName))) = LOWER(LTRIM(RTRIM(@RoleName))) AND RoleID != @RoleID", con);

                checkCmd.Parameters.AddWithValue("@RoleName", model.RoleName.Trim());
                checkCmd.Parameters.AddWithValue("@RoleID", model.RoleID);

                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    TempData["SweetAlertMessage"] = "Role already exists";
                    TempData["SweetAlertOptions"] = "error";
                    return RedirectToAction("Role");
                }

                if (model.RoleID == 0)
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO T_Role(RoleName,IsDelete) VALUES(@RoleName,@IsDelete)", con);

                    cmd.Parameters.AddWithValue("@RoleName", model.RoleName.Trim());
                    cmd.Parameters.AddWithValue("@IsDelete", model.IsDelete);

                    cmd.ExecuteNonQuery();

                    TempData["SweetAlertMessage"] = "Role Saved Successfully";
                    TempData["SweetAlertOptions"] = "success";
                }
                else
                {
                    SqlCommand cmd = new SqlCommand("UPDATE T_Role SET RoleName=@RoleName,IsDelete=@IsDelete WHERE RoleID=@RoleID", con);

                    cmd.Parameters.AddWithValue("@RoleName", model.RoleName.Trim());
                    cmd.Parameters.AddWithValue("@IsDelete", model.IsDelete);
                    cmd.Parameters.AddWithValue("@RoleID", model.RoleID);

                    cmd.ExecuteNonQuery();

                    TempData["SweetAlertMessage"] = "Role Updated Successfully";
                    TempData["SweetAlertOptions"] = "success";
                }
            }

            return RedirectToAction("Role", new { id = (int?)null });
        }
        public IActionResult DeleteRole(int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(
                "UPDATE T_Role SET IsDelete = 1 WHERE RoleID=@id", con);

                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }

            TempData["SweetAlertMessage"] = "Role Deleted Successfully";
            TempData["SweetAlertOptions"] = "success";

            return RedirectToAction("Role");
        }

        //===Role permission==

        [HttpGet]
        public IActionResult RolePermission()
        {
            PermissionVM model = new PermissionVM();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT RoleID,RoleName FROM T_Role WHERE IsDelete=0", con);
                SqlDataReader dr = cmd.ExecuteReader();

                model.RoleList = new List<SelectListItem>();

                while (dr.Read())
                {
                    model.RoleList.Add(new SelectListItem
                    {
                        Value = dr["RoleID"].ToString(),
                        Text = dr["RoleName"].ToString()
                    });
                }
            }

            return View(model);
        }
        public JsonResult GetRolePermissions(int roleId)
        {
            List<object> list = new List<object>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(@"
            SELECT p.PermissionName, rp.IsAllowed
            FROM T_Permissions p
            LEFT JOIN T_RolePermissions rp 
            ON p.PermissionId = rp.PermissionId AND rp.RoleId=@roleId
        ", con);

                cmd.Parameters.AddWithValue("@roleId", roleId);

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new
                    {
                        permissionName = dr["PermissionName"].ToString(),
                        isAllowed = dr["IsAllowed"] != DBNull.Value && Convert.ToBoolean(dr["IsAllowed"])
                    });
                }
            }

            return Json(list);
        }
        [HttpPost]
        public JsonResult SaveRolePermissions(PermissionVM model)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("SELECT PermissionId,PermissionName FROM T_Permissions", con);
                    SqlDataReader dr = cmd.ExecuteReader();

                    List<(int id, string name)> permissions = new();

                    while (dr.Read())
                    {
                        permissions.Add((
                            Convert.ToInt32(dr["PermissionId"]),
                            dr["PermissionName"].ToString()
                        ));
                    }

                    dr.Close();

                    foreach (var p in permissions)
                    {
                        bool value = false;

                        if (p.name == "Add") value = model.Add;
                        if (p.name == "Update") value = model.Update;
                        if (p.name == "Delete") value = model.Delete;
                        if (p.name == "List") value = model.List;

                        SqlCommand checkCmd = new SqlCommand(
                            "SELECT COUNT(*) FROM T_RolePermissions WHERE RoleId=@RoleId AND PermissionId=@PermissionId",
                            con);

                        checkCmd.Parameters.AddWithValue("@RoleId", model.RoleId);
                        checkCmd.Parameters.AddWithValue("@PermissionId", p.id);

                        int exists = (int)checkCmd.ExecuteScalar();

                        if (exists > 0)
                        {
                            SqlCommand updateCmd = new SqlCommand(
                                "UPDATE T_RolePermissions SET IsAllowed=@IsAllowed WHERE RoleId=@RoleId AND PermissionId=@PermissionId",
                                con);

                            updateCmd.Parameters.AddWithValue("@IsAllowed", value);
                            updateCmd.Parameters.AddWithValue("@RoleId", model.RoleId);
                            updateCmd.Parameters.AddWithValue("@PermissionId", p.id);

                            updateCmd.ExecuteNonQuery();
                        }
                        else
                        {
                            SqlCommand insertCmd = new SqlCommand(
                                "INSERT INTO T_RolePermissions(RoleId,PermissionId,IsAllowed) VALUES(@RoleId,@PermissionId,@IsAllowed)",
                                con);

                            insertCmd.Parameters.AddWithValue("@RoleId", model.RoleId);
                            insertCmd.Parameters.AddWithValue("@PermissionId", p.id);
                            insertCmd.Parameters.AddWithValue("@IsAllowed", value);

                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }

               
                return Json(new
                {
                    message = "Permissions Saved Successfully",
                    type = "success"
                });
            }
            catch (Exception)
            {
                
                return Json(new
                {
                    message = "Something went wrong!",
                    type = "error"
                });
            }
        }

       
        
    }
}