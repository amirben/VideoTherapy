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

        public Boolean isPause { set; get; }
        public Boolean isStart { set; get; }

        //Exercise gestures -> used for init gesture list for this exercise
        public Boolean isTrackable { set; get; }
        public List<VTGesture> VTGestureList { set; get; }
        public string ContinuousGestureName { set; get; }
        public VTGesture StartGesutre { set; get; }
     
        public Boolean isSucceed { set; get; }
        public Boolean IsPlayed { set; get;}

        //Rounds
        public ObservableCollection<Round> Rounds;
        public Round CurrentRound { set; get; }
        public int RoundIndex = 0;

        //Score
        public int ExerciseMotionScore { set; get; }
        public int ExerciseRepetitionDone { set; get; }
        public Score ExerciseScore { set; get; }

        //Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event ExerciseView.NextRoundUpdataDelegate NextRoundUpdateUIEvent;
        //public event ExerciseView.StopDetectionDelegate StopDetectionEvent;
        public event ExerciseView.UpdateLastRound UpdateLastRound;

        public delegate void NextRoundDelegate();
              

        public Exercise()
        {
           // NextRoundDelegate nextRoundDelegate = new NextRoundDelegate(NextRoundEvent);

        }

        public void AddNextRoundEvent()
        {
            //CurrentRound.NextRoundEvent += new NextRoundDelegate(NextRoundEvent);
            //NextRoundDelegate nextRoundDelegate = new NextRoundDelegate(NextRoundEvent);
            CurrentRound.NextRoundEvent += NextRoundEvent;
        }

        //Next round
        public void NextRoundEvent()
        {
            //StopDetectionEvent();

            if (!isPause)
            {
                RoundIndex++;
                this.NotifyPropertyChanged("RoundNumber");
                this.NotifyPropertyChanged("RoundProgress");
                UpdateLastRound();

                //Scoring update
                ExerciseMotionScore = Convert.ToInt32(Scoring.GetExerciseScore(this));
                this.NotifyPropertyChanged("ExerciseMotionScore");

                if (RoundIndex < Repetitions)
                {
                    //Stop detecion

                    //changing the round and add event to him
                    CurrentRound = Rounds[RoundIndex];
                    AddNextRoundEvent();

                    //printstatus();

                    //update UI of the change
                    NextRoundUpdateUIEvent();
                }
                else
                {
                    UpdateLastRound();
                    isPause = true;
                }
            }
        }

        //check if exercise completed or not
        public int SumUpExerciseRepetitions()
        {
            int numOfSuccesRound = 0;
            if (isTrackable)
            {
                foreach (var round in Rounds)
                {
                    if (round.RoundSuccess)
                    {
                        numOfSuccesRound++;
                    }
                }

                ExerciseRepetitionDone = numOfSuccesRound;
                isSucceed = (numOfSuccesRound / Repetitions) * 100 > 50 ? true : false;
            }
            else
            {
                ExerciseRepetitionDone = 0;
                isSucceed = false;
            }

            return ExerciseRepetitionDone;

        }

        public void ClearAllDataInExercise()
        {
            isStart = false;
            isSucceed = false;
            RoundIndex = 0;
            Rounds = null;
            ExerciseScore = null;
        }

        private void printstatus()
        {

            Console.WriteLine("=============== In Exercise ==================");
            Console.WriteLine(CurrentRound.RoundProgress);
            Console.WriteLine(CurrentRound.RoundSuccess);
            Console.WriteLine("Current round {0}",CurrentRound.RoundNumber);
            //Console.WriteLine("Exe: " + this.GetHashCode());
            Console.WriteLine();
            foreach (var item in CurrentRound.GestureList)
            {
                Console.WriteLine(item.Key);
                Console.WriteLine(item.Value.IsSuccess);
                Console.WriteLine(item.Value.ProgressValue);
                Console.WriteLine(item.Value.TrsholdProgressValue);
                Console.WriteLine();
            }
        }

        //Create rounds for exercise
        public void CreateRounds()
        {
            Rounds = new ObservableCollection<Round>();
            for (int i = 0; i < Repetitions; i++)
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
            AddNextRoundEvent();
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
