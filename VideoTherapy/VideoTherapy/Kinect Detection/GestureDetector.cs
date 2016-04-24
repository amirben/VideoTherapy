using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using VideoTherapy.Objects;
using System.Windows.Threading;
using System.IO;

namespace VideoTherapy.Kinect_Detection
{
    /// <summary>
    /// Gesture Detector class which listens for VisualGestureBuilderFrame events
    /// </summary>
    public class GestureDetector : IDisposable
    {
        /// <summary> Gesture frame source which should be tied to a body tracking ID </summary>
        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        /// <summary> Gesture frame reader which will handle gesture events coming from the sensor </summary>
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        private KinectSensor _sensor = null;

        private GestureAnalysis _gestureAnalysis;

        private Gesture ContinuousGestureData = null;

        public Boolean StopDetection { set; get; }

        //Constractor
        public GestureDetector(KinectSensor _sensor, GestureAnalysis _gestureAnalysis, Exercise _currentExercise)
        {
            if (_sensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }

            if (_gestureAnalysis == null)
            {
                throw new ArgumentNullException("gestureResultView");
            }

            this._sensor = _sensor;
            this._gestureAnalysis = _gestureAnalysis;


            // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
            vgbFrameSource = new VisualGestureBuilderFrameSource(_sensor, 0);
            //vgbFrameSource.TrackingIdLost += Source_TrackingIdLost;

            // open the reader for the vgb frames
            vgbFrameReader = vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null)
            {
                vgbFrameReader.IsPaused = true;
                vgbFrameReader.FrameArrived += VgbFrameReader_FrameArrived;
            }

            // load all gestures from the gesture database
            if (File.Exists(_currentExercise.DBPath))
            {
                using (var database = new VisualGestureBuilderDatabase(_currentExercise.DBPath))
                {
                    vgbFrameSource.AddGestures(database.AvailableGestures);

                    //setup the continuous gesture
                    foreach (var gesture in database.AvailableGestures)
                    {
                        if (gesture.Name.Equals(_currentExercise.ContinuousGestureName))
                        {
                            ContinuousGestureData = gesture;
                            break;
                        }
                    }
                }

                //todo - implmnt gesture disable
                foreach (var gesutre in this.vgbFrameSource.Gestures)
                {
                    foreach (var notTrackGesture in _currentExercise.VTGestureList)
                    {
                        if (gesutre.Name.Equals(notTrackGesture.GestureName) && !notTrackGesture.IsTrack)
                        {
                            vgbFrameSource.SetIsEnabled(gesutre, false);
                        }
                    }
                }
            }
            
        }

        private void VgbFrameReader_FrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            float progress = 0;

            //using (var frame = vgbFrameReader.CalculateAndAcquireLatestFrame())
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    // get all discrete and continuous gesture results that arrived with the latest frame
                    var discreteResults = frame.DiscreteGestureResults;
                    var continuousResults = frame.ContinuousGestureResults;

                    if (discreteResults != null)
                    {
                        foreach (var gesture in vgbFrameSource.Gestures)
                        {
                            DiscreteGestureResult discreteResult = null;
                            ContinuousGestureResult continuousResult = null;

                            if (gesture.GestureType == GestureType.Discrete)
                            {
                                discreteResults.TryGetValue(gesture, out discreteResult);

                                if (discreteResult != null)
                                {
                                    continuousResults.TryGetValue(ContinuousGestureData, out continuousResult);

                                    if (continuousResult != null)
                                    {
                                        progress = continuousResult.Progress;

                                        if (progress < 0f)
                                        {
                                            progress = 0.0f;
                                        }
                                        else if (progress > 1.0)
                                        {
                                            progress = 1.0f;
                                        }
                                    }
                                }
                            }

                            //update the analyzer
                            if (continuousResult != null && discreteResult != null)
                            {
                                //if (!StopDetection)
                                _gestureAnalysis.UpdateGestureResult(gesture.Name, discreteResult, progress);
                                
                                //Console.WriteLine("Gesture {0}, confidance {1}, progress", gesture.Name, discreteResult.Confidence, progress);
                            }
                        }// foreach
                    }//if (discrete != null)
                    //else
                    //{
                    //    //_gestureAnalysis.CheckIfRoundSucces();
                    //}
                }//if (frame != null)
            }//using
        }

        //public void GestureDetectionToAnalyze()
        //{
        //    float progress = 0;

        //    using (var frame = vgbFrameReader.CalculateAndAcquireLatestFrame())
        //    {
        //        if (frame != null)
        //        {
        //            // get all discrete and continuous gesture results that arrived with the latest frame
        //            var discreteResults = frame.DiscreteGestureResults;
        //            var continuousResults = frame.ContinuousGestureResults;
                    
        //            if (discreteResults != null)
        //            {
        //                foreach (var gesture in vgbFrameSource.Gestures)
        //                {
        //                    DiscreteGestureResult discreteResult = null;
        //                    ContinuousGestureResult continuousResult = null;

        //                    if (gesture.GestureType == GestureType.Discrete)
        //                    {
        //                        discreteResults.TryGetValue(gesture, out discreteResult);

        //                        if (discreteResult != null)
        //                        {
        //                            continuousResults.TryGetValue(ContinuousGestureData, out continuousResult);

        //                            if (continuousResult != null)
        //                            {
        //                                progress = continuousResult.Progress;

        //                                if (progress < 0f)
        //                                {
        //                                    progress = 0.0f;
        //                                }
        //                                else if (progress > 1.0)
        //                                {
        //                                    progress = 1.0f;
        //                                }
        //                            }
        //                        }
        //                    }

        //                    //update the analyzer
        //                    if (continuousResult != null && discreteResult != null)
        //                    {
        //                        //if (!StopDetection)
        //                        _gestureAnalysis.UpdateGestureResult(gesture.Name, discreteResult, progress);
        //                        //Console.WriteLine("Gesture {0}, confidance {1}, progress", gesture.Name, discreteResult.Confidence, progress);
        //                    }
        //                }// foreach
        //            }//if (discrete != null)
        //            else
        //            {
        //                //_gestureAnalysis.CheckIfRoundSucces();
        //            }
        //        }//if (frame != null)
        //    }//using
        //}


        /// <summary>
        /// Gets or sets the body tracking ID associated with the current detector
        /// The tracking ID can change whenever a body comes in/out of scope
        /// </summary>
        public ulong TrackingId
        {
            get
            {
                return this.vgbFrameSource.TrackingId;
            }

            set
            {
                if (this.vgbFrameSource.TrackingId != value)
                {
                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }



        /// <summary>
        /// Gets or sets a value indicating whether or not the detector is currently paused
        /// If the body tracking ID associated with the detector is not valid, then the detector should be paused
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return this.vgbFrameReader.IsPaused;
            }

            set
            {
                if (this.vgbFrameReader.IsPaused != value)
                {
                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        /// <summary>
        /// Disposes all unmanaged resources for the class
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader objects
        /// </summary>
        /// <param name="disposing">True if Dispose was called directly, false if the GC handles the disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.vgbFrameReader != null)
                {
                    //this.vgbFrameReader.FrameArrived -= this.Reader_GestureFrameArrived;
                    this.vgbFrameReader.Dispose();
                    this.vgbFrameReader = null;
                }

                if (this.vgbFrameSource != null)
                {
                    //this.vgbFrameSource.TrackingIdLost -= this.Source_TrackingIdLost;
                    this.vgbFrameSource.Dispose();
                    this.vgbFrameSource = null;
                }
            }
        }
    }
}
