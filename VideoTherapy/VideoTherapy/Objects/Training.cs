﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoTherapy.ServerConnections;
using VideoTherapy.Utils;

namespace VideoTherapy.Objects
{
    public class Training
    {
        public const float TRAINING_COMPLIANCE = 0.6f;

        public int TrainingId { set; get; }
        public int TrainingNumber { set; get; }
        public string TrainingName { set; get; }
        public bool IsRecommended { set; get; }
        public int Repetitions { set; get; }
        public int TrainingCompleted { set; get; }
        public DateTime? LastViewed { set; get; }
        public string TrainingThumbs { set; get; }
        public string CalGuid { set; get; }
        public string CalEventId { set; get; }
        public float TrainingComplianceValue { set; get; }

        public Dictionary<int, List<Exercise>> Playlist { set; get; }

        public Boolean IsTraceableTraining { set; get; }
        public Boolean Downloaded { set; get;}
        public Boolean SkipDemo { set; get; } 

        public int TrainingQuality { set; get; }
        public int TrainingScore { set; get; }

        public Boolean TrainingComplianceFlag { set; get; }

        public Exercise CurrentExercise { set; get; }

        private int currentSessionId = 1;


        public async void SumUpSession(int sessionId)
        {
            //currently sending only tracking exercises
            if (Playlist[sessionId][1].IsTraceable && Playlist[sessionId][1].IsStart)
            {
                Playlist[sessionId][1].ExerciseScore = Scoring.SumUpSessionScoring(Playlist[sessionId]);
                
                //send to server
                var response = await ApiConnection.ReportExerciseScoreApiAsync(Playlist[sessionId][1], this);
            }

        }

        public Exercise GetNextExercise()
        {
            //check if there are still exercises in the session
            if (!Playlist[currentSessionId].Last().Equals(CurrentExercise))
            {
                int i = Playlist[currentSessionId].IndexOf(CurrentExercise);

                if (i == -1 && SkipDemo)
                {
                    CurrentExercise = Playlist[currentSessionId][i + 2];
                }
                else
                {
                    CurrentExercise = Playlist[currentSessionId][i + 1];
                }
                
                return CurrentExercise;
            }

            //SumUpSession(currentSessionId);
            return GetNextSession();
            
        }

        public Exercise GetNextSession()
        {
            //sumup the score and send to server
            SumUpSession(currentSessionId);

            currentSessionId++;

            if (Playlist.ContainsKey(currentSessionId))
            {
                return GetNextExercise();
            }

            return null;
        }

        public Exercise GetPrevExercise()
        {
            CurrentExercise.ClearAllDataInExercise();

            if (!Playlist[currentSessionId].First().Equals(CurrentExercise))
            {
                int i = Playlist[currentSessionId].IndexOf(CurrentExercise);

                if (i == 1 && SkipDemo)
                {
                    GetPrevSession();
                    CurrentExercise = Playlist[currentSessionId].Last();
                }
                else
                {
                    CurrentExercise = Playlist[currentSessionId][i - 1];
                }

                return CurrentExercise;
            }

            GetPrevSession();
            CurrentExercise = Playlist[currentSessionId].Last();
            return CurrentExercise;
        }

        public Exercise GetPrevSession()
        {
            CurrentExercise.ClearAllDataInExercise();

            if (currentSessionId == 1)
            {
                return CurrentExercise;
            }
            else
            {
                currentSessionId--;

                if (SkipDemo)
                {
                    CurrentExercise = Playlist[currentSessionId][1];
                }
                else
                {
                    CurrentExercise = Playlist[currentSessionId].First();
                }
                
                return CurrentExercise;
            }
        }

        public bool CheckTrainingCompliance()
        {
            int numOfPlayed = 0;
            int numOfExercises = 0;

            foreach (var exercises in Playlist.Values)
            {
                foreach (var exercise in exercises)
                {
                    if (!exercise.isDemo)
                    {
                        if (exercise.IsCompliance)
                        {
                            numOfPlayed++;
                        }
                        numOfExercises++;
                    }
                }
               
            }

            float t = (numOfPlayed / (float) numOfExercises);
            TrainingComplianceFlag = t > TrainingComplianceValue;
            return TrainingComplianceFlag;
        }

        public void ClearTrainingData()
        {
            foreach (var key in Playlist.Keys)
            {
                foreach (Exercise exercise in Playlist[key])
                {
                    if (!exercise.isDemo)
                    {
                        exercise.ClearAllDataInExercise();
                    }
                }
            }

            CurrentExercise = null;
            currentSessionId = 1;
        }
    }
}
