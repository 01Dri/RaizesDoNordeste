using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteUni.Data;
using RestauranteUni.Domain.Core.Users;

namespace RestauranteUni.API.Controllers
{

    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class TestController : ControllerBase
    {
        private readonly ICurrentUser _currentUser;
        private readonly ApplicationDbContext _context;
        public TestController(ICurrentUser currentUser, ApplicationDbContext context)
        {
            _currentUser = currentUser;
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {

            return Ok($"Olá {_currentUser.AccountId}");
        }
        
        [HttpGet]
        [Route("Stock")]
        public IActionResult Test()
        {
            var menu = _context.Menus.Include(x => x.Items).
                ThenInclude(x => x.Ingredients).ThenInclude(x => x.StockIngredient)
                .FirstOrDefault(); 
            
            var items = menu.Items
                .SelectMany(x => x.Ingredients).Select(x => new
                {
                    Quantity = x.Quantity,
                    Name = x.StockIngredient.Name
                })
                .ToList();
            return Ok(items);
        }
        
        [HttpGet]
        [Route("orders")]
        public IActionResult Orders()
        {
            var orders = _context.Orders
                .Include(x => x.Items)
                .ThenInclude(x
                    => x.MenuItem)
                .Select(x => new OrderResponseDto
                {
                    Id = x.PublicId,
                    AccountId = x.AccountId,
                    RestaurantId = x.RestaurantId,
                    Items = x.Items.Select(i => new OrderItemResponseDto
                    {
                        MenuItemName = i.MenuItem.Title,
                        Quantity = i.Quantity
                    }).ToList()
                })
                .FirstOrDefault(x => x.AccountId == _currentUser.AccountId && x.RestaurantId == _currentUser.RestaurantId);
            
            return Ok(orders);
        }
    }

    class OrderResponseDto
    {
        public Guid Id { get; set; }
        public long? AccountId { get; set; }
        public Guid? RestaurantId { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = [];
    }
    
    class OrderItemResponseDto
    {
        public string MenuItemName { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
