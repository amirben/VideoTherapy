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
        ///<summary>
        ///check the current round progress of gestures
        ///</summary>
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


        ///<summary>
        /// get current round motion quality score
        ///</summary>
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


        ///<summary>
        ///update the exercise current motion score
        ///</summary>
        public static float GetExerciseMotionScore(Exercise CurrentExercise)
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

            CurrentExercise.ExerciseMotionScore = Convert.ToInt32(avg);
            return avg;
        }


        ///<summary>
        ///get current training score - num of repititions completed
        ///</summary>
        public static int GetTrainingScore(Training CurrentTraining)
        {
            int numOfSuccess = 0;
            int numOfExercises = 0;
            foreach (var key in CurrentTraining.Playlist.Keys)
            {
                Exercise exercise = CurrentTraining.Playlist[key][1];
                
                if (exercise.ExerciseScore != null && exercise.IsTraceable) //meaning that is a traceable
                {
                    numOfExercises += exercise.ExerciseScore.TotalRepetitions;
                    numOfSuccess += exercise.ExerciseScore.TotalRepetitionsDone;
                }
                else
                {
                    numOfExercises += exercise.Repetitions;
                }
            }

            float scoreRep = (numOfSuccess * 100 )/ (float) numOfExercises;
            //int scoreRep = (int)(t);
            return (numOfExercises != 0) ? (int) scoreRep : 0;
        }

        ///<summary>
        ///get current training moition quality score
        ///</summary>
        public static int GetTrainingQuailty(Training CurrentTraining)
        {
            float avg = 0;
            int numOfExe = 0;

            foreach (var key in CurrentTraining.Playlist.Keys)
            {
                Exercise exercise = CurrentTraining.Playlist[key][1];

                if (exercise.ExerciseScore != null && exercise.IsTraceable)
                {
                    avg += (exercise.ExerciseScore.MoitionQuality * 100);
                    numOfExe++;
                }
            }

            
            return (numOfExe != 0) ? (int) (avg / numOfExe) : 0;
        }

        ///<summary>
        ///summary the session scoring (all exercise with the same session id)
        ///</summary>
        public static Score SumUpSessionScoring(List<Exercise> CurrentSession)
        {
            Score sessionScore = new Score();

            int numOfExercisesStart = 0;
            foreach (Exercise exercise in CurrentSession)
            {
                if (!exercise.isDemo && exercise.IsStart) //only if exercise as been started it will calculate the score
                {
                    numOfExercisesStart++;

                    sessionScore.TotalRepetitions += exercise.Repetitions;
                    sessionScore.TotalRepetitionsDone += exercise.SumUpExerciseRepetitions();
                    sessionScore.MoitionQuality += exercise.ExerciseMotionScore;
                }
            }

            sessionScore.MoitionQuality /= numOfExercisesStart;
            sessionScore.MoitionQuality /= 100;

            Console.WriteLine("Session score: motion {0}, rep {1}", sessionScore.MoitionQuality, 
                sessionScore.TotalRepetitionsDone /(float)sessionScore.TotalRepetitions);

            return sessionScore;
        }


    }
}
