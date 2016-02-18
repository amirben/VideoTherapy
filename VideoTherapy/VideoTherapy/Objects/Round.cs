using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

using VideoTherapy.Utils;

namespace VideoTherapy.Objects
{
    public class Round : INotifyPropertyChanged
    {
        public int RoundNumber { set; get; }
        public Boolean RoundSuccess { set; get; }
        public Dictionary<string, VTGesture> GestureList;

        public event PropertyChangedEventHandler PropertyChanged;

        public int RoundRepetitions { set; get; }
        public float RoundProgress { set; get; }
        public int RoundMotionQuality { set; get; }

        public event Exercise.NextRoundDelegate NextRoundEvent;

        private string LastGesture = "squat_getting_up";

        public Round(int _roundNumber, int repetitions)
        {
            RoundNumber = _roundNumber;
            RoundRepetitions = repetitions;
            GestureList = new Dictionary<string, VTGesture>();
    
        }

        public void UpdateGestureDetection(string _gestureName, DiscreteGestureResult _gestureResult, float _gestureProgress)
        {
            if (GestureList.ContainsKey(_gestureName))
            {
                VTGesture _gesture = null;
                GestureList.TryGetValue(_gestureName, out _gesture);

                _gesture.ConfidenceValue = _gestureResult.Confidence;

                if (_gestureResult.Detected)
                {
                    _gesture.ProgressValue = _gestureProgress;
                }

                if (_gestureResult.Detected && _gesture.IsPassConfidanceTrshold() && _gesture.IsPassProgressTrshold())                                            
                {
                    _gesture.IsSuccess = true;
                    CheckProgress();
                }

                if (CheckRoundSuccess() && _gestureProgress <= 0.0f && !_gestureResult.Detected)
                {
                    //todo throw next round event
                    NextRoundEvent();
                }
            }
        }

        public Boolean CheckRoundSuccess()
        {
            // check if round finshed
            foreach (VTGesture _gesture in GestureList.Values)
            {
                if (!_gesture.IsSuccess)
                {
                    RoundSuccess = false;
                    return false;
                }

            }

            RoundSuccess = true;
            return true;
        }

        public void CheckProgress()
        {
            RoundProgress = Scoring.GetRoundProgress(GestureList);
            this.NotifyPropertyChanged("RoundProgress");

            RoundMotionQuality = Convert.ToInt32(Scoring.GetRoundMotionScore(GestureList));
            this.NotifyPropertyChanged("RoundMotionQuality");
        }


        private void printStatus()
        {
            Console.Write("Round num {0}   ", RoundNumber);

            foreach (VTGesture g in GestureList.Values)
            {
                Console.Write("n = {0}, p = {1} | ", g.GestureName, g.ProgressValue);

            }
            Console.WriteLine();
        }

        /// <summary>
        /// Notifies UI that a property has changed
        /// </summary>
        /// <param name="propertyName">Name of property that has changed</param> 
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    }
}
