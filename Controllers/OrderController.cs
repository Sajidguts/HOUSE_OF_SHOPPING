using HouseOfWani.Controllers;
using HouseOfWani.Models;
using HouseOfWani.Models.Order;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<OrderController> _logger;

    public OrderController(ILogger<OrderController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> PlaceOrder(PlaceOrderViewModel orderViewModel)
    {
        try
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not found while placing order.");
                return RedirectToAction("Login", "Account");
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);

            if (cart == null || !cart.Items.Any())
            {
                _logger.LogInformation($"Cart is empty for user: {userId}");
                return RedirectToAction("Index", "Cart");
            }
            //var order = new Order
            //{
            //    UserId = userId,
            //    TotalAmount = cart.Items.Sum(ci =>
            //        (ci.Product.DiscountedPrice > 0 ? ci.Product.DiscountedPrice : ci.Product.Price) * ci.Quantity),
            //    Status = "Confirmed",
            //    OrderDate = DateTime.Now
            //};

            var order = new Order
            {
                UserId = userId,
                //TotalAmount = cart.Items.Sum(ci =>
                //    (ci.Product.DiscountedPrice > 0 ? ci.Product.DiscountedPrice : ci.Product.Price) * ci.Quantity),
                TotalAmount = orderViewModel.TotalDAmount,
                cartId=orderViewModel.cardId,
                Status = "Confirmed",
                OrderDate = DateTime.Now
            };

         

          //  _context.Orders.Add(order);

            //foreach (var cartItem in cart.Items)
            //{
            //    // Add the order item
            //    _context.OrderItems.Add(new OrderItem
            //    {
            //        Order = order,
            //        ProductId = cartItem.ProductId,
            //        Quantity = cartItem.Quantity,
            //        PriceAtPurchase = cartItem.Product.DiscountedPrice > 0
            //            ? cartItem.Product.DiscountedPrice
            //            : cartItem.Product.Price
            //    });

            //    // 🛒 Decrease product stock here
            //    var product = await _context.Products.FindAsync(cartItem.ProductId);
            //    if (product != null)
            //    {
            //        product.Qauntity -= cartItem.Quantity;

            //        // Optional: Avoid negative stock
            //        if (product.Qauntity < 0)
            //            product.Qauntity = 0;
            //    }
            //}




            _context.Orders.Add(order);
            foreach (var cartItem in orderViewModel.CartItems)
            {
                // Add the order item
                var existingCartItem = await _context.CartItems
        .FirstOrDefaultAsync(c => c.Id == cartItem.Id); // Use Id or matching keys

                if (existingCartItem != null)
                {
                    // 2. Update its fields
                    existingCartItem.Quantity = cartItem.Quantity;
                    existingCartItem.SizeName = cartItem.SizeName;
                    existingCartItem.Price = cartItem.Product.DiscountedPrice > 0
                        ? cartItem.Product.DiscountedPrice
                        : cartItem.Product.Price;
                    existingCartItem.TotalPrice = existingCartItem.Price * cartItem.Quantity;
                }


                _context.OrderItems.Add(new OrderItem
                {
                    Order = order,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    
                    PriceAtPurchase = cartItem.Product.DiscountedPrice > 0
                        ? cartItem.Product.DiscountedPrice 
                        : cartItem.Product.Price ,


                });
                // 🛒 Decrease product stock here
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product != null)
                {
                    product.Qauntity -= cartItem.Quantity;

                    // Optional: Avoid negative stock
                    if (product.Qauntity < 0)
                        product.Qauntity = 0;
                }
            }

            cart.IsActive = false;

            // Remove the cart after placing order
            //_context.Carts.Remove(cart);

            await _context.SaveChangesAsync(); 

            _logger.LogInformation($"Order {order.Id} placed successfully for user {userId}.");
            return RedirectToAction("OrderSuccess", new { orderId = order.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while placing the order.");
            return View("Error");
        }
    }


    public async Task<IActionResult> OrderSuccess(int orderId)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            //if (order == null)
            //{
            //    _logger.LogWarning($"Order with ID {orderId} not found.");
            //    return NotFound();
            //}

            //return View(order);
            ViewData["Title"] = "Order Successful";
            return View(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving order with ID {orderId}.");
            return View("Error");
        }
    }
}


//using HouseOfWani.Controllers;
//using HouseOfWani.Models;
//using HouseOfWani.Models.Admin;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//public class OrderController : Controller
//{
//    private readonly ApplicationDbContext _context;
//    private readonly UserManager<ApplicationUser> _userManager;
//    private readonly ILogger<OrderController> _logger;

//    public OrderController(ILogger<OrderController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
//    {
//        _context = context;
//        _userManager = userManager;
//        _logger = logger;
//    }

//    public async Task<IActionResult> PlaceOrder(decimal TotalAmount)
//    {
//        try
//        {
//            // Retrieve the UserId from session
//            var userId = HttpContext.Session.GetString("UserId");

//            if (string.IsNullOrEmpty(userId))
//            {
//                _logger.LogWarning("User not found while placing order.");
//                return RedirectToAction("Login", "Account");
//            }

//            // Retrieve the user's cart by UserId
//            var cart = await _context.Carts
//                .Include(c => c.Items)
//                .ThenInclude(ci => ci.Product)
//                .FirstOrDefaultAsync(c => c.UserId == userId);

//            if (cart == null || !cart.Items.Any())
//            {
//                _logger.LogInformation($"Cart is empty for user: {userId}");
//                return RedirectToAction("Index", "Cart");
//            }

//            // Create new Order
//            var order = new Order
//            {
//                UserId = userId,
//                TotalAmount = cart.Items.Sum(ci =>
//                    (ci.Product.DiscountedPrice > 0 ? ci.Product.DiscountedPrice : ci.Product.Price) * ci.Quantity),
//                Status = "Confirmed",
//                OrderDate = DateTime.Now
//            };

//            _context.Orders.Add(order);
//            await _context.SaveChangesAsync();

//            // Add Order Items
//            foreach (var cartItem in cart.Items)
//            {
//                var orderItem = new OrderItem
//                {
//                    OrderId = order.Id,
//                    ProductId = cartItem.ProductId,
//                    Quantity = cartItem.Quantity,
//                    PriceAtPurchase = cartItem.Product.DiscountedPrice > 0
//                        ? cartItem.Product.DiscountedPrice
//                        : cartItem.Product.Price
//                };
//                _context.OrderItems.Add(orderItem);
//            }

//            await _context.SaveChangesAsync();

//            // Empty the user's cart
//            _context.Carts.Remove(cart);
//            await _context.SaveChangesAsync();

//            _logger.LogInformation($"Order {order.Id} placed successfully for user {userId}.");
//            return RedirectToAction("OrderSuccess", new { orderId = order.Id });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error occurred while placing the order.");
//            return View("Error");  // Or you can redirect to a custom error page
//        }
//    }




//    //public async Task<IActionResult> OrderSuccess(int orderId)
//    //{
//    //    try
//    //    {
//    //        var order = await _context.Orders.FindAsync(orderId);

//    //        if (order == null)
//    //        {
//    //            _logger.LogWarning($"Order with ID {orderId} not found.");
//    //            return NotFound();
//    //        }

//    //        return View(order);
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        _logger.LogError(ex, $"Error retrieving order with ID {orderId}.");
//    //        return View("Error");
//    //    }
//    //}
//    public async Task<IActionResult> OrderSuccess(int orderId)
//    {
//        try
//        {
//            var order = await _context.Orders
//                .Include(o => o.OrderItems)
//                .ThenInclude(oi => oi.Product) // Include Product details for each order item if needed
//                .FirstOrDefaultAsync(o => o.Id == orderId);

//            if (order == null)
//            {
//                _logger.LogWarning($"Order with ID {orderId} not found.");
//                return NotFound();
//            }

//            return View(order);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, $"Error retrieving order with ID {orderId}.");
//            return View("Error");
//        }
//    }

//}
