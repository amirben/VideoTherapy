using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoTherapy.Utils;

namespace VideoTherapy.Objects
{
    public class Exercise : INotifyPropertyChanged
    {
        public enum ExerciseMode { Demo, Traceable, TraceableDuplicate, NonTraceable, NonTraceableDuplicate }

        public ExerciseMode Mode { set; get; }
        public int ExerciseId { set; get; }
        public int ExerciseNum { set; get; }
        public String ExerciseName { set; get; }
        public int Repetitions { set; get; }
        public string ExerciseThumbs { set; get; }
        public Uri VideoPath { set; get; }
        public string DBPath { set; get; }
        public string DBUrl { set; get; }
        public int SessionId { set; get; }

        //for demo video
        public Boolean isDemo { set; get; }
        public Boolean isShow { set; get; }

        //for download - because there are repeated videos & images
        public Boolean isDuplicate { set; get; }
        public Boolean Downloaded { set; get; }
        public Boolean isLastDuplicate { set; get; }

        public Boolean IsFinish { set; get; }
        public Boolean IsStart { set; get; }

        //Exercise gestures -> used for init gesture list for this exercise
        public Boolean IsTraceable { set; get; }
        public List<VTGesture> VTGestureList { set; get; }
        public string ContinuousGestureName { set; get; }
        public VTGesture StartGesutre { set; get; }
     
        public Boolean IsSucceed { set; get; }
        public float SuccessRate { set; get; }
        public Boolean IsCompliance { set; get;}
        public int VideoComplianceValue { set; get; }
        //Rounds
        public ObservableCollection<Round> Rounds;
        public Round CurrentRound { set; get; }
        public int RoundIndex = 0;

        //Score
        public int ExerciseMotionScore { set; get; } //quailty
        public int ExerciseRepetitionDone { set; get; }
        public Score ExerciseScore { set; get; }

        //Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event ExerciseView.NextRoundUpdataDelegate NextRoundUpdateUIEvent;
        public event ExerciseView.UpdateLastRound UpdateLastRound;

        public delegate void NextRoundDelegate();
              

        public Exercise() {  }

        public void CheckComplianceOfExercise(int videoPosition)
        {
            if (videoPosition >= VideoComplianceValue)
            {
                IsCompliance = true;
            }
            else
            {
                IsCompliance = false;
            }

            
        }

        //Next round
        public void NextRoundEvent()
        {
            if (!IsFinish)
            {
                RoundIndex++;

                //Scoring update
                ExerciseMotionScore = Convert.ToInt32(Scoring.GetExerciseMotionScore(this));
                this.NotifyPropertyChanged("ExerciseMotionScore");

                CurrentRound = Rounds[RoundIndex];
                if (RoundIndex < Repetitions)
                {
                    //changing the round and add event to him
                    CurrentRound.NextRoundEvent += NextRoundEvent;
                }
                else
                {
                    IsFinish = true;
                }

                //update UI of the change
                NextRoundUpdateUIEvent();
            }
        }

        //check if exercise completed or not
        public int SumUpExerciseRepetitions()
        {
            int numOfSuccesRound = 0;
            if (IsTraceable)
            {
                foreach (var round in Rounds)
                {
                    if (round.RoundSuccess)
                    {
                        numOfSuccesRound++;
                    }
                }

                ExerciseRepetitionDone = numOfSuccesRound;
                float t =  numOfSuccesRound / (float) Repetitions;
                IsSucceed = (numOfSuccesRound / (float)Repetitions) > SuccessRate ? true : false;

                Console.WriteLine("Ex id {0}, Succeed = {1}, succeed/rep = {2}", ExerciseId, IsSucceed, numOfSuccesRound / (float) Repetitions);
            }
            else
            {
                ExerciseRepetitionDone = 0;
                IsSucceed = false;
            }

            return ExerciseRepetitionDone;

        }

        public void ClearAllDataInExercise()
        {
            IsStart = false;
            IsSucceed = false;
            RoundIndex = 0;
            Rounds = null;

            IsCompliance = false;
            IsFinish = false;

            ExerciseMotionScore = 0;
            ExerciseRepetitionDone = 0;
            ExerciseScore = null;
        }

        //Create rounds for exercise
        public void CreateRounds()
        {
            Rounds = new ObservableCollection<Round>();
            for (int i = 0; i <= Repetitions; i++)
            {
                Round round = new Round(i, Repetitions);
                
                Rounds.Add(round);

                foreach (VTGesture gesture in VTGestureList)
                {
                    Rounds[i].GestureList.Add(gesture.GestureName, new VTGesture(gesture));
                }

                //Rounds[i].ContinuousGesture = new ExerciseGesture(ContinuousGestureTemp);

            }

            CurrentRound = Rounds[0];
            CurrentRound.NextRoundEvent += NextRoundEvent;
        }

        /// <summary>
        /// Notifies UI that a property has changed
        /// </summary>
        /// <param name="propertyName">Name of property that has changed</param> 
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void CopyGesturesFromOrigin(Exercise copyFromExercise)
        {
            ContinuousGestureName = copyFromExercise.ContinuousGestureName;
            StartGesutre = new VTGesture(copyFromExercise.StartGesutre);

            VTGestureList = new List<VTGesture>();
            foreach (VTGesture gesture in copyFromExercise.VTGestureList)
            {
                VTGestureList.Add(new VTGesture(gesture));
            }
        }
    }
}
