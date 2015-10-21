using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Net.Http;
using Newtonsoft.Json;

namespace VideoTherapy.ServerConnections
{
    /// <summary>
    /// Class used for connecting to the server API
    /// </summary>
    public static class ApiConnection
    {
        /// <summary>
        /// Uri of the server
        /// </summary>
        private readonly static string apiUri = "https://videotherapy.co/dev/vt/api/dispatcher.php";

        /// <summary>
        /// The type of the Api that need to be use
        /// </summary>
        private static string ApiTypeString = "";

        /// <summary>
        /// Client key - Not implement yet on server side
        /// todo
        /// </summary>
        private static string clientKey = "8e28b8db-6395-4417-9df9-10dd0efb5ef9";

        /// <summary>
        /// Enum that represent the types of the API
        /// </summary>
        private enum ApiType { AppLogin, GetUserData, GetUserTraining, GetTraining };

        /// <summary>
        /// Initializes a new dictionary that represent the tuples for the REST-api request
        /// </summary>
        /// <param name="_apiType">The type of the api request</param>
        /// <param name="_keyValuePairs">A list of the key-value pairs</param>
        private static Dictionary<string, string> PairsDictinaryForApi(ApiType _apiType, params string[] _keyValuePairs)
        {
            SetApiType(_apiType);

            Dictionary<string, string> pairs = new Dictionary<string, string>();
            pairs.Add("api", ApiTypeString);
            pairs.Add("clientKey", clientKey);

            for (int i = 0; i < _keyValuePairs.Length; i+=2)
            {
                pairs.Add(_keyValuePairs[i], _keyValuePairs[i + 1]);
            }

            return pairs;
        }

        /// <summary>
        /// Change the type of the api request
        /// </summary>
        /// <param name="_apiType">The type of the api request</param>
        private static void SetApiType(ApiType _apiType)
        {
            switch (_apiType)
            {
                case ApiType.AppLogin:
                    ApiTypeString = "app-login";
                    break;

                case ApiType.GetUserData:
                    ApiTypeString = "get-user-data";
                    break;

                case ApiType.GetUserTraining:
                    ApiTypeString = "get-user-training";
                    break;

                case ApiType.GetTraining:
                    ApiTypeString = "get-training";
                    break;
            }
        }

        /// <summary>
        /// Requesting data from the API, returning the content from the API
        /// </summary>
        /// <param name="_pairs">The dictionary with the values of the request</param>
        private static async Task<string> ConnectingApiAsync(Dictionary<string, string> _pairs)
        {
            string content = "";
            using (HttpClient client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(_pairs, Formatting.Indented);
                //string json = new JavaScriptSerializer().Serialize((object) _pairs);
                HttpContent contentPost = new StringContent(json, Encoding.UTF8);

                var response = await client.PostAsync(apiUri, contentPost);
                var x = response.EnsureSuccessStatusCode();

                content = await response.Content.ReadAsStringAsync();

            }

            return content;
        }

        /// <summary>
        /// Request login from the API
        /// </summary>
        /// <param name="_email">The email that the user entered</param>
        /// <param name="_password">The password that the user entered</param>
        public static async Task<string> AppLoginApiAsync(string _email, string _password)
        {
            Dictionary<string, string> _pairs = PairsDictinaryForApi(ApiType.AppLogin,  "email", _email, "password", _password);

            return await ConnectingApiAsync(_pairs);
        }

        /// <summary>
        /// Requesting the user details (name, image and etc.), returning JSON with the content of the patient details
        /// </summary>
        /// <param name="_accountId">User account id</param>
        public static async Task<string> GetUserDataApiAsync(int _accountId)
        {
            Dictionary<string, string> _pairs = PairsDictinaryForApi(ApiType.GetUserData, "accountId", Convert.ToString(_accountId), "level", "1");

            return await ConnectingApiAsync(_pairs);
        }

        /// <summary>
        /// Request the user training list, returning JSON with the content of the training list
        /// </summary>
        /// <param name="_patientId">Patient id</param>
        public static async Task<string> GetUserTrainingApiAsync(int _patientId)
        {
            Dictionary<string, string> _pairs = PairsDictinaryForApi(ApiType.GetUserTraining, "patientId", Convert.ToString(_patientId), "status", "1");

            return await ConnectingApiAsync(_pairs);
        }

        /// <summary>
        /// Request the user selected training, returning JSON with the content of the training
        /// </summary>
        /// <param name="_trainingId">Active instance of the KinectSensor</param>
        public static async Task<string> GetTrainingApiAsync(int _trainingId)
        {
            Dictionary<string, string> _pairs = PairsDictinaryForApi(ApiType.GetTraining, "trainingId", Convert.ToString(_trainingId));

            return await ConnectingApiAsync(_pairs);
        }
    }
}
