using System.Collections.Generic;
using MoonAPNS;

namespace FeelKnitService.Push
{
    public class PushNotificationApple
    {
        public void SendNotification(string deviceToken, string message, Dictionary<string, string> customValues = null)
        {

            var payload = new NotificationPayload(deviceToken, message, 0, "default");
            if (customValues != null)
                foreach (var customValue in customValues)
                {
                    payload.AddCustom(customValue.Key, customValue.Value);
                }
            //payload1.AddCustom("CustomKey", "CustomValue");
            LogWriter.Write(deviceToken + message);
            var notificationList = new List<NotificationPayload> { payload };
            var cert = Properties.Settings.Default.ApnsCertificateName;
            var pwd = Properties.Settings.Default.ApnsCertificatePassword;
            var push = new PushNotification(false, System.Web.Hosting.HostingEnvironment.MapPath(string.Format("~/bin/{0}.p12",cert)), pwd);
            push.SendToApple(notificationList);
        }
    }
}
