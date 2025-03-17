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

        // Виправлено конструктор: передаємо UserManager як параметр
        public MerchandisesController(MerchShopeContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager; // Присвоюємо параметр полю
        }

        // Перегляд замовлень користувача
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

            // Передаємо статуси у ViewBag для адміністратора
            if (User.IsInRole("Admin"))
            {
                ViewBag.OrderStatuses = await _context.OrderStatuses.ToListAsync();
            }

            return View(orders);
        }

        // Перегляд кошика користувача
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

        // Список товарів для всіх
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

        // Деталі товару
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
                // Якщо валідація не пройшла, повторно завантажуємо MerchandiseName для відображення
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
                ReviewDate = DateTime.Now // Змінено з DateTime.UtcNow на DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = merchandiseId });
        }
        // Решта методів залишаються без змін
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
                    Price = ci.Merchandise.Price, // Використовуємо актуальну ціну з товару
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
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View("~/Views/Admin/Orders.cshtml", orders);
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