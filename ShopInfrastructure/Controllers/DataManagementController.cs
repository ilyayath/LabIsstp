using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopDomain.Models;
using ShopInfrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopInfrastructure.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DataManagementController : Controller
    {
        private readonly ExcelService _excelService;
        private static readonly List<string> SupportedTables = new List<string> { "Merchandises", "Brands", "Categories" };

        public DataManagementController(ExcelService excelService)
        {
            _excelService = excelService;
        }

        [HttpGet]
        public IActionResult Import()
        {
            var model = new ExcelImportViewModel();
            ViewBag.SupportedTables = SupportedTables;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(ExcelImportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.SupportedTables = SupportedTables;
                return View(model);
            }

            if (!model.ExcelFile.FileName.EndsWith(".xlsx"))
            {
                ModelState.AddModelError("", "Файл має бути у форматі .xlsx.");
                ViewBag.SupportedTables = SupportedTables;
                return View(model);
            }

            using (var stream = model.ExcelFile.OpenReadStream())
            {
                var (addedCount, skippedCount, errors) = await _excelService.ImportFromExcel(model.TableName, stream);
                var message = $"Додано {addedCount} нових записів. Пропущено {skippedCount} рядків.";
                if (errors.Any())
                {
                    message += " Помилки: " + string.Join("; ", errors);
                }
                TempData["SuccessMessage"] = message;
            }

            return RedirectToAction("Index", "Merchandises");
        }

        [HttpGet]
        public IActionResult Export()
        {
            ViewBag.SupportedTables = SupportedTables;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Export(string tableName)
        {
            if (string.IsNullOrEmpty(tableName) || !SupportedTables.Contains(tableName))
            {
                ModelState.AddModelError("", "Виберіть коректну таблицю.");
                ViewBag.SupportedTables = SupportedTables;
                return View();
            }

            var fileBytes = _excelService.ExportToExcel(tableName);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{tableName}.xlsx");
        }
    }
}