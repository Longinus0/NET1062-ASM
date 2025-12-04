using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ASM.Data;
using ASM.Models;

namespace ASM.Controllers
{
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly FastFoodDbContext _context;

        public AdminController(FastFoodDbContext context)
        {
            _context = context;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var deliveredOrders = await _context.Orders
                .Where(o => o.Status == "delivered")
                .Select(o => o.TotalAmount)
                .ToListAsync();
            
            var totalRevenue = deliveredOrders.Sum();

            var stats = new
            {
                TotalOrders = await _context.Orders.CountAsync(),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == "pending"),
                TotalUsers = await _context.Users.CountAsync(),
                TotalRevenue = totalRevenue,
                RecentOrders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => new 
                    {
                        o.OrderId,
                        UserName = o.User != null ? (o.User.LastName + " " + o.User.FirstName) : o.GuestName,
                        o.TotalAmount,
                        o.Status,
                        o.CreatedAt
                    })
                    .ToListAsync()
            };

            ViewBag.Stats = stats;
            return View();
        }

        [HttpGet("Users")]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new 
                {
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Phone,
                    u.Role,
                    u.CreatedAt
                })
                .ToListAsync();
                
            return View(users);
        }

        [HttpGet("Food")]
        public async Task<IActionResult> Food()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Select(p => new 
                {
                    p.ProductId,
                    p.Name,
                    p.Price,
                    p.Image,
                    p.IsAvailable,
                    Category = new { Name = p.Category.Name }
                })
                .ToListAsync();

            var categories = await _context.Categories
                .Select(c => new { c.CategoryId, c.Name })
                .ToListAsync();
                
            ViewBag.Categories = categories;
            return View(products);
        }

        [HttpGet("Combos")]
        public async Task<IActionResult> Combos()
        {
            var combos = await _context.Combos
                .Select(c => new 
                {
                    c.ComboId,
                    c.Name,
                    c.Description,
                    c.Price,
                    c.Image,
                    c.IsAvailable
                })
                .ToListAsync();
                
            return View(combos);
        }

        [HttpGet("Orders")]
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new 
                {
                    o.OrderId,
                    o.GuestName,
                    o.GuestPhone,
                    o.FullAddress,
                    o.TotalAmount,
                    o.Status,
                    o.CreatedAt,
                    User = o.User == null ? null : new 
                    {
                        o.User.FirstName,
                        o.User.LastName,
                        o.User.Phone
                    }
                })
                .ToListAsync();
                
            return View(orders);
        }

        [HttpPost("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return Json(new { success = false, message = "User not found" });
            
            if (user.Role == "admin" && user.Email == "admin@example.com") 
                 return Json(new { success = false, message = "Cannot delete super admin" });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost("UpdateOrderStatus")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateStatusRequest request)
        {
            var order = await _context.Orders.FindAsync(request.OrderId);
            if (order == null) return Json(new { success = false, message = "Order not found" });

            order.Status = request.Status;
            order.UpdatedAt = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public class UpdateStatusRequest
        {
            public int OrderId { get; set; }
            public string Status { get; set; } = "";
        }
    }
}
