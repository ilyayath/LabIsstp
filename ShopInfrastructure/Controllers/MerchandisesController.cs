using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopDomain.Models;
using ShopMVC.ShopInfrastructure;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShopInfrastructure.Controllers
{
    public class MerchandisesController : Controller
    {
        private readonly MerchShopeContext _context;
        private readonly UserManager<AppUser> _userManager;


        public MerchandisesController(MerchShopeContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager; 
        }


        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Orders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.MerchOrders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Merch)
                .Include(o => o.Status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();


            if (User.IsInRole("Admin"))
            {
                ViewBag.OrderStatuses = await _context.OrderStatuses.ToListAsync();
            }

            return View(orders);
        }


        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Cart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.UserCarts
                .Where(c => c.UserId == userId)
                .Include(c => c.Merchandise)
                .ToListAsync();

            return View(cartItems);
        }


        public async Task<IActionResult> Index(string searchString = "", int[] brandIds = null, int[] categoryIds = null, int[] sizeIds = null, int[] teamIds = null, decimal? minPrice = null, decimal? maxPrice = null, bool clearFilters = false)
        {
            if (clearFilters)
            {
                return RedirectToAction("Index");
            }

            ViewBag.SearchString = searchString;
            ViewBag.BrandIds = brandIds != null ? brandIds.ToList() : new List<int>();
            ViewBag.CategoryIds = categoryIds != null ? categoryIds.ToList() : new List<int>();
            ViewBag.SizeIds = sizeIds != null ? sizeIds.ToList() : new List<int>();
            ViewBag.TeamIds = teamIds != null ? teamIds.ToList() : new List<int>();
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            ViewBag.Brands = await _context.Brands.OrderBy(b => b.BrandName).ToListAsync();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Sizes = await _context.Sizes.OrderBy(s => s.SizeName).ToListAsync();
            ViewBag.Teams = await _context.Teams.OrderBy(t => t.TeamName).ToListAsync();

            var merchandises = _context.Merchandises
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .Include(m => m.Size)
                .Include(m => m.Team)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                merchandises = merchandises.Where(m => m.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }
            if (brandIds?.Length > 0)
            {
                merchandises = merchandises.Where(m => brandIds.Contains(m.BrandId.Value));
            }
            if (categoryIds?.Length > 0)
            {
                merchandises = merchandises.Where(m => categoryIds.Contains(m.CategoryId.Value));
            }
            if (sizeIds?.Length > 0)
            {
                merchandises = merchandises.Where(m => sizeIds.Contains(m.SizeId.Value));
            }
            if (teamIds?.Length > 0)
            {
                merchandises = merchandises.Where(m => teamIds.Contains(m.TeamId.Value));
            }
            if (minPrice.HasValue && minPrice >= 0)
            {
                merchandises = merchandises.Where(m => m.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue && maxPrice >= 0)
            {
                merchandises = merchandises.Where(m => m.Price <= maxPrice.Value);
            }

            var result = await merchandises.OrderBy(m => m.Name).ToListAsync();
            return View(result);
        }

        [Authorize(Roles = "Admin")] 
        public IActionResult Charts()
        {
            return View();
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merchandise = await _context.Merchandises
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .Include(m => m.Size)
                .Include(m => m.Team)
                .Include(m => m.Reviews)
                .ThenInclude(r => r.Buyer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (merchandise == null)
            {
                return NotFound();
            }

            return View(merchandise);
        }

        // Додавання відгуку (GET)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AddReview(int merchandiseId)
        {
            var merchandise = await _context.Merchandises.FindAsync(merchandiseId);
            if (merchandise == null)
            {
                return NotFound();
            }

            var model = new ReviewViewModel
            {
                MerchandiseId = merchandiseId,
                MerchandiseName = merchandise.Name
            };
            return View(model);
        }

        // Додавання відгуку (POST)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(ReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                
                var merchandise = await _context.Merchandises.FindAsync(model.MerchandiseId);
                if (merchandise != null)
                {
                    model.MerchandiseName = merchandise.Name;
                }
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var buyer = await _context.Buyers
                .FirstOrDefaultAsync(b => b.UserId == user.Id);
            if (buyer == null)
            {
                buyer = new Buyer { UserId = user.Id, Username = user.UserName };
                _context.Buyers.Add(buyer);
                await _context.SaveChangesAsync();
            }

            var review = new Review
            {
                BuyerId = buyer.Id,
                MerchandiseId = model.MerchandiseId,
                Rating = model.Rating,
                Comment = model.Comment,
                ReviewDate = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Відгук успішно додано!";
            return RedirectToAction("Details", new { id = model.MerchandiseId });
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReviewFromDetails(int merchandiseId, int? rating, string comment)
        {
            if (!ModelState.IsValid || rating < 1 || rating > 5 || string.IsNullOrEmpty(comment))
            {
                var merchandise = await _context.Merchandises
                    .Include(m => m.Brand)
                    .Include(m => m.Category)
                    .Include(m => m.Size)
                    .Include(m => m.Team)
                    .Include(m => m.Reviews)
                    .ThenInclude(r => r.Buyer)
                    .FirstOrDefaultAsync(m => m.Id == merchandiseId);

                if (merchandise == null)
                {
                    return NotFound();
                }

                TempData["ErrorMessage"] = "Оцінка має бути від 1 до 5, а коментар не може бути порожнім.";
                return View("Details", merchandise);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var buyer = await _context.Buyers
                .FirstOrDefaultAsync(b => b.UserId == user.Id);
            if (buyer == null)
            {
                buyer = new Buyer { UserId = user.Id, Username = user.UserName };
                _context.Buyers.Add(buyer);
                await _context.SaveChangesAsync();
            }

            var review = new Review
            {
                BuyerId = buyer.Id,
                MerchandiseId = merchandiseId,
                Rating = rating,
                Comment = comment,
                ReviewDate = DateTime.Now 
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = merchandiseId });
        }
      
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.UserCarts
                .Where(c => c.UserId == userId)
                .Include(c => c.Merchandise)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Кошик порожній.";
                return RedirectToAction("Cart");
            }

            return View(cartItems);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = await _context.Brands.OrderBy(b => b.BrandName).ToListAsync();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Sizes = await _context.Sizes.OrderBy(s => s.SizeName).ToListAsync();
            ViewBag.Teams = await _context.Teams.OrderBy(t => t.TeamName).ToListAsync();

            return View(new Merchandise());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Price,ImageUrl,BrandId,CategoryId,SizeId,TeamId,Description")] Merchandise merchandise)
        {
            if (ModelState.IsValid)
            {
                _context.Add(merchandise);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(AdminMerchandises));
            }

            ViewBag.Brands = await _context.Brands.OrderBy(b => b.BrandName).ToListAsync();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Sizes = await _context.Sizes.OrderBy(s => s.SizeName).ToListAsync();
            ViewBag.Teams = await _context.Teams.OrderBy(t => t.TeamName).ToListAsync();

            return View(merchandise);
        }

        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CreateOrderFromCart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.UserCarts
                .Where(c => c.UserId == userId)
                .Include(c => c.Merchandise)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Кошик порожній.";
                return RedirectToAction("Cart");
            }

            var order = new MerchOrder
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                StatusId = 1,
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    MerchId = ci.MerchandiseId,
                    Quantity = ci.Quantity,
                    Price = ci.Merchandise.Price, 
                    Merch = ci.Merchandise
                }).ToList()
            };

            _context.MerchOrders.Add(order);
            _context.UserCarts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Замовлення успішно створено!";
            return RedirectToAction("Orders");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merchandise = await _context.Merchandises
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .Include(m => m.Size)
                .Include(m => m.Team)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (merchandise == null)
            {
                return NotFound();
            }

            ViewBag.Brands = await _context.Brands.OrderBy(b => b.BrandName).ToListAsync();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Sizes = await _context.Sizes.OrderBy(s => s.SizeName).ToListAsync();
            ViewBag.Teams = await _context.Teams.OrderBy(t => t.TeamName).ToListAsync();

            return View(merchandise);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,ImageUrl,BrandId,CategoryId,SizeId,TeamId,Description")] Merchandise updatedMerchandise)
        {
            if (id != updatedMerchandise.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var merchandise = await _context.Merchandises.FindAsync(id);
                    if (merchandise == null)
                    {
                        return NotFound();
                    }

                    merchandise.Name = updatedMerchandise.Name;
                    merchandise.Price = updatedMerchandise.Price;
                    merchandise.ImageUrl = updatedMerchandise.ImageUrl;
                    merchandise.BrandId = updatedMerchandise.BrandId;
                    merchandise.CategoryId = updatedMerchandise.CategoryId;
                    merchandise.SizeId = updatedMerchandise.SizeId;
                    merchandise.TeamId = updatedMerchandise.TeamId;

                    _context.Update(merchandise);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MerchandiseExists(updatedMerchandise.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(AdminMerchandises));
            }

            ViewBag.Brands = await _context.Brands.OrderBy(b => b.BrandName).ToListAsync();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Sizes = await _context.Sizes.OrderBy(s => s.SizeName).ToListAsync();
            ViewBag.Teams = await _context.Teams.OrderBy(t => t.TeamName).ToListAsync();

            return View(updatedMerchandise);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, int statusId)
        {
            var order = await _context.MerchOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.StatusId = statusId;
            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Статус замовлення #{id} оновлено.";
            return RedirectToAction("Orders");
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, int statusId)
        {
            var order = await _context.MerchOrders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Merch)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var newStatus = await _context.OrderStatuses.FindAsync(statusId);
            if (newStatus == null)
            {
                return BadRequest("Невірний статус.");
            }

            order.StatusId = statusId;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Статус замовлення оновлено.";
            return RedirectToAction(nameof(Orders));
        }

        [Authorize(Roles = "Admin")]
        [Route("/Admin/Orders")]
        public async Task<IActionResult> AdminOrders()
        {
            var orders = await _context.MerchOrders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Merch)
                .Include(o => o.Status)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.OrderStatuses = await _context.OrderStatuses.ToListAsync(); // Передаємо статуси
            return View("~/Views/Admin/Orders.cshtml", orders);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ExportOrdersToExcel()
        {
            var orders = await _context.MerchOrders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Merch)
                .Include(o => o.Status)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Orders");
                var currentRow = 1;

                // Заголовки
                worksheet.Cell(currentRow, 1).Value = "ID";
                worksheet.Cell(currentRow, 2).Value = "Користувач";
                worksheet.Cell(currentRow, 3).Value = "Дата";
                worksheet.Cell(currentRow, 4).Value = "Сума";
                worksheet.Cell(currentRow, 5).Value = "Статус";
                worksheet.Cell(currentRow, 6).Value = "Товари";

                // Стилі для заголовків
                worksheet.Row(currentRow).Style.Font.Bold = true;

                // Дані
                foreach (var order in orders)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = order.Id;
                    worksheet.Cell(currentRow, 2).Value = order.User?.Email ?? "Невідомо";
                    worksheet.Cell(currentRow, 3).Value = order.OrderDate?.ToString("dd.MM.yyyy HH:mm") ?? "Невідомо";
                    worksheet.Cell(currentRow, 4).Value = order.OrderItems.Sum(oi => oi.Merch.Price * oi.Quantity);
                    worksheet.Cell(currentRow, 5).Value = order.Status?.StatusName ?? "Невідомо";
                    worksheet.Cell(currentRow, 6).Value = string.Join(", ", order.OrderItems.Select(oi => $"{oi.Merch.Name} (x{oi.Quantity})"));
                }

                // Автонастройка ширини колонок
                worksheet.Columns().AdjustToContents();

                // Збереження файлу в пам’ять
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OrdersReport.xlsx");
                }
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult ImportMerchandises()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ImportMerchandises(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                ModelState.AddModelError("", "Будь ласка, виберіть файл Excel.");
                return View();
            }

            if (!excelFile.FileName.EndsWith(".xlsx"))
            {
                ModelState.AddModelError("", "Файл має бути у форматі .xlsx.");
                return View();
            }

            int addedCount = 0;
            int skippedCount = 0;
            var errors = new List<string>();

            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rowCount = worksheet.RowsUsed().Count();

                    for (int row = 2; row <= rowCount; row++) 
                    {
                        try
                        {
                            var name = worksheet.Cell(row, 1).GetString();
                            var brandId = worksheet.Cell(row, 4).GetValue<int>();
                            var categoryId = worksheet.Cell(row, 5).GetValue<int>();
                            var sizeId = worksheet.Cell(row, 6).GetValue<int>();
                            var teamId = worksheet.Cell(row, 7).GetValue<int>();

                            
                            if (!await _context.Brands.AnyAsync(b => b.Id == brandId))
                            {
                                errors.Add($"Рядок {row}: BrandId {brandId} не існує.");
                                skippedCount++;
                                continue;
                            }
                            if (!await _context.Categories.AnyAsync(c => c.Id == categoryId))
                            {
                                errors.Add($"Рядок {row}: CategoryId {categoryId} не існує.");
                                skippedCount++;
                                continue;
                            }
                            if (!await _context.Sizes.AnyAsync(s => s.Id == sizeId))
                            {
                                errors.Add($"Рядок {row}: SizeId {sizeId} не існує.");
                                skippedCount++;
                                continue;
                            }
                            if (!await _context.Teams.AnyAsync(t => t.Id == teamId))
                            {
                                errors.Add($"Рядок {row}: TeamId {teamId} не існує.");
                                skippedCount++;
                                continue;
                            }

                            
                            var existingMerchandise = await _context.Merchandises
                                .FirstOrDefaultAsync(m => m.Name == name);

                            if (existingMerchandise != null)
                            {
                                skippedCount++; 
                                continue;
                            }

                            var merchandise = new Merchandise
                            {
                                Name = name,
                                Price = worksheet.Cell(row, 2).GetValue<decimal>(),
                                ImageUrl = worksheet.Cell(row, 3).GetString(),
                                BrandId = brandId,
                                CategoryId = categoryId,
                                SizeId = sizeId,
                                TeamId = teamId,

                            };

                            _context.Merchandises.Add(merchandise);
                            addedCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Рядок {row}: Помилка обробки - {ex.Message}");
                            skippedCount++;
                        }
                    }

                    await _context.SaveChangesAsync();

                    
                    var message = $"Додано {addedCount} нових товарів. Пропущено {skippedCount} рядків.";
                    if (errors.Any())
                    {
                        message += " Помилки: " + string.Join("; ", errors);
                    }
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("AdminMerchandises");
                }
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var order = await _context.MerchOrders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Merch)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status?.StatusName != "Pending" && order.Status?.StatusName != "Processing")
            {
                TempData["Error"] = "Замовлення не можна скасувати в поточному статусі.";
                return RedirectToAction(nameof(Orders));
            }

            order.StatusId = (await _context.OrderStatuses.FirstOrDefaultAsync(s => s.StatusName == "Cancelled"))?.Id;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Замовлення успішно скасовано.";
            return RedirectToAction(nameof(Orders));
        }

        [Authorize(Roles = "Admin")]
        [Route("/Admin/Merchandises")]
        public async Task<IActionResult> AdminMerchandises()
        {
            var merchandises = await _context.Merchandises
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .Include(m => m.Size)
                .Include(m => m.Team)
                .OrderBy(m => m.Name)
                .ToListAsync();

            return View("~/Views/Admin/Merchandises.cshtml", merchandises);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merchandise = await _context.Merchandises
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .Include(m => m.Size)
                .Include(m => m.Team)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (merchandise == null)
            {
                return NotFound();
            }

            return View(merchandise);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var merchandise = await _context.Merchandises.FindAsync(id);
            if (merchandise != null)
            {
                _context.Merchandises.Remove(merchandise);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(AdminMerchandises));
        }

        private bool MerchandiseExists(int id)
        {
            return _context.Merchandises.Any(e => e.Id == id);
        }
    }
}