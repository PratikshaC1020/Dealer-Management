using BespokeSoftware.Models;
using BespokeSoftware.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using static BespokeSoftware.Models.Dealer;

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

    [HttpGet]
    public JsonResult GetCities(int stateId)
    {
        var cities = repo.GetCitiesByState(stateId);

        return Json(cities);
    }

   
    public IActionResult Delete(int id)
    {
        repo.DeleteDealer(id);
        return RedirectToAction("Index");
    }
}