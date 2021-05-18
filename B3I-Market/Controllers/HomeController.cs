using B3I_Market.Helpers;
using Microsoft.AspNetCore.Mvc;
using BLL;

namespace B3I_Market.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetItems(int id)
        {
            return PartialView(CityLogic.GetListOfCitiesById(id));
        }

        public IActionResult SetCity(string state,string city)
        {
            HttpContext.Session.SetOrUpdate<string>("CityUser", city);
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}