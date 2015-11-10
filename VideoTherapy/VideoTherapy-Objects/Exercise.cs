using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VideoTherapy_Objects
{
    public class Exercise : INotifyPropertyChanged
    {
        public int ExerciseId { set; get; }
        public int ExerciseNum { set; get; }
        public String ExerciseName { set; get; }
        public int Repetitions { set; get; }
        public string ExerciseThumbs { set; get; }
        public string VideoPath { set; get; }
        public string DBPath { set; get; }

        //for demo video
        public Boolean isDemo { set; get; }
        public Boolean isShow { set; get; }

        //for download - because there are repeated videos & images
        public Boolean isDuplicate { set; get; }
        public Boolean Downloaded { set; get; }

        public Boolean isPause { set; get; }
        public Boolean isStart { set; get; }

        //Exercise gestures -> used for init gesture list for this exercise
        public List<VTGesture> VTGestureList { set; get; }
        public string ContinuousGestureName { set; get; }
        public VTGesture StartGesutre { set; get; }

        //Rounds
        public ObservableCollection<Round> Rounds;
        public Round CurrentRound;
        public int RoundIndex = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public delegate void NextRoundDelegate();

        public Exercise()
        {
            NextRoundDelegate nextRoundDelegate = new NextRoundDelegate(NextRound);

        }

        public void AddNextRoundEvent()
        {
            NextRoundDelegate nextRoundDelegate = new NextRoundDelegate(NextRound);
            CurrentRound.NextRoundEvent += CurrentRound_NextRoundEvent;
        }

        private void CurrentRound_NextRoundEvent()
        {
            NextRound();
        }

        //Next round
        public void NextRound()
        {
            if (!isPause)
            {
                RoundIndex++;
                this.NotifyPropertyChanged("RoundIndex");

                if (RoundIndex < Repetitions)
                {
                    CurrentRound = Rounds[RoundIndex];
                }

                else
                {
                    //todo need to play next exercise -> throw event for next video
                }
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


        //DELETE AFTER DEMO
        public void createGestures()
        {
            VTGesture g1 = new VTGesture();
            g1.GestureName = "squat_getting_down";
            g1.MinProgressValue = 0;
            g1.MaxProgressValue = 1.0f;
            g1.TrsholdProgressValue = 0.3f;
            g1.ConfidanceTrshold = 0.0f;
            g1.IsTrack = true;

            VTGesture g2 = new VTGesture();
            g2.GestureName = "squat_getting_up";
            g2.MinProgressValue = 1.0f;
            g2.MaxProgressValue = 0.0f;
            g2.TrsholdProgressValue = 0.3f;
            g2.ConfidanceTrshold = 0.0f;
            g2.IsTrack = true;

            StartGesutre = new VTGesture();
            StartGesutre.GestureName = "squat_start";
            StartGesutre.MinProgressValue = 0;
            StartGesutre.MaxProgressValue = 1.0f;
            StartGesutre.TrsholdProgressValue = 0.3f;
            StartGesutre.ConfidanceTrshold = 0.0f;
            StartGesutre.IsTrack = true;

            ContinuousGestureName = "squatProgress";

            VTGestureList = new List<VTGesture>();
            VTGestureList.Add(g1);
            VTGestureList.Add(g2);
        }
    }
}
