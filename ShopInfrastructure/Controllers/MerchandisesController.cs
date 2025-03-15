using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopDomain.Models;
using ShopInfrastructure;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ShopMVC.ShopInfrastructure;
using System.Diagnostics;
namespace ShopInfrastructure.Controllers
{
    public class MerchandisesController : Controller
    {
        private readonly MerchShopeContext _context;

        public MerchandisesController(MerchShopeContext context)
        {
            _context = context;
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
            ViewBag.BrandIds = brandIds != null ? brandIds.ToList() : new List<int>(); // Перетворюємо в список
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
                .FirstOrDefaultAsync(m => m.Id == id);

            if (merchandise == null)
            {
                return NotFound();
            }

            return View(merchandise);
        }

        // Оформлення замовлення
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
        // Нова дія для створення товару (GET)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = await _context.Brands.OrderBy(b => b.BrandName).ToListAsync();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Sizes = await _context.Sizes.OrderBy(s => s.SizeName).ToListAsync();
            ViewBag.Teams = await _context.Teams.OrderBy(t => t.TeamName).ToListAsync();

            return View(new Merchandise());
        }

        // Нова дія для створення товару (POST)
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
        // Створення замовлення
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
                    Price = ci.Price,
                    Merch = ci.Merchandise
                }).ToList()
            };

            _context.MerchOrders.Add(order);
            _context.UserCarts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Замовлення успішно створено!";
            return RedirectToAction("Orders");
        }

        // Редагування товару (Admin)
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

            // Завантажуємо списки для випадаючих меню
            ViewBag.Brands = await _context.Brands.OrderBy(b => b.BrandName).ToListAsync();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Sizes = await _context.Sizes.OrderBy(s => s.SizeName).ToListAsync();
            var teams = await _context.Teams.OrderBy(t => t.TeamName).ToListAsync();
            ViewBag.Teams = teams;

            // Дебаг: виводимо кількість команд і поточний TeamId
            Debug.WriteLine($"Teams count: {teams.Count}, Current TeamId: {merchandise.TeamId}");

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

                    // Дебаг: виводимо отриманий TeamId
                    Debug.WriteLine($"Received TeamId: {updatedMerchandise.TeamId}");

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

        // Перегляд усіх замовлень (Admin)
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

        // Перегляд усіх товарів (Admin)
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

        // Нова дія для видалення товару (POST)
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