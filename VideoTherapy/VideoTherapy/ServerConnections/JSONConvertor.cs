﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VideoTherapy.Objects;

namespace VideoTherapy.ServerConnections
{
    public static class JSONConvertor
    {

        public static Patient CreatePatient(string _JSONcontent)
        {
            dynamic o = JsonConvert.DeserializeObject<object>(_JSONcontent);

            Patient _patient = new Patient();
            _patient.AccountId = o.data;


            return _patient;
        }

        public static void GettingPatientData(Patient _patient, string _JSONcontent)
        {
            dynamic o = JsonConvert.DeserializeObject<object>(_JSONcontent);

            _patient.FirstName = o.data.firstName;
            _patient.LastName = o.data.lastName;
            _patient.Email = o.data.email;
            _patient.Gender = o.data.gender;
            _patient.Age = Int32.Parse((string) o.data.age);
            _patient.ImageThumb = o.data.profilePhoto;
            _patient.BirthDay = Convert.ToDateTime((string) o.data.birthday);
            _patient.UserProfileLink = o.data.profileUrl;
        }

        public static void GettingPatientTreatment(Patient _patient, string _JSONcontent)
        {
            dynamic d =
                    (JsonConvert.DeserializeObject<IDictionary<string, object>>(_JSONcontent))["data"];

            
            string json = d.ToString();
            Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            _patient.PatientTreatment = new Treatment();
            //todo - need to be change when there are more the one
            _patient.PatientTreatment.TreatmentNumber = 1;
            _patient.PatientTreatment.TrainingList = new List<Training>();

            foreach (KeyValuePair<string, object> item in dic)
            {
                Training newTraining = new Training();

                json = item.Value.ToString();
                Dictionary<string, object> currentObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                _patient.PatientTreatment.TrainingList.Add(newTraining);
                
                newTraining.TrainingNumber = _patient.PatientTreatment.TrainingList.IndexOf(newTraining) + 1;
                newTraining.TrainingName = currentObj["label"].ToString();
                newTraining.TrainingId = Int32.Parse(currentObj["id"].ToString());
                newTraining.TrainingThumbs = currentObj["thumbnail"].ToString();
            }
        }

        public static void GettingPatientTraining(Training _training, string _JSONcontent)
        {
            dynamic d =
                    (JsonConvert.DeserializeObject<IDictionary<string, object>>(_JSONcontent))["data"];

            _training.Playlist = new List<Exercise>();
            int numOfExercises = 1;

            foreach (var item in d.sessions)
            {
                Exercise demoExercise = new Exercise();
                demoExercise.ExerciseName = item.exerciseLabel;
                demoExercise.ExerciseId = item.exerciseId;
                demoExercise.ExerciseThumbs = item.exerciseThumbnail;
                demoExercise.VideoPath = item.exerciseVideoDemo;
                demoExercise.ExerciseNum = numOfExercises;
                demoExercise.isDemo = true;

                _training.Playlist.Add(demoExercise);

                numOfExercises++;

                int x = item.repeats;
                for (int i = 0; i < x; i++)
                {
                    Exercise newExercise = new Exercise();

                    newExercise.ExerciseName = item.exerciseLabel;
                    newExercise.ExerciseId = item.exerciseId;
                    newExercise.ExerciseThumbs = item.exerciseThumbnail;
                    newExercise.VideoPath = item.exerciseVideo;
                    newExercise.Repetitions = item.repeats;
                    newExercise.ExerciseNum = numOfExercises;

                    //for future download for not duplicate the same file
                    if (i != 0)
                    {
                        newExercise.isDuplicate = true;
                    }

                    _training.Playlist.Add(newExercise);
                    numOfExercises++;
                }

                
            }
        }
    }
}

