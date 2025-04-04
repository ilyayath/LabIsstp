using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMVC.ShopInfrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace ShopInfrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartsApiController : ControllerBase
    {
        private readonly MerchShopeContext _context;

        public ChartsApiController(MerchShopeContext context)
        {
            _context = context;
        }


        [HttpGet("orders-by-year")]
        public async Task<IActionResult> GetOrdersByYear()
        {
            var ordersByYear = await _context.MerchOrders
                .GroupBy(o => o.OrderDate.HasValue ? o.OrderDate.Value.Year : 0)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .OrderBy(x => x.Year)
                .ToListAsync();


            var data = new object[ordersByYear.Count + 1];
            data[0] = new object[] { "Рік", "Кількість замовлень" }; 
            for (int i = 0; i < ordersByYear.Count; i++)
            {
                data[i + 1] = new object[] { ordersByYear[i].Year.ToString(), ordersByYear[i].Count };
            }

            return Ok(data);
        }


        [HttpGet("merch-by-category")]
        public async Task<IActionResult> GetMerchByCategory()
        {
            var merchByCategory = await _context.Merchandises
                .Include(m => m.Category)
                .GroupBy(m => m.Category.CategoryName)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync();


            var data = new object[merchByCategory.Count + 1];
            data[0] = new object[] { "Категорія", "Кількість товарів" }; 
            for (int i = 0; i < merchByCategory.Count; i++)
            {
                data[i + 1] = new object[] { merchByCategory[i].Category, merchByCategory[i].Count };
            }

            return Ok(data);
        }
    }
}