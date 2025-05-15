using HouseOfWani.Models;
using HouseOfWani.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using HouseOfWani.Models.Order;
namespace HouseOfWani.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AccountController> _logger;

        public AdminController(ILogger<AccountController> logger,IWebHostEnvironment webHostEnvironment, ApplicationDbContext context)
        {
            _context = context;
            _env = webHostEnvironment;
            _logger = logger;
        }

        //[HttpGet]
        //public async Task<IActionResult> UserIndex()
        //{
          
        //    return View();

        //}

        //    // GET: Products
        //    [HttpPost]
        //public async Task<IActionResult> UserIndex(List<IFormFile> files)
        //{
        //    try
        //    {
        //        var banner = await _context.BannerImages.FirstOrDefaultAsync();
        //        var uploadsFolderPath = Path.Combine(_env.WebRootPath, "Banner");
        //        Directory.CreateDirectory(uploadsFolderPath); // Ensure directory exists

        //        for (int i = 0; i < files.Count; i++)
        //        {
        //            var file = files[i];
        //            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
        //            var extension = Path.GetExtension(file.FileName);
        //            var uniqueFileName = $"{Guid.NewGuid()}_{fileNameWithoutExt}{extension}";
        //            var fullPath = Path.Combine(uploadsFolderPath, uniqueFileName);

        //            // Save the file to the server
        //            using (var fileStream = new FileStream(fullPath, FileMode.Create))
        //            {
        //                await file.CopyToAsync(fileStream);
        //            }

        //            // Optionally assign to banner properties
        //            if (i == 0) banner.SummerImage1 = uniqueFileName;  // Adjust according to your model
        //            if (i == 1) banner.SummerImage2 = uniqueFileName;  // Adjust according to your model
        //        }

        //        await _context.SaveChangesAsync();
        //        return RedirectToAction("UserIndex");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while retrieving the users for the Index page.");
        //        ModelState.AddModelError("", "An error occurred while loading the users. Please try again later.");
        //        return RedirectToAction("Index", "Home");
        //    }
           
        //}

            // GET: Products
            [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Fetch products with related ProductSizes and Size information
                var products = await _context.Products
                    .Include(p => p.ProductSizes)
                    .ThenInclude(ps => ps.Size)
                    .ToListAsync();

                // Process the products to include image paths
                foreach (var product in products)
                {
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        // Set the image URL to the correct path
                        product.ImageUrl = Path.Combine("/uploads", product.ImageUrl).Replace("\\", "/");
                    }
                }

                // Return the view with the list of products
                return View(products);
            }
            catch (Exception ex)
            {
                // Log the error with details of the exception
                _logger.LogError(ex, "An error occurred while retrieving the products for the Index page.");

                // Optionally add a user-friendly error message to the ModelState
                ModelState.AddModelError("", "An error occurred while loading the products. Please try again later.");

                // Return the view (or a fallback action, like redirecting to the Home page)
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                // Check if the id is null
                if (id == null)
                {
                    return NotFound();
                }

                // Fetch the product with the given id
                var product = await _context.Products
                    .FirstOrDefaultAsync(m => m.Id == id);

                // If the product is not found, return NotFound()
                if (product == null)
                {
                    return NotFound();
                }

                // Return the view with the product details
                return View(product);
            }
            catch (Exception ex)
            {
                // Log the error with the exception details
                _logger.LogError(ex, "An error occurred while retrieving the product details for id: {ProductId}", id);

                // Optionally, add a user-friendly error message to the ModelState
                ModelState.AddModelError("", "An error occurred while loading the product details. Please try again later.");

                // Return a fallback view, such as the Home page or an error page
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                // Fetch available sizes for the dropdown
                ViewBag.sizes = new SelectList(_context.Sizes, "Id", "Label");

                // Return the view to create a new product
                return View();
            }
            catch (Exception ex)
            {
                // Log the exception with details
                _logger.LogError(ex, "An error occurred while loading the sizes for the Create page.");

                // Optionally, add a user-friendly error message to the ModelState
                ModelState.AddModelError("", "An error occurred while loading the page. Please try again later.");

                // Return a fallback view or redirect to a safe page
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile file, int[] selectedSizes)
        {
            try
            {
                // Ensure the model state is valid
                //if (!ModelState.IsValid)
                //{
                //    return View(product);
                //}

                // Extract file information
                var fileNameOnly = Path.GetFileNameWithoutExtension(file.FileName); // Extract file name without extension
                var extension = Path.GetExtension(file.FileName);                  // Extract file extension
                var uniqueFileName = $"{Guid.NewGuid()}_{fileNameOnly}{extension}"; // Create a unique name

                var uploadsFolderPath = Path.Combine(_env.WebRootPath, "uploads"); // Path to the uploads folder
                var fullPath = Path.Combine(uploadsFolderPath, uniqueFileName);    // Full path for saving the file

                // Ensure the uploads directory exists
                if (!Directory.Exists(uploadsFolderPath))
                {
                    Directory.CreateDirectory(uploadsFolderPath);
                }

                // Save the file to the server
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Add the product to the context
                _context.Add(product);
                await _context.SaveChangesAsync();

                // Process selected sizes and add them to the ProductSize table
                if (selectedSizes != null)
                {
                    foreach (var sizeId in selectedSizes)
                    {
                        var productSize = new ProductSize
                        {
                            ProductId = product.Id,
                            SizeId = sizeId
                        };
                        _context.Add(productSize);
                    }
                }

                // Save the ProductSize entries to the database
                await _context.SaveChangesAsync();

                // Store the relative image URL in the product record
                product.ImageUrl = uniqueFileName;

                // Update the product record
                _context.Update(product);
                await _context.SaveChangesAsync();

                // Redirect to the Index action
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception details
                _logger.LogError(ex, "An error occurred while creating a new product.");

                // Optionally, add a user-friendly error message to the ModelState
                ModelState.AddModelError("", "An error occurred while creating the product. Please try again later.");

                // Return the view with the product data so that the user can attempt again
                return View(product);
            }
        }


       
        public async Task<IActionResult> Sizes()
        {
            try
            {
                var sizes = await _context.Sizes.ToListAsync();
                return View(sizes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the list of sizes.");

                // Optionally, you can show a friendly message to the user
                ModelState.AddModelError("", "An error occurred while loading sizes. Please try again later.");

                // Redirect to a safe page like Index or Home
                return RedirectToAction("Index", "Home");
            }
        }


        // GET: Admin/Sizes/Create
       
        [HttpGet]
        public IActionResult CreateSize()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the CreateSize page.");

                // Optionally show a friendly message to the user
                ModelState.AddModelError("", "An error occurred while loading the page. Please try again later.");

                // Redirect to a safe page like Index or Home
                return RedirectToAction("Index", "Home");
            }
        }


        // POST: Admin/Sizes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSize(List<string> sizes)
        {
            try
            {
                if (sizes == null || sizes.Count == 0)
                {
                    // Handle empty or null list, maybe return an error or show a message
                    TempData["Error"] = "No sizes provided!";
                    return RedirectToAction(nameof(Index));
                }

                // Create Size entities from the provided size list
                var sizeEntities = sizes.Select(size => new Size { Label = size }).ToList();

                // Add the sizes to the database
                _context.Sizes.AddRange(sizeEntities);
                await _context.SaveChangesAsync();

                // Redirect back to the Index action after successful creation
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception details with context information
                _logger.LogError(ex, "An error occurred while creating sizes.");

                // Optionally, add a user-friendly error message to TempData or ModelState
                TempData["Error"] = "An error occurred while creating sizes. Please try again later.";

                // Redirect to the Index page in case of error
                return RedirectToAction(nameof(Index));
            }
        }

     
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                // Fetch the product, including its sizes
                var product = await _context.Products
                    .Include(p => p.ProductSizes)
                    .ThenInclude(ps => ps.Size)
                    .FirstOrDefaultAsync(m => m.Id == id);

                // If no product is found, return NotFound
                if (product == null)
                {
                    return NotFound();
                }

                // Set the image URL path correctly
                product.ImageUrl = Url.Content($"~/uploads/{product.ImageUrl}");

                // Pass available sizes to the view
                ViewBag.Sizes = _context.Sizes.ToList();

                // Return the view with the product
                return View(product);
            }
            catch (Exception ex)
            {
                // Log the exception with details about the error
                _logger.LogError(ex, "An error occurred while trying to edit product with ID: {ProductId}", id);

                // Optionally, add a user-friendly error message
                TempData["Error"] = "An error occurred while trying to edit the product. Please try again later.";

                // Redirect to the Index page or any safe fallback route
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl,Stock,CountryId,CurrencyId,ProductSizes,DiscountedPrice,IsActive")] Product product, IFormFile file, List<int> selectedSizes)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            try
            {
                var oldProduct = await _context.Products
                    .Include(p => p.ProductSizes)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (oldProduct == null)
                {
                    return NotFound();
                }

                // Update fields
                oldProduct.Name = product.Name;
                oldProduct.Description = product.Description;
                oldProduct.Price = product.Price;
                oldProduct.Qauntity = product.Qauntity;
                oldProduct.CountryId = product.CountryId;
                oldProduct.CurrencyId = product.CurrencyId;
                oldProduct.DiscountedPrice = product.DiscountedPrice;
                oldProduct.IsActive = product.IsActive;

                // Handle image update
                if (file != null && file.Length > 0)
                {
                    var existingImageName = Path.GetFileName(oldProduct.ImageUrl);
                    var newFileName = Path.GetFileName(file.FileName);

                    if (!string.Equals(existingImageName, newFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Delete old image
                        if (!string.IsNullOrEmpty(oldProduct.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_env.WebRootPath, "uploads", oldProduct.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save new image
                        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                        var extension = Path.GetExtension(file.FileName);
                        var uniqueFileName = $"{Guid.NewGuid()}_{fileNameWithoutExt}{extension}";

                        var uploadsFolderPath = Path.Combine(_env.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolderPath))
                        {
                            Directory.CreateDirectory(uploadsFolderPath);
                        }

                        var fullPath = Path.Combine(uploadsFolderPath, uniqueFileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        oldProduct.ImageUrl = uniqueFileName;
                    }
                }

                // Handle product sizes
                var existingSizes = await _context.ProductSizes
                    .Where(ps => ps.ProductId == id)
                    .ToListAsync();  // ❗ Added ToListAsync()

                _context.ProductSizes.RemoveRange(existingSizes);

                if (selectedSizes != null && selectedSizes.Any())
                {
                    foreach (var sizeId in selectedSizes)
                    {
                        _context.ProductSizes.Add(new ProductSize
                        {
                            ProductId = id,
                            SizeId = sizeId
                        });
                    }
                }

                // Save changes
                _context.Update(oldProduct);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating product with ID: {ProductId}", product.Id);

                if (!ProductExists(product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while editing product with ID: {ProductId}", product.Id);

                TempData["Error"] = "An error occurred while editing the product. Please try again later.";

                // ⚡ Important: repopulate ViewBag.Sizes when returning view
                ViewBag.Sizes = new SelectList(_context.Sizes, "Id", "Label", selectedSizes);

                return View(product);
            }
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl,Stock,CountryId,CurrencyId,ProductSizes,DiscountedPrice")] Product product, IFormFile file, List<int> selectedSizes)
        //{
        //    if (id != product.Id)
        //    {
        //        return NotFound();
        //    }

        //    try
        //    {
        //        // Fetch the existing product from the database
        //        var oldProduct = await _context.Products
        //            .Include(p => p.ProductSizes)
        //            .FirstOrDefaultAsync(p => p.Id == id);

        //        // If the product is not found, return NotFound
        //        if (oldProduct == null)
        //        {
        //            return NotFound();
        //        }

        //        // Update basic fields of the product
        //        oldProduct.Name = product.Name;
        //        oldProduct.Description = product.Description;
        //        oldProduct.Price = product.Price;
        //        oldProduct.Stock = product.Stock;
        //        oldProduct.CountryId = product.CountryId;
        //        oldProduct.CurrencyId = product.CurrencyId;
        //        oldProduct.DiscountedPrice = product.DiscountedPrice;

        //        // Handle image file upload if a new file is provided
        //        if (file != null && file.Length > 0)
        //        {
        //            var fileName = Path.GetFileName(file.FileName);

        //            // Compare existing image name with new file name
        //            var existingImageName = Path.GetFileName(oldProduct.ImageUrl);

        //            if (!string.Equals(fileName, existingImageName, StringComparison.OrdinalIgnoreCase))
        //            {
        //                // Delete old image if it exists
        //                if (!string.IsNullOrEmpty(oldProduct.ImageUrl))
        //                {
        //                    var folderPath = Path.Combine(_env.WebRootPath, "uploads");
        //                    var oldImagePath = Path.Combine(folderPath, oldProduct.ImageUrl.TrimStart('/'));
        //                    if (System.IO.File.Exists(oldImagePath))
        //                    {
        //                        System.IO.File.Delete(oldImagePath);
        //                    }
        //                }

        //                // Generate a unique name for the new image
        //                var fileNameOnly = Path.GetFileNameWithoutExtension(file.FileName);
        //                var extension = Path.GetExtension(file.FileName);
        //                var uniqueFileName = $"{Guid.NewGuid()}_{fileNameOnly}{extension}";

        //                var uploadsFolderPath = Path.Combine(_env.WebRootPath, "uploads");
        //                var fullPath = Path.Combine(uploadsFolderPath, uniqueFileName);

        //                // Ensure the uploads folder exists
        //                if (!Directory.Exists(uploadsFolderPath))
        //                {
        //                    Directory.CreateDirectory(uploadsFolderPath);
        //                }

        //                // Save the new image
        //                using (var stream = new FileStream(fullPath, FileMode.Create))
        //                {
        //                    await file.CopyToAsync(stream);
        //                }

        //                // Update the product's image URL
        //                oldProduct.ImageUrl = uniqueFileName;
        //            }
        //        }

        //        // Update product sizes
        //        var existingSizes = _context.ProductSizes.Where(ps => ps.ProductId == id);
        //        _context.ProductSizes.RemoveRange(existingSizes);

        //        if (selectedSizes != null && selectedSizes.Any())
        //        {
        //            foreach (var sizeId in selectedSizes)
        //            {
        //                _context.ProductSizes.Add(new ProductSize
        //                {
        //                    ProductId = id,
        //                    SizeId = sizeId
        //                });
        //            }
        //        }

        //        // Save changes to the database
        //        _context.Update(oldProduct);
        //        await _context.SaveChangesAsync();

        //        // Redirect to the product list (or another appropriate page)
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        // Log concurrency issues
        //        _logger.LogError(ex, "A concurrency error occurred while updating product with ID: {ProductId}", product.Id);

        //        // Check if the product still exists
        //        if (!ProductExists(product.Id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;  // Rethrow the exception if not handled
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log general exceptions
        //        _logger.LogError(ex, "An unexpected error occurred while editing the product with ID: {ProductId}", product.Id);

        //        // Add a friendly error message
        //        TempData["Error"] = "An error occurred while editing the product. Please try again later.";

        //        // Return to the view with the product data
        //        ViewBag.Sizes = new SelectList(_context.Sizes, "Id", "Label", selectedSizes);
        //        return View(product);
        //    }
        //}

        private bool ProductExists(int id)
        {
            try
            {
                return _context.Products.Any(e => e.Id == id);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while checking if product exists with ID: {ProductId}", id);

                // Optionally, return false or handle the error as needed
                return false;
            }
        }


        [HttpGet]
        public IActionResult ProductDetail(int Id)
        {
            try
            {
                // You can add any logic related to the product detail here if needed
                // For example, fetching product details from the database (if required)

                ViewBag.Id = Id;  // Passing the Id to the view

                return View();
            }
            catch (Exception ex)
            {
                // Log the exception with relevant details
                _logger.LogError(ex, "An error occurred while loading product details for product ID: {ProductId}", Id);

                // Optional: Provide a user-friendly message
                TempData["Error"] = "An error occurred while loading the product details. Please try again later.";

                // Fallback: Redirect to a safe page (e.g., Home page or Index)
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public async Task<IActionResult> ProductDetail(int productId, List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    ModelState.AddModelError("files", "Please select at least one file to upload.");
                    return View(); // Or return with the model if you need to
                }

                var uploadPath = Path.Combine(_env.WebRootPath, "uploads");

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + file.FileName;
                        var filePath = Path.Combine(uploadPath, fileName);

                        // Ensure upload directory exists
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var productDetail = new ProductDetail
                        {
                            ProductId = productId,
                            ImageUrl = fileName
                        };

                        _context.ProductDetails.Add(productDetail);
                    }
                }

                // Save to the database
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the error with the logger
                _logger.LogError(ex, "An error occurred while uploading files for product ID: {ProductId}", productId);

                // Optionally, set an error message to TempData for user feedback
                TempData["Error"] = "An error occurred while uploading the files. Please try again later.";

                // Return to the view or redirect as needed
                return View("Error");
            }
        }
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                // Log the error with the logger
                _logger.LogError(ex, "An error occurred while fetching the product for deletion with ID: {ProductId}", id);

                // Optionally, you can display a friendly error message to the user
                TempData["Error"] = "An error occurred while trying to fetch the product details. Please try again later.";

                // Return to a safe page or redirect
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var product = await _context.Products.Include(p => p.ProductSizes).FirstOrDefaultAsync(p => p.Id == id);

                if (product != null)
                {
                    if (!String.IsNullOrEmpty(product.ImageUrl))
                    {
                        var ImageName = product.ImageUrl.TrimStart('/');
                        var oldImagePath = Path.Combine(_env.WebRootPath, "uploads", ImageName);

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath); // Delete the old image
                        }

                        // Remove associated product sizes
                        foreach (var productSize in product.ProductSizes)
                        {
                            _context.Remove(productSize);
                        }
                    }

                    // Remove the product
                    _context.Products.Remove(product);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the error with the logger
                _logger.LogError(ex, "An error occurred while deleting the product with ID: {ProductId}", id);

                // Optionally, you can display a friendly error message to the user
                TempData["Error"] = "An error occurred while trying to delete the product. Please try again later.";

                // Return to a safe page or redirect
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult AddCountryWithCurrency()
        {
            try
            {
                var model = new CountryCurrencyViewModel();
                return View(model);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while loading the AddCountryWithCurrency view.");

                // Optionally, you can add an error message for the user or return an error view
                TempData["ErrorMessage"] = "An unexpected error occurred while loading the page. Please try again later.";

                // Return a fallback view, for example, the Index view or an error page
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Country(int id)
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the Country view for Id: {Id}", id);

                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpGet]
        public IActionResult Currency(int id)
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the Currency view for Id: {Id}", id);

                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpPost]
        public IActionResult Country(Country country)
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the Country form submission for Country: {@Country}", country);

                TempData["ErrorMessage"] = "An unexpected error occurred while processing your request. Please try again later.";
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public IActionResult Currency(Currency currency)
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the Currency form submission for Currency: {@Currency}", currency);

                TempData["ErrorMessage"] = "An unexpected error occurred while processing your request. Please try again later.";
                return RedirectToAction("Index", "Home");
            }
        }
        //[HttpGet]
        //public IActionResult CreateSize()
        //{
        //    return View();
        //}


        // GET: Admin/Sizes
        //public async Task<IActionResult> Sizes()
        //{
        //    var sizes = await _context.Sizes.ToListAsync();
        //    return View(sizes);
        //}
        //[HttpGet]
        //public IActionResult ProductDetail(int Id) 
        //{
        //    //var model = new ProductDetail
        //    //{
        //    //    ProductId = Id // Setting the FK to the product you're adding images for
        //    //};
        //    ViewBag.Id = Id;
        //    return View();

        //}
        //[HttpPost]
        //public async Task<IActionResult> ProductDetail(int productId, List<IFormFile> files)
        //{

        //    if (files == null || files.Count == 0)
        //    {
        //        ModelState.AddModelError("files", "Please select at least one file to upload.");
        //        return View(); // Or return with the model if you need to
        //    }
        //    var uploadPath = Path.Combine(_env.WebRootPath, "uploads");

        //    foreach (var file in files)
        //    {
        //        if (file.Length > 0)
        //        {
        //            var fileName = Guid.NewGuid() + file.FileName;
        //            var filePath = Path.Combine(uploadPath, fileName);

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await file.CopyToAsync(stream);
        //            }

        //            var productDetail = new ProductDetail
        //            {
        //                ProductId = productId,
        //                ImageUrl=fileName
        //            };

        //            _context.ProductDetails.Add(productDetail);


        //            // Save path and productId to DB if needed

        //        }
        //    }
        //    try
        //    {
        //        await _context.SaveChangesAsync();

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error: {ex.Message}");
        //        return View("Error");
        //    }

        //    return RedirectToAction("Index");
        //}
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl,Stock,CountryId,CurrencyId,ProductSizes")] Product product, IFormFile file, List<int> selectedSizes)
        //{
        //    if (id != product.Id)
        //    {
        //        return NotFound();
        //    }

        //    //if (ModelState.IsValid)
        //    //{
        //    try
        //    { 
        //        //   var currentProductSizes = _context.ProductSizes.Where(ps => ps.ProductId ==4).ToList();

        //        var currentProductSizes = _context.ProductSizes
        //                               ?.Where(ps => ps.ProductId == product.Id)
        //                               .ToList();
        //        var uniqueFileName = file.FileName;
        //        var oldproduct = _context.Products.Find(id);
        //        if (oldproduct == null)
        //        {
        //            return NotFound();
        //        }
        //        if (file != null)
        //        {

        //            if (oldproduct.ImageUrl != uniqueFileName)
        //            {


        //                //var oldImageName = Path.GetFileName(product.ImageUrl); // Get the old image name
        //                var ImageName = oldproduct.ImageUrl.TrimStart('/');
        //                var oldImagePath = Path.Combine(_env.WebRootPath,"uploads");
        //                var fullPath = Path.Combine(oldImagePath, ImageName);

        //                if (System.IO.File.Exists(fullPath))
        //                {
        //                    System.IO.File.Delete(fullPath); // Delete the old image
        //                }
        //                var fileNameOnly = Path.GetFileNameWithoutExtension(file.FileName); // Extract file name without extension
        //                var extension = Path.GetExtension(file.FileName);                  // Extract file extension
        //                var uploadsFolderPath = Path.Combine(_env.WebRootPath, "uploads"); // Path to the uploads folder
        //                 fullPath = Path.Combine(uploadsFolderPath, uniqueFileName);    // Full path for saving the file
        //                                                                                   // Save the file to the server
        //                using (var fileStream = new FileStream(fullPath, FileMode.Create))
        //                {
        //                    await file.CopyToAsync(fileStream);
        //                }
        //                _context.Add(product);
        //                await _context.();
        //                foreach (var productSize in product.ProductSizes)
        //                {
        //                    _context.Remove(productSize);
        //                }



        //                if (selectedSizes != null)
        //                {
        //                    foreach (var sizeId in selectedSizes)
        //                    {
        //                        var productSize = new ProductSize
        //                        {
        //                            ProductId = product.Id,
        //                            SizeId = sizeId
        //                        };
        //                        _context.Add(productSize);
        //                    }
        //                }
        //            }
        //            else
        //            {

        //            }
        //        }
        //    }


        //    //    var fileNameOnly = Path.GetFileNameWithoutExtension(file.FileName); // Extract file name without extension
        //    //        var extension = Path.GetExtension(file.FileName);                  // Extract file extension
        //    //        var uniqueFileName = $"{Guid.NewGuid()}_{fileNameOnly}{extension}"; // Create a unique name
        //    //        var uploadsFolderPath = Path.Combine(_env.WebRootPath, "uploads"); // Path to the uploads folder
        //    //        var fullPath = Path.Combine(uploadsFolderPath, uniqueFileName);    // Full path for saving the file

        //    //// Ensure the uploads directory exists
        //    //if (!Directory.Exists(uploadsFolderPath))
        //    //{
        //    //    Directory.CreateDirectory(uploadsFolderPath);
        //    //}

        //    //// Save the file to the server
        //    //using (var fileStream = new FileStream(fullPath, FileMode.Create))
        //    //{
        //    //    await file.CopyToAsync(fileStream);
        //    //}
        //    //_context.Add(product);
        //    //await _context.SaveChangesAsync();


        //    //foreach (var productSize in currentProductSizes)
        //    //    {
        //    //        _context.ProductSizes.Remove(productSize);
        //    //    }

        //    //    // Add new sizes
        //    //    foreach (var sizeId in selectedSizes)
        //    //    {
        //    //        var productSize = new ProductSize
        //    //        {
        //    //            ProductId = product.Id,
        //    //            SizeId = sizeId
        //    //        };
        //    //        _context.ProductSizes.Add(productSize);
        //    //    }

        //    //    _context.Update(product);
        //    //    await _context.SaveChangesAsync();
        //    //}
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ProductExists(product.Id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //        return RedirectToAction(nameof(Index));
        //   // }
        //    ViewBag.Sizes = new SelectList(_context.Sizes, "Id", "Label", selectedSizes);
        //    return View(product);
        //}

        //// GET: Products/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var product = await _context.Products
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(product);
        //}

        // POST: Products/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var product = await _context.Products.Include(p => p.ProductSizes).FirstOrDefaultAsync(p => p.Id == id);

        //    //var product = await _context.Products.FindAsync(id);
        //    if (product != null)
        //    {
        //        if (!String.IsNullOrEmpty(product.ImageUrl))
        //        {
        //            //var oldImageName = Path.GetFileName(product.ImageUrl); // Get the old image name
        //            var ImageName = product.ImageUrl.TrimStart('/');
        //            //var oldImagePath = Path.Combine(_env.WebRootPath, ImageName);

        //            var oldImagePath = Path.Combine(_env.WebRootPath, "uploads", ImageName);

        //            if (System.IO.File.Exists(oldImagePath))
        //            {
        //                System.IO.File.Delete(oldImagePath); // Delete the old image
        //            }
        //            foreach (var productSize in product.ProductSizes)
        //            {
        //                _context.Remove(productSize);
        //            }
        //        }
        //        _context.Products.Remove(product);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool ProductExists(int id)
        //{
        //    return _context.Products.Any(e => e.Id == id);
        //}

        //[HttpGet]
        //public IActionResult Country(int id)
        //{
        //    return View();
        //}
        //[HttpPost]
        //public IActionResult Country(Country country )
        //{
        //    return View();
        //}
        //[HttpGet]
        //public IActionResult AddCountryWithCurrency()
        //{
        //    var model = new CountryCurrencyViewModel();
        //    return View(model);
        //}

        //[HttpGet]
        //public IActionResult Currency(int id)
        //{
        //    return View();
        //}

        //[HttpPost]
        //public IActionResult Currency(Currency currency)
        //{
        //    return View();
        //}
        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl,Stock,CountryId,CurrencyId,ProductSizes,DiscountedPrice")] Product product, IFormFile file, List<int> selectedSizes)
        //{
        //    if (id != product.Id)
        //    {
        //        return NotFound();
        //    }

        //    try
        //    {
        //        var oldProduct = await _context.Products
        //            .Include(p => p.ProductSizes)
        //            .FirstOrDefaultAsync(p => p.Id == id);

        //        if (oldProduct == null)
        //        {
        //            return NotFound();
        //        }

        //        // Update basic fields
        //        oldProduct.Name = product.Name;
        //        oldProduct.Description = product.Description;
        //        oldProduct.Price = product.Price;
        //        oldProduct.Stock = product.Stock;
        //        oldProduct.CountryId = product.CountryId;
        //        oldProduct.CurrencyId = product.CurrencyId;
        //        oldProduct.DiscountedPrice = product.DiscountedPrice;

        //        if (file != null && file.Length > 0)
        //        {
        //            var fileName = Path.GetFileName(file.FileName);

        //            // Compare existing image name with new file name
        //            var existingImageName = Path.GetFileName(oldProduct.ImageUrl);

        //            if (!string.Equals(fileName, existingImageName, StringComparison.OrdinalIgnoreCase))
        //            {
        //                // Delete old image if it exists
        //                if (!string.IsNullOrEmpty(oldProduct.ImageUrl))
        //                {

        //                    var FolderPath = Path.Combine(_env.WebRootPath, "uploads");
        //                    var oldImagePath = Path.Combine(FolderPath, oldProduct.ImageUrl.TrimStart('/'));
        //                    if (System.IO.File.Exists(oldImagePath))
        //                    {
        //                        System.IO.File.Delete(oldImagePath);
        //                    }
        //                }
        //                var fileNameOnly = Path.GetFileNameWithoutExtension(file.FileName); // Extract file name without extension
        //                var extension = Path.GetExtension(file.FileName);                  // Extract file extension
        //                var uniqueFileName = $"{Guid.NewGuid()}_{fileNameOnly}{extension}"; // Create a unique name

        //                var uploadsFolderPath = Path.Combine(_env.WebRootPath, "uploads"); // Path to the uploads folder
        //                var fullPath = Path.Combine(uploadsFolderPath, uniqueFileName);    // Full path for saving the file

        //                // Save new image
        //                //var uploadsFolderPath = Path.Combine(_env.WebRootPath, "uploads");

        //                if (!Directory.Exists(uploadsFolderPath))
        //                {
        //                    Directory.CreateDirectory(uploadsFolderPath);
        //                }

        //               // var newImagePath = Path.Combine(uploadsFolderPath, fileName);
        //                using (var stream = new FileStream(fullPath, FileMode.Create))
        //                {
        //                    await file.CopyToAsync(stream);
        //                }

        //                oldProduct.ImageUrl = uniqueFileName;
        //            }
        //        }

        //        // Update product sizes
        //        var existingSizes = _context.ProductSizes.Where(ps => ps.ProductId == id);
        //        _context.ProductSizes.RemoveRange(existingSizes);

        //        if (selectedSizes != null && selectedSizes.Any())
        //        {
        //            foreach (var sizeId in selectedSizes)
        //            {
        //                _context.ProductSizes.Add(new ProductSize
        //                {
        //                    ProductId = id,
        //                    SizeId = sizeId
        //                });
        //            }
        //        }

        //        _context.Update(oldProduct);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ProductExists(product.Id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    ViewBag.Sizes = new SelectList(_context.Sizes, "Id", "Label", selectedSizes);
        //    return View(product);
        //}
        //// add multiple image for product id
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        //public async Task<IActionResult> CreateSize(List<string> sizes)
        //{
        //    if (sizes == null || sizes.Count == 0)
        //    {
        //        // Handle empty or null list, maybe return an error or show a message
        //        TempData["Error"] = "No sizes provided!";
        //        return RedirectToAction(nameof(Index));
        //    }
        //    //if (ModelState.IsValid)
        //    //{
        //    //foreach (var sizes in Sizes)
        //    //{
        //    //    _context.Sizes.Add(sizes);
        //    //    await _context.SaveChangesAsync();

        //    //}
        //    var sizeEntities = sizes.Select(size => new Size { Label = size }).ToList();
        //    _context.Sizes.AddRange(sizeEntities);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //    //}
        //    //return View(size);
        //}
        // GET: Products/Edit/5
        //[HttpGet]
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    //var product = await _context.Products.FindAsync(id);
        //    var product = await _context.Products
        //   .Include(p => p.ProductSizes)
        //   .ThenInclude(ps => ps.Size)
        //   .FirstOrDefaultAsync(m => m.Id == id);
        //    product.ImageUrl = Url.Content($"~/uploads/{product.ImageUrl}");

        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    //ViewData["Sizes"] = new SelectList(_context.Sizes, "Id", "Label", product.ProductSizes.Select(ps => ps.SizeId).ToList());
        //    //ViewBag.Sizes = new SelectList(_context.Sizes.ToList(), "Id", "Label");
        //    ViewBag.Sizes = _context.Sizes.ToList();
        //    return View(product);
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(Product product, IFormFile file, int[] selectedSizes)
        //{
        //    //if (ModelState.IsValid)
        //    //{
        //    var fileNameOnly = Path.GetFileNameWithoutExtension(file.FileName); // Extract file name without extension
        //    var extension = Path.GetExtension(file.FileName);                  // Extract file extension
        //    var uniqueFileName = $"{Guid.NewGuid()}_{fileNameOnly}{extension}"; // Create a unique name

        //    var uploadsFolderPath = Path.Combine(_env.WebRootPath, "uploads"); // Path to the uploads folder
        //    var fullPath = Path.Combine(uploadsFolderPath, uniqueFileName);    // Full path for saving the file

        //    // Ensure the uploads directory exists
        //    if (!Directory.Exists(uploadsFolderPath))
        //    {
        //        Directory.CreateDirectory(uploadsFolderPath);
        //    }

        //    // Save the file to the server
        //    using (var fileStream = new FileStream(fullPath, FileMode.Create))
        //    {
        //        await file.CopyToAsync(fileStream);
        //    }
        //    _context.Add(product);
        //    await _context.SaveChangesAsync();
        //    if (selectedSizes != null)
        //    {

        //        foreach (var sizeId in selectedSizes)
        //        {
        //            var productSize = new ProductSize
        //            {
        //                ProductId = product.Id,
        //                SizeId = sizeId
        //            };
        //            _context.Add(productSize);
        //        }
        //    }
        //    await _context.SaveChangesAsync();

        //    // Store the relative image URL in the database
        //    // product.ImageUrl = $"/uploads/{uniqueFileName}";
        //    product.ImageUrl = uniqueFileName;

        //    _context.Update(product);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //    //}
        //    // return View(product);
        //}
    }
}
