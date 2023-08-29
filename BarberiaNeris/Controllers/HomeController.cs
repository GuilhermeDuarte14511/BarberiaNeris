using Microsoft.AspNetCore.Mvc;

namespace BarberiaNeris.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
