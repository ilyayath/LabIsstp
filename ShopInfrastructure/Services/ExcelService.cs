using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ShopDomain.Models;
using ShopMVC.ShopInfrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ShopInfrastructure.Services
{
    public class ExcelService
    {
        private readonly MerchShopeContext _context;

        public ExcelService(MerchShopeContext context)
        {
            _context = context;
        }

        public async Task<(int addedCount, int skippedCount, List<string> errors)> ImportFromExcel(string tableName, Stream excelStream)
        {
            int addedCount = 0;
            int skippedCount = 0;
            var errors = new List<string>();

            using (var workbook = new XLWorkbook(excelStream))
            {
                var worksheet = workbook.Worksheet(1);
                var rowCount = worksheet.RowsUsed().Count();

                // Перевірка формату таблиці
                if (!ValidateTableFormat(tableName, worksheet, errors))
                {
                    errors.Add($"Неправильний формат таблиці для {tableName}. Перевірте заголовки.");
                    return (addedCount, skippedCount, errors);
                }

                switch (tableName)
                {
                    case "Merchandises":
                        (addedCount, skippedCount, errors) = await ImportMerchandises(worksheet, rowCount, errors);
                        break;
                    case "Brands":
                        (addedCount, skippedCount, errors) = await ImportBrands(worksheet, rowCount, errors);
                        break;
                    case "Categories":
                        (addedCount, skippedCount, errors) = await ImportCategories(worksheet, rowCount, errors);
                        break;
                    default:
                        errors.Add("Непідтримувана таблиця.");
                        break;
                }

                if (errors.Count == 0 || addedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }
            }

            return (addedCount, skippedCount, errors);
        }

        public byte[] ExportToExcel(string tableName)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(tableName);

                switch (tableName)
                {
                    case "Merchandises":
                        ExportMerchandises(worksheet);
                        break;
                    case "Brands":
                        ExportBrands(worksheet);
                        break;
                    case "Categories":
                        ExportCategories(worksheet);
                        break;
                    default:
                        throw new ArgumentException("Непідтримувана таблиця.");
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        private bool ValidateTableFormat(string tableName, IXLWorksheet worksheet, List<string> errors)
        {
            var headers = worksheet.Row(1).CellsUsed().Select(c => c.GetString().Trim()).ToList();
            var expectedHeaders = GetExpectedHeaders(tableName);

            if (headers.Count != expectedHeaders.Count || !headers.SequenceEqual(expectedHeaders))
            {
                errors.Add($"Очікувані заголовки: {string.Join(", ", expectedHeaders)}. Отримано: {string.Join(", ", headers)}.");
                return false;
            }
            return true;
        }

        private List<string> GetExpectedHeaders(string tableName)
        {
            return tableName switch
            {
                "Merchandises" => new List<string> { "Name", "Price", "ImageUrl", "BrandId", "CategoryId", "SizeId", "TeamId" }, // Без Description
                "Brands" => new List<string> { "BrandName" },
                "Categories" => new List<string> { "CategoryName" },
                _ => new List<string>()
            };
        }

        private async Task<(int addedCount, int skippedCount, List<string> errors)> ImportMerchandises(IXLWorksheet worksheet, int rowCount, List<string> errors)
        {
            int addedCount = 0;
            int skippedCount = 0;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var name = worksheet.Cell(row, 1).GetString();
                    if (string.IsNullOrWhiteSpace(name)) { errors.Add($"Рядок {row}: Назва товару обов’язкова."); skippedCount++; continue; }

                    if (!decimal.TryParse(worksheet.Cell(row, 2).GetString(), out var price) || price < 0)
                    {
                        errors.Add($"Рядок {row}: Некоректна ціна."); skippedCount++; continue;
                    }

                    var imageUrl = worksheet.Cell(row, 3).GetString();

                    if (!int.TryParse(worksheet.Cell(row, 4).GetString(), out var brandId) || !await _context.Brands.AnyAsync(b => b.Id == brandId))
                    {
                        errors.Add($"Рядок {row}: BrandId {worksheet.Cell(row, 4).GetString()} не існує або некоректний."); skippedCount++; continue;
                    }

                    if (!int.TryParse(worksheet.Cell(row, 5).GetString(), out var categoryId) || !await _context.Categories.AnyAsync(c => c.Id == categoryId))
                    {
                        errors.Add($"Рядок {row}: CategoryId {worksheet.Cell(row, 5).GetString()} не існує або некоректний."); skippedCount++; continue;
                    }

                    if (!int.TryParse(worksheet.Cell(row, 6).GetString(), out var sizeId) || !await _context.Sizes.AnyAsync(s => s.Id == sizeId))
                    {
                        errors.Add($"Рядок {row}: SizeId {worksheet.Cell(row, 6).GetString()} не існує або некоректний."); skippedCount++; continue;
                    }

                    if (!int.TryParse(worksheet.Cell(row, 7).GetString(), out var teamId) || !await _context.Teams.AnyAsync(t => t.Id == teamId))
                    {
                        errors.Add($"Рядок {row}: TeamId {worksheet.Cell(row, 7).GetString()} не існує або некоректний."); skippedCount++; continue;
                    }

                    if (await _context.Merchandises.AnyAsync(m => m.Name == name)) { errors.Add($"Рядок {row}: Товар '{name}' уже існує."); skippedCount++; continue; }

                    _context.Merchandises.Add(new Merchandise
                    {
                        Name = name,
                        Price = price,
                        ImageUrl = imageUrl,
                        BrandId = brandId,
                        CategoryId = categoryId,
                        SizeId = sizeId,
                        TeamId = teamId
                    });
                    addedCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Рядок {row}: Помилка обробки - {ex.Message}");
                    skippedCount++;
                }
            }

            return (addedCount, skippedCount, errors);
        }

        private async Task<(int addedCount, int skippedCount, List<string> errors)> ImportBrands(IXLWorksheet worksheet, int rowCount, List<string> errors)
        {
            int addedCount = 0;
            int skippedCount = 0;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var brandName = worksheet.Cell(row, 1).GetString();
                    if (string.IsNullOrWhiteSpace(brandName)) { errors.Add($"Рядок {row}: Назва бренду обов’язкова."); skippedCount++; continue; }
                    if (await _context.Brands.AnyAsync(b => b.BrandName == brandName)) { errors.Add($"Рядок {row}: Бренд '{brandName}' уже існує."); skippedCount++; continue; }

                    _context.Brands.Add(new Brand { BrandName = brandName });
                    addedCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Рядок {row}: Помилка обробки - {ex.Message}");
                    skippedCount++;
                }
            }

            return (addedCount, skippedCount, errors);
        }

        private async Task<(int addedCount, int skippedCount, List<string> errors)> ImportCategories(IXLWorksheet worksheet, int rowCount, List<string> errors)
        {
            int addedCount = 0;
            int skippedCount = 0;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var categoryName = worksheet.Cell(row, 1).GetString();
                    if (string.IsNullOrWhiteSpace(categoryName)) { errors.Add($"Рядок {row}: Назва категорії обов’язкова."); skippedCount++; continue; }
                    if (await _context.Categories.AnyAsync(c => c.CategoryName == categoryName)) { errors.Add($"Рядок {row}: Категорія '{categoryName}' уже існує."); skippedCount++; continue; }

                    _context.Categories.Add(new Category { CategoryName = categoryName });
                    addedCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Рядок {row}: Помилка обробки - {ex.Message}");
                    skippedCount++;
                }
            }

            return (addedCount, skippedCount, errors);
        }

        private void ExportMerchandises(IXLWorksheet worksheet)
        {
            worksheet.Cell(1, 1).Value = "Name";
            worksheet.Cell(1, 2).Value = "Price";
            worksheet.Cell(1, 3).Value = "ImageUrl";
            worksheet.Cell(1, 4).Value = "BrandId";
            worksheet.Cell(1, 5).Value = "CategoryId";
            worksheet.Cell(1, 6).Value = "SizeId";
            worksheet.Cell(1, 7).Value = "TeamId";

            var merchandises = _context.Merchandises.ToList();
            for (int i = 0; i < merchandises.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = merchandises[i].Name;
                worksheet.Cell(i + 2, 2).Value = merchandises[i].Price;
                worksheet.Cell(i + 2, 3).Value = merchandises[i].ImageUrl;
                worksheet.Cell(i + 2, 4).Value = merchandises[i].BrandId;
                worksheet.Cell(i + 2, 5).Value = merchandises[i].CategoryId;
                worksheet.Cell(i + 2, 6).Value = merchandises[i].SizeId;
                worksheet.Cell(i + 2, 7).Value = merchandises[i].TeamId;
            }
        }

        private void ExportBrands(IXLWorksheet worksheet)
        {
            worksheet.Cell(1, 1).Value = "BrandName";

            var brands = _context.Brands.ToList();
            for (int i = 0; i < brands.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = brands[i].BrandName;
            }
        }

        private void ExportCategories(IXLWorksheet worksheet)
        {
            worksheet.Cell(1, 1).Value = "CategoryName";

            var categories = _context.Categories.ToList();
            for (int i = 0; i < categories.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = categories[i].CategoryName;
            }
        }
    }
}