
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using HouseOfWani.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json;
using HouseOfWani.Models.Order;

namespace HouseOfWani.Controllers
{

    [Route("Account")]
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;
        private readonly EmailSender _emailSender;


        //public AccountController(ILogger<AccountController> logger,RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        //{
        //    _roleManager = roleManager;
        //    _userManager = userManager;
        //    _signInManager = signInManager;
        //    _logger = logger;
        //}
        public AccountController(EmailSender emailSender,ILogger<AccountController> logger, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _emailSender = emailSender;
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }
       

        [HttpGet("register")]
        public IActionResult Register()
        {
            try
            {
                // Check if the user is already authenticated and if they are not an Admin
                if (User.Identity.IsAuthenticated && !User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Home");
                }

                // Return the view if no errors occur
                return View();
            }
            catch (Exception ex)
            {
                // Log the exception details
                _logger.LogError(ex, "An unexpected error occurred during user registration.");

                // Optional: Add a user-friendly error message to the ModelState
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");

                // Fallback: Redirect to a safe page in case of an error
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid Data" });

                // Save user data temporarily
                HttpContext.Session.SetString("PendingUserData", JsonConvert.SerializeObject(model));

                // Generate OTP and email to user
                var otp = new Random().Next(100000, 999999).ToString();
                HttpContext.Session.SetString("PendingUserOtp", otp);

                // TODO: Send otp to email  SendOtpEmailAsync
                await _emailSender.SendOtpEmailAsync(model, otp);
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                _logger.LogError(ex, "Error while sending OTP");
                return Json(new { success = false, message = "An error occurred while processing your request. Please try again later." });
            }
            return Json(new { success = true });
            
        }
      
        [HttpPost("VerifyOtpAjax")]
        public async Task<IActionResult> VerifyOtpAjax(string enteredOtp)
        {
            var sessionOtp = HttpContext.Session.GetString("PendingUserOtp");
            var userDataJson = HttpContext.Session.GetString("PendingUserData");

            if (sessionOtp == null || userDataJson == null)
            {
                return Json(new { success = false, message = "Session expired. Please register again." });
            }

            if (enteredOtp != sessionOtp)
            {
                return Json(new { success = false, message = "Invalid OTP. Please try again." });
            }

            var model = JsonConvert.DeserializeObject<RegisterViewModel>(userDataJson);

            // Now Create the user
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                LastLogin = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            { 
                    if ((await _userManager.Users.CountAsync()) == 1)
                    {
                        await EnsureRoleExists("Admin");
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }
                    else
                    {
                        await EnsureRoleExists("User");
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                HttpContext.Session.Remove("PendingUserOtp");
                HttpContext.Session.Remove("PendingUserData");
                return Json(new { success = true, message = "OTP verified. Registration complete!" });
            }
            else
            {
                return Json(new { success = false, message = string.Join(",", result.Errors.Select(e => e.Description)) });
            }
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid email format." });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // To prevent email harvesting, always return the same response
                return Json(new { success = true, message = "If the email address exists in our system, you will receive a password reset link shortly." });
            }

            try
            {
                // Generate Reset Token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action("ResetPassword", "Account", new { token = token, email = model.Email }, Request.Scheme);

                // Send email (implement email service)
                var emailSubject = "Reset Your Password";
                var emailBody = $"To reset your password, please click on the link below:\n{resetLink}";

                // Example: Send email (implement this logic in your email service)
                // await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);

                return Json(new { success = true, message = "If the email address exists in our system, you will receive a password reset link shortly." });
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                _logger.LogError(ex, "Error occurred while sending password reset email.");
                return Json(new { success = false, message = "An error occurred while processing your request. Please try again later." });
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return Json(new { success = false, message = "Invalid email format." });


        //    var user = await _userManager.FindByEmailAsync(model.Email);
        //    if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        //    {
        //        // To prevent email harvesting, always return same response
        //        TempData["Message"] = "If that email address is in our system, we have sent you a reset link.";
        //        return Json(new { success = true, message = "If the email address exists in our system, you will receive a password reset link shortly." });
        //    }

        //    // Generate Reset Token
        //    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //    var resetLink = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);

        //    // Send email (you need to set up email service to send the reset link)
        //    var emailSubject = "Reset Your Password";
        //    var emailBody = $"To reset your password, please click on the link below:\n{resetLink}";

        //    // Example: Send email (Implement your email sending logic here)
        //  //  await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);

        //    // Return success message in JSON format to trigger the popup
        //    return Json(new { success = true, message = "If the email address exists in our system, you will receive a password reset link shortly." });
        //}




        //[HttpPost("register")]
        //public async Task<IActionResult> Register(RegisterViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    try
        //    {
        //        var user = new ApplicationUser
        //        {
        //            UserName = model.Email,
        //            Email = model.Email,
        //            FirstName = model.FirstName,
        //            LastName = model.LastName,
        //            LastLogin = DateTime.UtcNow
        //        };

        //        var result = await _userManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            if ((await _userManager.Users.CountAsync()) == 1)
        //            {
        //                await EnsureRoleExists("Admin");
        //                await _userManager.AddToRoleAsync(user, "Admin");
        //            }
        //            else
        //            {
        //                await EnsureRoleExists("User");
        //                await _userManager.AddToRoleAsync(user, "User");
        //            }

        //            // Generate and Send OTP
        //            var otp = new Random().Next(100000, 999999).ToString();
        //            HttpContext.Session.SetString("OTP", otp);
        //            HttpContext.Session.SetString("RegisteredUserEmail", model.Email);

        //           // await SendOtpEmailAsync(model.Email, otp);

        //            // Instead of redirecting, return success response
        //            return Json(new { success = true, message = "Registration successful! Please enter OTP to verify." });
        //        }

        //        return Json(new { success = false, errors = result.Errors.Select(e => e.Description).ToList() });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Registration error for {Email}", model.Email);
        //        return Json(new { success = false, message = "Unexpected error occurred." });
        //    }
        //}
        ////[HttpGet("verify-otp")]
        //[HttpPost]
        //public async Task<IActionResult> VerifyOtpAjax(string enteredOtp)
        //{
        //    var otp = HttpContext.Session.GetString("OTP");
        //    var email = HttpContext.Session.GetString("RegisteredUserEmail");

        //    if (otp == null || email == null)
        //        return Json(new { success = false, message = "Session expired. Please register again." });

        //    if (enteredOtp == otp)
        //    {
        //        var user = await _userManager.FindByEmailAsync(email);
        //        if (user != null)
        //        {
        //            user.EmailConfirmed = true;
        //            await _userManager.UpdateAsync(user);

        //            // Clear OTP session
        //            HttpContext.Session.Remove("OTP");
        //            HttpContext.Session.Remove("RegisteredUserEmail");

        //            return Json(new { success = true, message = "Email verified successfully!" });
        //        }
        //    }

        //    return Json(new { success = false, message = "Invalid OTP. Please try again." });
        //}



        //[HttpPost("register")]
        //public async Task<IActionResult> Register(RegisterViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    try
        //    {
        //        var user = new ApplicationUser
        //        {
        //            UserName = model.Email,
        //            Email = model.Email,
        //            FirstName = model.FirstName,
        //            LastName = model.LastName,
        //            LastLogin = DateTime.UtcNow
        //        };

        //        var result = await _userManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            if ((await _userManager.Users.CountAsync()) == 1)
        //            {
        //                await EnsureRoleExists("Admin");
        //                await _userManager.AddToRoleAsync(user, "Admin");
        //            }
        //            else
        //            {
        //                await EnsureRoleExists("User");
        //                await _userManager.AddToRoleAsync(user, "User");
        //            }

        //            var otp = new Random().Next(100000, 999999).ToString();
        //            await SendOtpEmailAsync(model.Email, otp);

        //            TempData["SuccessMessage"] = "Registration successful! Please log in.";
        //            return RedirectToAction("Login");
        //        }

        //        foreach (var error in result.Errors)
        //        {
        //            ModelState.AddModelError("", error.Description);
        //        }

        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Optional: Log the error if you have a logger
        //        // _logger.LogError(ex, "An error occurred during user registration.");
        //        _logger.LogError(ex, "An unexpected error occurred during user registration for email: {Email}", model.Email);
        //        ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
        //        return View(model);
        //    }
        //}
        private async Task SendOtpEmailAsync(string email, string otp)
        {
            var subject = "Verify Your Email - OTP Code";
            var body = $"<p>Your OTP code for email verification is: <strong>{otp}</strong></p><p>Thank you for registering!</p>";

            // Assuming you have an email sender service
           // await _emailSender.SendEmailAsync(email, subject, body);
        }

        //public async Task<IActionResult> Register(RegisterViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    var user = new ApplicationUser
        //    {
        //        UserName = model.Email,
        //        Email = model.Email,
        //        FirstName = model.FirstName,
        //        LastName = model.LastName,
        //        LastLogin = DateTime.UtcNow

        //    };

        //    var result = await _userManager.CreateAsync(user, model.Password);
        //    if (result.Succeeded)
        //    {
        //        if ((await _userManager.Users.CountAsync()) == 1)
        //        {
        //            await EnsureRoleExists("Admin");
        //            await _userManager.AddToRoleAsync(user, "Admin");
        //        }
        //        else
        //        {
        //            await EnsureRoleExists("User");
        //            await _userManager.AddToRoleAsync(user, "User");
        //        }

        //        TempData["SuccessMessage"] = "Registration successful! Please log in.";
        //        return RedirectToAction("Login");
        //    }

        //    foreach (var error in result.Errors)
        //        ModelState.AddModelError("", error.Description);

        //    return View(model);
        //}

        [HttpGet("login")]
        public IActionResult Login()
        {
        //    if (User.Identity.IsAuthenticated)
        //        return RedirectToAction("Index", "Home");

            return View();
        }

        //[HttpPost("login")]
        //public async Task<IActionResult> Login(LoginViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);


        //    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
        //    if (result.Succeeded)
        //    {
        //        var user = await _userManager.FindByEmailAsync(model.Email);
        //        if (user != null)
        //        {
        //            user.LastLogin = DateTime.UtcNow;
        //            // Store user details in session
        //            HttpContext.Session.SetString("UserId", Convert.ToString(user.Id));
        //            HttpContext.Session.SetString("UserEmail", user.Email);

        //            // Retrieve user roles
        //            var roles = await _userManager.GetRolesAsync(user);
        //            string userRole = roles.FirstOrDefault() ?? "User"; // Default to "User" if no roles found
        //            HttpContext.Session.SetString("UserRole", userRole);
        //            TempData["UserRole"] = userRole;
        //            if (userRole == "User")
        //                return RedirectToAction("Index", "Home");

        //            return RedirectToAction("Index", "Admin");
        //        }
        //    }

        //    ModelState.AddModelError("", "Invalid login attempt");
        //    return View(model);
        //}
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        user.LastLogin = DateTime.UtcNow;

                        // Store user details in session
                        HttpContext.Session.SetString("UserId", Convert.ToString(user.Id));
                        HttpContext.Session.SetString("UserEmail", user.Email);

                        // Retrieve user roles
                        var roles = await _userManager.GetRolesAsync(user);
                        string userRole = roles.FirstOrDefault() ?? "User"; // Default to "User" if no roles found
                        HttpContext.Session.SetString("UserRole", userRole);
                        TempData["UserRole"] = userRole;

                        if (userRole == "User")
                            return RedirectToAction("Index", "Home");

                        return RedirectToAction("Index", "Admin");
                    }
                }

                ModelState.AddModelError("", "Invalid login attempt");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for email: {Email}", model.Email);

                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                return View(model);
            }
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                HttpContext.Session.Clear();

                _logger.LogInformation("User logged out successfully.");

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during logout.");

                // Optional: You can redirect to a safe page even if logout fails
                TempData["ErrorMessage"] = "An error occurred while logging out. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        private async Task EnsureRoleExists(string roleName)
        {
            try
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!result.Succeeded)
                    {
                        _logger.LogWarning("Failed to create role: {RoleName}. Errors: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                    else
                    {
                        _logger.LogInformation("Role {RoleName} created successfully.", roleName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while ensuring role {RoleName} exists.", roleName);
                throw;  // Optional: rethrow the exception if you want to propagate it
            }
        }

    }
}
