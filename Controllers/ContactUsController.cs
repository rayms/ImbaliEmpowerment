using Microsoft.AspNetCore.Mvc;

namespace MyWebProject.Controllers
{
    public class ContactUsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
