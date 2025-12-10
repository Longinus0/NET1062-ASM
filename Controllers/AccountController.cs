using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using ASM.Data;
using ASM.Models;

namespace ASM.Controllers
{
    public class AccountController : Controller
    {
        private readonly FastFoodDbContext _context;

        public AccountController(FastFoodDbContext context)
        {
            _context = context;
        }

        // --- Helper to Create Claims & Sign In ---
        private async Task SignInUserAsync(User user, bool isPersistent)
        {
            var fullName = $"{user.LastName} {user.FirstName}".Trim();
            if (string.IsNullOrEmpty(fullName)) fullName = "User";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role),
                // ADDED: Store Avatar in Claims
                new Claim("Avatar", user.Avatar ?? "") 
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = isPersistent };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), 
                authProperties);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (string.IsNullOrEmpty(model.Identifier) || string.IsNullOrEmpty(model.Password))
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });

            var user = await _context.Users.FirstOrDefaultAsync(u => 
                (u.Email == model.Identifier || u.Phone == model.Identifier) && 
                u.Password == model.Password);

            if (user == null) return Json(new { success = false, message = "Tài khoản hoặc mật khẩu không chính xác." });

            // Use Helper to Sign In
            await SignInUserAsync(user, model.RememberMe);

            return Json(new { success = true, role = user.Role });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto model)
        {
            if (!User.Identity!.IsAuthenticated) return Json(new { success = false, message = "Unauthorized" });

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.Include(u => u.DefaultAddress).FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return Json(new { success = false, message = "User not found" });

            // Update Fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.Phone;
            user.Gender = model.Gender;
            user.Avatar = model.Avatar;
            
            if (!string.IsNullOrEmpty(model.Address))
            {
                if (user.DefaultAddress == null)
                {
                    var newAddr = new Address 
                    { 
                        UserId = user.UserId,
                        Street = model.Address, 
                        RecipientName = $"{model.LastName} {model.FirstName}", 
                        Phone = user.Phone,
                        District = "Unknown",
                        City = "Hồ Chí Minh",
                        IsDefault = 1
                    };
                    _context.Addresses.Add(newAddr);
                    user.DefaultAddress = newAddr;
                }
                else
                {
                    user.DefaultAddress.Street = model.Address;
                }
            }

            await _context.SaveChangesAsync();

            // CRITICAL: Refresh the Auth Cookie with new Name/Avatar data
            await SignInUserAsync(user, true); 

            return Json(new { success = true });
        }

        // ... (Keep Logout, Register, Profile GET, and other methods exactly as they were) ...
        
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Phone) || string.IsNullOrEmpty(model.Password))
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email || u.Phone == model.Phone);
            if (existingUser != null) return Json(new { success = false, message = "Email hoặc SĐT đã tồn tại." });

            var newUser = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                Password = model.Password, 
                Gender = model.Gender,
                Role = "customer",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đăng ký thành công!" });
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity!.IsAuthenticated) return RedirectToAction("Login");
            
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            
            var userDto = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => new 
                {
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Phone,
                    u.Gender,
                    u.Avatar,
                    u.CreatedAt,
                    DefaultAddress = u.DefaultAddress == null ? null : new 
                    {
                        u.DefaultAddress.Street,
                        u.DefaultAddress.District,
                        u.DefaultAddress.City
                    }
                })
                .FirstOrDefaultAsync();
            
            if (userDto == null) return RedirectToAction("Login");

            return View(userDto);
        }
        
        [HttpGet]
        public async Task<IActionResult> History()
        {
            if (!User.Identity!.IsAuthenticated) return RedirectToAction("Login");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Include(o => o.OrderDetails)
                .ToListAsync();

            return View(orders);
        }

        public IActionResult GoogleLogin() => Challenge(new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") }, GoogleDefaults.AuthenticationScheme);
        public async Task<IActionResult> GoogleResponse()
        {
            await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult FacebookLogin() => Challenge(new AuthenticationProperties { RedirectUri = Url.Action("FacebookResponse") }, FacebookDefaults.AuthenticationScheme);

        public class LoginRequest { public string Identifier { get; set; } = ""; public string Password { get; set; } = ""; public bool RememberMe { get; set; } }
        public class RegisterRequest { public string FirstName { get; set; } = ""; public string LastName { get; set; } = ""; public string Email { get; set; } = ""; public string Phone { get; set; } = ""; public string Password { get; set; } = ""; public string Gender { get; set; } = ""; }
        
        public class UserUpdateDto
        {
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Phone { get; set; } = "";
            public string Gender { get; set; } = "";
            public string Address { get; set; } = "";
            public string Avatar { get; set; } = "";
        }
    }
}
