using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataCreator
{
    using System.Net.Mail;
    using System;
    public class MailSender
    {

        public void SendEmail(string feelingId, string username)
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
            message.Subject = "Feeling Reported!!";
            //Set IsBodyHtml to true means you can send HTML email.
            message.IsBodyHtml = true;
            message.Body = string.Format("FeelingId {0} has been reported by {1}", feelingId, username);
            message.To.Add("neoms21@gmail.com");

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                //Error, could not send the message
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
