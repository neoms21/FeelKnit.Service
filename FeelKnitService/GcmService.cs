using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using FeelKnitService.Model;
using Newtonsoft.Json.Linq;

namespace FeelKnitService
{
    public class GcmService
    {

        public void SendRequest(string userNameFromComment, List<User> users)
        {
            // Create a request using a URL that can receive a post. 
            var request = WebRequest.Create("https://android.googleapis.com/gcm/send");
            // Set the Method property of the request to POST.
            request.Method = "POST";

            var x = new JObject();
            var arr = new JArray();
            //{
            //    users
            //};
            users.ForEach(u => arr.Add(u.Key));
            x.Add("registration_ids", arr);
            var data = new JObject { { "message", "Youv'e received comment on your recent feeling from" }, { "user", userNameFromComment } };
            x.Add("data", data);
            var postData = Newtonsoft.Json.JsonConvert.SerializeObject(x);
            var byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/json";
            request.Headers.Add(string.Format("Authorization: key={0}", "AIzaSyBQe8AA1hv5zW2SJ308KUHXqBnMNee0950"));            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            var dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();
            // Get the response.
            var response = request.GetResponse();
            // Display the status.
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            var reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            Console.WriteLine(responseFromServer);
            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();
        }
    }
}