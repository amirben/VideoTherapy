﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Microsoft.Kinect;
using VideoTherapy.Objects;
using VideoTherapy.Kinect;
using VideoTherapy.Kinect_Detection;
using System.ComponentModel;
using System.Windows.Threading;

using VideoTherapy.Views.ExerciseScreen;
using VideoTherapy.Utils;

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for ExerciseView.xaml
    /// </summary>
    /// 
    public partial class ExerciseView : UserControl,  IDisposable
    {
        public Exercise CurrentExercise { set; get; }

        public List<Exercise> Playlist { set; get; }
        public Dictionary<int, List<Exercise>> Playlist2 { set; get; }
        private int _currentSessionId;

        public Patient CurrentPatient;
        public Training CurrentTraining;

        public MainWindow MainWindow { set; get; }

        private int _currentExerciseIndex;
        private Boolean _playPause;

        private KinectSensor _sensor;
        private MultiSourceFrameReader _reader;

        private BackgroundRemovalTool _backgroundRemovalTool;
        private DrawSkeleton _drawSkeleton;

        private GestureAnalysis _gestureAnalysis;
        private GestureDetector _gestureDetector;
        
        // Array for the bodies
        private Body[] bodies = null;

        // index for the currently tracked body
        private ulong bodyIndex;

        // flag to asses if a body is currently tracked
        private bool _bodyTracked = false;
        public bool BodyTracked
        {
            get
            {
                return _bodyTracked;
            }

            set
            {
                _bodyTracked = value;
                CheckForPause();
            }
        }

        //pause popup
        private PausePopUp pausePopUp;

        //summary popup
        private SummaryPopUp summary;

        // questionnaire popup
        private QuestionnairePopUp questionnairePopUp;

        //questionnaire finished popup
        private QuestionFinishedPopUp questionnaireFinishedPopUp;

        //Nottrackable popup
        private NoTrackingPopUp noTrackingPopUp;

        //delegates
        public delegate void NextRoundUpdataDelegate();
        public delegate void StopDetectionDelegate();
        public delegate void StartGestureDetected();

        private DispatcherTimer UITimerChange;
        private Boolean StopDetection = false;

        //close app
        public event MainWindow.CloseAppDelegate CloseApp;


        public ExerciseView(Patient currentPatient, Training currentTraining)
        {
            InitializeComponent();

            CurrentPatient = currentPatient;
            CurrentTraining = currentTraining;

            Playlist2 = currentTraining.Playlist2;
            CurrentExercise = Playlist2[1][0];
            _currentSessionId = 1;

            NextRoundUpdataDelegate nextRoundUpdateDelegate = new NextRoundUpdataDelegate(UpdateDataContext);
            CurrentExercise.NextRoundUpdateUIEvent += CurrentExercise_NextRoundUpdateUIEvet;
            //CurrentExercise.NextRoundUpdateUIEvent += new NextRoundUpdataDelegate(UpdateDataContext);
            CurrentExercise.StopDetectionEvent += CurrentExercise_StopDetectionEvent;

            //CurrentExercise.PropertyChanged += CurrentExercise_PropertyChanged;

            //Data context update
            UpdateDataContext();

            this.Loaded += ExerciseView_Loaded;
            InitGestureDetection();

            ExerciseVideo.Play();
            _playPause = true;

            UITimerChange = new DispatcherTimer();
            UITimerChange.Interval = new TimeSpan(0, 0, 0, 0, 700);
            UITimerChange.Tick += UITimerChange_Tick;
        }

        private void UITimerChange_Tick(object sender, EventArgs e)
        {

            ExerciseStatus.DataContext = CurrentExercise.CurrentRound;
            RoundIndexText.DataContext = CurrentExercise.CurrentRound;
            RoundMotionQualityGrid.DataContext = CurrentExercise.CurrentRound;
            StopDetection = false;
            _gestureDetector.StopDetecion = false;

            printStatus();

            UITimerChange.Stop();
        }

        private void UpdateDataContext()
        {
            //Data context update
            DataContext = CurrentExercise;
            UserProfile.DataContext = CurrentPatient;
            CurrentTrainingLbl.DataContext = CurrentTraining;

            ExerciseMotionQualityGrid.DataContext = CurrentExercise;

            ExerciseStatus.DataContext = CurrentExercise.CurrentRound;
            RoundIndexText.DataContext = CurrentExercise.CurrentRound;
            RoundMotionQualityGrid.DataContext = CurrentExercise.CurrentRound;
        }

        private void CurrentExercise_StopDetectionEvent()
        {
            StopDetection = true;
            _gestureDetector.StopDetecion = true;
        }

        private void CurrentExercise_NextRoundUpdateUIEvet()
        {
            UITimerChange.Start();

            //ExerciseStatus.DataContext = CurrentExercise.CurrentRound;
            //RoundIndexText.DataContext = CurrentExercise.CurrentRound;
            //RoundMotionQualityGrid.DataContext = CurrentExercise.CurrentRound;
        }

        private void ExerciseView_Loaded(object sender, RoutedEventArgs e)
        {

            CreatePopUps();

            //_sensor = KinectSensor.GetDefault();
            ////todo - check, not working
            //if (_sensor != null)
            //{
            //    _sensor.Open();

            //    // 2) Initialize the background removal tool.
            //    _backgroundRemovalTool = new BackgroundRemovalTool(_sensor.CoordinateMapper);
            //    _drawSkeleton = new DrawSkeleton(_sensor,(int) (KinectSkeleton.Width), (int)(KinectSkeleton.Height));

            //    _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex | FrameSourceTypes.Body);
            //    _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

            //    //Gesture detection
            //    Exercise tempExercise = CurrentExercise;
            //    _gestureAnalysis = new GestureAnalysis(ref tempExercise);
            //    _gestureDetector = new GestureDetector(_sensor, _gestureAnalysis, CurrentExercise);

            //    _gestureAnalysis.startGestureDeteced += _gestureAnalysis_startGestureDeteced;

            //    //ExerciseVideo.Source = new Uri("http://mil01.objectstorage.softlayer.net/v1/AUTH_2bc94b1c-c83f-4247-b43c-c1dfaf29db00/vtexercisevideos2/he/82.mp4");              

            //    //_timer.Start();
            //    ExerciseVideo.Play();
            //    _playPause = true;
            //}

        }

        private void InitGestureDetection()
        {
            if (!CurrentExercise.isDemo && CurrentExercise.isTrackable)
            {
                SetUIInTrackingMode();

                _sensor = KinectSensor.GetDefault();
                //todo - check, not working
                if (_sensor != null)
                {
                    _sensor.Open();

                    // 2) Initialize the background removal tool.
                    _backgroundRemovalTool = new BackgroundRemovalTool(_sensor.CoordinateMapper);
                    _drawSkeleton = new DrawSkeleton(_sensor, (int)(KinectSkeleton.Width), (int)(KinectSkeleton.Height));

                    _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex | FrameSourceTypes.Body);
                    _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

                    //Gesture detection
                    Exercise tempExercise = CurrentExercise;
                    
                    _gestureAnalysis = new GestureAnalysis(ref tempExercise);
                    _gestureDetector = new GestureDetector(_sensor, _gestureAnalysis, CurrentExercise);

                    _gestureAnalysis.startGestureDeteced += _gestureAnalysis_startGestureDeteced;

                    CurrentExercise.CreateRounds();

                    //_timer.Start();
                    ExerciseVideo.Play();
                    _playPause = true;
                }
            }
            else
            {
                if (CurrentExercise.isDemo)
                {
                    SetUIInDemoMode();
                }

                else
                {
                    if (!CurrentExercise.isTrackable)
                    {
                        OpenNotTrackablePopUp();
                        SetUIInNoTrackableMode();
                    }
                }

            }


        }

        private void _gestureAnalysis_startGestureDeteced()
        {
            WaitForStartLbl.Visibility = Visibility.Hidden;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            if (!CurrentExercise.isDemo)
            {
                bool detected = false;

                var reference = e.FrameReference.AcquireFrame();

                using (var colorFrame = reference.ColorFrameReference.AcquireFrame())
                using (var depthFrame = reference.DepthFrameReference.AcquireFrame())
                using (var bodyFrame = reference.BodyFrameReference.AcquireFrame())
                using (var bodyIndexFrame = reference.BodyIndexFrameReference.AcquireFrame())
                {
                    if (colorFrame != null && depthFrame != null && bodyIndexFrame != null && bodyFrame != null)
                    {
                        // 3) Update the image source.
                        KinectShilloute.Source = _backgroundRemovalTool.GreenScreen(colorFrame, depthFrame, bodyIndexFrame);

                        KinectSkeleton.Source = _drawSkeleton.DrawBodySkeleton(bodyFrame);

                        detected = GetTrackingId(bodyFrame);
                        
                        //_gestureDetector.IsPaused = !BodyTracked;
                        //_gestureDetector.TrackingId = bodyIndex;
                        //Console.WriteLine("Pause: {0}, ID: {1}", _gestureDetector.IsPaused, _gestureDetector.TrackingId);
                        //_gestureDetector.GestureDetectionToAnalyze();
                    }
                }

                if (detected && !StopDetection)
                {
                    
                    ClosePausePopUp();

                    Body activeBody = this.bodies[this.bodyIndex];

                    // visualize the new gesture data
                    if (activeBody.TrackingId != this._gestureDetector.TrackingId)
                    {
                        // if the tracking ID changed, update the detector with the new value
                        this._gestureDetector.TrackingId = activeBody.TrackingId;
                    }

                    if (this._gestureDetector.TrackingId == 0)
                    {
                        // the active body is not tracked, pause the detector and update the UI
                        this._gestureDetector.IsPaused = true;
                    }
                    else
                    {
                        // the active body is tracked, unpause the detector
                        this._gestureDetector.IsPaused = false;

                        // get the latest gesture frame from the sensor and updates the UI with the results
                        this._gestureDetector.GestureDetectionToAnalyze();
                    }
                }

                //todo - update the logic of pop-up pause
                else
                {
                    //OpenPausePopUp();
                    if (!BodyTracked)
                        OpenPausePopUp();
                }

            }
            else
            {
                ExerciseStatus.Visibility = Visibility.Hidden;
                DemoLbl.Visibility = Visibility.Visible;
            }


        }

        //used for check if body is track
        public bool GetTrackingId(BodyFrame bodyFrame)
        {   
            if (bodies == null)
            {
                bodies = new Body[bodyFrame.BodyCount];
            }

            bodyFrame.GetAndRefreshBodyData(bodies);

            Body body = null;

            if (BodyTracked)
            {
                if (bodies[bodyIndex].IsTracked)
                {
                    body = bodies[bodyIndex];
                }
                else
                {
                    BodyTracked = false;
                }
            }
            if (!BodyTracked)
            {
                for (int i = 0; i < bodies.Length; ++i)
                {
                    if (bodies[i].IsTracked)
                    {
                        bodyIndex = (ulong) i;
                        BodyTracked = true;
                        break;
                    }
                    else
                    {
                        bodyIndex = 0;
                    }
                }
            }

            return BodyTracked;
        }
  
        private void CheckForPause()
        {
            if (BodyTracked)
            {
                ClosePausePopUp();
            }
            else
            {
                OpenPausePopUp();
            }
        }

        private void PlayNextVideo()
        {
            if (!(_currentExerciseIndex + 1 >= Playlist.Count))
            {
                _currentExerciseIndex++;
                //CurrentExercise = Playlist[_currentExerciseIndex];

                //clearing the view
                KinectShilloute.Source = null;
                KinectSkeleton.Source = null;

                //NEW
                Dispose();
                InitGestureDetection();

                DataContext = CurrentExercise;
                //_gestureAnalysis.CurrentExercise = CurrentExercise;

                NextRoundUpdataDelegate nextRoundUpdateDelegate = new NextRoundUpdataDelegate(UpdateDataContext);
                CurrentExercise.NextRoundUpdateUIEvent += CurrentExercise_NextRoundUpdateUIEvet;
                //CurrentExercise.NextRoundUpdateUIEvent += new NextRoundUpdataDelegate(UpdateDataContext);
                CurrentExercise.StopDetectionEvent += CurrentExercise_StopDetectionEvent;


                UpdateDataContext();
                ExerciseStatus.DataContext = CurrentExercise.CurrentRound;
                RoundIndexText.DataContext = CurrentExercise.CurrentRound;
                RoundMotionQualityGrid.DataContext = CurrentExercise.CurrentRound;

                ExerciseVideo.Play();
            }

            //End of training
            else
            {
                OpenSummaryPopUp();
                CloseAllComponents();
            }
        }

        private void PlayNextVideo2()
        {
            CurrentExercise.CheckIfExerciseStatus();

            //incase there is more then one repeation in session (some exercise many times)
            if (!CurrentTraining.Playlist2[_currentSessionId].Last().Equals(CurrentExercise))
            {
                //clearing the view
                KinectShilloute.Source = null;
                KinectSkeleton.Source = null;

                //NEW
                Dispose();

                int i = CurrentTraining.Playlist2[_currentSessionId].IndexOf(CurrentExercise);
                CurrentExercise = CurrentTraining.Playlist2[_currentSessionId][i + 1];

                InitGestureDetection();

                DataContext = CurrentExercise;
                //_gestureAnalysis.CurrentExercise = CurrentExercise;

                NextRoundUpdataDelegate nextRoundUpdateDelegate = new NextRoundUpdataDelegate(UpdateDataContext);
                CurrentExercise.NextRoundUpdateUIEvent += CurrentExercise_NextRoundUpdateUIEvet;
                CurrentExercise.NextRoundUpdateUIEvent += new NextRoundUpdataDelegate(UpdateDataContext);
                CurrentExercise.StopDetectionEvent += CurrentExercise_StopDetectionEvent;


                UpdateDataContext();
                ExerciseStatus.DataContext = CurrentExercise.CurrentRound;
                RoundIndexText.DataContext = CurrentExercise.CurrentRound;
                RoundMotionQualityGrid.DataContext = CurrentExercise.CurrentRound;

                WaitForStartLbl.Visibility = Visibility.Visible;
                ExerciseVideo.Play();
            }
            else
            {
                //in case there is no more exercise in the same session (new type of exercise)
                if (Playlist2.ContainsKey(_currentSessionId + 1))
                {
                    //MoveInDictionary(1);
                    _currentSessionId++;
                    PlayNextVideo2();
                }
                else
                {
                    //show questionere pop-up
                    OpenSummaryPopUp();
                    CloseAllComponents();
                }
            }
            
        }

        private void PlayPrevVideo2()
        {
            //incase there is more then one repeation in session (some exercise many times)
            if (!CurrentTraining.Playlist2[_currentSessionId].First().Equals(CurrentExercise))
            {
                //clearing the view
                KinectShilloute.Source = null;
                KinectSkeleton.Source = null;

                //NEW
                Dispose();

                int i = CurrentTraining.Playlist2[_currentSessionId].IndexOf(CurrentExercise);
                CurrentExercise = CurrentTraining.Playlist2[_currentSessionId][i - 1];

                InitGestureDetection();

                DataContext = CurrentExercise;
                //_gestureAnalysis.CurrentExercise = CurrentExercise;

                NextRoundUpdataDelegate nextRoundUpdateDelegate = new NextRoundUpdataDelegate(UpdateDataContext);
                CurrentExercise.NextRoundUpdateUIEvent += CurrentExercise_NextRoundUpdateUIEvet;
                //CurrentExercise.NextRoundUpdateUIEvent += new NextRoundUpdataDelegate(UpdateDataContext);
                CurrentExercise.StopDetectionEvent += CurrentExercise_StopDetectionEvent;


                UpdateDataContext();
                ExerciseStatus.DataContext = CurrentExercise.CurrentRound;
                RoundIndexText.DataContext = CurrentExercise.CurrentRound;
                RoundMotionQualityGrid.DataContext = CurrentExercise.CurrentRound;

                ExerciseVideo.Play();
            }
            else
            {
                //in case there is no more exercise in the same session (new type of exercise)
                if (Playlist2.ContainsKey(_currentSessionId - 1))
                {
                    KinectShilloute.Source = null;
                    KinectSkeleton.Source = null;

                    //NEW
                    Dispose();

                    _currentSessionId--;
                    CurrentExercise = Playlist2[_currentSessionId].Last();

                    InitGestureDetection();

                    DataContext = CurrentExercise;
                    //_gestureAnalysis.CurrentExercise = CurrentExercise;

                    NextRoundUpdataDelegate nextRoundUpdateDelegate = new NextRoundUpdataDelegate(UpdateDataContext);
                    CurrentExercise.NextRoundUpdateUIEvent += CurrentExercise_NextRoundUpdateUIEvet;
                    //CurrentExercise.NextRoundUpdateUIEvent += new NextRoundUpdataDelegate(UpdateDataContext);
                    CurrentExercise.StopDetectionEvent += CurrentExercise_StopDetectionEvent;


                    UpdateDataContext();
                    ExerciseStatus.DataContext = CurrentExercise.CurrentRound;
                    RoundIndexText.DataContext = CurrentExercise.CurrentRound;
                    RoundMotionQualityGrid.DataContext = CurrentExercise.CurrentRound;

                    ExerciseVideo.Play();
                }
            }

        }

        private void SetUIInNoTrackableMode()
        {
            RoundMotionPanel.Visibility = Visibility.Collapsed;
            NARoundMotion.Visibility = Visibility.Visible;

            ExerciseMotionPanel.Visibility = Visibility.Collapsed;
            NAExerciseMotion.Visibility = Visibility.Visible;

            ExerciseStatus.Visibility = Visibility.Visible;
            RoundIndexPanel.Visibility = Visibility.Collapsed;
            ExerciseRepetionsLbl.Visibility = Visibility.Collapsed;
            NARoundIndex.Visibility = Visibility.Visible;
            NotTraceableLbl.Visibility = Visibility.Visible;

            DemoStatus.Visibility = Visibility.Collapsed;
        }

        private void SetUIInDemoMode()
        {
            ExerciseStatus.Visibility = Visibility.Collapsed;

            RoundMotionPanel.Visibility = Visibility.Collapsed;
            NARoundMotion.Visibility = Visibility.Visible;

            ExerciseMotionPanel.Visibility = Visibility.Collapsed;
            NAExerciseMotion.Visibility = Visibility.Visible;

            DemoStatus.Visibility = Visibility.Visible;
        }

        private void SetUIInTrackingMode()
        {
            UpdateDataContext();
            
            //Demo panel - set off
            DemoStatus.Visibility = Visibility.Collapsed;

            ExerciseStatus.Visibility = Visibility.Visible;
            RoundIndexPanel.Visibility = Visibility.Visible;
            ExerciseRepetionsLbl.Visibility = Visibility.Visible;
            NARoundIndex.Visibility = Visibility.Collapsed;
            NotTraceableLbl.Visibility = Visibility.Collapsed;
            
            //not trackable - set off
            RoundMotionPanel.Visibility = Visibility.Visible;
            NARoundMotion.Visibility = Visibility.Collapsed;

            ExerciseMotionPanel.Visibility = Visibility.Visible;
            NAExerciseMotion.Visibility = Visibility.Collapsed;


        }

        #region popups
    
        private void OpenSummaryPopUp()
        {
            summary = new SummaryPopUp();
            int width = (int)(ActualWidth / 2.5);
            int height = (int)(ActualHeight * 0.75);

            summary.SetSize(height, width);

            //CurrentTraining.TrainingScore = Convert.ToInt32(Scoring.GetTrainingScore(CurrentTraining));

            summary.CurrentTraining = CurrentTraining;
            summary.CurrentPatient = CurrentPatient;
            summary.ExerciseView = this;
            summary.UpdateScore();

            ExerciseWindow.Children.Add(summary);
        }

        public void ClosedSummaryPopUp(Boolean goToQuestions)
        {
            ExerciseWindow.Children.Remove(summary);
            summary = null;

            //if the user decided to answer the questions
            if (goToQuestions)
            {
                OpenQuestionnairePopUp();
            }
            else
            {
                MainWindow.OpenTreatmentWindow(this.CurrentPatient);
            }
                
        }

        private void OpenQuestionnairePopUp()
        {
            questionnairePopUp = new QuestionnairePopUp();

            int width = (int)(ActualWidth / 2.5);
            int height = (int)(ActualHeight * 0.65);

            questionnairePopUp.SetSize(height, width);

            questionnairePopUp.ExerciseView = this;

            ExerciseWindow.Children.Add(questionnairePopUp);
        }

        public void CloseQuestionnairePopUp(Boolean isFinished)
        {
            ExerciseWindow.Children.Remove(questionnairePopUp);

            //in case that the user finished his questionnaire
            if (isFinished)
            {
                OpenQuestionnaireFinishedPopUp();
            }
            else
            {
                MainWindow.OpenTreatmentWindow(this.CurrentPatient);
            }
                
        }

        private void OpenQuestionnaireFinishedPopUp()
        {
            questionnaireFinishedPopUp = new QuestionFinishedPopUp();

            int width = (int)(ActualWidth / 2.3);
            int height = (int)(ActualHeight * 0.55);

            questionnaireFinishedPopUp.SetSize(height, width);
            questionnaireFinishedPopUp.ExerciseView = this;

            ExerciseWindow.Children.Add(questionnaireFinishedPopUp);

        }
        
        public void CloseQuestionnaireFinishedPopUp()
        {
            ExerciseWindow.Children.Remove(questionnaireFinishedPopUp);

            MainWindow.OpenTreatmentWindow(this.CurrentPatient);
        }

        private void CreatePopUps()
        {
            pausePopUp = new PausePopUp();
            int width = (int)(ActualWidth * 0.5);
            int height = (int)(ActualHeight * 0.35);

            pausePopUp.SetSize(height, width);


            noTrackingPopUp = new NoTrackingPopUp();
            width = (int)(ActualWidth * 0.45);
            height = (int)(ActualHeight * 0.55);

            noTrackingPopUp.SetSize(height, width);
            noTrackingPopUp.ExerciseView = this;
        }

        private void OpenPausePopUp()
        {
            if (!ExerciseWindow.Children.Contains(pausePopUp))
            {
                ExerciseWindow.Children.Add(pausePopUp);

                StopDetection = true;

                ExerciseVideo.Pause();

                _playPause = false;
            }
            

            
        }

        private void ClosePausePopUp()
        {
            if (ExerciseWindow.Children.Contains(pausePopUp))
            {
                ExerciseWindow.Children.Remove(pausePopUp);
                //todo - resume tracking

                StopDetection = false;

                ExerciseVideo.Play();

                _playPause = true;
            }

        }
        
        private void OpenNotTrackablePopUp()
        {
            if (!ExerciseWindow.Children.Contains(noTrackingPopUp))
            {
                ExerciseWindow.Children.Add(noTrackingPopUp);
                noTrackingPopUp.StartTimer();
            }
        }

        public void CloseNotTrackablePopUp()
        {
            if (ExerciseWindow.Children.Contains(noTrackingPopUp))
            {
                ExerciseWindow.Children.Remove(noTrackingPopUp);
                noTrackingPopUp.StopTimer();
            }
        }

        #endregion

        #region events
        private void NextVideoClick(object sender, MouseButtonEventArgs e)
        {
            //PlayNextVideo();
            PlayNextVideo2();
        }

        private void PrevVideoClick(object sender, MouseButtonEventArgs e)
        {
            PlayPrevVideo2();
            //if (!(_currentExerciseIndex <= 0))
            //{
            //    _currentExerciseIndex--;

            //    CurrentExercise = Playlist[_currentExerciseIndex];

            //    //clearing the view
            //    KinectShilloute.Source = null;
            //    KinectSkeleton.Source = null;


            //    DataContext = CurrentExercise;

            //    ////FOR DEMO!!!
            //    //ForDemo();


            //    ExerciseVideo.Play();
            //}
        }

        private void PlayPauseClick(object sender, MouseButtonEventArgs e)
        {
            //todo - need to stop detecion

            if (_playPause)
            {
                ExerciseVideo.Pause();
                _playPause = false;
                PlayPauseVideo.Source = new BitmapImage(new Uri("Images\\play.png", UriKind.RelativeOrAbsolute));
                PlayPauseVideo.Margin = new Thickness(8,0,0,0);
            }
            else
            {
                ExerciseVideo.Play();
                _playPause = true;
                PlayPauseVideo.Source = new BitmapImage(new Uri("Images\\pause.png", UriKind.RelativeOrAbsolute));
                PlayPauseVideo.Margin = new Thickness(0, 0, 0, 0);
            }
        }

        private void ExerciseVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayNextVideo2();
            //PlayNextVideo();
        }

        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseApp(CurrentPatient);
        }
        #endregion

        private void CloseAllComponents()
        {
            ExerciseVideo.Stop();
            StopDetection = true;

            //clearing the view
            KinectShilloute.Source = null;
            KinectSkeleton.Source = null;

            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        public void Dispose()
        {
            if (_gestureDetector != null)
            {
                _gestureDetector.Dispose();
            }

            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_gestureAnalysis != null)
            {
                _gestureAnalysis = null;
            }

            if (_sensor != null)
            {
                _sensor.Close();
                _sensor = null;
            }


        }


        //=====================================
        //PLEASE DELETE AFTER DEMO
        //sqaut
        private void ForDemo()
        {
            CurrentExercise.Repetitions = 5;
            //CurrentExercise.DBPath = CurrentExercise.createGestures();
            //CurrentExercise.CreateRounds();

        }

        private void ForDemo2()
        {
            CurrentExercise.Repetitions = 5;
            //CurrentExercise.DBPath = CurrentExercise.CreateGestures2();
            CurrentExercise.CreateRounds();

        }

        private void printStatus()
        {

            Console.WriteLine("================ In View =================");
            Console.WriteLine("Current round {0}", CurrentExercise.CurrentRound.RoundNumber);
            Console.WriteLine("Current round in exe {0}", CurrentExercise.RoundIndex);
            //Console.WriteLine("Current exe " + CurrentExercise.GetHashCode());
            Console.WriteLine(CurrentExercise.CurrentRound.RoundProgress);
            Console.WriteLine(CurrentExercise.CurrentRound.RoundSuccess);
            foreach (var item in CurrentExercise.CurrentRound.GestureList)
            {
                Console.WriteLine(item.Key);
                Console.WriteLine(item.Value.IsSuccess);
                Console.WriteLine(item.Value.ProgressValue);
                Console.WriteLine();
            }
        }

        //=====================================
    }
}
