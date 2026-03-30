using BespokeSoftware.Models;
using BespokeSoftware.Models.DealerModels;
using BespokeSoftware.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;
using static BespokeSoftware.Models.Dealer;

[Authorize(AuthenticationSchemes = "MyCookieAuth")]
public class DealerController : Controller
{
    private readonly DealerRepository repo;
    private readonly string _connectionString;
    public DealerController(IConfiguration configuration)
    {
        repo = new DealerRepository(configuration);
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }



    [Authorize(Roles = "Admin,Supervisor,Executive")]

    public IActionResult Index()
    {
        var dealers = repo.GetDealerList();
        return View(dealers);
    }
    public IActionResult GetDealerAddress(int dealerId)
    {
        var list = repo.GetDealerAddress(dealerId);
        return PartialView("_DealerAddress", list);
    }

    public IActionResult GetDealerCommunication(int dealerId)
    {
        var list = repo.GetDealerPersons(dealerId);
        return PartialView("_DealerPersons", list);
    }

    public IActionResult GetDealerNotes(int dealerId)
    {
        var list = repo.GetDealerNotes(dealerId);
        return PartialView("_DealerNotes", list);
    }
    public IActionResult _AddEditDealer()
    {
        DealerViewModel model = new DealerViewModel();
        model.Dealer = new Dealer();
        model.Dealer.DealerCode = repo.GetDealerCode();
        ViewBag.DepartmentList = repo.GetDepartments();
        ViewBag.CategoryList = repo.GetCategories();
        ViewBag.PaymentModeList = repo.GetPaymentModes();
        ViewBag.WeeklyOffList = repo.GetWeeklyOffDays();
        ViewBag.CommunicationIds = repo.GetPersonData();
        model.States = repo.GetStates();
        return View(model);
    }
    [HttpPost]
    public JsonResult AddCategoryView(string name)
    {
        using (SqlConnection con = new SqlConnection(_connectionString))
        {
            string query = "INSERT INTO T_Category (Category,IsDelete) OUTPUT INSERTED.Id VALUES (@name, '0')";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@name", name);

            con.Open();
            int id = (int)cmd.ExecuteScalar();

            return Json(new { id = id, name = name });
        }
    }
    [HttpPost]
    public JsonResult AddNote(DealerNotesVM model)
    {
        using (SqlConnection con = new SqlConnection(_connectionString))
        {
            string query = @"INSERT INTO T_DealerNotes (CategoryId, NoteFor, NoteDate, NoteText, DealerId)
                         VALUES (@CategoryId, @NoteFor, @NoteDate, @NoteText, @DealerId)";

            SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@CategoryId", model.CategoryId);
            cmd.Parameters.AddWithValue("@NoteFor", model.NoteFor ?? "");
            cmd.Parameters.AddWithValue("@NoteDate", model.NoteDate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@NoteText", model.NoteText ?? "");
            cmd.Parameters.AddWithValue("@DealerId", model.DealerId ?? "");

            con.Open();
            cmd.ExecuteNonQuery();

            return Json(true);
        }
    }


    [HttpPost]
    public async Task<IActionResult> SaveDealerFull([FromForm] DealerViewModel model)
    {
        try
        {
            // DEBUG
            if (model == null)
            {
                return Json(new { success = false, message = "Model is null" });
            }

            if (model.Dealer == null)
            {
                return Json(new { success = false, message = "Dealer data missing" });
            }
            model.Dealer.DealerCode = repo.GetDealerCode();
            //  CALL REPOSITORY
            var result = await repo.SaveDealerFull(model);

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    public JsonResult GetCategories()
    {
        var list = repo.GetCategories();
        return Json(list);
    }

    [HttpPost]
    public IActionResult AddCategory(string category)
    {
        repo.AddCategory(category);
        return Ok();
    }

    //[HttpPost]
    //public IActionResult InsertDealer(DealerViewModel model)
    //{
    //    // direct available
    //    var noteIds = model.Dealer.NoteIds;

    //    // check
    //    if (noteIds != null)
    //    {
    //        foreach (var id in noteIds)
    //        {
    //            Console.WriteLine(id);
    //        }
    //    }
    //    if (model.NotesA != null)
    //    {
    //        foreach (var note in model.NotesA)
    //        {
    //            var cat = note.CategoryId;
    //            var text = note.NoteText;
    //        }
    //    }
    //    model.Dealer.DealerCode = repo.GetDealerCode();
    //    if (model != null)
    //    {
    //        repo.InsertDealerFull(model);

    //        return RedirectToAction("Index");
    //    }

    //    ViewBag.DepartmentList = repo.GetDepartments();
    //    ViewBag.CategoryList = repo.GetCategories();
    //    ViewBag.PaymentModeList = repo.GetPaymentModes();
    //    ViewBag.WeeklyOffList = repo.GetWeeklyOffDays();
    //    ViewBag.CommunicationIds = repo.GetPersonData();
    //    //model.States = repo.GetStates();

    //    return View("_AddEditDealer", model);
    //}
    [Authorize(Roles = "Admin,Supervisor")]
    [HttpPost]
    public IActionResult InsertDealer(DealerViewModel model)
    {
        var files = Request.Form.Files;

        model.Dealer.DealerCode = repo.GetDealerCode();

        if (model != null)
        {
            repo.InsertDealerFull(model, files);
            return RedirectToAction("Index");
        }
        ViewBag.DepartmentList = repo.GetDepartments();
        ViewBag.CategoryList = repo.GetCategories();
        ViewBag.PaymentModeList = repo.GetPaymentModes();
        ViewBag.WeeklyOffList = repo.GetWeeklyOffDays();
        ViewBag.CommunicationIds = repo.GetPersonData();
        return View("_AddEditDealer", model);
    }


    [Authorize(Roles = "Admin,Supervisor")]
    public IActionResult EditDealer(int id)
    {
        DealerEditVM model = repo.GetDealerFullById(id);

        ViewBag.CategoryList = repo.GetCategories();
        ViewBag.PaymentModeList = repo.GetPaymentModes();
        ViewBag.WeeklyOffList = repo.GetWeeklyOffDays();

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Supervisor")]
    public IActionResult EditDealer(DealerEditVM model)
    {
        if (model == null || model.Dealer == null)
            return View(model);

        repo.UpdateDealerFull(model);

        return RedirectToAction("Index"); 
    }

    //[HttpPost]
    //public IActionResult UpdateDealer(DealerViewModel model)
    //{
    //    repo.UpdateDealerFull(model);

    //    return RedirectToAction("Index");
    //}

    [HttpGet]
    public JsonResult GetCities(int stateId)
    {
        var cities = repo.GetCitiesByState(stateId);

        return Json(cities);
    }


    //public IActionResult DeleteDealer(int id)
    //{
    //    repo.DeleteDealer(id);
    //    return RedirectToAction("Index");
    //}
    [Authorize(Roles = "Admin,Supervisor,Executive")]
    public IActionResult ViewDealerDetails(int dealerId)
    {
       // ViewBag.CategoryList = repo.GetCategories();
        var data = repo.GetDealerFullDetails(dealerId);
        var categories = repo.GetCategories();

        data.Categories = categories.Select(x => new CategoryVM
        {
            CategoryId = x.ID,     // ⚠️ property नाव check कर
            CategoryName = x.CategoryName
        }).ToList();
        return View("ViewDealerInfo", data);
    }

    //public IActionResult GetDealerAddress(int dealerId)
    //{
    //    var addressList = repo.GetDealerAddress(dealerId);

    //    return PartialView("_DealerAddress", addressList);
    //}
    //public IActionResult GetDealerCommunication(int dealerId)
    //{
    //    var commList = repo.GetDealerCommunication(dealerId);

    //    return PartialView("_DealerCommunication", commList);
    //}

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int dealerId)
    {
        if (dealerId <= 0)
        {
            return Json(new { success = false, message = "Invalid Dealer Id" });
        }

        try
        {
            repo.DeleteDealer(dealerId);

            return Json(new { success = true, message = "Dealer deleted successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    public IActionResult PrintNotes(int dealerId)
    {
        DealerFullViewModel model = new DealerFullViewModel();

        model.Notes = new List<DealerNotesVM>();

        using (SqlConnection con = new SqlConnection(_connectionString))
        {
            con.Open();

            using (SqlCommand cmd = new SqlCommand("sp_GetDealerNotes", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DealerId", dealerId);

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    model.Notes.Add(new DealerNotesVM
                    {
                        CategoryId = Convert.ToInt32(dr["CategoryId"]), // ✅ FIX
                        CategoryName = dr["Category"].ToString(),
                        NoteFor = dr["NoteFor"].ToString(),
                        NoteText = dr["NoteText"].ToString(),
                        NoteDate = Convert.ToDateTime(dr["NoteDate"])
                    });
                }
            }
        }

        return View("PrintNotes", model);
    }
}


