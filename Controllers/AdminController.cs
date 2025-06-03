using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Imbali.Models;

public class AdminController : Controller
{
    private const string AdminUser = "ImbaliEmpowerment";
    private const string AdminPass = "4815162342";

    private static readonly string JsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "infoboxes.json");
    private static readonly string JsonBackupPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "infoboxes_backup.json");

    // /Admin/Login
    public IActionResult Login()
    {
        return View();
    }

    // /Admin/Login
    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        if (username == AdminUser && password == AdminPass)
        {
            HttpContext.Session.SetString("IsAdmin", "true");
            return RedirectToAction("EditInfoBoxes");
        }

        ViewBag.Error = "Invalid credentials.";
        return View();
    }

    //  /Admin/Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    //  /Admin/EditInfoBoxes
    public IActionResult EditInfoBoxes()
    {
        if (!IsAdmin()) return RedirectToAction("Login");

        List<InfoBox> infoBoxes = new List<InfoBox>();

        if (System.IO.File.Exists(JsonFilePath))
        {
            var json = System.IO.File.ReadAllText(JsonFilePath);
            infoBoxes = JsonConvert.DeserializeObject<List<InfoBox>>(json) ?? new List<InfoBox>();
        }

        return View(infoBoxes); 

    }
    [HttpPost]
    public IActionResult SaveInfoBoxes([FromBody] List<InfoBox> infoBoxes)
    {
        if (!IsAdmin()) return Unauthorized();

        Console.WriteLine("Received InfoBoxes Count: " + (infoBoxes?.Count ?? 0));

        try
        {
            
            if (System.IO.File.Exists(JsonFilePath))
            {
                System.IO.File.Copy(JsonFilePath, JsonBackupPath, true);
                Console.WriteLine("🗂️ Backup created at: " + JsonBackupPath);
            }

            // If null, treat as empty list
            var safeInfoBoxes = infoBoxes ?? new List<InfoBox>();

            var json = JsonConvert.SerializeObject(safeInfoBoxes, Formatting.Indented);
            Console.WriteLine("Saving JSON:\n" + json);

            System.IO.File.WriteAllText(JsonFilePath, json);
            Console.WriteLine("✅ JSON saved successfully.");

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error saving JSON: " + ex.Message);
            return StatusCode(500, "Error saving data.");
        }
    }


    private bool IsAdmin()
    {
        return HttpContext.Session.GetString("IsAdmin") == "true";
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile imageFile)
    {
        if (!IsAdmin()) return Unauthorized();

        if (imageFile != null && imageFile.Length > 0)
        {
            var fileName = Path.GetFileName(imageFile.FileName);
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Cover");
            var filePath = Path.Combine(folderPath, fileName);

            // Ensure directory exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            var relativePath = "/images/Cover/" + fileName;

            // Return path for saving in InfoBox
            return Json(new { imagePath = relativePath });
        }

        return BadRequest("No file uploaded.");
    }

    [HttpPost]
    public async Task<IActionResult> UploadFlipboxImage(IFormFile imageFile, string folder)
    {
        if (!IsAdmin()) return Unauthorized();

        if (imageFile == null || string.IsNullOrWhiteSpace(folder))
            return BadRequest("Missing file or folder.");

        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folder);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        var fileName = Path.GetFileName(imageFile.FileName);
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        return Json(new { path = $"/images/{folder}/{fileName}" });
    }

    [HttpPost]
    public IActionResult ClearFlipboxImages([FromBody] FolderRequest request)
    {
        if (!IsAdmin()) return Unauthorized();

        var folderName = request?.FolderName;
        if (string.IsNullOrWhiteSpace(folderName)) return BadRequest("Folder name required.");

        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folderName);
        if (!Directory.Exists(folderPath)) return NotFound("Folder not found.");

        try
        {
            foreach (var file in Directory.GetFiles(folderPath))
            {
                System.IO.File.Delete(file);
            }

            return Ok($"Cleared all images from {folderName}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, " Failed to delete images: " + ex.Message);
        }
    }

    [HttpGet]
    public IActionResult ListFlipboxImages(string folder)
    {
        if (!IsAdmin()) return Unauthorized();

        if (string.IsNullOrWhiteSpace(folder)) return BadRequest("Folder name is required.");

        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folder);
        if (!Directory.Exists(folderPath)) return Json(new List<string>());

        var urls = Directory.GetFiles(folderPath)
            .Select(f => $"/images/{folder}/{Path.GetFileName(f)}")
            .ToList();

        return Json(urls);
    }


}

public class FolderRequest
{
    public string FolderName { get; set; }
}
