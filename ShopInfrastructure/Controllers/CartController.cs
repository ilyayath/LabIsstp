using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopDomain.Models;
using ShopInfrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;
using ShopMVC.ShopInfrastructure;

[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly MerchShopeContext _context;
    private readonly UserManager<AppUser> _userManager;

    public CartController(MerchShopeContext context, UserManager<AppUser> userManager)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        if (request.Quantity < 1)
        {
            return BadRequest("Кількість повинна бути більше 0.");
        }

        var merchandise = await _context.Merchandises.FindAsync(request.MerchandiseId);
        if (merchandise == null)
        {
            Console.WriteLine($"Merchandise {request.MerchandiseId} not found.");
            return NotFound("Товар не знайдено.");
        }

        if (User.Identity.IsAuthenticated)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User ID not found in claims.");
                return BadRequest("Не вдалося отримати ID користувача.");
            }

            Console.WriteLine($"Adding to cart for user: {userId}, merch: {request.MerchandiseId}");
            var cartItem = await _context.UserCarts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.MerchandiseId == request.MerchandiseId);

            if (cartItem == null)
            {
                cartItem = new UserCart
                {
                    UserId = userId,
                    MerchandiseId = request.MerchandiseId,
                    Quantity = request.Quantity,
                    Price = merchandise.Price
                };
                _context.UserCarts.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += request.Quantity;
                cartItem.Price = merchandise.Price; // Оновлюємо ціну
            }

            var changes = await _context.SaveChangesAsync();
            if (changes == 0)
            {
                Console.WriteLine("No changes saved to UserCarts.");
                return StatusCode(500, "Не вдалося зберегти товар у кошику.");
            }

            return Ok(new { Message = "Товар додано до кошика", CartItem = cartItem });
        }
        else
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(c => c.MerchandiseId == request.MerchandiseId);

            if (cartItem == null)
            {
                cart.Add(new CartItem
                {
                    MerchandiseId = request.MerchandiseId,
                    Quantity = request.Quantity,
                    Price = merchandise.Price
                });
            }
            else
            {
                cartItem.Quantity += request.Quantity;
                cartItem.Price = merchandise.Price;
            }

            HttpContext.Session.SetObject("Cart", cart);
            Console.WriteLine($"Added to session cart: {request.MerchandiseId}");
            return Ok(new { Message = "Товар додано до кошика (сесія)" });
        }
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserCart(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("ID користувача не вказано.");
        }

        var cart = await _context.UserCarts
            .Where(c => c.UserId == userId)
            .Include(c => c.Merchandise)
            .ToListAsync();

        return Ok(cart);
    }

    [HttpGet("count/{userId}")]
    public async Task<IActionResult> GetCartCount(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("ID користувача не вказано.");
        }

        var count = await _context.UserCarts
            .Where(c => c.UserId == userId)
            .SumAsync(c => c.Quantity);

        return Ok(new { Count = count });
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveFromCart([FromBody] CartItemRequest request)
    {
        if (string.IsNullOrEmpty(request.UserId) || request.MerchandiseId <= 0)
        {
            return BadRequest("Невірні дані для видалення.");
        }

        var item = await _context.UserCarts
            .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.MerchandiseId == request.MerchandiseId);

        if (item == null)
        {
            return NotFound("Товар не знайдено в кошику.");
        }

        _context.UserCarts.Remove(item);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Товар видалено з кошика" });
    }

    public class AddToCartRequest
    {
        public int MerchandiseId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class CartItemRequest
    {
        public string UserId { get; set; }
        public int MerchandiseId { get; set; }
    }

    public class CartItem
    {
        public int MerchandiseId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}

public static class SessionExtensions
{
    public static void SetObject(this ISession session, string key, object value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}