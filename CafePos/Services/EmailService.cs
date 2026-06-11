using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace QLKTX.Services
{
    public static class EmailService
    {
        public static async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            // ⚙️ Cấu hình SMTP server (ví dụ Gmail)
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("takenteam.contact@gmail.com", "ttdmzeczteprokqg"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("takenteam.contact@gmail.com", "Hệ thống Quán CafePOS"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
