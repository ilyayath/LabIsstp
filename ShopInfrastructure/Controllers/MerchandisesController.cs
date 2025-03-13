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

        public async Task<IActionResult> Index(string searchString = "", int? brandId = null, int? categoryId = null, int? sizeId = null, int? teamId = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            ViewBag.Brands = await _context.Brands.ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Sizes = await _context.Sizes.ToListAsync();
            ViewBag.Teams = await _context.Teams.ToListAsync();

            var merchandises = _context.Merchandises
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .Include(m => m.Size)
                .Include(m => m.Team)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                merchandises = merchandises.Where(m => m.Name.Contains(searchString));
            }
            if (brandId.HasValue) merchandises = merchandises.Where(m => m.BrandId == brandId.Value);
            if (categoryId.HasValue) merchandises = merchandises.Where(m => m.CategoryId == categoryId.Value);
            if (sizeId.HasValue) merchandises = merchandises.Where(m => m.SizeId == sizeId.Value);
            if (teamId.HasValue) merchandises = merchandises.Where(m => m.TeamId == teamId.Value);
            if (minPrice.HasValue) merchandises = merchandises.Where(m => m.Price >= minPrice.Value);
            if (maxPrice.HasValue) merchandises = merchandises.Where(m => m.Price <= maxPrice.Value);

            var result = await merchandises.ToListAsync();
            return View(result);
        }

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
                UserId = userId, // Використовуємо UserId
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
    }
}