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
        
        //Rounds
        public ObservableCollection<Round> Rounds;
        public Round CurrentRound { set; get; }
        public int RoundIndex = 0;

        //Score
        public int ExerciseMotionScore { set; get; }

        //Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event ExerciseView.NextRoundUpdataDelegate NextRoundUpdateUIEvent;
        public event ExerciseView.StopDetectionDelegate StopDetectionEvent;

        public delegate void NextRoundDelegate();
              

        public Exercise()
        {
            NextRoundDelegate nextRoundDelegate = new NextRoundDelegate(NextRoundEvent);

        }

        public void AddNextRoundEvent()
        {
            //CurrentRound.NextRoundEvent += new NextRoundDelegate(NextRoundEvent);
            NextRoundDelegate nextRoundDelegate = new NextRoundDelegate(NextRoundEvent);
            CurrentRound.NextRoundEvent += NextRoundEvent;
        }

        //Next round
        public void NextRoundEvent()
        {
            StopDetectionEvent();

            if (!isPause)
            {
                RoundIndex++;
                this.NotifyPropertyChanged("RoundNumber");
                this.NotifyPropertyChanged("RoundProgress");

                //Scoring update
                ExerciseMotionScore = Convert.ToInt32(Scoring.GetExerciseScore(this));
                this.NotifyPropertyChanged("ExerciseMotionScore");

                if (RoundIndex < Repetitions)
                {
                    //printstatus();

                    //Stop detecion

                    //changing the round and add event to him
                    CurrentRound = Rounds[RoundIndex];
                    AddNextRoundEvent();

                    //printstatus();

                    //update UI of the change
                    NextRoundUpdateUIEvent();
                }
            }
        }

        //check if exercise completed or not
        public void CheckIfExerciseStatus()
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

                isSucceed = (numOfSuccesRound / Repetitions) * 100 > 50 ? true : false;
            }
            else
            {
                isSucceed = false;
            }
            

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

        //DELETE AFTER DEMO
        /*public string createGestures()
        {
            VTGesture g1 = new VTGesture();
            g1.GestureName = "squat_getting_down";
            g1.MinProgressValue = 0;
            g1.MaxProgressValue = 1.0f;
            g1.TrsholdProgressValue = 0.4f;
            g1.ConfidanceTrshold = 0.4f;
            g1.IsTrack = true;

            VTGesture g2 = new VTGesture();
            g2.GestureName = "squat_getting_up";
            g2.MinProgressValue = 1.0f;
            g2.MaxProgressValue = 0.0f;
            g2.TrsholdProgressValue = 0.4f;
            g2.ConfidanceTrshold = 0.4f;
            g2.IsTrack = true;

            StartGesutre = new VTGesture();
            StartGesutre.GestureName = "squat_start";
            StartGesutre.MinProgressValue = 0;
            StartGesutre.MaxProgressValue = 1.0f;
            StartGesutre.TrsholdProgressValue = 0.01f;
            StartGesutre.ConfidanceTrshold = 0.0f;
            StartGesutre.IsTrack = true;

            ContinuousGestureName = "squatProgress";

            VTGestureList = new List<VTGesture>();
            VTGestureList.Add(g1);
            VTGestureList.Add(g2);

            return @"C:\Users\amirb\Desktop\Afeka\VideoTherapy\VideoTherapy - Client\VideoTherapy\VideoTherapy\VideoTherapy\DB\squat.gbd";
        }

        public string CreateGestures2()
        {
            VTGesture g1 = new VTGesture();
            g1.GestureName = "limbs_up_Right";
            g1.MinProgressValue = 0;
            g1.MaxProgressValue = 0.5f;
            g1.TrsholdProgressValue = 0.25f;
            g1.ConfidanceTrshold = 0.4f;
            g1.IsTrack = true;

            VTGesture g2 = new VTGesture();
            g2.GestureName = "limbs_down_Right";
            g2.MinProgressValue = 0.5f;
            g2.MaxProgressValue = 1.0f;
            g2.TrsholdProgressValue = 0.75f;
            g2.ConfidanceTrshold = 0.4f;
            g2.IsTrack = true;

            VTGesture g3 = new VTGesture();
            g3.GestureName = "limbs_up_Left";
            g3.MinProgressValue = 1.0f;
            g3.MaxProgressValue = 0.5f;
            g3.TrsholdProgressValue = 0.75f;
            g3.ConfidanceTrshold = 0.4f;
            g3.IsTrack = true;

            VTGesture g4 = new VTGesture();
            g4.GestureName = "limbs_down_Left";
            g4.MinProgressValue = 0.5f;
            g4.MaxProgressValue = 0.0f;
            g4.TrsholdProgressValue = 0.25f;
            g4.ConfidanceTrshold = 0.4f;
            g4.IsTrack = true;

            StartGesutre = new VTGesture();
            StartGesutre.GestureName = "limbs_start";
            StartGesutre.MinProgressValue = 0;
            StartGesutre.MaxProgressValue = 1.0f;
            StartGesutre.TrsholdProgressValue = 0.01f;
            StartGesutre.ConfidanceTrshold = 0.0f;
            StartGesutre.IsTrack = true;

            ContinuousGestureName = "lower_limbsProgress";

            VTGestureList = new List<VTGesture>();
            VTGestureList.Add(g1);
            VTGestureList.Add(g2);
            VTGestureList.Add(g3);
            VTGestureList.Add(g4);

            return @"C:\Users\amirb\Desktop\Afeka\VideoTherapy\VideoTherapy - Client\VideoTherapy\VideoTherapy\VideoTherapy\DB\lower_limbs.gbd";
        }*/
    }
}
