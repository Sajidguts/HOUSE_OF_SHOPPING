using System.Configuration;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;

namespace HouseOfWani.Models.Order
{
    public class EmailSender
    {
        public IConfiguration _configuration { get; }
        public EmailSender(IConfiguration configuration) 
        {
            _configuration = configuration;
        
        }
        //public async Task SendOtpEmailAsync(string email,string otp)
        //{
        //    var fromEmail = _configuration["EmailSettings:FromEmail"];
        //    var password = _configuration["EmailSettings:Password"];
        //    var smtpHost = _configuration["EmailSettings:SmtpHost"];
        //    var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
        //    string ToEmail = "sajid.wani.ahmed@gmail.com";
        //    string subject = "Your One-Time Password (OTP)";
        //    string body = $"Dear user,\n\nYour OTP is: {otp}\n\nPlease use this to complete your verification." +
        //        $" This OTP is valid for 10 minutes.\n\nThank you,\nYour Company Name";
        //    var mail = new MailMessage
        //    {

        //        From = new MailAddress(fromEmail),
        //        Subject = subject,
        //        Body = body,
        //        IsBodyHtml = true
        //    };

        //    mail.To.Add(ToEmail);

        //    using var smtpClient = new SmtpClient(smtpHost, smtpPort)
        //    {
        //        Credentials = new NetworkCredential(fromEmail, password),
        //        EnableSsl = true
        //    };

        //    await smtpClient.SendMailAsync(mail);
        //}


        public async Task SendOtpEmailAsync(RegisterViewModel model, string otp)
        {
            model.Email = "sajid.wani.ahmed@gmail.com";
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var password = _configuration["EmailSettings:Password"];
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
           // var id = HttpContext.Session.GetString("PendingUserData");
            
            string subject = "Your One-Time Password (OTP)";
            string fullName=model.FirstName + " " + model.LastName;
            //string body = $"Dear user,\n\nYour OTP is: {otp}\n\nPlease use this to complete your verification." +
            //              $" This OTP is valid for 10 minutes.\n\nThank you,\nYour Company Name";

            string body = $"Dear {fullName},\n\n" +
                $"Thank you for registering with House of Wani!\n\n" +
                $"Your One-Time Password (OTP) for completing your registration is: {otp}\n\n" +
                $"Please enter this code to verify your account. This OTP is valid for 10 minutes.\n\n" +
                $"If you did not initiate this request, please ignore this email.\n\n" +
                $"Regards,\nHouse of Wani Team";
            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(model.Email); // <-- use the method parameter

            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mail);
        }


    }
}
