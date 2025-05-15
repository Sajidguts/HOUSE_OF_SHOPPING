using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HouseOfWani.Models;
using HouseOfWani.Models.Order;
using HouseOfWani.Models.Admin;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.IdentityModel.Logging;

namespace HouseOfWani.Controllers
{
    public class BannerImagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AccountController> _logger;

        public BannerImagesController(ILogger<AccountController> logger, IWebHostEnvironment webHostEnvironment, ApplicationDbContext context)
        {
            _context = context;
            _env = webHostEnvironment;
            _logger = logger;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBlog(int id, BlogPost post, IFormFile file)
        {
            if (id != post.Id)
                return NotFound();

            try
            {
                var existingPost = await _context.BlogPosts.FirstOrDefaultAsync(p => p.Id == id);
                if (existingPost == null)
                    return NotFound();

                // Update text fields
                existingPost.Title = post.Title;
                existingPost.Summary = post.Summary;
                existingPost.Date = DateTime.Now; // Optional: update to current
                existingPost.Slug = SlugHelper.ToSlug(post.Title);
                existingPost.ReadMoreUrl = "/Blog/" + existingPost.Slug;

                // Handle image update
                if (file != null && file.Length > 0)
                {
                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(existingPost.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_env.WebRootPath, "Blog", existingPost.ImageUrl);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save new image safely with unique name
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                    var extension = Path.GetExtension(file.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{fileNameWithoutExt}{extension}";

                    var imageFolder = Path.Combine(_env.WebRootPath, "Blog");
                    if (!Directory.Exists(imageFolder))
                    {
                        Directory.CreateDirectory(imageFolder);
                    }

                    var fullPath = Path.Combine(imageFolder, uniqueFileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    existingPost.ImageUrl = uniqueFileName;
                }


                _context.Update(existingPost);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(BlogIndex));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating blog post with ID: {BlogPostId}", post.Id);

                if (!_context.BlogPosts.Any(p => p.Id == id))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while editing blog post with ID: {BlogPostId}", post.Id);
                TempData["Error"] = "An error occurred while updating the blog post. Please try again later.";
                return View(post);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AltText,Title,Description,IsActive")] BannerImage bannerImage, IFormFile file)
        {
            if (id != bannerImage.Id)
            {
                return NotFound();
            }

            try
            {
                var oldBanner = await _context.BannerImages.FirstOrDefaultAsync(b => b.Id == id);
                if (oldBanner == null)
                {
                    return NotFound();
                }

                // Update text fields
                oldBanner.AltText = bannerImage.AltText;
                oldBanner.Title = bannerImage.Title;
                oldBanner.Description = bannerImage.Description;
                oldBanner.IsActive = bannerImage.IsActive;

                // Handle image update
                if (file != null && file.Length > 0)
                {
                    var oldImageName = Path.GetFileName(oldBanner.SummerImage1);
                    var newFileName = Path.GetFileName(file.FileName);

                    if (!string.Equals(oldImageName, newFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Delete old image
                        if (!string.IsNullOrEmpty(oldBanner.SummerImage1))
                        {
                            var oldImagePath = Path.Combine(_env.WebRootPath, "Banner", oldBanner.SummerImage1);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save new image
                        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                        var extension = Path.GetExtension(file.FileName);
                        var uniqueFileName = $"{Guid.NewGuid()}_{fileNameWithoutExt}{extension}";

                        var bannerFolder = Path.Combine(_env.WebRootPath, "Banner");
                        if (!Directory.Exists(bannerFolder))
                        {
                            Directory.CreateDirectory(bannerFolder);
                        }

                        var fullPath = Path.Combine(bannerFolder, uniqueFileName);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        oldBanner.SummerImage1 = uniqueFileName;
                    }
                }

                _context.Update(oldBanner);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating banner image with ID: {BannerId}", bannerImage.Id);

                if (!BannerImageExists(bannerImage.Id))
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
                _logger.LogError(ex, "Unexpected error while editing banner image with ID: {BannerId}", bannerImage.Id);
                TempData["Error"] = "An error occurred while editing the banner image. Please try again later.";
                return View(bannerImage);
            }
        }

        //  blog index start//
        public async Task<IActionResult> BlogIndex()
        {
            try
            {
                var posts = await _context.BlogPosts.ToListAsync();

                //foreach (var post in posts)
                //{
                //    if (!string.IsNullOrEmpty(post.ImageUrl))
                //    {
                //         post.ImageUrl = Path.Combine("/Blog/", post.ImageUrl).Replace("\\", "/");
                //        //post.ImageUrl = "/Blog/" + post.ImageUrl.TrimStart('/');
                //        //post.ImageUrl = $"/Blog/{post.ImageUrl}";
                //    }
                //}

                return View(posts);
            }
            catch (Exception ex)
            {
                // Optional: log the error (e.g., using ILogger)
                // _logger.LogError(ex, "Failed to load blog posts");

                return BadRequest("An error occurred while loading blog posts: " + ex.Message);
            }
        }
        [HttpGet]
        public IActionResult CreateBlog()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBlog(BlogPost post, IFormFile files)
        {
            try
            {
                if (files != null && files.Length > 0)
                {
                    var uploadsFolderPath = Path.Combine(_env.WebRootPath, "Blog");
                    Directory.CreateDirectory(uploadsFolderPath); // Ensure directory exists

                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(files.FileName);
                    var extension = Path.GetExtension(files.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{fileNameWithoutExt}{extension}";
                    var fullPath = Path.Combine(uploadsFolderPath, uniqueFileName);

                    // Save the file to the server
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await files.CopyToAsync(fileStream);
                    }

                    post.ImageUrl = uniqueFileName;
                    //    post.ReadMoreUrl = "/Blog/" + uniqueFileName;
                }

                //if (ModelState.IsValid)
                //{
                post.Slug = SlugHelper.ToSlug(post.Title);
                post.ReadMoreUrl = "/Blog/" + post.Slug;
                post.Date = DateTime.Now;
                _context.BlogPosts.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(BlogIndex));
                //}

                //                return View(post);
            }
            catch (Exception ex)
            {
                // Optionally log the error: _logger.LogError(ex, "Error creating blog post");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the blog post: " + ex.Message);
                return View(post);
            }
        }
        public async Task<IActionResult> EditBlog(int id)
        {
            try
            {
                var post = await _context.BlogPosts.FindAsync(id);
                if (post == null)
                {
                    return NotFound();
                }

                return View(post);
            }
            catch (Exception ex)
            {
                // Optional: log the error
                _logger.LogError(ex, "Error fetching blog post with ID {id}", id);
                return BadRequest("An error occurred while fetching the blog post: " + ex.Message);
            }
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditBlog(int id, BlogPost post)
        //{
        //    try
        //    {
        //        if (id != post.Id)
        //            return NotFound();

        //        if (ModelState.IsValid)
        //        {
        //            try
        //            {
        //                _context.Update(post);
        //                await _context.SaveChangesAsync();
        //            }
        //            catch (DbUpdateConcurrencyException)
        //            {
        //                if (!_context.BlogPosts.Any(p => p.Id == id))
        //                    return NotFound();
        //                throw; // Rethrow if it's an unknown concurrency issue
        //            }

        //            return RedirectToAction(nameof(BlogIndex));
        //        }

        //        return View(post);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Optionally log: _logger.LogError(ex, "Error updating blog post");
        //        ModelState.AddModelError(string.Empty, "An error occurred while updating the post: " + ex.Message);
        //        return View(post);
        //    }
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditBlog(int id, BlogPost post, IFormFile file)
        //{
        //    if (id != post.Id)
        //        return NotFound();

        //    try
        //    {
        //        var existingPost = await _context.BlogPosts.FirstOrDefaultAsync(p => p.Id == id);
        //        if (existingPost == null)
        //            return NotFound();

        //        // Update text fields
        //        existingPost.Title = post.Title;
        //        existingPost.Summary = post.Summary;
        //        existingPost.Date = DateTime.Now; // Optional: update to current
        //        existingPost.Slug = SlugHelper.ToSlug(post.Title);
        //        existingPost.ReadMoreUrl = "/Blog/" + existingPost.Slug;

        //        // Handle image update
        //        if (file != null && file.Length > 0)
        //        {
        //            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        //            var extension = Path.GetExtension(file.FileName).ToLower();

        //            if (!allowedExtensions.Contains(extension))
        //            {
        //                ModelState.AddModelError("", "Unsupported image format.");
        //                return View(existingPost);
        //            }

        //            // Delete old image
        //            if (!string.IsNullOrEmpty(existingPost.ImageUrl))
        //            {
        //                var oldPath = Path.Combine(_env.WebRootPath, "Blog", existingPost.ImageUrl);
        //                if (System.IO.File.Exists(oldPath))
        //                {
        //                    System.IO.File.Delete(oldPath);
        //                }
        //            }

        //            // Save new image
        //            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
        //            var uniqueFileName = $"{Guid.NewGuid()}_{fileNameWithoutExt}{extension}";
        //            var savePath = Path.Combine(_env.WebRootPath, "Blog", uniqueFileName);

        //            Directory.CreateDirectory(Path.Combine(_env.WebRootPath, "Blog")); // Ensure folder

        //            using (var stream = new FileStream(savePath, FileMode.Create))
        //            {
        //                await file.CopyToAsync(stream);
        //            }

        //            existingPost.ImageUrl = uniqueFileName;
        //        }

        //        _context.Update(existingPost);
        //        await _context.SaveChangesAsync();

        //        return RedirectToAction(nameof(BlogIndex));
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        _logger.LogError(ex, "Concurrency error while updating blog post with ID: {BlogPostId}", post.Id);

        //        if (!_context.BlogPosts.Any(p => p.Id == id))
        //            return NotFound();
        //        else
        //            throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unexpected error while editing blog post with ID: {BlogPostId}", post.Id);
        //        TempData["Error"] = "An error occurred while updating the blog post. Please try again later.";
        //        return View(post);
        //    }
        //}

        public async Task<IActionResult> DeleteBlog(int id)
        {
            try
            {
                var post = await _context.BlogPosts.FirstOrDefaultAsync(p => p.Id == id);
                if (post == null)
                {
                    return NotFound();
                }

                return View(post);
            }
            catch (Exception ex)
            {
                // Optional: log the error using ILogger
                // _logger.LogError(ex, "Error fetching blog post with ID {id}", id);

                // Optionally add model error or return a custom error page
                ModelState.AddModelError(string.Empty, "An error occurred while fetching the blog post: " + ex.Message);
                return View("Error"); // You can change this to a custom error view
            }
        }
        [HttpPost, ActionName("DeleteBlog")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBlogConfirmed(int id)
        {
            try
            {
                var post = await _context.BlogPosts.FindAsync(id);
                if (post == null)
                {
                    return NotFound();
                }

                _context.BlogPosts.Remove(post);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(BlogIndex));
            }
            catch (Exception ex)
            {
                // Optionally log the error using ILogger
                _logger.LogError(ex, "Error deleting blog post with ID {id}", id);

                // Optionally add model error or return custom error view
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the blog post: " + ex.Message);
                return View("Error"); // You can replace this with your custom error page
            }
        }
        [Route("Blog/{slug}")]
        public async Task<IActionResult> BlogDetails(string slug)
        {
            var post = await _context.BlogPosts.FirstOrDefaultAsync(p => p.Slug == slug);
            if (post == null) return NotFound();
            return View(post);
        }


        // BlogIndex End//
        //public BannerImagesController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}

        // GET: BannerImages
        public async Task<IActionResult> Index()
        {
            try
            {
                var list = await _context.BannerImages.ToListAsync();

                foreach (var bannerImage in list)
                {
                    if (!string.IsNullOrEmpty(bannerImage.SummerImage1))
                    {
                        bannerImage.SummerImage1 = "/Banner/" + bannerImage.SummerImage1;
                    }
                }

                return View(list);
            }
            catch (Exception ex)
            {
                // Optionally log the error using ILogger
                // _logger.LogError(ex, "Error fetching or processing banner images");

                // Optionally add model error or return custom error view
                ModelState.AddModelError(string.Empty, "An error occurred while processing the banner images: " + ex.Message);
                return View("Error"); // You can replace this with your custom error page
            }


        }

        // GET: BannerImages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var bannerImage = await _context.BannerImages
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (bannerImage == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(bannerImage.SummerImage1) &&
                    !bannerImage.SummerImage1.StartsWith("/Banner/"))
                {
                    bannerImage.SummerImage1 = "/Banner/" + bannerImage.SummerImage1;
                }

                return View(bannerImage);
            }
            catch (Exception ex)
            {
                // Optionally log the error using ILogger
                _logger.LogError(ex, "Error fetching or processing banner image with ID {id}", id);

                // Optionally add a model error or return a custom error view
                ModelState.AddModelError(string.Empty, "An error occurred while processing the banner image: " + ex.Message);
                return View("Error"); // You can replace this with your custom error page
            }
        }

        // GET: BannerImages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BannerImages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AltText,Title,Description,IsActive")] BannerImage bannerImage, IFormFile files)
        {

            try
            {
                // return RedirectToAction(nameof(Index));
                bannerImage.CreatedAt = DateTime.Now;
                bannerImage.ButtonLink = bannerImage.ButtonLink ?? "#";
                bannerImage.ButtonText = "asd";
                //  var banner = await _context.BannerImages.FirstOrDefaultAsync();
                var uploadsFolderPath = Path.Combine(_env.WebRootPath, "Banner");
                Directory.CreateDirectory(uploadsFolderPath); // Ensure directory exists


                //var file = files[i];
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(files.FileName);
                var extension = Path.GetExtension(files.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}_{fileNameWithoutExt}{extension}";
                var fullPath = Path.Combine(uploadsFolderPath, uniqueFileName);

                // Save the file to the server
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await files.CopyToAsync(fileStream);
                }


                // Optionally assign to banner properties
                //if (i == 0) bannerImage.SummerImage1 = uniqueFileName;  // Adjust according to your model
                //if (i == 1) bannerImage.SummerImage2 = uniqueFileName;  // Adjust according to your model
                bannerImage.SummerImage1 = uniqueFileName;
                _context.Add(bannerImage);



                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
            return RedirectToAction("Index");
        }

        // GET: BannerImages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var bannerImage = await _context.BannerImages.FindAsync(id);
                if (bannerImage == null)
                {
                    return NotFound();
                }

                return View(bannerImage);
            }
            catch (Exception ex)
            {
                // Optionally log the error using ILogger
                _logger.LogError(ex, "Error fetching banner image with ID {id}", id);

                // Optionally add a model error or return a custom error view
                ModelState.AddModelError(string.Empty, "An error occurred while processing the banner image: " + ex.Message);
                return View("Error"); // You can replace this with your custom error page
            }
        }

        // POST: BannerImages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,AltText,Title,Description,IsActive")] BannerImage bannerImage)
        //{
        //    if (id != bannerImage.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {

        //            _context.Update(bannerImage);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!BannerImageExists(bannerImage.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(bannerImage);
        //}

        // GET: BannerImages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var bannerImage = await _context.BannerImages
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (bannerImage == null)
                {
                    return NotFound();
                }

                return View(bannerImage);
            }
            catch (Exception ex)
            {
                // Optionally log the error using ILogger
                // _logger.LogError(ex, "Error fetching banner image with ID {id}", id);

                // Optionally add a model error or return a custom error view
                ModelState.AddModelError(string.Empty, "An error occurred while processing the banner image: " + ex.Message);
                return View("Error"); // You can replace this with your custom error page
            }
        }

        // POST: BannerImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var bannerImage = await _context.BannerImages.FindAsync(id);
                if (bannerImage == null)
                {
                    return NotFound();
                }

                // Delete the image file from the server if it exists
                if (!string.IsNullOrEmpty(bannerImage.SummerImage1))
                {
                    // Extract just the filename in case SummerImage1 contains "/Banner/filename.jpg"
                    var fileName = Path.GetFileName(bannerImage.SummerImage1);
                    var imagePath = Path.Combine(_env.WebRootPath, "Banner", fileName);

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Remove the database record
                _context.BannerImages.Remove(bannerImage);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Optionally log the error using ILogger
                // _logger.LogError(ex, "Error deleting banner image with ID {id}", id);

                // Optionally add a model error or return a custom error view
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the banner image: " + ex.Message);
                return View("Error"); // You can replace this with your custom error page
            }
        }
        private bool BannerImageExists(int id)
        {
            return _context.BannerImages.Any(e => e.Id == id);
        }


        [HttpGet]
        public async Task<IActionResult> DealOfTheWeekIndex(int id)
        {
            var list = await _context.DealOfTheWeeks.ToListAsync();
            return View(list);
        }



        [HttpGet]
        public async Task<IActionResult> DealOfTheWeekCreate(int id)
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DealOfTheWeekCreate(DealOfTheWeek model, IFormFile files)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string uniqueFileName = string.Empty;
            if (files != null && files.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Deals");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(files.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await files.CopyToAsync(stream);
                }
            }

            // Example: Save to database
            var deal = new DealOfTheWeek
            {
                Title = model.Title,
                Subtitle = model.Subtitle,
                Description = model.Description,
                IsActive = model.IsActive,
                SalePrice = model.SalePrice,
                DealTitle = model.DealTitle,
                DealEndTime = model.DealEndTime,
                ImageUrl = "/Deals/" + uniqueFileName
            };

            _context.DealOfTheWeeks.Add(deal);
            await _context.SaveChangesAsync();

            return RedirectToAction("DealOfTheWeekIndex");

        }

        public async Task<IActionResult> DealOfTheWeekEdit(int? id)
        {
            if (id == null) return NotFound();

            var deal = await _context.DealOfTheWeeks.FindAsync(id);
            if (deal == null) return NotFound();

            return View(deal);
        }
        // POST: DealOfTheWeek/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DealOfTheWeekEdit(int id, DealOfTheWeek model, IFormFile files)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var deal = await _context.DealOfTheWeeks.FindAsync(id);
                try
                {
                    if (deal == null) return NotFound();

                    if (files != null && files.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_env.WebRootPath, "Deals");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        // Delete the old image
                        if (!string.IsNullOrEmpty(deal.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_env.WebRootPath, deal.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(files.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await files.CopyToAsync(stream);
                        }

                        deal.ImageUrl = "/Deals/" + uniqueFileName;
                    }

                    deal.Title = model.Title;
                    deal.Subtitle = model.Subtitle;
                    deal.Description = model.Description;
                    deal.SalePrice = model.SalePrice;
                    deal.DealTitle = model.DealTitle;
                    deal.DealEndTime = model.DealEndTime;
                    deal.IsActive = model.IsActive;

                    _context.Update(deal);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("DealOfTheWeekIndex");
                }
                catch
                {
                    return View(model);
                }
            }
            else
            {
                var deal = await _context.DealOfTheWeeks.FindAsync(id);

                try
                {
                    if (deal != null)
                    {
                        if (!string.IsNullOrEmpty(deal.ImageUrl))
                            model.ImageUrl = deal.ImageUrl;
                    }
                }
                catch (Exception ex)
                {
                    return View(model);

                }
            }

                return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> DealOfTheWeekDelete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var delelete_DealTheWeek = await _context.DealOfTheWeeks
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (delelete_DealTheWeek == null)
                {
                    return NotFound();
                }

                return View(delelete_DealTheWeek);
            }
            catch (Exception ex)
            {
                // Optionally log the error using ILogger
                // _logger.LogError(ex, "Error fetching banner image with ID {id}", id);

                // Optionally add a model error or return a custom error view
                ModelState.AddModelError(string.Empty, "An error occurred while processing the banner image: " + ex.Message);
                return View("Error"); // You can replace this with your custom error page
            }
        }
        [HttpPost, ActionName("DealOfTheWeekDelete")]
        public async Task<IActionResult> DealOfTheWeekDeleteConfirmed(int id)
        {
            var deal = await _context.DealOfTheWeeks.FindAsync(id);
            if (deal != null)
            {
                _context.DealOfTheWeeks.Remove(deal);
                await _context.SaveChangesAsync();
                if (!string.IsNullOrEmpty(deal.ImageUrl))
                {
                    var imagePath = Path.Combine(_env.WebRootPath, deal.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
            }

            return RedirectToAction("DealOfTheWeekIndex");
        }
        [HttpGet]
        public async Task<IActionResult> DealOfTheWeekEdit(int id)
        {
            try
            {
                var post = await _context.DealOfTheWeeks.FindAsync(id);
                if (post == null)
                {
                    return NotFound();
                }

                return View(post);
            }
            catch (Exception ex)
            {
                // Optional: log the error
                _logger.LogError(ex, "Error fetching blog post with ID {id}", id);
                return BadRequest("An error occurred while fetching the blog post: " + ex.Message);
            }
        }
        public async Task<IActionResult> DealOfTheWeekDetails(int? id)
        {
            if (id == null) return NotFound();

            var deal = await _context.DealOfTheWeeks.FirstOrDefaultAsync(d => d.Id == id);
            if (deal == null) return NotFound();

            return View(deal);
        }



    }
}
