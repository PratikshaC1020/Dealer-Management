using BespokeSoftware.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System;

namespace BespokeSoftware.Controllers
{
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

                SqlCommand cmd2 = new SqlCommand("SELECT DepID,Department,IsDelete FROM T_Department", con);

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
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                if (model.DepID == 0)
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO T_Department(Department,IsDelete) VALUES(@Department,@IsDelete)", con);

                    cmd.Parameters.AddWithValue("@Department", model.DepartmentName.Trim());
                    cmd.Parameters.AddWithValue("@IsDelete", model.IsDelete);

                    cmd.ExecuteNonQuery();

                    model.DepartmentName = "";
                    model.IsDelete = false;

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

            return RedirectToAction("Department", new { id = (int?)null });
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
                SqlCommand cmd2 = new SqlCommand("SELECT ID,Category,IsDelete FROM T_Category", con);

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
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

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

                SqlCommand cmd2 = new SqlCommand("SELECT ID,PaymentMode,IsDelete FROM T_PaymentMode", con);

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
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

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

        [HttpGet]
        public IActionResult User(int? id)
        {
            User model = new User();

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

                SqlCommand cmd2 = new SqlCommand("SELECT * FROM T_User u left join T_Role r on r.RoleID = u.RoleId left join T_Department d on d.DepID = u.DepID", con);
                SqlDataReader dr2 = cmd2.ExecuteReader();

                while (dr2.Read())
                {
                    model.UserList.Add(new User
                    {
                        UserID = Convert.ToInt32(dr2["UserID"]),
                        Name = dr2["Name"].ToString(),
                        Email = dr2["Email"].ToString(),
                        MobileNo = dr2["MobileNo"].ToString(),
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
            if (!ModelState.IsValid)
            {
                ViewBag.DepartmentList = GetDepartmentList();
                ViewBag.RoleList = GetRoleList();
                return View(model);
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                if (model.UserID == 0)
                {
                    SqlCommand cmd = new SqlCommand(
                    "INSERT INTO T_User(Name,Email,MobileNo,DepID,Password,RoleId,IsActive) VALUES(@Name,@Email,@MobileNo,@DepID,@Password,@RoleId,@IsActive)", con);

                    cmd.Parameters.AddWithValue("@Name", model.Name.Trim());
                    cmd.Parameters.AddWithValue("@Email", model.Email.Trim());
                    cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo.Trim());
                    cmd.Parameters.AddWithValue("@Password", model.Password.Trim());
                    cmd.Parameters.AddWithValue("@DepID", (object?)model.DepID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoleId", (object?)model.RoleId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                    cmd.ExecuteNonQuery();

                    TempData["SweetAlertMessage"] = "User Saved Successfully";
                    TempData["SweetAlertOptions"] = "success";
                }
                else
                {
                    SqlCommand cmd = new SqlCommand(
                    "UPDATE T_User SET Name=@Name,Email=@Email,MobileNo=@MobileNo,Password=@Password,DepID=@DepID ,RoleId=@RoleId,IsActive=@IsActive WHERE UserID=@UserID", con);

                    cmd.Parameters.AddWithValue("@Name", model.Name.Trim());
                    cmd.Parameters.AddWithValue("@Email", model.Email.Trim());
                    cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo.Trim());
                    cmd.Parameters.AddWithValue("@Password", model.Password.Trim());
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
    }
}