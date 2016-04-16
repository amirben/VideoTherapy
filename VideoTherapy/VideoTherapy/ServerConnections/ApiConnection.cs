using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Net.Http;
using Newtonsoft.Json;

using VideoTherapy.Objects;

namespace VideoTherapy.ServerConnections
{

    /// <summary>
    /// Class used for connecting to the server API
    /// </summary>
    public static class ApiConnection
    {
        /// <summary>
        /// Level of patient for connection to server
        /// </summary>
        public static readonly int PATIENT_LEVEL = 100;

        /// <summary>
        /// Level of therapist for connection to server
        /// </summary>
        public static readonly int THERAPIST_LEVEL = 1;

        /// <summary>
        /// Uri of the server
        /// </summary>
        /// https://videotherapy.co/dev/vt/api/dispatcher.php
        private readonly static string apiUri = "http://dev.videotherapy.co/vt/api/dispatcher.php";

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
        private enum ApiType { AppLogin, GetUserData, GetTreatment, GetTraining, GetExerciseGestures,
                                ReportExercise, GetTreatmentByUser, ReportTrainingFeedback };

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

                case ApiType.GetTreatment:
                    ApiTypeString = "get-treatment";
                    break;

                case ApiType.GetTraining:
                    ApiTypeString = "get-training";
                    break;

                case ApiType.GetExerciseGestures:
                    ApiTypeString = "get-exercise-gestures";
                    break;

                case ApiType.ReportExercise:
                    ApiTypeString = "report-treatment-session-usage";
                    break;

                case ApiType.GetTreatmentByUser:
                    ApiTypeString = "treatments/get-treatments-by-user";
                    break;

                case ApiType.ReportTrainingFeedback:
                    ApiTypeString = "treatments/report-training-feedback";
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


                //todo -check error
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
        /// /// <param name="_level">The type of the user (patient of therapist)</param>
        public static async Task<string> GetUserDataApiAsync(int _accountId, int _level)
        {
            Dictionary<string, string> _pairs = PairsDictinaryForApi(ApiType.GetUserData, "accountId", Convert.ToString(_accountId), "level", Convert.ToString(_level));

            return await ConnectingApiAsync(_pairs);
        }

        /// <summary>
        /// Request the user treatment, returning JSON with the content of the treatment
        /// </summary>
        /// <param name="_treatmentId">Patient id</param>
        public static async Task<string> GetTreatmentApiAsync(int _treatmentId)
        {
            string byUsage = "byUsage";
            string byUsageValue = "1";
            Dictionary<string, string> _pairs = PairsDictinaryForApi(ApiType.GetTreatment, "treatmentId", Convert.ToString(_treatmentId), byUsage, byUsageValue);

            return await ConnectingApiAsync(_pairs);
        }

        /// <summary>
        /// Request the user treatments by user id, returning JSON with the content of the treatment
        /// </summary>
        /// <param name="_patientId">Patient id</param>
        public static async Task<string> GetTreatmentByUserApiAsync(int _accountId)
        {
            Dictionary<string, string> _pairs = PairsDictinaryForApi(ApiType.GetTreatmentByUser, "accountId", Convert.ToString(_accountId));

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

        /// <summary>
        /// Request the gestures for exercise, returning JSON with the content of the gestures
        /// </summary>
        /// <param name="_exerciseId">Active instance of the KinectSensor</param>
        public static async Task<string> GetExerciseGesturesApiAsync(int _exerciseId)
        {
            Dictionary<string, string> _pairs = PairsDictinaryForApi(ApiType.GetExerciseGestures, "exerciseId", Convert.ToString(_exerciseId));

            return await ConnectingApiAsync(_pairs);
        }

        /// <summary>
        /// 
        /// <param name="_exerciseId">Active instance of the KinectSensor</param>
        /// </summary>
        public static async Task<string> ReportExerciseScoreApiAsync(Exercise exercise, Training training)
        {
            //todo - add scoring data in exercise class
            Dictionary<string, string> _pairs = PairsDictinaryForApi(ApiType.ReportExercise, "exerciseId", exercise.ExerciseId.ToString(), "numRepitionTotal", exercise.ExerciseScore.TotalRepetitions.ToString(),
                                                                        "numRepitionDone", exercise.ExerciseScore.TotalRepetitionsDone.ToString(), "motionQuality", exercise.ExerciseScore.MoitionQuality.ToString(),
                                                                        "calGuid", training.CalGuid, "calEventId", training.CalEventId );

            return await ConnectingApiAsync(_pairs);
        }

        /// <summary>
        /// Report the the feedback to server
        /// <param name="feedbackJson">The answers that the user answer about feedback</param>
        /// </summary>
        public static async Task<string> ReportTrainingFeedback(string feedbackJson, string calGuid, string eventId)
        {
            Dictionary<string, string> _pairs = PairsDictinaryForApi(ApiType.ReportTrainingFeedback, "calGuid", calGuid, "eventId", eventId, "feedback", feedbackJson);

            return await ConnectingApiAsync(_pairs);
        }
    }
}
