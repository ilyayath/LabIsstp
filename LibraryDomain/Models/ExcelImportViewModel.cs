using Microsoft.AspNetCore.Http; // Додаємо цей простір імен
using System.ComponentModel.DataAnnotations;

namespace ShopDomain.Models
{
    public class ExcelImportViewModel
    {
        [Required(ErrorMessage = "Виберіть таблицю для імпорту")]
        public string TableName { get; set; }

        [Required(ErrorMessage = "Виберіть файл Excel")]
        public IFormFile ExcelFile { get; set; }
    }
}