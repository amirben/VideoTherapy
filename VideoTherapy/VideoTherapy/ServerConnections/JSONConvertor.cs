using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using VideoTherapy.Objects;
using VideoTherapy.Utils;

namespace VideoTherapy.ServerConnections
{
    /// <summary>
    /// Class used for saving the error from server
    /// </summary>
    public class ErrorMessege : EventArgs
    {
        public string Messege { get; set; }
        public int Code { get; set; }
        public ErrorMessege(string messege, int code)
        {
            Code = code;
            Messege = messege;
        }
    }

    /// <summary>
    /// Class used for convert from json to objects using newtonsoft
    /// </summary>
    public static class JSONConvertor
    {
        /// <summary>
        /// Enum used for all error codes in server
        /// </summary>
        private enum ServerCode { API_OK = 100, ERROR_SYSTEM_UNDER_MAINTENANCE, ERROR_INVALID_CLIENT_VERSION, ERROR_API_NAME,
                                    ERROR_API_NOT_EXIST, ERROR_NO_REQUEST, ERROR_INVALID_REQUEST, ERROR_EMPTY_RESPONSE, ERROR_DB_INSERT_ERROR,
                                    ERROR_USER_IS_NOT_PERMITTED, ERROR_BAD_API_KEY, ERROR_UPLOAD_FAILURE, ERROR_SIGNUP_ACCOUNT_ALREADY_EXISTS,
                                    ERROR_SIGNUP_ACCOUNT_FAILURE, ERROR_LOGIN_ACCOUNT_FAILURE, ERROR_LOGIN_ACCOUNT_FAILURE_BAD_CREDENTIAL,
                                    ERROR_ACCOUNT_FAILURE, ERROR_THERAPIST_FAILURE, ERROR_PATIENT_DIAGNOSIS_FAILURE, ERROR_TREATMENT_CONFIRMATION_FAILURE,
                                    ERROR_TREATMENT_UPDATE_STATUS, ERROR_NO_TRAINING_FOUND, ERROR_FAIL_TO_CREATE_TRAINING, ERROR_FAIL_TO_DELETE_TRAINING,
                                    ERROR_FAIL_TO_RESET_PASSWORD, ERROR_UPDATE_EVALUATION, ERROR_FAIL_TO_CREATE_ACCOUNT_INFO, ERROR_FAIL_TO_TRACK_TRAINING,
                                    ERROR_FAIL_TO_MARK_EVENT_AS_COMPLETED }

        /// <summary>
        /// Event handler on error accure
        /// </summary>
        public static event EventHandler ErrorEvent;

        /// <summary>
        /// Parse from json content to new patient object, get his id
        /// </summary>
        /// <param name="_JSONcontent">The response json</param>
        public static Patient CreatePatient(string _JSONcontent)
        {
            dynamic o = JsonConvert.DeserializeObject<object>(_JSONcontent);

            bool checkError = checkForErrors((int)o.code);

            if (checkError)
            {
                ErrorEvent(null, new ErrorMessege(((ServerCode)o.code).ToString(), (int)o.code));

                return null;
            }

            Patient _patient = new Patient();
            _patient.AccountId = o.data;

            return _patient;
        }

        /// <summary>
        /// Parse from json content to new therapist and his information from the json
        /// <param name="_JSONcontent">The response json</param>
        /// <param name="_therapist">The therapist object</param>
        /// </summary>
        public static void GettingTherapistData(Therapist _therapist, string _JSONcontent)
        {
            dynamic o = JsonConvert.DeserializeObject<object>(_JSONcontent);

            bool checkError = checkForErrors((int)o.code);
            
            if (checkError)
            {
                ErrorEvent(null, new ErrorMessege(((ServerCode)o.code).ToString(), (int)o.code));
                return;
            }

            _therapist.FirstName = o.data.firstName;
            _therapist.LastName = o.data.lastName;
            _therapist.Email = o.data.email;
            _therapist.Gender = o.data.gender;
            _therapist.Age = Int32.Parse((string)o.data.age);
            _therapist.ImageThumb = o.data.profilePhoto;
            _therapist.BirthDay = Convert.ToDateTime((string)o.data.birthday);
            _therapist.UserProfileLink = o.data.profileUrl;

        }

        /// <summary>
        /// Parse from json content to current patient and his information from the json
        /// </summary>
        /// <param name="_JSONcontent">The response json</param>
        /// <param name="_patient">The _patient object</param>
        public static void GettingPatientData(Patient _patient, string _JSONcontent)
        {
            dynamic o = JsonConvert.DeserializeObject<object>(_JSONcontent);

            bool checkError = checkForErrors((int)o.code);
            
            if (checkError)
            {
                ErrorEvent(null, new ErrorMessege(((ServerCode)o.code).ToString(), (int)o.code));
                return;
            }

            _patient.FirstName = o.data.firstName;
            _patient.LastName = o.data.lastName;
            _patient.Email = o.data.email;
            _patient.Gender = o.data.gender;
            _patient.Age = Int32.Parse((string)o.data.age);
            _patient.ImageThumb = o.data.profilePhoto;
            _patient.BirthDay = Convert.ToDateTime((string)o.data.birthday);
            _patient.UserProfileLink = o.data.profileUrl;

            _patient.PatientTreatment = new Treatment();
            _patient.PatientTreatment.TreatmentId = o.data.treatmentIds;

        }


        /// <summary>
        /// Parse from json content to current patient treatment
        /// </summary>
        /// <param name="_JSONcontent">The response json</param>
        /// <param name="_patient">The _patient object</param>
        public static void GettingPatientTreatment(Patient _patient, string _JSONcontent)
        {
            //todo - run over the treatments
            dynamic d =
                    (JsonConvert.DeserializeObject<IDictionary<string, object>>(_JSONcontent));


            bool checkError = checkForErrors((int)d["code"]);
            if (checkError)
            {
                ErrorEvent(null, new ErrorMessege(((ServerCode)d["code"]).ToString(), (int)d["code"]));
                return;
            }

            dynamic list = (JsonConvert.DeserializeObject<IDictionary<string, object>>(_JSONcontent))["data"];

            List<Treatment> treatments = new List<Treatment>();

            foreach (var item in list)
            {
                Treatment treatment = new Treatment();

                //treatment details
                treatment.TreatmentNumber = list.IndexOf(item) + 1;
                treatment.TreatmentId = item.treatmentId;
                treatment.StartDate = DateTime.Parse((string)item.treatmentStartTime["date"]);
                treatment.EndDate = DateTime.Parse((string)item.treatmentEndTime["date"]);
                treatment.TreatmentProgress = DateFormat.CalcTreatementDateProgress(treatment.StartDate, treatment.EndDate);

                //treatment scoring
                Dictionary<string, float> scoringDic = JsonConvert.DeserializeObject<Dictionary<string, float>>(item.scoring.ToString());
                treatment.TreatmentCompliance = (int) (scoringDic["num_repition_done"] / scoringDic["num_repition_total"] * 100);
                treatment.TreatmentScore = (int) (scoringDic["motion_quality"] * 100);

                //therapist details
                treatment.TreatmentTherapist = new Therapist();
                treatment.TreatmentTherapist.AccountId = item.therapistId;
                treatment.TreatmentTherapist.FirstName = item.therapistFirst;
                treatment.TreatmentTherapist.LastName = item.therapistLast;
                treatment.TreatmentTherapist.ImageThumb = item.therapistThumbnail;
                //todo - change when will be a the date of start connection between therapist and patient
                treatment.TreatmentTherapist.StartDate = treatment.StartDate;

                //trainings:
                treatment.TrainingList = CreateTrainingList(item["trainings"].ToString());

                //set recomended training to treatment
                //todo 
                foreach (var training in treatment.TrainingList)
                {
                    if (training.isRecommended)
                    {
                        treatment.RecommendedTraining = training;
                        break;
                    }
                }

                treatments.Add(treatment);
            }

            //Now there is only one treatment:
            _patient.PatientTreatment = treatments.First();
        }

        /// <summary>
        /// Parse from json content to training list
        /// <param name="_JSONcontent">The response json</param>
        /// </summary>
        public static List<Training> CreateTrainingList(string _JSONcontent)
        {
            List<Training> trainingList = new List<Training>();

            Dictionary<int, object> trainingListJson = JsonConvert.DeserializeObject<Dictionary<int, object>>(_JSONcontent);

            //List<object> trainingListJson = JsonConvert.DeserializeObject<List<object>>(_JSONcontent);

            foreach (object item in trainingListJson.Values)
            {
                Training newTraining = new Training();
                object temp;
                int tempInt;
                float tempFloat1, tempFloat2;

                Dictionary<string, object> currentObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(item.ToString());

                trainingList.Add(newTraining);

                newTraining.TrainingNumber = trainingList.IndexOf(newTraining) + 1;
                currentObj.TryGetValue("label", out temp);
                newTraining.TrainingName = temp.ToString();

                currentObj.TryGetValue("id", out temp);
                Int32.TryParse(temp.ToString(), out tempInt);
                newTraining.TrainingId = tempInt;

                currentObj.TryGetValue("thumbnail", out temp);
                newTraining.TrainingThumbs = temp.ToString();

                //todo - yoav need to change the api
                currentObj.TryGetValue("lastTrainedDate", out temp);
                Console.WriteLine(temp.ToString());
                if (!temp.ToString().Equals("[]"))
                {
                    Dictionary<string, string> tempDate = JsonConvert.DeserializeObject<Dictionary<string, string>>(temp.ToString());
                    DateTime tempDateTime;
                    DateTime.TryParse(tempDate["date"].ToString(), out tempDateTime);

                    newTraining.LastViewed = tempDateTime;
                }
                
                //add scoring!!
                Dictionary<string, float> scoringDic = JsonConvert.DeserializeObject<Dictionary<string, float>>(currentObj["session_usage"].ToString());
                scoringDic.TryGetValue("num_repeatition_done", out tempFloat1);
                scoringDic.TryGetValue("num_repeatition_total", out tempFloat2);
                if (tempFloat1 == 0)
                {
                    newTraining.TrainingCompliance = 0;
                }
                else
                {
                    newTraining.TrainingCompliance = (int)(tempFloat1 / tempFloat2 * 100);
                }
                //newTraining.TrainingCompliance = (int)(scoringDic["num_repeatition_done"] / scoringDic["num_repeatition_total"] * 100);

                scoringDic.TryGetValue("motion_quality", out tempFloat1);
                newTraining.TrainingScore = (int)(tempFloat1 * 100);
                //newTraining.TrainingScore = (int)(scoringDic["motion_quality"] * 100);


                //check if there is calEvents exist
                object tempCalEvents;
                currentObj.TryGetValue("calEventsUsage", out tempCalEvents);

                if (tempCalEvents is JObject)
                {
                    Dictionary<string, int> calEvents = JsonConvert.DeserializeObject<Dictionary<string, int>>(tempCalEvents.ToString());
                    calEvents.TryGetValue("total", out tempInt);
                    newTraining.Repetitions = tempInt;
                    //newTraining.Repetitions = Int32.Parse(calEvents["total"].ToString());

                    calEvents.TryGetValue("completed", out tempInt);
                    newTraining.TrainingCompleted = tempInt;
                    //newTraining.TrainingCompleted = Int32.Parse(calEvents["completed"].ToString());
                }

                //check if this training is the upcoming one
                Object checkUpComming = false;
                currentObj.TryGetValue("upcomingEvent", out checkUpComming);
                if (checkUpComming != null && (Boolean)checkUpComming)
                {
                    newTraining.isRecommended = true;
                }


                object calGuid;
                currentObj.TryGetValue("cal_guid", out calGuid);
                newTraining.CalGuid = calGuid.ToString();

                currentObj.TryGetValue("next_cal_event_id", out calGuid);
                newTraining.CalGuid = calGuid.ToString();
            }

            return trainingList;
        }

        /// <summary>
        /// Parse from json content to current patient training
        /// </summary>
        /// <param name="_JSONcontent">The response json</param>
        /// <param name="_training">The _training object</param>
        public static void GettingPatientTraining2(Training _training, string _JSONcontent)
        {
            dynamic d =
                    (JsonConvert.DeserializeObject<IDictionary<string, object>>(_JSONcontent));
    
            //checking errors
            bool checkError = checkForErrors((int)d["code"]);
            if (checkError)
            {
                ErrorEvent(null, new ErrorMessege(((ServerCode)d["code"].code).ToString(), (int)d["code"].code));
                return;
            }

            _training.Playlist = new Dictionary<int, List<Exercise>>();
            int numOfExercises = 1;

            //iterat over the exercise session 
            foreach (var item in d["data"].sessions)
            {
                //Creating demo exercise
                Exercise demoExercise = new Exercise();
                demoExercise.ExerciseName = item.exerciseLabel;
                demoExercise.ExerciseId = item.exerciseId;
                demoExercise.ExerciseThumbs = item.exerciseThumbnail;
                string tt = item.exerciseVideoDemo;
                demoExercise.VideoPath = new Uri(HttpsReplaceToHttp.ReplaceHttpsToHttp(tt));
                demoExercise.ExerciseNum = numOfExercises;
                demoExercise.isDemo = true;
                demoExercise.SessionId = item.sessionId;
                demoExercise.Mode = Exercise.ExerciseMode.Demo;

                int temp = item.sessionId;
                _training.Playlist[temp] = new List<Exercise>();
                _training.Playlist[temp].Add(demoExercise);

                numOfExercises++;

                //duplicate the exercise if there are repeats for him by therapist
                //sessRepeats - number of duplicates exercise
                int x = item.sessRepeats;
                for (int i = 1; i <= x; i++)
                {
                    Exercise newExercise = new Exercise();

                    newExercise.ExerciseName = item.exerciseLabel;
                    newExercise.ExerciseId = item.exerciseId;
                    newExercise.ExerciseThumbs = item.exerciseThumbnail;
                    tt = item.exerciseVideo;
                    newExercise.VideoPath = new Uri(HttpsReplaceToHttp.ReplaceHttpsToHttp(tt));
                    newExercise.Repetitions = item.exerciseCycles;
                    newExercise.ExerciseNum = numOfExercises;
                    newExercise.SessionId = item.sessionId;
                    newExercise.isTrackable = item.isTrackable;
                    newExercise.Mode = (bool) item.isTrackable ? Exercise.ExerciseMode.Traceable : Exercise.ExerciseMode.NonTraceable;

                    //for future download for not duplicate the same file
                    if (i != 1)
                    {
                        newExercise.Mode = newExercise.Mode == Exercise.ExerciseMode.Traceable ? Exercise.ExerciseMode.TraceableDuplicate : Exercise.ExerciseMode.NonTraceableDuplicate; 
                        newExercise.isDuplicate = true;
                    }
                    numOfExercises++;

                    _training.Playlist[temp].Add(newExercise);
                }
            }
        }


        /// <summary>
        /// Parse from json content to exercise gestures details
        /// </summary>
        /// <param name="_JSONcontent">The response json</param>
        /// <param name="_exercise">The _exercise object</param>
        public static void GettingExerciseGesture(Exercise _exercise, string _JSONcontent)
        {
            dynamic d = (JsonConvert.DeserializeObject<IDictionary<string, object>>(_JSONcontent));

            //check errors
            bool checkError = checkForErrors((int)d["code"]);
            if (checkError)
            {
                ErrorEvent(null, new ErrorMessege(((ServerCode)d["code"].code).ToString(), (int)d["code"].code));
                return;
            }

            _exercise.ContinuousGestureName = d["data"].gesture_progress_name;
            _exercise.DBUrl = d["data"].file;

            _exercise.StartGesutre = new VTGesture();
            _exercise.StartGesutre.GestureName = Convert.ToString(d["data"].start_gesture_name);
            _exercise.StartGesutre.ConfidanceTrshold = 0.2f;

            string json = d["data"]["gesture_list"].ToString();
            List<object> gestureList = JsonConvert.DeserializeObject<List<object>>(json);

            //createing gestures list
            _exercise.VTGestureList = new List<VTGesture>();
            foreach (var item in gestureList)
            {
                VTGesture gesture = new VTGesture();

                json = item.ToString();
                Dictionary<string, object> currentObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                gesture.GestureName = currentObj["gestureName"].ToString();
                gesture.MinProgressValue = (float) Double.Parse(currentObj["startGestureValue"].ToString());
                gesture.MaxProgressValue = (float)Double.Parse(currentObj["endGestureValue"].ToString());
                gesture.TrsholdProgressValue = (float)Double.Parse(currentObj["progressThd"].ToString());
                gesture.ConfidanceTrshold = (float)Double.Parse(currentObj["confidenceThd"].ToString());
                gesture.IsTrack = currentObj["isTrackableItem"].ToString().Equals("1") ? true : false;


                _exercise.VTGestureList.Add(gesture);
            }
        }

        /// <summary>
        /// check if the response code is API_OK
        /// </summary>
        /// <param name="code">the error code</param>
        public static bool checkForErrors(int code)
        {
            return !code.Equals((int) ServerCode.API_OK) ? true : false;
        }
    }
}

