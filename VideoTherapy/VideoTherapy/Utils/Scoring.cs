using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoTherapy.Objects;

namespace VideoTherapy.Utils
{
    public static class Scoring
    {
        public static float GetRoundProgress(Dictionary<string, VTGesture> GestureList)
        {
            int sum = 0;

            foreach (VTGesture _gesture in GestureList.Values)
            {
                if (_gesture.IsSuccess)
                {
                    sum++;
                }

            }

            return ((float)sum / (float)GestureList.Count) * 100;
        }

        public static float GetRoundMotionScore(Dictionary<string, VTGesture> GestureList)
        {
            float avg = 0;
            foreach (var gesture in GestureList.Values)
            {
                if (gesture.MaxProgressValue > gesture.MinProgressValue)
                {
                    avg += gesture.ProgressValue * 100;
                }
                    
                else
                {
                    avg += (1 - gesture.ProgressValue) * 100;
                }
            }

            avg /= GestureList.Count;

            return avg;
        }


        // check to be how much from how much in precents = num of repeationg succes
        public static float GetExerciseScore(Exercise CurrentExercise)
        {
            float avg = 0;
            int numOfSucces = 0;
            foreach (var round in CurrentExercise.Rounds)
            {
                if (round.RoundSuccess)
                {
                    avg += round.RoundMotionQuality;
                    numOfSucces++;
                }
                    
            }

            avg /= numOfSucces;

            return avg;
        }

        //todo - ExerciceQuality - like in the above, need to change

        public static float GetTrainingScore(Training CurrentTraining)
        {
            int numOfSuccess = 0;
            int numOfExercises = 0;
            foreach (var key in CurrentTraining.Playlist.Keys)
            {
                Exercise exercise = CurrentTraining.Playlist[key][1];
                
                if (exercise.ExerciseScore != null)
                {
                    numOfExercises += exercise.ExerciseScore.TotalRepetitions;
                    numOfSuccess += exercise.ExerciseScore.TotalRepetitionsDone;
                }

                //foreach (var exercise in CurrentTraining.Playlist[key])
                //{
                //    if (!exercise.isDemo)
                //    {
                //        if (exercise.isSucceed)
                //        {
                //            numOfSuccess++;
                //        }
                //        numOfExercises++;
                //    }
                //}
            }

            return (numOfSuccess / numOfExercises) * 100;
        }

        public static float GetTrainingQuailty(Training CurrentTraining)
        {
            float avg = 0;
            int numOfExe = 0;

            foreach (var key in CurrentTraining.Playlist.Keys)
            {
                Exercise exercise = CurrentTraining.Playlist[key][1];

                if (exercise.ExerciseScore != null)
                {
                    avg += exercise.ExerciseScore.MoitionQuality;
                    numOfExe++;
                }

                //foreach (var exercise in CurrentTraining.Playlist[key])
                //{
                //    if (!exercise.isDemo)
                //    {
                //        avg+= exercise.ExerciseMotionScore;
                //        numOfExe++;
                //    }
                //}
            }

            return avg / numOfExe;
        }


    }
}
