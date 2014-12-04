using System;
using System.IO;
using PushSharp;
using PushSharp.Apple;

namespace FeelKnitService.Push
{
    public class PushNotificationApple
    {
        private static PushBroker _pushBroker;

        public PushNotificationApple()
        {
            if (_pushBroker != null) return;

            //Create our push services broker
            _pushBroker = new PushBroker();

            //Wire up the events for all the services that the broker registers
            _pushBroker.OnNotificationSent += NotificationSent;
            _pushBroker.OnChannelException += ChannelException;
            _pushBroker.OnServiceException += ServiceException;
            _pushBroker.OnNotificationFailed += NotificationFailed;
            _pushBroker.OnDeviceSubscriptionExpired += DeviceSubscriptionExpired;
            _pushBroker.OnDeviceSubscriptionChanged += DeviceSubscriptionChanged;
            _pushBroker.OnChannelCreated += ChannelCreated;
            _pushBroker.OnChannelDestroyed += ChannelDestroyed;

            //-------------------------
            // APPLE NOTIFICATIONS
            //-------------------------
            //Configure and start Apple APNS
            // IMPORTANT: Make sure you use the right Push certificate.  Apple allows you to generate one for connecting to Sandbox,
            //   and one for connecting to Production.  You must use the right one, to match the provisioning profile you build your
            //   app with!
            // Make sure you provide the correct path to the certificate, in my case this is how I did it in a WCF service under Azure,
            // but in your case this might be different. Putting the .p12 certificate in the main directory of your service 
            // (in case you have a webservice) is never a good idea, people can download it from there..
            //System.Web.Hosting.HostingEnvironment.MapPath("~/folder/file");
            //var appleCert = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Resources/PushSharp.Apns.Sandbox.p12"));
            var appleCert = File.ReadAllBytes(System.Web.Hosting.HostingEnvironment.MapPath("~/iosPushCertificate.p12"));

            //IMPORTANT: If you are using a Development provisioning Profile, you must use the Sandbox push notification server 
            //  (so you would leave the first arg in the ctor of ApplePushChannelSettings as 'false')
            //  If you are using an AdHoc or AppStore provisioning profile, you must use the Production push notification server
            //  (so you would change the first arg in the ctor of ApplePushChannelSettings to 'true')
            _pushBroker.RegisterAppleService(new ApplePushChannelSettings(false, appleCert, "Elys1um")); //Extension method
        }

        public void SendNotification(string deviceToken, string message)
        {
            //Fluent construction of an iOS notification
            //IMPORTANT: For iOS you MUST MUST MUST use your own DeviceToken here that gets generated within your iOS app itself when the Application Delegate
            //  for registered for remote notifications is called, and the device token is passed back to you
            if (_pushBroker != null)
            {
                _pushBroker.QueueNotification(new AppleNotification()
                                           .ForDeviceToken(deviceToken)
                                           .WithAlert(message)
                                           .WithBadge(1)
                                           .WithSound("sound.caf"));
            }
        }

        private void ChannelDestroyed(object sender)
        {
        }
        private void ChannelCreated(object sender, PushSharp.Core.IPushChannel pushChannel)
        {
        }
        private void DeviceSubscriptionChanged(object sender, string oldSubscriptionId, string newSubscriptionId, PushSharp.Core.INotification notification)
        {
        }
        private void DeviceSubscriptionExpired(object sender, string expiredSubscriptionId, DateTime expirationDateUtc, PushSharp.Core.INotification notification)
        {
        }
        private void NotificationFailed(object sender, PushSharp.Core.INotification notification, Exception error)
        {
        }
        private void ServiceException(object sender, Exception error)
        {
        }
        private void ChannelException(object sender, PushSharp.Core.IPushChannel pushChannel, Exception error)
        {
        }
        private void NotificationSent(object sender, PushSharp.Core.INotification notification)
        {
        }
    }
}
