using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASM.Models;
using ASM.Data;

namespace ASM.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FastFoodDbContext _context;

        public HomeController(ILogger<HomeController> logger, FastFoodDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // URL: /
        public async Task<IActionResult> Index()
        {
            var productsQuery = await _context.Products
                                         .Include(p => p.Category)
                                         .Where(p => p.IsAvailable == 1)
                                         .ToListAsync();

            var products = productsQuery.Select(p => new 
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Image = p.Image,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                OldPrice = (decimal?)null 
            }).ToList();

            var categoriesQuery = await _context.Categories
                                           .OrderBy(c => c.DisplayOrder)
                                           .ToListAsync();
            
            var categories = categoriesQuery.Select(c => new 
            {
                CategoryId = c.CategoryId,
                Name = c.Name
            }).ToList();

            ViewBag.Categories = categories;
            return View(products);
        }

        // URL: /Home/Combo
        public async Task<IActionResult> Combo()
        {
            var combosQuery = await _context.Combos
                                       .Where(c => c.IsAvailable == 1)
                                       .ToListAsync();
            
            var combos = combosQuery.Select(c => new 
            {
                ComboId = c.ComboId,
                Name = c.Name,
                Description = c.Description,
                Price = c.Price,
                Image = c.Image,
                OldPrice = (decimal?)(c.Price * 1.2m), 
                Serves = 2,
                Type = "all",
                Tag = "Hot",
                Items = new List<string> { "Món chính", "Nước ngọt" }
            }).ToList();

            return View(combos);
        }
        
        // URL: /Home/Menu
        public async Task<IActionResult> Menu()
        {
            // 1. Fetch Products
            var productsQuery = await _context.Products
                                         .Include(p => p.Category)
                                         .Where(p => p.IsAvailable == 1)
                                         .ToListAsync();

            // Project to DTO
            var products = productsQuery.Select(p => new 
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Image = p.Image,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                Rating = 4.5, // Mock data
                OldPrice = (decimal?)null 
            }).ToList();

            // 2. Fetch Categories
            var categoriesQuery = await _context.Categories
                                           .OrderBy(c => c.DisplayOrder)
                                           .ToListAsync();
            
            var categories = categoriesQuery.Select(c => new 
            {
                CategoryId = c.CategoryId,
                Name = c.Name
            }).ToList();

            ViewBag.Categories = categories;
            return View(products);
        }

        // URL: /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
