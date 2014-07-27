using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FeelKnitService.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FeelKnitService
{
    public class GcmService
    {
        private Action<string> _action;

        public void SendRequest(Feeling feeling, string userNameFromComment, List<User> users, Action<string> action)
        {
            _action = action;
            var feelinguser = users.FirstOrDefault(u => u.UserName.Equals(feeling.UserName, StringComparison.OrdinalIgnoreCase));
            if (feelinguser == null)
                return;

            Task.Factory.StartNew(() =>
                SendGcmRequest(userNameFromComment, new List<User> { feelinguser },
                    string.Format("Comment on feeling: '{0}' from {1}", feeling.FeelingText, userNameFromComment)));
            Task.Factory.StartNew(() => SendGcmRequest(userNameFromComment, users.Where(u => u.UserName != feelinguser.UserName).ToList(),
                string.Format("Comment on comment")));
            //dataStream = response.GetResponseStream();
            //var reader = new StreamReader(dataStream);
            //string responseFromServer = reader.ReadToEnd();
            //// Display the content.
            //Console.WriteLine(responseFromServer);
            //// Clean up the streams.
            //reader.Close();
            //dataStream.Close();
            //response.Close();
        }

        private void SendGcmRequest(string userNameFromComment, List<User> users, string message)
        {
            // Create a request using a URL that can receive a post. 
            try
            {
                if (users.Count() == 1 && users.First().UserName.Equals(userNameFromComment))
                    return;
                var request = WebRequest.Create("https://android.googleapis.com/gcm/send");
                // Set the Method property of the request to POST.
                request.Method = "POST";
                var jsonObject = new JObject();
                var arr = new JArray();
                users.ToList().ForEach(u => arr.Add(u.Key));
                jsonObject.Add("registration_ids", arr);
                var data = new JObject
                {
                    {"message", message},
                    {"user", userNameFromComment}
                };
                jsonObject.Add("data", data);
                var postData = JsonConvert.SerializeObject(jsonObject);
                var byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "application/json";
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
                    _action(responseFromServer);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _action(e.ToString());
                _action(e.ToString());
            }
        }
    }
}