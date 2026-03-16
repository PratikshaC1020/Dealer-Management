using BespokeSoftware.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BespokeSoftware.Controllers
{
    public class LoginController : Controller
    {
        private readonly string _connectionString;

        public LoginController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        // Login POST
        [HttpPost]
        public IActionResult Login(Login model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string query = @"SELECT u.UserID, u.Name, r.RoleName 
                                     FROM T_User u left join T_Role r on u.RoleId = r.RoleID
                                     WHERE Name=@UserName 
                                     AND Password=@Password 
                                     AND IsActive=1";

                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {
                        SqlCommand cmd = new SqlCommand(query, con);

                        cmd.Parameters.AddWithValue("@UserName", model.UserName.Trim());
                        cmd.Parameters.AddWithValue("@Password", model.Password.Trim());

                        con.Open();

                        SqlDataReader dr = cmd.ExecuteReader();

                        if (dr.Read())
                        {
                            HttpContext.Session.SetString("UserID", dr["UserID"].ToString());
                            HttpContext.Session.SetString("UserName", dr["Name"].ToString());
                            HttpContext.Session.SetString("Role", dr["RoleName"].ToString());

                           // return RedirectToAction("AdminDashboard", "Admin");
                            return RedirectToAction("Department", "MasterData");
                        }
                        else
                        {
                            TempData["SweetAlertMessage"] = "Invalid UserID or Password";
                            TempData["SweetAlertOptions"] = "error";
                            //ViewBag.Error = "Invalid UserID or Password";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["SweetAlertMessage"] = ex.Message;
                TempData["SweetAlertOptions"] = "error";
                ViewBag.Error = ex.Message;
            }

            return View(model);
        }

        // Logout
        //public IActionResult Logout()
        //{
        //    HttpContext.Session.Clear();
        //    return RedirectToAction("Index");
        //}
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

