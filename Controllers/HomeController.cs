using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HouseOfWani.Models;
using HouseOfWani.Models.Admin;
using HouseOfWani.Models.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Abstractions;

namespace HouseOfWani.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public ApplicationDbContext _db;
    private const int PageSize = 9;
    
    public HomeController(ILogger<HomeController> logger,ApplicationDbContext db)
    {
        _logger = logger;
        _db=db;

    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new HomePageViewModel();
        try
        {
          


            var summer = await _db.BannerImages
         .Where(b => b.IsActive == true)  // Filter for active banner images
         .ToListAsync();
            var blog= await _db.BlogPosts.ToListAsync();


        foreach (var bannerImage in summer)
        {
                if (!string.IsNullOrEmpty(bannerImage.SummerImage1))
                {
                    bannerImage.SummerImage1 = "/Banner/" + bannerImage.SummerImage1;

                }
        }
            foreach (var blogimage in blog)
            {
                if (!string.IsNullOrEmpty(blogimage.ImageUrl))
                {
                    blogimage.ImageUrl = "/Blog/" + blogimage.ImageUrl;

                }
            }
            model.BlogPosts = blog;



            ViewBag.BannerImages = summer;
        model.BannerImages = summer;
        List<Product> list = new List<Product>();
       
             //list = _db.Products
             //   .Include(p => p.productdetails)
             //   .ToList();
             list = await _db.Products
                          .Where(b => b.IsActive == true)  // Filter for active banner images
                          .ToListAsync();
            

            foreach (var product in list)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    product.ImageUrl = "/uploads/" + product.ImageUrl;

                }
            }
            model.Products = list;
            ///////// make it dynamic
            //ViewBag.BannerImages = new List<string>
            //{
            //            "/images/hero1.jpg",
            //            "/images/hero2.jpg",
            //            "/images/hero3.jpg"
            // };
            var deal = await _db.DealOfTheWeeks.ToListAsync();
            model.DealOfTheWeeks = deal;

        }
        catch (Exception ex)
        {


        }

        //     return RedirectToAction("Index","Products");
        return View(model);
    }
    [HttpGet]
    public IActionResult About()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult Shop(int page = 1, string sortOrder = null)
    {
        const int PageSize = 2;

        int totalItems = 0;
        int totalPages = 0;
        List<Product> products = new List<Product>();

        try
        {
            //var query = _db.Products.AsQueryable();
            var query = _db.Products
               .Where(p => p.IsActive == true)
               .AsQueryable();

            // Apply sorting
            if (!string.IsNullOrEmpty(sortOrder))
            {
                if (sortOrder == "LowToHigh")
                {
                    query = query.OrderBy(p => p.Price);
                }
                else if (sortOrder == "HighToLow")
                {
                    query = query.OrderByDescending(p => p.Price);
                }
            }

            // Count total items after sorting (for correct pagination)
            totalItems = query.Count();
            totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            // Apply pagination
            products = query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Fix image URLs
            foreach (var product in products)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    product.ImageUrl = "/uploads/" + product.ImageUrl;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while loading products in Shop action.");
            products = new List<Product>();
        }

        var productViewModels = new ProductListViewModel
        {
            Products = products,
            CurrentPage = page,
            TotalPages = totalPages
        };

        return View(productViewModels);
    }

    //public IActionResult Shop(int page = 1, string sortOrder = null)
    //{
    //    const int PageSize = 2;

    //    // Initialize variables
    //    int totalItems = 0;
    //    int totalPages = 0;
    //    List<Product> products = new List<Product>();

    //    try
    //    {
    //        // Start query
    //        var query = _db.Products.AsQueryable();

    //        // Apply sorting
    //        if (!string.IsNullOrEmpty(sortOrder))
    //        {
    //            if (sortOrder == "LowToHigh")
    //            {
    //                query = query.OrderBy(p => p.Price);
    //            }
    //            else if (sortOrder == "HighToLow")
    //            {
    //                query = query.OrderByDescending(p => p.Price);
    //            }
    //        }

    //        // Count total items after sorting (for correct pagination)
    //        totalItems = query.Count();
    //        totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

    //        // Get paged products
    //        products = query
    //            .Skip((page - 1) * PageSize)
    //            .Take(PageSize)
    //            .ToList();

    //        // Fix image URLs
    //        foreach (var product in products)
    //        {
    //            if (!string.IsNullOrEmpty(product.ImageUrl))
    //            {
    //                product.ImageUrl = "/uploads/" + product.ImageUrl;
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "An error occurred while loading products in Shop action.");
    //        products = new List<Product>();
    //    }

    //    // Map to ViewModel
    //    var productViewModels = new ProductListViewModel
    //    {
    //        Products = products,
    //        CurrentPage = page,
    //        TotalPages = totalPages,
    //        TotalProducts = totalItems
    //    };

    //    return View(productViewModels);
    //}


    //public IActionResult Shop(int page = 1)
    //{
    //    // Number of products per page
    //    const int PageSize = 2;

    //    // Get the total number of products (this is used to calculate total pages)
    //    int totalItems = _db.Products.Count();
    //    int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

    //    // Get the products for the current page
    //    List<Product> products = new List<Product>();

    //    try
    //    {
    //        products = _db.Products
    //            .Skip((page - 1) * PageSize) // Skip the previous pages' products
    //            .Take(PageSize) // Take only the products for the current page
    //            .ToList();

    //        // Adjust the image URLs if they are not empty
    //        foreach (var product in products)
    //        {
    //            if (!string.IsNullOrEmpty(product.ImageUrl))
    //            {
    //                product.ImageUrl = "/uploads/" + product.ImageUrl;
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "An error occurred while loading products in Shop action.");
    //        products = new List<Product>(); // Return an empty list on error
    //    }

    //    // Map products to ProductListViewModel
    //    var productViewModels = new ProductListViewModel
    //    {
    //        Products = products,   // List of products
    //        CurrentPage = page,    // Current page number
    //        TotalPages = totalPages // Total number of pages
    //    };

    //    // Return the view with the ProductListViewModel
    //    return View(productViewModels);
    //}


    //public IActionResult Shop(int page = 1)
    //{
    //    // Number of products per page
    //    const int PageSize = 9;

    //    // Get the total number of products (this is used to calculate total pages)
    //    int totalItems = _db.Products.Count();
    //    int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

    //    // Ensure the page number is valid
    //    if (page < 1) page = 1;
    //    if (page > totalPages && totalPages > 0) page = totalPages;

    //    // Get the products for the current page
    //    List<Product> products = new List<Product>();

    //    try
    //    {
    //        products = _db.Products
    //            .Skip((page - 1) * PageSize) // Skip the previous pages' products
    //            .Take(PageSize) // Take only the products for the current page
    //            .ToList();

    //        // Adjust the image URLs if they are not empty
    //        foreach (var product in products)
    //        {
    //            if (!string.IsNullOrEmpty(product.ImageUrl))
    //            {
    //                product.ImageUrl = "/uploads/" + product.ImageUrl;
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "An error occurred while loading products in Shop action.");
    //        products = new List<Product>(); // Return an empty list on error
    //    }

    //    // Map products to ProductViewModel
    //    var productViewModels = products.Select(product => new Product
    //    {
    //        Id = product.Id,
    //        Name = product.Name,
    //        Description = product.Description,
    //        Price = product.Price,
    //        DiscountedPrice = product.DiscountedPrice,
    //        ImageUrl = product.ImageUrl,
    //        Stock = product.Stock
    //    }).ToList();

    //    // Create the ProductListViewModel
    //    var viewModel = new ProductListViewModel
    //    {
    //        Products = productViewModels,
    //        CurrentPage = page,
    //        TotalPages = totalPages
    //    };

    //    // Return the view with the ProductListViewModel
    //    return View(viewModel);
    //}



    //public IActionResult Shop(int page = 1)
    //{
    //    // Number of products per page
    //    const int PageSize = 9;

    //    // Get the total number of products (this is used to calculate total pages)
    //    int totalItems = _db.Products.Count();
    //    int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

    //    // Ensure the page number is valid
    //    if (page < 1) page = 1;
    //    if (page > totalPages && totalPages > 0) page = totalPages;

    //    // Get the products for the current page
    //    List<Product> products = new List<Product>();

    //    try
    //    {
    //        products = _db.Products
    //            .Skip((page - 1) * PageSize) // Skip the previous pages' products
    //            .Take(PageSize) // Take only the products for the current page
    //            .ToList();

    //        // Adjust the image URLs if they are not empty
    //        foreach (var product in products)
    //        {
    //            if (!string.IsNullOrEmpty(product.ImageUrl))
    //            {
    //                product.ImageUrl = "/uploads/" + product.ImageUrl;
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "An error occurred while loading products in Shop action.");
    //        products = new List<Product>(); // Return an empty list on error
    //    }

    //    // Map products to ProductViewModel
    //    var productViewModels = products.Select(product => new Product
    //    {
    //        Id = product.Id,
    //        Name = product.Name,
    //        Description = product.Description,
    //        Price = product.Price,
    //        DiscountedPrice = product.DiscountedPrice,
    //        ImageUrl = product.ImageUrl,
    //        Stock = product.Stock
    //    }).ToList();

    //    // Create the ProductListViewModel
    //    var viewModel = new ProductListViewModel
    //    {
    //        Products = productViewModels,
    //        CurrentPage = page,
    //        TotalPages = totalPages
    //    };

    //    // Return the view with the ProductListViewModel
    //    return View(viewModel);
    //}
    //public IActionResult Shop(int page = 1)
    //{
    //    // Number of products per page
    //    const int PageSize = 9;

    //    // Get the total number of products (this is used to calculate total pages)
    //    int totalItems = _db.Products.Count();
    //    int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

    //    // Get the products for the current page
    //    List<Product> products = new List<Product>();

    //    try
    //    {
    //        products = _db.Products
    //            .Skip((page - 1) * PageSize) // Skip the previous pages' products
    //            .Take(PageSize) // Take only the products for the current page
    //            .ToList();

    //        // Adjust the image URLs if they are not empty
    //        foreach (var product in products)
    //        {
    //            if (!string.IsNullOrEmpty(product.ImageUrl))
    //            {
    //                product.ImageUrl = "/uploads/" + product.ImageUrl;
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "An error occurred while loading products in Shop action.");
    //        products = new List<Product>(); // Return an empty list on error
    //    }

    //    // Map products to ProductListViewModel
    //    List<ProductListViewModel> productViewModels = products.Select(product => new ProductListViewModel
    //    {
    //        Product = product.Id,
    //        Name = product.Name,
    //        ImageUrl = product.ImageUrl,
    //        Price = product.Price
    //    }).ToList();

    //    // Return the view with the list of ProductListViewModel
    //    return View(productViewModels);
    //}














    //[HttpGet]
    //public IActionResult Shop()
    //{


    //    List<Product> products = new List<Product>();

    //    try
    //    {
    //        products = _db.Products.ToList();

    //        foreach (var product in products)
    //        {
    //            if (!string.IsNullOrEmpty(product.ImageUrl))
    //            {
    //                product.ImageUrl = "/uploads/" + product.ImageUrl;
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "An error occurred while loading products in Shop action.");

    //        // Fallback to empty list if something went wrong
    //        products = new List<Product>();
    //    }

    //    return View(products);
    //}

    //[HttpGet]
    //public IActionResult Shop()
    //{
    //    List<Product> products = new List<Product>();

    //    try
    //    {
    //        products = _db.Products.ToList();


    //        foreach (var product in products)
    //        {
    //            if (!string.IsNullOrEmpty(product.ImageUrl))
    //            {
    //                product.ImageUrl = "/uploads/" + product.ImageUrl;
    //            }
    //        }
    //    }
    //    catch(Exception ex)
    //    {
    //        _logger.LogError(ex, "Error occurred while loading products.");

    //        // Optionally, you can show an error view or return empty list
    //        products = new List<Product>();
    //    }
    //    return View(products);
    //}



    //[HttpGet]
    //[Route("Home/Details/{productId}")]
    //public IActionResult Details(int productId)
    //{
    //    var product = _db.Products
    //                     .Include(p => p.productdetails)
    //                     .FirstOrDefault(p => p.Id == productId);

    //    if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
    //    {
    //        product.ImageUrl = "/uploads/" + product.ImageUrl;
    //    }
    //    //var moreproduct = _db.Products.ToList();
    //    if (product!=null&& product.productdetails != null)
    //    {



    //        foreach (var image in product.productdetails)
    //        {

    //                if (!string.IsNullOrEmpty(image.ImageUrl))
    //                {
    //                image.ImageUrl = "/uploads/" + image.ImageUrl;
    //                }
    //        }

    //        product.ProductSizes = _db.ProductSizes
    //            .Include(ps => ps.Size)
    //            .Where(ps => ps.ProductId == product.Id)
    //            .ToList();

    //    }

    //    return View(product);
    // }
    [HttpGet]
    [Route("Home/Details/{productId}")]
    public async Task <IActionResult> Details(int productId)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while fetching product details. ProductId: {ProductId}", productId);
            return BadRequest("Invalid request.");
        }

        try
        {
            var product = _db.Products
                             .Include(p => p.productdetails)
                             .FirstOrDefault(p => p.Id == productId);

            if (product == null)
            {
                _logger.LogWarning("Product not found for ProductId: {ProductId}", productId);
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                product.ImageUrl = "/uploads/" + product.ImageUrl;
            }

            if (product.productdetails != null)
            {
                foreach (var image in product.productdetails)
                {
                    if (!string.IsNullOrEmpty(image.ImageUrl))
                    {
                        image.ImageUrl = "/uploads/" + image.ImageUrl;
                    }
                }
            }

            product.ProductSizes = _db.ProductSizes
                .Include(ps => ps.Size)
                .Where(ps => ps.ProductId == product.Id)
                .ToList();
            ViewBag.productList =  await _db.ProductDetails.ToListAsync();


            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while loading details for ProductId: {ProductId}", productId);
            return View("Error"); // Redirects to a common Error view (you should create one)
        }
    }
    //public IActionResult Details(int productId)
    //{
    //    try
    //    {
    //        var product = _db.Products
    //                         .Include(p => p.productdetails)
    //                         .FirstOrDefault(p => p.Id == productId);

    //        if (product != null)
    //        {
    //            if (!string.IsNullOrEmpty(product.ImageUrl))
    //            {
    //                product.ImageUrl = "/uploads/" + product.ImageUrl;
    //            }

    //            if (product.productdetails != null)
    //            {
    //                foreach (var image in product.productdetails)
    //                {
    //                    if (!string.IsNullOrEmpty(image.ImageUrl))
    //                    {
    //                        image.ImageUrl = "/uploads/" + image.ImageUrl;
    //                    }
    //                }
    //            }

    //            product.ProductSizes = _db.ProductSizes
    //                .Include(ps => ps.Size)
    //                .Where(ps => ps.ProductId == product.Id)
    //                .ToList();
    //        }

    //        return View(product);
    //    }
    //    catch (Exception ex)
    //    {
    //        // Log the error (assuming you have a logger injected, if not, you should add one)
    //        //_logger.LogError(ex, $"Error loading details for productId: {productId}");

    //        Console.WriteLine($"Error loading product details: {ex.Message}"); // fallback logging

    //        // Option 1: Redirect to an error page
    //        // return RedirectToAction("Error", "Home");

    //        // Option 2: Return empty view with null model
    //        return View(null);
    //    }
    //}

    [HttpGet]
    public IActionResult ShopDetails()
    {
        return View();
    }
    /// <summary>
    /// / add to cart
    /// </summary>
    ///// <returns></returns>
    //[HttpPost]
    //public async Task<JsonResult> AddToCartAjax(int productId,string selectedsize)
    //{

    //    try
    //    {

    //        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
    //        if (product == null)
    //        {
    //            return Json(new { success = false, message = "Product not found." });
    //        }
    //        var cartItem = new CartItem
    //        {
    //            ProductId = productId,
    //            SizeName = selectedsize,
    //            Quantity = 1,
    //            Price = product.Price,
    //            // UserId = userId, //Only if you're tracking logged-in users
    //            AddedAt = DateTime.UtcNow
    //        };

    //        _db.CartItems.Add(cartItem);
    //        await _db.SaveChangesAsync();
    //    }
    //    catch (Exception ex) 
    //    {

    //    }

    //    return Json(new { success = true });
    //}
    //////////  AddtocartAjax shifted to gntroller
    [HttpPost]
    //public async Task<JsonResult> AddToCartAjax(int productId, string selectedSize)
    //{
    //    try
    //    {
    //        var userId =HttpContext.Session.GetString("UserId"); 

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
    //            userId=userId
               
    //        };

    //        await _db.CartItems.AddAsync(cartItem);
    //        await _db.SaveChangesAsync();
    //        //var cartItemCount = await _db.CartItems.CountAsync();
    //        var cartItemCount = await _db.CartItems
    //                          .Where(x => x.userId == userId)
    //                          .CountAsync();

    //        return Json(new { success = true, message = "Product added to cart.", cartItemCount = cartItemCount });
    //    }
    //    catch (Exception ex)
    //    {
           
    //        return Json(new { success = false, message = "An error occurred.", error = ex.Message });
    //    }
    //}

    [HttpGet]
    public IActionResult ShoppingCart()
    {
        return View();
    }
    [HttpGet]
    public IActionResult CheckOut()
    {
        return View();
    }
    [HttpGet]
    public IActionResult BlogDetails()
    {
        return View();
    }


    [HttpGet]
    public IActionResult Contact()
    {
        return View();
    }
    [HttpGet]
    public IActionResult Blog()
    {
        return View();
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    //[HttpGet]
    //public IActionResult ViewCart()
    //{


    //    return View();
    //}
}
