using HouseOfWani.Models;
using HouseOfWani.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using HouseOfWani.Models.Order;

namespace HouseOfWani.Controllers
{
    public class CartController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public ApplicationDbContext _db;
        public CartController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult MoveToCart(int cartItemId)
        {
            var cartItem = _db.CartItems.FirstOrDefault(item => item.Id == cartItemId);
            if (cartItem != null)
            {
              
                _db.SaveChanges();
            }

            return RedirectToAction("ViewCart");
        }
        public async Task<IActionResult> ViewCart(int id=0)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Find Cart for this user
                //var cart = _db.Carts
                //              .Include(c => c.Items)
                //              .ThenInclude(item => item.Product)
                //              .FirstOrDefault(c => c.UserId == userId);
                var cart =  await _db.Carts
                            .Where(c => c.UserId == userId && c.IsActive)
                            .OrderByDescending(c => c.Id) // or .OrderByDescending(c => c.CreatedDate) if available
                            .Include(c => c.Items)
                                .ThenInclude(item => item.Product)
                            .FirstOrDefaultAsync();
                if (cart == null || cart.Items == null || !cart.Items.Any())
                {
                    return View("EmptyCart"); // Show Empty Cart page
                }

                // Adjust image URLs if necessary
                foreach (var item in cart.Items)
                {
                    if (!string.IsNullOrEmpty(item.Product.ImageUrl) && !item.Product.ImageUrl.StartsWith("/uploads/"))
                    {
                        item.Product.ImageUrl = "/uploads/" + item.Product.ImageUrl;
                    }
                }
                //var cartItems = cart.Items.Where(item => !item.IsSavedForLater).ToList();
                //var savedForLaterItems = cart.Items.Where(item => item.IsSavedForLater).ToList();

                //// Pass both lists to the view
                //var viewModel = new CartViewModel
                //{
                //    CartItems = cartItems,
                //    SavedForLaterItems = savedForLaterItems
                //};

                return View(cart.Items.ToList()); // Send list of CartItems to View
            }
            catch (Exception ex)
            {
                // Log the error if you have logger (_logger.LogError(ex, "Error loading cart"))
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        //public IActionResult ViewCart()
        //{
        //    try
        //    {
        //        var userId = HttpContext.Session.GetString("UserId");
        //        var cartItems = _db.CartItems
        //                         .Include(c => c.Product) // this includes the whole Product entity, including ImageUrl
        //                         .Where(c => c.userId == userId)
        //                         .ToList();
        //        if (cartItems == null || cartItems.Count == 0)
        //        {
        //            return View("EmptyCart"); // Return an empty cart view if no items are found
        //        }
        //        foreach (var item in cartItems)
        //        {
        //            if (!string.IsNullOrEmpty(item.Product.ImageUrl) && !item.Product.ImageUrl.StartsWith("/uploads/"))
        //            {
        //                item.Product.ImageUrl = "/uploads/" + item.Product.ImageUrl;
        //            }
        //        }
        //        //var groupedItems = cartItems
        //        //                .GroupBy(c => c.ProductId)
        //        //                .Select(g => g.ToList())
        //        //                .ToList();




        //        //ViewBag.GroupedItems = groupedItems;
        //        // return RedirectToAction("details","Home");
        //        return View(cartItems);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle the exception (e.g., log it, show an error message, etc.)
        //        return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        //    }
        //}
        [HttpPost]
        public async Task<IActionResult> SaveForLater(int cartItemId)
        {
            var cartItem = await _db.CartItems.FindAsync(cartItemId);

            if (cartItem != null)
            {
               // cartItem.IsSavedForLater = true;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("ViewCart");
        }
        [HttpPost]
        public async Task<JsonResult> AddToCartAjax(string selectedSize, int productId = 0)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not logged in." });
                }

                // 1. Check if user already has a Cart
                //var cart = await _db.Carts
                // .Where(c => c.UserId == userId)
                // .Include(c => c.Items) // Include all items
                // .FirstOrDefaultAsync();
                var cart = await _db.Carts
                            .Where(c => c.UserId == userId && c.IsActive)
                            .Include(c => c.Items)
                            .FirstOrDefaultAsync();
             
                if (cart == null && productId!=0)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        Items = new List<CartItem>()
                    };
                    await _db.Carts.AddAsync(cart);
                    await _db.SaveChangesAsync();  // Save first to get Cart.Id
                }

                // 2. If productId = 0, just return cart count
                if (productId == 0)
                {
                    //var result1 = _db.CartItems
                    //          .Include(ci => ci.Cart)
                    //          .Where(ci => ci.Cart.IsActive == false)
                    //          .ToList();
                    //int cartItemCount1 = result1.Count;

                    int cartItemCount1 = _db.CartItems
                                         .Where(ci => ci.Cart.IsActive == true && ci.Cart.UserId == userId) // Filter by UserId as well
                                         .Include(ci => ci.Cart) // Ensure the Cart is included before accessing it
                                         .Count();

                    //int cartItemCount1 = cart.Items.Count;
                    return Json(new { success = true, message = "Cart loaded.", cartItemCount = cartItemCount1 });
                }

                // 3. Find product
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.IsActive==true);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found." });
                }

                // 4. Check if CartItem already exists (same product + same size)
                var existingCartItem = cart.Items.FirstOrDefault(x => x.ProductId == productId && x.SizeName == selectedSize);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += 1;
                    existingCartItem.TotalPrice = existingCartItem.Price * existingCartItem.Quantity;
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = productId,
                        SizeName = selectedSize,
                        Quantity = 1,
                        Price = product.Price,
                        TotalPrice = product.Price
                    };
                    cart.Items.Add(cartItem);
                }

                await _db.SaveChangesAsync();

                //int cartItemCount = cart.Items.Sum(x => x.Quantity);
                // int cartItemCount = cart.Items.Count;
                //int cartItemCount=_db.CartItems
                //           .Where(ci => ci.Cart.IsActive == true)
                //           .Include(ci => ci.Cart) // Ensure the Cart is included before accessing it
                //           .Count();
                int cartItemCount = _db.CartItems
                                    .Where(ci => ci.Cart.IsActive == true && ci.Cart.UserId == userId) // Filter by UserId as well
                                    .Include(ci => ci.Cart) // Ensure the Cart is included before accessing it
                                    .Count();

                return Json(new { success = true, message = "Product added to cart.", cartItemCount = cartItemCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred.", error = ex.Message });
            }
        }


        //[HttpPost]
        //public async Task<JsonResult> AddToCartAjax(string selectedSize,int productId=0)
        //{
        //    try
        //    {

        //        var userId = HttpContext.Session.GetString("UserId");
        //    if (productId == 0)
        //    {
        //        var cartItemCount1 = await _db.CartItems
        //                            .Where(x => x.userId == userId && _db.Products.Any(p => p.Id == x.ProductId))
        //                            .GroupBy(x => x.ProductId)
        //                            .CountAsync();
        //        return Json(new { success = true, message = "Product added to cart.", cartItemCount = cartItemCount1 });

        //    }
        //    var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        //        if (product == null)
        //        {
        //            return Json(new { success = false, message = "Product not found." });
        //        }

        //        var cartItem = new CartItem
        //        {
        //            ProductId = productId,
        //            SizeName = selectedSize,
        //            Quantity = 1,
        //            Price = product.Price,
        //            userId = userId

        //        };

        //        await _db.CartItems.AddAsync(cartItem);
        //        await _db.SaveChangesAsync();
        //    //var cartItemCount = await _db.CartItems.CountAsync();
        //    var cartItemCount = await _db.CartItems
        //                       .Where(x => x.userId == userId && _db.Products.Any(p => p.Id == x.ProductId))
        //                       .GroupBy(x => x.ProductId)
        //                       .CountAsync();


        //    return Json(new { success = true, message = "Product added to cart.", cartItemCount = cartItemCount });
        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { success = false, message = "An error occurred.", error = ex.Message });
        //    }
        //} 
        //[HttpPost]
        //public async Task<JsonResult> AddToCartAjax(string selectedSize, int productId = 0)
        //{
        //    try
        //    { 

        //        var userId = HttpContext.Session.GetString("UserId");
        //        if (string.IsNullOrEmpty(userId))
        //        {
        //            return Json(new { success = false, message = "User not logged in." });
        //        }

        //        if (productId == 0)
        //        {

        //            var cartItemCount1 = await _db.CartItems
        //                            .Where(x => x.userId == userId)
        //                            .CountAsync();

        //            return Json(new { success = true, message = "Product added to cart.", cartItemCount = cartItemCount1 });
        //        }

        //        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        //        if (product == null)
        //        {
        //            return Json(new { success = false, message = "Product not found." });
        //        }

        //        var cartItem = new CartItem
        //        {
        //            ProductId = productId,
        //            SizeName = selectedSize,
        //            Quantity = 1,
        //            Price = product.Price,
        //            userId = userId
        //        };

        //        await _db.CartItems.AddAsync(cartItem);
        //        await _db.SaveChangesAsync();

        //        var cartItemCount = await _db.CartItems
        //                        .Where(x => x.userId == userId)
        //                        .CountAsync();

        //        return Json(new { success = true, message = "Product added to cart.", cartItemCount = cartItemCount });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Optional: Log the error using logger (_logger.LogError)
        //        // _logger.LogError(ex, "Error in AddToCartAjax for productId: {productId}");

        //        return Json(new { success = false, message = "An error occurred while adding the product to the cart.", error = ex.Message });
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> RemoveCartItem(int id)
        {
            try
            {
                // Retrieve the UserId from session
                var userId = HttpContext.Session.GetString("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "Please login to modify your cart.";
                    return RedirectToAction("Login", "Account");
                }

                // Find the CartItem by Id
                var cartItem = await _db.CartItems
                                         .FirstOrDefaultAsync(ci => ci.Id == id); // No need for userId in this condition
                var cartId = cartItem.CartId;
                if (cartItem == null)
                {
                    TempData["ErrorMessage"] = "Cart item not found.";
                    return RedirectToAction("ViewCart");
                }

                // Remove the CartItem from the database
                _db.CartItems.Remove(cartItem);
                await _db.SaveChangesAsync();
                var remainingItemsCount = await _db.CartItems
                                           .Where(ci => ci.CartId == cartId)
                                           .CountAsync();

                if (remainingItemsCount == 0)
                {
                    var cart = await _db.Carts.FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId);
                    if (cart != null)
                    {
                        cart.IsActive = false;
                      //  _db.Carts.Remove(cart);
                        await _db.SaveChangesAsync();
                    }
                }

                TempData["SuccessMessage"] = "Item removed successfully.";
                return RedirectToAction("ViewCart");
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                // _logger.LogError(ex, "Error while removing cart item.");

                TempData["ErrorMessage"] = "Something went wrong while removing the item.";
                return RedirectToAction("ViewCart");
            }
        }


        //[HttpGet]
        //public async Task<IActionResult> RemoveCartItem(int id)
        //{
        //    try
        //    {
        //        // Retrieve the UserId from session
        //        var userId = HttpContext.Session.GetString("UserId");

        //        // Check if the user is logged in
        //        //if (string.IsNullOrEmpty(userId))
        //        //{
        //        //    // Redirect to the login page if the user is not logged in
        //        //    return RedirectToAction("Login", "Account");
        //        //}

        //        // Retrieve the cart item using the given ProductId (CartItemId)
        //        var cartItem = await _db.CartItems
        //                         .Where(ci => ci.Id == id && ci.userId == userId) // Ensure both ProductId and UserId match
        //                         .FirstOrDefaultAsync();
        //        // If the cart item exists, remove it
        //        if (cartItem != null)
        //        {
        //            // Remove the CartItem from the database
        //            _db.CartItems.Remove(cartItem);

        //            // Save changes to the database
        //            await _db.SaveChangesAsync();
        //        }

        //        // After removal, redirect to the ViewCart page to show the updated cart
        //        return RedirectToAction("ViewCart");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error if you have a logger (optional, but recommended)
        //        //_logger.LogError(ex, $"Error occurred while removing cart item with ProductId: {ProductId}");

        //        // Store an error message in TempData to display to the user
        //        TempData["ErrorMessage"] = "Something went wrong while removing the item from your cart.";

        //        // Redirect back to the cart view with the error message
        //        return RedirectToAction("ViewCart");
        //    }
        //}

        //[HttpGet]
        //public async Task<IActionResult> RemoveCartItem(int ProductId)
        //{
        //    try
        //    {
        //        var userId = HttpContext.Session.GetString("UserId");
        //        if (string.IsNullOrEmpty(userId))
        //        {
        //            return RedirectToAction("Login", "Account"); // or some session expired handling
        //        }

        //        //var cartItem = _db.CartItems
        //        //                  .Include(x => x.Product)
        //        //                  .FirstOrDefault(x => x.userId == userId && x.Id == ProductId);
        //        var cartItem = await _db.CartItems
        //                        .Where(ci => ci.Id == ProductId) // Only using the Id
        //                        .FirstOrDefaultAsync();

        //        if (cartItem != null)
        //        {
        //            // Remove the CartItem
        //            _db.CartItems.Remove(cartItem);
        //            await _db.SaveChangesAsync();
        //        }


        //        return RedirectToAction("ViewCart"); // After removing, redirect to cart page
        //    }
        //    catch (Exception ex)
        //    {
        //        // Optional: Log the error if you have logger
        //        // _logger.LogError(ex, $"Error removing cart item with ProductId: {ProductId}");

        //        TempData["ErrorMessage"] = "Something went wrong while removing the item.";
        //        return RedirectToAction("ViewCart"); // Redirect to cart view with error
        //    }
        //}

        //public IActionResult RemoveCartItem(int ProductId)

        //{
        //    try
        //    {
        //        var userId = HttpContext.Session.GetString("UserId");
        //        var cartItem = _db.CartItems.Include(x => x.Product).Where(x => x.userId == userId && x.Id == ProductId).FirstOrDefault();

        //        if (cartItem != null)
        //        {
        //            //_db.CartItems.Remove(cartItem);

        //            //_db.SaveChanges();
        //        }
        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle the exception (e.g., log it, show an error message, etc.)
        //        return View("ViewCart");

        //    }
        //}

    }
}
