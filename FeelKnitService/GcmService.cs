
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace FeelKnitService
{
    public class GcmService
    {

        public void SendRequest(string userName)
        {
            // Create a request using a URL that can receive a post. 
            var request = WebRequest.Create("https://android.googleapis.com/gcm/send");
            // Set the Method property of the request to POST.
            request.Method = "POST";
            string deviceId = "APA91bGehTL2Qcxd0nZO0Tz2x_EAu8bo5oFqFXjwqJMG4MrxwXwfg4sbhtijBQd8E__xIffe71q9X_Qt1e1EzcYGA0ZzZ1veB8387a5O9EDCEJ8iDRFeGiAm_9eHXjblxDCvmkYWmpFMVppxZlADiztG1xEhAKnA19fl4VT7fl88-cOVYN3eFic";

            var x = new JObject();
            var arr = new JArray
            {
                "APA91bGehTL2Qcxd0nZO0Tz2x_EAu8bo5oFqFXjwqJMG4MrxwXwfg4sbhtijBQd8E__xIffe71q9X_Qt1e1EzcYGA0ZzZ1veB8387a5O9EDCEJ8iDRFeGiAm_9eHXjblxDCvmkYWmpFMVppxZlADiztG1xEhAKnA19fl4VT7fl88-cOVYN3eFic"
            };

            x.Add("registration_ids", arr);
            var data = new JObject { { "message", "Youv'e received commen on your recent feeling from" }, { "user", userName } };
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
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
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