using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ASM.Data;
using ASM.Models;

namespace ASM.Controllers
{
    [Route("Admin")]
    [Authorize(Roles = "admin")]
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

        // Helper to attempt to read the current authenticated user's id from claims
        private int? GetCurrentUserId()
        {
            try
            {
                var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ?? User.FindFirst("user_id");
                if (idClaim != null && int.TryParse(idClaim.Value, out var id)) return id;
            }
            catch
            {
                // ignore and return null when claims are not present
            }
            return null;
        }

        // --- User management: Create, Edit ---
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromForm] User model)
        {
            if (model == null) return Json(new { success = false, message = "Invalid user data" });

            // Simple uniqueness check for email/phone
            if (!string.IsNullOrEmpty(model.Email) && await _context.Users.AnyAsync(u => u.Email == model.Email))
                return Json(new { success = false, message = "Email already in use" });

            model.CreatedAt = DateTime.Now;
            _context.Users.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, userId = model.UserId });
        }

        [HttpPost("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromForm] User update)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return Json(new { success = false, message = "User not found" });

            // Prevent deleting or demoting the currently logged-in admin via this endpoint
            var currentId = GetCurrentUserId();
            if (currentId.HasValue && currentId.Value == id && update.Role != "admin")
            {
                return Json(new { success = false, message = "Cannot change role of the admin currently signed in" });
            }

            user.FirstName = update.FirstName;
            user.LastName = update.LastName;
            user.Email = update.Email;
            user.Phone = update.Phone;
            if (!string.IsNullOrEmpty(update.Password)) user.Password = update.Password; // keep existing if empty
            user.Role = update.Role ?? user.Role;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // --- Category management ---
        [HttpPost("CreateCategory")]
        public async Task<IActionResult> CreateCategory([FromForm] Category model)
        {
            if (model == null) return Json(new { success = false, message = "Invalid category" });
            _context.Categories.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, categoryId = model.CategoryId });
        }

        [HttpPost("UpdateCategory/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromForm] Category update)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return Json(new { success = false, message = "Category not found" });
            cat.Name = update.Name;
            cat.Image = update.Image;
            cat.DisplayOrder = update.DisplayOrder;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost("DeleteCategory/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.CategoryId == id);
            if (cat == null) return Json(new { success = false, message = "Category not found" });
            if (cat.Products != null && cat.Products.Any())
                return Json(new { success = false, message = "Cannot delete category with products" });
            _context.Categories.Remove(cat);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // --- Product (Food) management ---
        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProduct([FromForm] Product model)
        {
            if (model == null) return Json(new { success = false, message = "Invalid product" });
            _context.Products.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, productId = model.ProductId });
        }

        [HttpPost("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] Product update)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return Json(new { success = false, message = "Product not found" });
            p.Name = update.Name;
            p.Description = update.Description;
            p.Price = update.Price;
            p.Image = update.Image;
            p.IsAvailable = update.IsAvailable;
            p.CategoryId = update.CategoryId;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return Json(new { success = false, message = "Product not found" });
            _context.Products.Remove(p);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // --- Combo management ---
        // Accepts optional CSV of productId:quantity pairs in `items` form field, e.g. "1:2,3:1"
        [HttpPost("CreateCombo")]
        public async Task<IActionResult> CreateCombo([FromForm] Combo model, [FromForm] string? items)
        {
            if (model == null) return Json(new { success = false, message = "Invalid combo" });
            _context.Combos.Add(model);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(items))
            {
                var pairs = items.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in pairs)
                {
                    var parts = p.Split(':');
                    if (parts.Length >= 1 && int.TryParse(parts[0], out var prodId))
                    {
                        var qty = 1;
                        if (parts.Length == 2) int.TryParse(parts[1], out qty);
                        _context.ComboDetails.Add(new ComboDetail { ComboId = model.ComboId, ProductId = prodId, Quantity = qty });
                    }
                }
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, comboId = model.ComboId });
        }

        [HttpPost("UpdateCombo/{id}")]
        public async Task<IActionResult> UpdateCombo(int id, [FromForm] Combo update, [FromForm] string? items)
        {
            var combo = await _context.Combos.Include(c => c.ComboDetails).FirstOrDefaultAsync(c => c.ComboId == id);
            if (combo == null) return Json(new { success = false, message = "Combo not found" });
            combo.Name = update.Name;
            combo.Description = update.Description;
            combo.Price = update.Price;
            combo.Image = update.Image;
            combo.IsAvailable = update.IsAvailable;

            // Replace details if items specified
            if (!string.IsNullOrEmpty(items))
            {
                _context.ComboDetails.RemoveRange(combo.ComboDetails);
                var pairs = items.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in pairs)
                {
                    var parts = p.Split(':');
                    if (parts.Length >= 1 && int.TryParse(parts[0], out var prodId))
                    {
                        var qty = 1;
                        if (parts.Length == 2) int.TryParse(parts[1], out qty);
                        _context.ComboDetails.Add(new ComboDetail { ComboId = combo.ComboId, ProductId = prodId, Quantity = qty });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost("DeleteCombo/{id}")]
        public async Task<IActionResult> DeleteCombo(int id)
        {
            var combo = await _context.Combos.Include(c => c.ComboDetails).FirstOrDefaultAsync(c => c.ComboId == id);
            if (combo == null) return Json(new { success = false, message = "Combo not found" });
            if (combo.ComboDetails != null) _context.ComboDetails.RemoveRange(combo.ComboDetails);
            _context.Combos.Remove(combo);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // --- Admin profile update ---
        [HttpGet("Profile")]
        public async Task<IActionResult> Profile()
        {
            var currentId = GetCurrentUserId();
            if (!currentId.HasValue) return RedirectToAction("Login", "Account");
            var user = await _context.Users.FindAsync(currentId.Value);
            if (user == null) return RedirectToAction("Login", "Account");
            return View(user);
        }

        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromForm] User update)
        {
            var currentId = GetCurrentUserId();
            if (!currentId.HasValue) return Json(new { success = false, message = "Not authenticated" });
            var user = await _context.Users.FindAsync(currentId.Value);
            if (user == null) return Json(new { success = false, message = "User not found" });
            user.FirstName = update.FirstName;
            user.LastName = update.LastName;
            user.Phone = update.Phone;
            if (!string.IsNullOrEmpty(update.Password)) user.Password = update.Password;
            user.Avatar = update.Avatar;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
