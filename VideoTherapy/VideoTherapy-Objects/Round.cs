//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Kinect;
//using Microsoft.Kinect.VisualGestureBuilder;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;


//namespace VideoTherapy_Objects
//{
//    public class Round : INotifyPropertyChanged
//    {
//        public int RoundNumber { set; get; }
//        public Boolean RoundSuccess { set; get; }
//        public Dictionary<string, VTGesture> GestureList;

//        public event PropertyChangedEventHandler PropertyChanged;

//        public int RoundRepetitions { set; get; }
//        public float RoundProgress { set; get; }

//        public event Exercise.NextRoundDelegate NextRoundEvent;

//        public Round(int _roundNumber, int repetitions)
//        {
//            RoundNumber = _roundNumber;
//            RoundRepetitions = repetitions;
//            GestureList = new Dictionary<string, VTGesture>();
    
//        }

//        public void UpdateGestureDetection(string _gestureName, DiscreteGestureResult _gestureResult, float _gestureProgress)
//        {
//            if (GestureList.ContainsKey(_gestureName))
//            {
//                VTGesture _gesture = null;
//                GestureList.TryGetValue(_gestureName, out _gesture);

//                _gesture.ConfidenceValue = _gestureResult.Confidence;

//                if (_gestureResult.Detected)
//                {
//                    _gesture.ProgressValue = _gestureProgress;
//                }

//                if (_gestureResult.Detected && _gesture.IsPassConfidanceTrshold() && _gesture.IsPassProgressTrshold())                                            
//                {
//                    _gesture.IsSuccess = true;
//                    CheckProgress();
//                }

//                if (CheckRoundSuccess())
//                {
//                    //todo throw next round event
//                    NextRoundEvent();
//                }
//            }
//        }

//        public Boolean CheckRoundSuccess()
//        {
//            // check if round finshed
//            foreach (VTGesture _gesture in GestureList.Values)
//            {
//                if (!_gesture.IsSuccess)
//                {
//                    return false;
//                }

//            }

//            RoundSuccess = true;
//            return true;
//        }

//        public void CheckProgress()
//        {
//            int sum = 0;

//            foreach (VTGesture _gesture in GestureList.Values)
//            {
//                if (_gesture.IsSuccess)
//                {
//                    sum++;
//                }

//            }

//            RoundProgress = ((float)sum / (float)GestureList.Count) * 100;
//            this.NotifyPropertyChanged("RoundProgress");
//        }


//        /// <summary>
//        /// Notifies UI that a property has changed
//        /// </summary>
//        /// <param name="propertyName">Name of property that has changed</param> 
//        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
//        {
//            if (this.PropertyChanged != null)
//            {
//                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
//            }
//        }


//    }
//}
