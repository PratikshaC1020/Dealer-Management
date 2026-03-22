using BespokeSoftware.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

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
        public async Task<IActionResult> Login(Login model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string query = @"SELECT u.UserID, u.Name, r.RoleName 
                                 FROM T_User u 
                                 LEFT JOIN T_Role r ON u.RoleId = r.RoleID
                                 WHERE Name=@UserName AND Password=@Password AND IsActive=1";

                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {
                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@UserName", model.UserName.Trim());
                        cmd.Parameters.AddWithValue("@Password", model.Password.Trim());

                        con.Open();
                        SqlDataReader dr = cmd.ExecuteReader();

                        if (dr.Read())
                        {
                            HttpContext.Session.SetString("UserName", dr["Name"].ToString());
                            HttpContext.Session.SetString("UserId", dr["UserID"].ToString());
                            // CREATE CLAIMS
                            var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, dr["UserID"].ToString()),
                            new Claim(ClaimTypes.Name, dr["Name"].ToString()),
                            new Claim(ClaimTypes.Role, dr["RoleName"].ToString())
                        };

                            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

                            await HttpContext.SignInAsync("MyCookieAuth",
                                new ClaimsPrincipal(claimsIdentity),
                                new AuthenticationProperties
                                {
                                    IsPersistent = true
                                });

                            // Redirect to protected page
                            return RedirectToAction("Index", "Dealer");
                        }
                        else
                        {
                            TempData["SweetAlertMessage"] = "Invalid UserID or Password";
                            TempData["SweetAlertOptions"] = "error";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["SweetAlertMessage"] = ex.Message;
                TempData["SweetAlertOptions"] = "error";
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Login", "Login");
        }
        // Logout
        //public IActionResult Logout()
        //{
        //    HttpContext.Session.Clear();
        //    return RedirectToAction("Index");
        //}
        //public IActionResult Logout()
        //{
        //    HttpContext.Session.Clear();
        //    return RedirectToAction("Login");

        //}
    }
}

