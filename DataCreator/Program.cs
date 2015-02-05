using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FeelKnitService;
using FeelKnitService.Helpers;
using FeelKnitService.Model;
using Nancy.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Formatting = System.Xml.Formatting;

namespace DataCreator
{
    class Program
    {
        //private const string Url = "http://localhost/FeelKnitService/users";

        private static readonly List<int> ConsumedRandoms = new List<int>();
        //private static string _jsonString = "";//@"{""object"":{""name"":""Name""}}";
        private static List<Tuple<string, int>> _feelings = new List<Tuple<string, int>>{
            new Tuple<string,int>("Happy",1),
            new Tuple<string,int>("Sad",2),
            new Tuple<string,int>("Proud",3),
            new Tuple<string,int>("Blessed",4),
            new Tuple<string,int>("Worried",5),
            new Tuple<string,int>("Frustrated",6),
            new Tuple<string,int>("Lonely",7),
            new Tuple<string,int>("Relieved",8),
            new Tuple<string,int>("Sick",9),
            new Tuple<string,int>("Loved",10),
            new Tuple<string,int>("Embarassed",11),
            new Tuple<string,int>("Ashamed",12),
            new Tuple<string,int>("Scared",13),
        };

        private static List<Tuple<double, double>> _locations = new List<Tuple<double, double>>
        {
            new Tuple<double, double>(117.913,33.835),
            new Tuple<double, double>(39.9166,116.383),
            new Tuple<double, double>(44.8333,-0.6166),
            new Tuple<double, double>(49.65,-1.467),
            new Tuple<double, double>(28.9,77.21675),
            new Tuple<double, double>(18.96670,72.833),
            new Tuple<double, double>(22.5333,88.3667),
            new Tuple<double, double>(13.003,80.1830),
            new Tuple<double, double>(42.564,18.251),
            new Tuple<double, double>(13.80,77.30),
        };

        private static string[] _comments =
        {
            "Hey, Don't worry",
            "Congrats!!!",
            "Bravooo",
            "Hip Hip Hurray",
            "Woo HOO",
            "Sorry about that",
            "What has happened has happened"
        };

        private static List<string> images = new List<string>
        {
            "thor",
            "hulk",
            "girl1",
            "girl2",
            "chef",
            "blackwidow",
            "girl3",
            "ironman",
            "gru",
            "eveilminion",
            "kungfuminion"
        };
        private static readonly FeelingsContext _context = new FeelingsContext();
        private static JavaScriptSerializer _javaScriptSerializer;


        static void Main(string[] args)
        {
            //Console.WriteLine(images.Count());
            //FormatFeelings();
            // var feelings = 

            _javaScriptSerializer = new JavaScriptSerializer();

            //var jsonSerializerSettings = new JsonSerializerSettings
            //{
            //    ContractResolver = new CamelCasePropertyNamesContractResolver()
            //};
            //string json = JsonConvert.SerializeObject(new Feeling { FeelingText = "sdfsdf" }, Newtonsoft.Json.Formatting.Indented, jsonSerializerSettings);
            //Console.WriteLine(json);
            //var hash = PasswordHash.CreateHash("km");

            //var splits = hash.Split(':');

            //var x = PasswordHash.ValidatePassword("HelloWorld", hash);

            _context.ApplicationSettings.Insert(new ApplicationSetting { FeelingsUpdated = false });

            //_jsonString = javaScriptSerializer.Serialize(new User { User = "xyz", Password = "welcome1", EmailAddress = "ksjdf@fkjsd.com" });
            // EmailHelper.Send("sdfs", "sdfs", "sdfjks@as.com");
            // CreateFeels();
            //CreateUsers();
            //CreateFeelings();
            // CreateComments();
            // CreateApplicationSettings();
            //CallService();
            Console.WriteLine("Done!!!!");
            Console.ReadLine();
            // PostRequest(jsonString, URL);
        }


        private static void FormatComments()
        {
            var users = _context.Users.FindAll();
            var feelings = _context.Feelings.FindAll().Where(f => f.Comments.Any() && f.Comments.All(c => c.User == null)).ToList();
            Console.WriteLine(feelings.Count());
            var index = 1;
            var count = 2;
            foreach (var feeling in feelings)
            {

                foreach (var comment in feeling.Comments)
                {
                    var user = users.FirstOrDefault(u => u.UserName == comment.User);
                    if (user != null)
                        comment.User = user.UserName;
                }

                _context.Feelings.Save(feeling);

                index++;
            }

            Console.WriteLine(index);
        }

        private static void SetAvatars()
        {

            var users = _context.Users.FindAll();

            foreach (var user in users)
            {
                var randomNumber = GetRandom(11);
                user.Avatar = images[randomNumber];
                _context.Users.Save(user);
            }
        }

        private async static void CallService()
        {
            var httpClient = new HttpClient();
            string content = JsonConvert.SerializeObject(new User { UserName = "xxx", Password = "yyy" });
            Console.WriteLine(content);
            //var response = await httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(obj)));

            //response.EnsureSuccessStatusCode();

            //string content = await response.Content.ReadAsStringAsync();
            //var resText = await Task.Run(() => JsonConvert.DeserializeObject(content));

            HttpContent cntnt = new StringContent(JsonConvert.SerializeObject(content));

            try
            {
                // Send the json to the server using POST
                Task<HttpResponseMessage> getResponse = httpClient.PostAsync("http://localhost/feelknitservice/users", cntnt);
                // Wait for the response and read it to a string var

                HttpResponseMessage response = await getResponse;
                var responseStr = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error communicating with the server: " + e.Message);
            }

        }

        private static void CreateApplicationSettings()
        {
            _context.ApplicationSettings.Insert(new ApplicationSetting { FeelingsUpdated = true });
        }

        private static void CreateFeels()
        {
            foreach (var f in _feelings)
            {
                var feeling = new Feel
                {
                    Text = f.Item1,
                    Rank = f.Item2
                };
                PostRequest(_javaScriptSerializer.Serialize(feeling), "http://localhost/FeelKnitService/feelings/createfeel");
            }

        }

        private static void CreateComments()
        {
            var res = GetRequest("http://localhost/FeelKnitService/feelings.json");

            var feelings = _javaScriptSerializer.Deserialize<IEnumerable<Feeling>>(res);
            var random = new Random();
            foreach (var feeling in feelings)
            {
                for (int i = 0; i < 20; i++)
                {

                    var comment = new Comment()
                    {
                        Text = _comments[random.Next(1, 7)],
                        User = "User" + random.Next(1, 11)
                    };

                    PostRequest(_javaScriptSerializer.Serialize(comment),
                        string.Format("http://localhost/FeelKnitService/comments/{0}", feeling.Id));
                }

                Console.WriteLine(feeling.Id);
            }
        }

        private static void CreateComments(Feeling feeling)
        {
            var random = new Random();

            for (var i = 0; i < 20; i++)
            {

                var comment = new Comment()
                {
                    Text = _comments[random.Next(1, 7)],
                    User = "User" + random.Next(1, 11),
                    PostedAt = feeling.FeelingDate.AddMinutes(10 + i)
                };
                feeling.Comments.Add(comment);
            }
        }

        //private static void CreateFeelings()
        //{
        //    for (var i = 1; i < 201; i++)
        //    {
        //        var index = new Random().Next(1, _locations.Count);
        //        var feeling = new Feeling
        //        {
        //            FeelingText = _feelings[GetRandom(_feelings.Count())],
        //            FeelingDate = DateTime.UtcNow.AddDays(-new Random().Next(1, 11)),
        //            UserName = "User" + new Random().Next(1, 11),
        //            Reason = "reason for feeling",
        //            Action = "action for feeling",
        //            Longitude = _locations[index].Item2,
        //            Latitude = _locations[index].Item1,
        //            Comments = new List<Comment>()
        //        };
        //        CreateComments(feeling);
        //        PostRequest(_javaScriptSerializer.Serialize(feeling), "http://localhost/FeelKnitService/feelings");
        //        Console.WriteLine("Feeling num {0} created", i);
        //    }
        //}

        private static void CreateUsers()
        {
            for (int i = 1; i < 11; i++)
            {
                var username = "User" + GetRandom(11);
                var user = new User
                {
                    UserName = username,
                    Password = "Welcome1",
                    EmailAddress = username + "@gmail.com"
                };
                PostRequest(_javaScriptSerializer.Serialize(user), "http://localhost/FeelKnitService/users");
                //Console.WriteLine("Success for {0}", username);
            }
        }

        private static void PostRequest(string json, string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.ContentLength = json.Length;
            using (var webStream = request.GetRequestStream())
            using (var requestWriter = new StreamWriter(webStream, Encoding.ASCII))
            {
                requestWriter.Write(json);
            }

            try
            {
                var webResponse = request.GetResponse();
                using (var webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (var responseReader = new StreamReader(webStream))
                        {
                            string response = responseReader.ReadToEnd();
                            //Console.Out.WriteLine(response);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("-----------------");
                Console.Out.WriteLine(e.Message);
            }
        }

        private static string GetRequest(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            //using (var webStream = request.GetRequestStream())
            //using (var requestWriter = new StreamWriter(webStream, Encoding.ASCII))
            //{
            //    //requestWriter.Write(json);
            //}

            string response = string.Empty;
            try
            {
                var webResponse = request.GetResponse();
                using (var webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (var responseReader = new StreamReader(webStream))
                        {
                            response = responseReader.ReadToEnd();
                            //Console.Out.WriteLine(response);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("-----------------");
                Console.Out.WriteLine(e.Message);
            }

            return response;
        }

        private static int GetRandom(int maxValue)
        {
            var random = new Random();
            int value = 1;

            while (ConsumedRandoms.Contains(value))
            {
                value = random.Next(1, maxValue);
            }
            ConsumedRandoms.Add(value);
            if (ConsumedRandoms.Count == maxValue - 1)
                ConsumedRandoms.Clear();
            return value;

        }
    }
}
