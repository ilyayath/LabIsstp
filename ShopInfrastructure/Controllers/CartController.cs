using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopDomain.Models;
using ShopMVC.ShopInfrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly MerchShopeContext _context;
    private readonly UserManager<AppUser> _userManager;

    public CartController(MerchShopeContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] CartItem cartItem)
    {
        if (cartItem == null || string.IsNullOrEmpty(cartItem.UserId) || cartItem.MerchandiseId <= 0)
        {
            return BadRequest("Невірні дані кошика.");
        }

        // Перевіряємо, чи існує користувач
        var user = await _userManager.FindByIdAsync(cartItem.UserId);
        if (user == null)
        {
            return BadRequest("Користувача не знайдено.");
        }

        // Перевіряємо, чи товар уже в кошику
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == cartItem.UserId && c.MerchandiseId == cartItem.MerchandiseId);

        if (existingItem != null)
        {
            // Оновлюємо кількість, якщо товар уже є
            existingItem.Quantity += cartItem.Quantity;
        }
        else
        {
            // Додаємо новий товар
            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Товар додано до кошика", CartItem = cartItem });
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserCart(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("ID користувача не вказано.");
        }

        var cart = await _context.CartItems
            .Where(c => c.UserId == userId)
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

        var count = await _context.CartItems
            .CountAsync(c => c.UserId == userId);

        return Ok(new { count });
    }
    [HttpPost("remove")]
    public async Task<IActionResult> RemoveFromCart([FromBody] CartItemRequest request)
    {
        if (string.IsNullOrEmpty(request.UserId) || request.MerchandiseId <= 0)
        {
            return BadRequest("Невірні дані для видалення.");
        }

        var item = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.MerchandiseId == request.MerchandiseId);

        if (item == null)
        {
            return NotFound("Товар не знайдено в кошику.");
        }

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Товар видалено з кошика" });
    }

    public class CartItemRequest
    {
        public string UserId { get; set; }
        public int MerchandiseId { get; set; }
    }
}