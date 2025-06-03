using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace MyWebProject.Controllers
{
    public class DonationController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public DonationController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Donation landing page
        public IActionResult Index()
        {
            return View();
        }

        // Called after donation
        public IActionResult ThankYou(string custom_book)
        {
            // Check if user actually donated
            if (HttpContext.Session.GetString("HasDonated") != "true")
            {
                // Redirect or block access if session is missing
                return RedirectToAction("Index"); // or use return Unauthorized();
            }

            ViewBag.BookChoice = custom_book;
            return View();
        }


        // Secure download for "Becoming You"
        public IActionResult DownloadBook1()
        {
            if (!UserHasDonated())
                return Unauthorized("🔒 Donation required to access this eBook.");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "PrivateDownloads", "Becoming-You.pdf");
            if (!System.IO.File.Exists(filePath))
                return NotFound("Book not found.");

            return File(System.IO.File.ReadAllBytes(filePath), "application/pdf", "Becoming-You.pdf");
        }

        // Secure download for "A Period Pocket Guide"
        public IActionResult DownloadBook2()
        {
            if (!UserHasDonated())
                return Unauthorized(" Donation required to access this eBook.");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "PrivateDownloads", "A-Period-Pocket-Guide.pdf");
            if (!System.IO.File.Exists(filePath))
                return NotFound("Book not found.");

            return File(System.IO.File.ReadAllBytes(filePath), "application/pdf", "A-Period-Pocket-Guide.pdf");
        }

        // Shared check
        private bool UserHasDonated()
        {
            return HttpContext.Session.GetString("HasDonated") == "true";
        }
    }
}
