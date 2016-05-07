using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect.VisualGestureBuilder;
using VideoTherapy.Objects;
using VideoTherapy.Views;

namespace VideoTherapy.Kinect_Detection
{
    public class GestureAnalysis
    {
        public Exercise CurrentExercise { set; get; }

        public event ExerciseView.StartGestureDetected startGestureDeteced;

        public GestureAnalysis(ref Exercise _currentExercise)
        {
            if (_currentExercise == null)
            {
                throw new ArgumentNullException("No exercise");
            }

            this.CurrentExercise = _currentExercise;
        }

        public void UpdateGestureResult(string gestureName, DiscreteGestureResult result, float progress)
        {
            ////Checking for start geture
            //if (!CurrentExercise.IsStart)
            //{
            //    VTGesture startGesture = CurrentExercise.StartGesutre;

            //    //checking if the gesture is the start
            //    if (gestureName.Equals(startGesture.GestureName))
            //    {
            //        //checking if it detected
            //        // todo - add confidace threshold
            //        if (result.Detected ) //&& result.Confidence >= startGesture.ConfidanceTrshold)
            //        {
            //            startGesture.IsSuccess = result.Detected;
            //            //startGesture.ConfidenceValue = result.Confidence;
            //            CurrentExercise.IsStart = true;

            //            //throw event to UI
            //            startGestureDeteced();
            //        }
            //    }
            //}
            //else
            //{
               
            //}

            if (!CurrentExercise.IsFinish)
            {
                if (result.Detected)
                {
                    CurrentExercise.IsStart = true;
                    startGestureDeteced();
                }

                if (progress <= 0.02f && result.Confidence <= 0.001f && !result.Detected)
                {
                    CheckIfRoundSucces();
                }
                else
                {
                    CurrentExercise.CurrentRound.UpdateGestureDetection(gestureName, result, progress);
                    
                }
            }
        }

        public void CheckIfRoundSucces()
        {
            CurrentExercise.CurrentRound.CheckRoundSuccess();
        }
    }
}
