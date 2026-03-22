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
        public IActionResult Person()
        {
            return View(new Person
            {
                Addresses = new List<PersonAddress>(),
                Communications = new List<PersonCommunication>()
            });
        }

        [HttpPost]
        public IActionResult InsertPerson(Person model)
        {
            if (model == null)
                return View("Person");

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // ================= INSERT PERSON =================
                    int personId = 0;

                    string personSql = @"
                INSERT INTO T_Person
                (Title, FirstName, MiddleName, LastName, Gender, DOB, AnniversaryDate,
                 AadhaarNo, PANNo, PersonType, Remark)
                OUTPUT INSERTED.PersonID
                VALUES
                (@Title, @FirstName, @MiddleName, @LastName, @Gender, @DOB, @AnniversaryDate,
                 @AadhaarNo, @PANNo, @PersonType, @Remark)";

                    using (SqlCommand cmd = new SqlCommand(personSql, con, tran))
                    {
                        cmd.Parameters.AddWithValue("@Title", model.Title ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@FirstName", model.FirstName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@MiddleName", model.MiddleName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@LastName", model.LastName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Gender", model.Gender ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@DOB", model.DOB ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@AnniversaryDate", model.AnniversaryDate ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@AadhaarNo", model.AadhaarNo ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@PANNo", model.PANNo ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@PersonType", model.PersonType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Remark", model.Remark ?? (object)DBNull.Value);

                        personId = (int)cmd.ExecuteScalar();
                    }

                    // ================= INSERT COMMUNICATION =================
                    if (model.Communications != null)
                    {
                        foreach (var c in model.Communications)
                        {
                            if (!string.IsNullOrWhiteSpace(c?.CommunicationType) ||
                                !string.IsNullOrWhiteSpace(c?.Value))
                            {
                                string commSql = @"
                            INSERT INTO T_PersonCommunication
                            (PersonID, CommunicationType, Value)
                            VALUES (@PersonID, @Type, @Value)";

                                using (SqlCommand cmd = new SqlCommand(commSql, con, tran))
                                {
                                    cmd.Parameters.AddWithValue("@PersonID", personId);
                                    cmd.Parameters.AddWithValue("@Type", c.CommunicationType ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Value", c.Value ?? (object)DBNull.Value);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // ================= INSERT ADDRESS =================
                    if (model.Addresses != null)
                    {
                        foreach (var a in model.Addresses)
                        {
                            if (!string.IsNullOrWhiteSpace(a?.AddressType) ||
                                !string.IsNullOrWhiteSpace(a?.AddressLine))
                            {
                                string addrSql = @"
                            INSERT INTO T_PersonAddress
                            (PersonID, AddressType, AddressLine)
                            VALUES (@PersonID, @Type, @Address)";

                                using (SqlCommand cmd = new SqlCommand(addrSql, con, tran))
                                {
                                    cmd.Parameters.AddWithValue("@PersonID", personId);
                                    cmd.Parameters.AddWithValue("@Type", a.AddressType ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Address", a.AddressLine ?? (object)DBNull.Value);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // ================= INSERT IMAGE (BASE64 + FILE NAME) =================
                    if (!string.IsNullOrEmpty(model.ImagePath))
                    {
                        string imgSql = @"
                    INSERT INTO T_Image (Type, IdentityID, ImageBase64, FileName)
                    VALUES ('Person', @IdentityID, @Image, @FileName)";

                        using (SqlCommand cmd = new SqlCommand(imgSql, con, tran))
                        {
                            cmd.Parameters.AddWithValue("@IdentityID", personId.ToString());
                            cmd.Parameters.AddWithValue("@Image", model.ImagePath);
                            cmd.Parameters.AddWithValue("@FileName", model.FileName ?? (object)DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    // ================= COMMIT =================
                    tran.Commit();

                    TempData["SweetAlertMessage"] = "Person Save Successfully";
                    TempData["SweetAlertOptions"] = "success";
                    return RedirectToAction("Person");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    TempData["SweetAlertMessage"] = ex.Message;
                    TempData["SweetAlertOptions"] = "warning";
                    return View("Person", model);
                }
            }
        }

        [HttpGet]
        public IActionResult PersonList()
        {
            List<Person> list = new List<Person>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                string sql = @"
        SELECT p.*, i.ImageBase64
        FROM T_Person p
        LEFT JOIN T_Image i 
            ON i.IdentityID = CAST(p.PersonID AS NVARCHAR) 
            AND i.Type = 'Person'";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new Person
                        {
                            Id = Convert.ToInt32(rdr["PersonID"]),
                            Title = rdr["Title"].ToString(),
                            FirstName = rdr["FirstName"].ToString(),
                            MiddleName = rdr["MiddleName"].ToString(),
                            LastName = rdr["LastName"].ToString(),
                            Gender = rdr["Gender"].ToString(),
                            AadhaarNo = rdr["AadhaarNo"].ToString(),
                            PANNo = rdr["PANNo"].ToString(),
                            PersonType = rdr["PersonType"].ToString(),
                            ImagePath = rdr["ImageBase64"]?.ToString()
                        });
                    }
                }
            }

            return View(list);
        }
        public JsonResult GetCommunication(int id)
        {
            var list = new List<PersonCommunication>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                string sql = "SELECT * FROM T_PersonCommunication WHERE PersonID=@id";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new PersonCommunication
                            {
                                CommunicationType = rdr["CommunicationType"].ToString(),
                                Value = rdr["Value"].ToString()
                            });
                        }
                    }
                }
            }

            return Json(list);
        }
        public JsonResult GetAddress(int id)
        {
            var list = new List<PersonAddress>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                string sql = "SELECT * FROM T_PersonAddress WHERE PersonID=@id";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new PersonAddress
                            {
                                AddressType = rdr["AddressType"].ToString(),
                                AddressLine = rdr["AddressLine"].ToString()
                            });
                        }
                    }
                }
            }

            return Json(list);
        }

        [HttpGet]
        public IActionResult EditPerson(int id)
        {
            Person model = new Person();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                // PERSON
                string sql = @"SELECT * FROM T_Person WHERE PersonID=@id";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            model.Id = id;
                            model.Title = rdr["Title"].ToString();
                            model.FirstName = rdr["FirstName"].ToString();
                            model.MiddleName = rdr["MiddleName"].ToString();
                            model.LastName = rdr["LastName"].ToString();
                            model.Gender = rdr["Gender"].ToString();
                            model.AadhaarNo = rdr["AadhaarNo"].ToString();
                            model.PANNo = rdr["PANNo"].ToString();
                            model.PersonType = rdr["PersonType"].ToString();
                            model.Remark = rdr["Remark"].ToString();
                        }
                    }
                }

                // IMAGE
                string imgSql = @"SELECT TOP 1 * FROM T_Image 
                          WHERE Type='Person' AND IdentityID=@id";

                using (SqlCommand cmd = new SqlCommand(imgSql, con))
                {
                    cmd.Parameters.AddWithValue("@id", id.ToString());

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            model.ImagePath = rdr["ImageBase64"]?.ToString();
                            model.FileName = rdr["FileName"]?.ToString();
                        }
                    }
                }

                // COMMUNICATION
                model.Communications = new List<PersonCommunication>();
                string commSql = "SELECT * FROM T_PersonCommunication WHERE PersonID=@id";

                using (SqlCommand cmd = new SqlCommand(commSql, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            model.Communications.Add(new PersonCommunication
                            {
                                CommunicationType = rdr["CommunicationType"].ToString(),
                                Value = rdr["Value"].ToString()
                            });
                        }
                    }
                }

                // ADDRESS
                model.Addresses = new List<PersonAddress>();
                string addrSql = "SELECT * FROM T_PersonAddress WHERE PersonID=@id";

                using (SqlCommand cmd = new SqlCommand(addrSql, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            model.Addresses.Add(new PersonAddress
                            {
                                AddressType = rdr["AddressType"].ToString(),
                                AddressLine = rdr["AddressLine"].ToString()
                            });
                        }
                    }
                }
            }

            return View("EditPerson", model); // same view reuse
        }

        [HttpPost]
        public IActionResult UpdatePerson(Person model)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // ================= PERSON UPDATE =================
                    string updatePerson = @"UPDATE T_Person SET 
                                    Title=@Title,
                                    FirstName=@FirstName,
                                    MiddleName=@MiddleName,
                                    LastName=@LastName,
                                    Gender=@Gender,
                                    AadhaarNo=@AadhaarNo,
                                    PANNo=@PANNo,
                                    PersonType=@PersonType,
                                    Remark=@Remark
                                    WHERE PersonID=@Id";

                    using (SqlCommand cmd = new SqlCommand(updatePerson, con, tran))
                    {
                        cmd.Parameters.AddWithValue("@Title", model.Title ?? "");
                        cmd.Parameters.AddWithValue("@FirstName", model.FirstName ?? "");
                        cmd.Parameters.AddWithValue("@MiddleName", model.MiddleName ?? "");
                        cmd.Parameters.AddWithValue("@LastName", model.LastName ?? "");
                        cmd.Parameters.AddWithValue("@Gender", model.Gender ?? "");
                        cmd.Parameters.AddWithValue("@AadhaarNo", model.AadhaarNo ?? "");
                        cmd.Parameters.AddWithValue("@PANNo", model.PANNo ?? "");
                        cmd.Parameters.AddWithValue("@PersonType", model.PersonType ?? "");
                        cmd.Parameters.AddWithValue("@Remark", model.Remark ?? "");
                        cmd.Parameters.AddWithValue("@Id", model.Id);

                        cmd.ExecuteNonQuery();
                    }

                    // ================= IMAGE LOGIC =================
                    string isDeleted = Request.Form["IsImageDeleted"];

                    if (isDeleted == "true")
                    {
                        string delImg = @"DELETE FROM T_Image 
                                  WHERE Type='Person' AND IdentityID=@id";

                        using (SqlCommand cmd = new SqlCommand(delImg, con, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", model.Id.ToString());
                            cmd.ExecuteNonQuery();
                        }
                    }

                    if (!string.IsNullOrEmpty(model.ImagePath))
                    {
                        string check = @"SELECT COUNT(*) FROM T_Image 
                                 WHERE Type='Person' AND IdentityID=@id";

                        SqlCommand cmdCheck = new SqlCommand(check, con, tran);
                        cmdCheck.Parameters.AddWithValue("@id", model.Id.ToString());

                        int count = (int)cmdCheck.ExecuteScalar();

                        if (count > 0)
                        {
                            string updateImg = @"UPDATE T_Image 
                                        SET ImageBase64=@img, FileName=@name
                                        WHERE Type='Person' AND IdentityID=@id";

                            SqlCommand cmd = new SqlCommand(updateImg, con, tran);
                            cmd.Parameters.AddWithValue("@img", model.ImagePath);
                            cmd.Parameters.AddWithValue("@name", model.FileName ?? "");
                            cmd.Parameters.AddWithValue("@id", model.Id.ToString());
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            string insertImg = @"INSERT INTO T_Image(Type, IdentityID, ImageBase64, FileName)
                                         VALUES('Person', @id, @img, @name)";

                            SqlCommand cmd = new SqlCommand(insertImg, con, tran);
                            cmd.Parameters.AddWithValue("@id", model.Id.ToString());
                            cmd.Parameters.AddWithValue("@img", model.ImagePath);
                            cmd.Parameters.AddWithValue("@name", model.FileName ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // ================= DELETE OLD COMMUNICATION =================
                    string delComm = "DELETE FROM T_PersonCommunication WHERE PersonID=@id";

                    using (SqlCommand cmd = new SqlCommand(delComm, con, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", model.Id);
                        cmd.ExecuteNonQuery();
                    }

                    // ================= INSERT NEW COMMUNICATION =================
                    if (model.Communications != null)
                    {
                        foreach (var item in model.Communications)
                        {
                            if (!string.IsNullOrEmpty(item.CommunicationType) ||
                                !string.IsNullOrEmpty(item.Value))
                            {
                                string ins = @"INSERT INTO T_PersonCommunication
                                       (PersonID, CommunicationType, Value)
                                       VALUES (@id, @type, @val)";

                                SqlCommand cmd = new SqlCommand(ins, con, tran);
                                cmd.Parameters.AddWithValue("@id", model.Id);
                                cmd.Parameters.AddWithValue("@type", item.CommunicationType ?? "");
                                cmd.Parameters.AddWithValue("@val", item.Value ?? "");
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    // ================= DELETE OLD ADDRESS =================
                    string delAddr = "DELETE FROM T_PersonAddress WHERE PersonID=@id";

                    using (SqlCommand cmd = new SqlCommand(delAddr, con, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", model.Id);
                        cmd.ExecuteNonQuery();
                    }

                    // ================= INSERT NEW ADDRESS =================
                    if (model.Addresses != null)
                    {
                        foreach (var item in model.Addresses)
                        {
                            if (!string.IsNullOrEmpty(item.AddressType) ||
                                !string.IsNullOrEmpty(item.AddressLine))
                            {
                                string ins = @"INSERT INTO T_PersonAddress
                                       (PersonID, AddressType, AddressLine)
                                       VALUES (@id, @type, @line)";

                                SqlCommand cmd = new SqlCommand(ins, con, tran);
                                cmd.Parameters.AddWithValue("@id", model.Id);
                                cmd.Parameters.AddWithValue("@type", item.AddressType ?? "");
                                cmd.Parameters.AddWithValue("@line", item.AddressLine ?? "");
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    // ================= COMMIT =================
                    tran.Commit();

                    TempData["SweetAlertMessage"] = "Person Updated Successfully";
                    TempData["SweetAlertType"] = "success";

                    return RedirectToAction("PersonList");
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    TempData["SweetAlertMessage"] = ex.Message;
                    TempData["SweetAlertType"] = "error";

                    return View("EditPerson", model);
                }
            }
        }
        public IActionResult DeletePerson(int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // 🔥 DELETE COMMUNICATION
                    string delComm = "DELETE FROM T_PersonCommunication WHERE PersonID=@id";
                    using (SqlCommand cmd = new SqlCommand(delComm, con, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }

                    // 🔥 DELETE ADDRESS
                    string delAddr = "DELETE FROM T_PersonAddress WHERE PersonID=@id";
                    using (SqlCommand cmd = new SqlCommand(delAddr, con, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }

                    // 🔥 DELETE IMAGE
                    string delImg = "DELETE FROM T_Image WHERE Type='Person' AND IdentityID=@id";
                    using (SqlCommand cmd = new SqlCommand(delImg, con, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", id.ToString());
                        cmd.ExecuteNonQuery();
                    }

                    // 🔥 DELETE PERSON
                    string delPerson = "DELETE FROM T_Person WHERE PersonID=@id";
                    using (SqlCommand cmd = new SqlCommand(delPerson, con, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }

                    tran.Commit();

                    TempData["SweetAlertMessage"] = "Person Deleted Successfully";
                    TempData["SweetAlertType"] = "success";
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    TempData["SweetAlertMessage"] = ex.Message;
                    TempData["SweetAlertType"] = "error";
                }
            }

            return RedirectToAction("PersonList");
        }
    }
}