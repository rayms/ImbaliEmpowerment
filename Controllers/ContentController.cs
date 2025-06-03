using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Imbali.Models;
using System.IO;
using System.Collections.Generic;

public class ContentController : Controller
{
    private static readonly string JsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "infoboxes.json");

    public IActionResult Page(int id)
    {
        // Load JSON
        List<InfoBox> infoBoxes = new List<InfoBox>();
        if (System.IO.File.Exists(JsonFilePath))
        {
            var json = System.IO.File.ReadAllText(JsonFilePath);
            infoBoxes = JsonConvert.DeserializeObject<List<InfoBox>>(json);
        }

        if (id < 0 || id >= infoBoxes.Count)
        {
            return NotFound(); // If id is invalid
        }

        var selectedBox = infoBoxes[id];
        return View(selectedBox); // Return to Page.cshtml
    }
}
