using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ASM.Data;
using ASM.Models;

namespace ASM.Controllers
{
    public class OrderController : Controller
    {
        private readonly FastFoodDbContext _context;

        public OrderController(FastFoodDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            try 
            {
                var order = new Order
                {
                    FullAddress = request.Address,
                    PaymentMethod = request.PaymentMethod,
                    TotalAmount = request.TotalAmount,
                    Note = request.Note,
                    Status = "pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                if (User.Identity!.IsAuthenticated)
                {
                    order.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                }
                else
                {
                    order.GuestName = request.GuestName;
                    order.GuestPhone = request.GuestPhone;
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in request.Items)
                {
                    var detail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    };

                    if (item.Type == "product") detail.ProductId = item.Id;
                    else detail.ComboId = item.Id;

                    _context.OrderDetails.Add(detail);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, orderId = order.OrderId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class CheckoutRequest
    {
        public string GuestName { get; set; }
        public string GuestPhone { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public string Note { get; set; }
        public decimal TotalAmount { get; set; }
        public List<CartItem> Items { get; set; }
    }

    public class CartItem
    {
        public int Id { get; set; }
        public string Type { get; set; } // 'product' or 'combo'
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
