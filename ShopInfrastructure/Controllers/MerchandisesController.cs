using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopDomain.Models;
using ShopInfrastructure;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ShopMVC.ShopInfrastructure;

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
        [Authorize]
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
        [Authorize]
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

        // Список товарів із фільтрами
        public async Task<IActionResult> Index(string searchString = "", int[] brandIds = null, int[] categoryIds = null, int[] sizeIds = null, int[] teamIds = null, decimal? minPrice = null, decimal? maxPrice = null, bool clearFilters = false)
        {
            if (clearFilters)
            {
                return RedirectToAction("Index");
            }

            // Зберігаємо параметри фільтра у ViewBag
            ViewBag.SearchString = searchString;
            ViewBag.BrandIds = brandIds ?? Array.Empty<int>();
            ViewBag.CategoryIds = categoryIds ?? Array.Empty<int>();
            ViewBag.SizeIds = sizeIds ?? Array.Empty<int>();
            ViewBag.TeamIds = teamIds ?? Array.Empty<int>();
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            // Завантажуємо списки для фільтрів
            ViewBag.Brands = await _context.Brands.OrderBy(b => b.BrandName).ToListAsync();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Sizes = await _context.Sizes.OrderBy(s => s.SizeName).ToListAsync();
            ViewBag.Teams = await _context.Teams.OrderBy(t => t.TeamName).ToListAsync();

            // Базовий запит
            var merchandises = _context.Merchandises
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .Include(m => m.Size)
                .Include(m => m.Team)
                .AsQueryable();

            // Фільтри
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

        // Перегляд кошика перед оформленням замовлення
        [Authorize]
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

        // Створення замовлення з кошика
        [Authorize]
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
                StatusId = 1, // Початковий статус замовлення
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
    }
}