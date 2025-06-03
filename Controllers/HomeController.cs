using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Imbali.Models;
using System.IO;
using System.Linq;
using Newtonsoft.Json;



namespace Imbali.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // ?? Load FlipBox images dynamically from folders
            ViewBag.FlipBox1Images = LoadImagesFromFolder("FlipBox1");
            ViewBag.FlipBox2Images = LoadImagesFromFolder("FlipBox2");
            ViewBag.FlipBox3Images = LoadImagesFromFolder("FlipBox3");

            // ?? Load Info Boxes from JSON as strongly-typed List<InfoBox>
            var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "infoboxes.json");
            List<InfoBox> infoBoxes = new List<InfoBox>();

            if (System.IO.File.Exists(jsonFilePath))
            {
                var json = System.IO.File.ReadAllText(jsonFilePath);
                infoBoxes = JsonConvert.DeserializeObject<List<InfoBox>>(json) ?? new List<InfoBox>();
            }

            ViewBag.InfoBoxes = infoBoxes;

            return View(infoBoxes);
        }

        // ?? Helper to load images
        private List<string> LoadImagesFromFolder(string folderName)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folderName);

            if (!Directory.Exists(folderPath))
            {
                return new List<string>();
            }

            return Directory.GetFiles(folderPath)
                            .OrderBy(f => f)
                            .Select(f => $"/images/{folderName}/{Path.GetFileName(f)}")
                            .ToList();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
