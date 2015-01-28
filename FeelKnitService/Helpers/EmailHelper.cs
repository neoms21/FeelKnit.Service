using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;

namespace FeelKnitService.Helpers
{
    public class EmailHelper
    {

        public void SendEmail(string subject, string content)
        {
            Send(subject, content, "neoms21@gmail.com", "sanket.mali@gmail.com");
        }

        public static void Send(string subject, string msg, params string[] toAdd)
        {
            var smtpClient = new SmtpClient();
            var basicCredential = new NetworkCredential("highgroundfeelknit@gmail.com", "feelknitt");
            var message = new MailMessage();
            var fromAddress = new MailAddress("highgroundfeelknit@gmail.com");

            smtpClient.Host = "smtp.gmail.com";
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = basicCredential;
            smtpClient.EnableSsl = true;

            message.From = fromAddress;
            message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
            message.Subject = subject;
            //Set IsBodyHtml to true means you can send HTML email.
            message.IsBodyHtml = true;
            message.Body = msg;
            foreach (var address in toAdd)
            {
                message.To.Add(address);
            }
            try
            {
                smtpClient.SendCompleted += smtpClient_SendCompleted;
                smtpClient.SendAsync(message, null);
            }
            catch (Exception ex)
            {
                //Error, could not send the message
                Console.WriteLine(ex.ToString());
            }
        }

        private static void smtpClient_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Console.WriteLine(e.Error);
        }
    }
}
