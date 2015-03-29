using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FeelKnitService.Helpers;
using FeelKnitService.Model;
using FeelKnitService.Push;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace FeelKnitService
{
    public class PushNotificationService
    {
        private Action<string> _action;

        public void SendCommentNotifications(Feeling feeling, Comment comment, List<User> users, Action<string> action, User feelingUser)
        {
            _action = action;


            SendNotifications(comment.User, new List<User> { feelingUser },
                string.Format("Comment '{0}' on feeling: '{1}' from ", comment.Text, feeling.FeelingText), feeling);

            SendNotifications(comment.User, users, string.Format("Comment '{0}' on comment", comment.Text), feeling);

        }

        private void SendNotifications(string userNameFromComment, List<User> users, string message, Feeling feeling)
        {
            // Create a request using a URL that can receive a post. 

            if (users.Count() == 1 && users.First().UserName.Equals(userNameFromComment) || !users.Any())
                return;

            var byteArray = GetGcmData(userNameFromComment, users, message, feeling);
            SendAndroidNotification(byteArray);

            SendIosNotification(users.Where(u => !string.IsNullOrWhiteSpace(u.IosKey)).Select(u => u.IosKey),
                string.Format("{0} from {1}", message, userNameFromComment), new Dictionary<string, string> { { "feelingId", feeling.Id } });

        }

        private static byte[] GetGcmData(string userNameFromComment, IEnumerable<User> users, string message, Feeling feeling)
        {
            var jsonObject = new JObject();
            var arr = new JArray();
            users.Where(u => !string.IsNullOrWhiteSpace(u.Key)).ForEach(u => arr.Add(u.Key));
            jsonObject.Add("registration_ids", arr);
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var data = new JObject
            {
                {"message", message},
                {"user", userNameFromComment},
                {"feeling", JsonConvert.SerializeObject(feeling.Id, Formatting.Indented, jsonSerializerSettings)}
            };

            jsonObject.Add("data", data);
            var postData = JsonConvert.SerializeObject(jsonObject);
            var byteArray = Encoding.UTF8.GetBytes(postData);
            return byteArray;
        }

        public void SendSameFeelingNotification(IEnumerable<string> gcmTokens, IEnumerable<string> apnsTokens, string message)
        {
            SendIosNotification(apnsTokens, message);

            var tokens = gcmTokens as IList<string> ?? gcmTokens.ToList();
            if (!tokens.Any())
                return;

            var jsonObject = new JObject();
            var arr = new JArray();
            tokens.ForEach(u => arr.Add(u));
            jsonObject.Add("registration_ids", arr);
            var data = new JObject
            {
                {"message", message},
            };

            jsonObject.Add("data", data);
            var postData = JsonConvert.SerializeObject(jsonObject);
            var byteArray = Encoding.UTF8.GetBytes(postData);

            SendAndroidNotification(byteArray);
        }

        private void SendIosNotification(IEnumerable<string> apnsTokens, string message, Dictionary<string, string> customValues = null)
        {
            foreach (var apnsToken in apnsTokens)
            {
                var push = new PushNotificationApple();
                push.SendNotification(apnsToken, message, customValues);
            }
        }

        private void SendAndroidNotification(byte[] byteArray)
        {

            // Create a request using a URL that can receive a post. 
            try
            {
                //var jsonObject = new JObject();
                //var arr = new JArray();
                //tokens.ForEach(u => arr.Add(u));
                //jsonObject.Add("registration_ids", arr);

                //var data = new JObject
                //{
                //    {"message", message},
                //};

                //jsonObject.Add("data", data);
                //var postData = JsonConvert.SerializeObject(jsonObject);
                //var byteArray = Encoding.UTF8.GetBytes(postData);


                var request = WebRequest.Create("https://android.googleapis.com/gcm/send");
                // Set the Method property of the request to POST.
                request.Method = "POST"; request.ContentType = "application/json";
                request.Headers.Add(string.Format("Authorization: key={0}", "AIzaSyBQe8AA1hv5zW2SJ308KUHXqBnMNee0950"));
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                var dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                var response = request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var responseFromServer = reader.ReadToEnd();
                    if (responseFromServer.Contains("failure\":1"))
                        _action(responseFromServer);
                }
            }
            catch (Exception e)
            {
                LogWriter.Write(e.ToString());
                if (_action != null)
                    _action(e.ToString());
            }
        }
    }
}