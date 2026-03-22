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

    public IActionResult _AddEditDealer()
    {
        DealerViewModel model = new DealerViewModel();
        model.Dealer = new Dealer();
        model.Dealer.DealerCode = repo.GetDealerCode();
        ViewBag.DepartmentList = repo.GetDepartments();
        ViewBag.CategoryList = repo.GetCategories();
        ViewBag.PaymentModeList = repo.GetPaymentModes();
        ViewBag.WeeklyOffList = repo.GetWeeklyOffDays();
        model.States = repo.GetStates();
        return View(model);
    }

    [HttpPost]
    public IActionResult InsertDealer(DealerViewModel model)
    {
        // direct available
        var noteIds = model.Dealer.NoteIds;

        // check
        if (noteIds != null)
        {
            foreach (var id in noteIds)
            {
                Console.WriteLine(id);
            }
        }
        if (model.NotesA != null)
        {
            foreach (var note in model.NotesA)
            {
                var cat = note.CategoryId;
                var text = note.NoteText;
            }
        }
        if (model != null)
        {
            repo.InsertDealerFull(model);

            return RedirectToAction("Index");
        }

        ViewBag.DepartmentList = repo.GetDepartments();
        ViewBag.CategoryList = repo.GetCategories();
        ViewBag.PaymentModeList = repo.GetPaymentModes();
        ViewBag.WeeklyOffList = repo.GetWeeklyOffDays();
        model.States = repo.GetStates();

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

    public IActionResult GetDealerAddress(int dealerId)
    {
        var addressList = repo.GetDealerAddress(dealerId);

        return PartialView("_DealerAddress", addressList);
    }
    public IActionResult GetDealerCommunication(int dealerId)
    {
        var commList = repo.GetDealerCommunication(dealerId);

        return PartialView("_DealerCommunication", commList);
    }
}