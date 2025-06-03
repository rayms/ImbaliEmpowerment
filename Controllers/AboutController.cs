using Microsoft.AspNetCore.Mvc;

namespace MyWebProject.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

