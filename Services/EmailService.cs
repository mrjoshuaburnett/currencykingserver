using CurrencyKing.Data.DatabaseModels;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CurrencyKing.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<Response> SendEmail(string toEmail, string subject, string htmlContent)
        {
            var client = new SendGridClient(_config.GetSection("SendGridKey").Value);
            var fromEmailAddress = new EmailAddress("support@currencyking.com", "Currency King Support");
            var plainTextContent = Regex.Replace(htmlContent, "<[^>]*>", string.Empty);
            var toEmailAddress = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(fromEmailAddress, toEmailAddress, string.Format("Currency King Support - {0}", subject), plainTextContent, htmlContent);
            return await client.SendEmailAsync(msg);
        }

        public async Task<Response> SendSignUpEmail(User user, PasswordReset passwordReset)
        {

            var passwordResetUrl = String.Format("{0}/resetpassword/{1}", _config.GetSection("AppUrl").Value, passwordReset.Id);

            var customContent = String.Format(
                "<tr><td><span style=font-size:24px;'>Welcome to Currency King!</span></td></tr>" +
                "<tr><td style = 'width:100%;padding-top:10px;padding-bottom:10px;text-align:center' > " +
                "<img style='max-width:50%;vertical-align:middle;margin-left:auto;margin-right:auto' src='https://storage.googleapis.com/restaurant_files_yc/currencyKingTransparent.png' />" +
                "</td></tr>" +
                "<tr><td>You can now check currencies any time you want! Click below to complete regitsration.</td></tr>" +
                "<tr><td style='padding-top:25px;padding-bottom:25px;'><a type='button' style='background-color:#D79922;color:white;text-decoration:none;padding:15px;border-radius:2px;width:100%' href='{0}'>COMPLETE REGISTRATION</a></td></tr>", passwordResetUrl);


            var emailContent = String.Format("{0}{1}", EmailBody(customContent, user.FullName), StandardEmailFooter());

            return await SendEmail(user.EmailAddress, "💱 Welcome to Currency King! 💱", emailContent);
        }

        public async Task<Response> SendPasswordResetEmail(User user, PasswordReset passwordReset)
        {
            var passwordResetUrl = String.Format("{0}/resetpassword/{1}", _config.GetSection("AppUrl").Value, passwordReset.Id);
            var customContent = String.Format(
                "<tr><td>It looks like you're having some trouble getting signed into your account.</td></tr>" +
                "<tr><td style='padding-top:15px;padding-bottom:15px;'><a type='button' style='background-color:#D79922;color:white;text-decoration:none;padding:15px;border-radius:2px;width:100%' href='{0}'>RESET PASSWORD</a></td></tr>" +
                "<tr><td style='padding-top:15px;padding-bottom:15px;'>If you did not request this password reset, please contact <a href='mailto:support@currencyKing.co.uk'>support@currencyKing.co.uk</a></td></tr>"
                , passwordResetUrl);

            var emailContent = String.Format("{0}{1}", EmailBody(customContent, user.FullName), StandardEmailFooter());

            return await SendEmail(user.EmailAddress, "Password Reset 🔑", emailContent);
        }






        public string EmailBody(string customContent, string userName)
        {
            return String.Format("<div style='max-width:500px;margin-left:auto;margin-right:auto'>" +
                              "<table align='center' style='margin-left:auto;margin-right:auto;width:100%;'>" +
                                "<tr>" +
                                  "<td style='width:100%;padding-top:10px;padding-bottom:10px'>" +
                                    "<a href='https://currency-king.azurewebsites.net' target='_blank'><img style='max-width:50%;vertical-align:middle' src='https://storage.googleapis.com/restaurant_files_yc/currencyKingTransparent.png' /></a>" +
                                  "</td>" +
                                "</tr>" +
                                "<tr>" +
                                  "<td style='padding-top:25px;padding-bottom:25px;'>" +
                                    "<strong>Hi{0},</strong>" +
                                  "</td>" +
                                "</tr>" +
                                "{1}" +
                                "<tr>" +
                                  "<td style='padding-top:25px'>" +
                                    "Thanks," +
                                  "</td>" +
                                "</tr>" +
                                "<tr>" +
                                  "<td style='padding-bottom:25px;'>" +
                                    "The Currency King Team" +
                                  "</td>" +
                                "</tr>" +
                              "</table>" +
                            "</div>", String.IsNullOrWhiteSpace(userName) ? "" : " " + userName, customContent);
        }

        public string StandardEmailFooter()
        {
            return String.Format("<table style='width:100%;border-collapse: collapse;'>" +
      "<tr><td style = 'background-color:#ec633e;padding-left:15px;padding-right:15px;padding-top:15px;text-align:center'>" +
          "<span style ='color:white'>© 2021 Currency King Ltd, All rights reserved </span>" +
        "</td>" +
      "</tr> " +
      "<tr><td style = 'background-color:#ec633e;padding-left:15px;padding-right:15px;padding-top:15px;text-align:center;color:white'>Visit <a href='https://currency-king.azurewebsites.net' target='_blank'>https://currency-king.azurewebsites.net</a> to learn more about us!</td></tr>" +
    "</table> ");
        }
    }
}
