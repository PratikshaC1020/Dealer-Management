using BespokeSoftware.Models;
using BespokeSoftware.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using static BespokeSoftware.Models.Dealer;

[Authorize(AuthenticationSchemes = "MyCookieAuth")]
public class DealerController : Controller
{
    private readonly DealerRepository repo;

    public DealerController(IConfiguration configuration)
    {
        repo = new DealerRepository(configuration);
    }

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
    public async Task<IActionResult> SaveDealerFull([FromBody] Dealer.DealerViewModel model)
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

   

    public IActionResult EditDealer(int id)
    {
        DealerViewModel model = repo.GetDealerFullById(id);

        ViewBag.DepartmentList = repo.GetDepartments();
        ViewBag.CategoryList = repo.GetCategories();
        ViewBag.PaymentModeList = repo.GetPaymentModes();
        ViewBag.WeeklyOffList = repo.GetWeeklyOffDays();

        model.States = repo.GetStates();

        return View(model);
    }

    [HttpPost]
    public IActionResult UpdateDealer(DealerViewModel model)
    {
        repo.UpdateDealerFull(model);

        return RedirectToAction("Index");
    }

    [HttpGet]
    public JsonResult GetCities(int stateId)
    {
        var cities = repo.GetCitiesByState(stateId);

        return Json(cities);
    }


    public IActionResult DeleteDealer(int id)
    {
        repo.DeleteDealer(id);
        return RedirectToAction("Index");
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
}