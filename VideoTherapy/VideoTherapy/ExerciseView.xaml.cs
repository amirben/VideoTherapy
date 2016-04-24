using System;
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
using VideoTherapy.ServerConnections;

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for ExerciseView.xaml
    /// </summary>
    /// 
    public partial class ExerciseView : UserControl,  IDisposable
    {
        public Exercise CurrentExercise { set; get; }

        public Dictionary<int, List<Exercise>> Playlist { set; get; }

        public Patient CurrentPatient;
        public Training CurrentTraining;

        public MainWindow MainWindow { set; get; }

        private Boolean inPlayMode;

        private KinectSensor _sensor;
        private MultiSourceFrameReader _reader;

        private BackgroundRemovalTool _backgroundRemovalTool;
        private DrawSkeleton _drawSkeleton;

        private GestureAnalysis _gestureAnalysis;
        private GestureDetector _gestureDetector;
        
        // Array for the bodies
        private Body[] bodies = null;

        //// index for the currently tracked body
        //private ulong bodyIndex;

        //// flag to asses if a body is currently tracked
        //private bool _bodyTracked = false;
        //public bool BodyTracked
        //{
        //    get
        //    {
        //        return _bodyTracked;
        //    }

        //    set
        //    {
        //        _bodyTracked = value;
        //        CheckForPause();
        //    }
        //}

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

        //More then one patient detected popup
        private MoreThenOnePopUp moreThenOnePopUp;

        //delegates
        public delegate void NextRoundUpdataDelegate();
        public delegate void StopDetectionDelegate();
        public delegate void StartGestureDetected();
        public delegate void UpdateLastRound();
        public delegate void ClosePausePopupDelegate();

        //lost tracking timer
        public DispatcherTimer TrackingLostTimer;
        private int trackingLostCounter = 0;
        private const int POPUP_SHOW_AFTER_SEC = 5;

        //close app
        public event MainWindow.GoBackToTreatmentScreen GoBackToTreatmentScreen;

        public ExerciseView(Patient currentPatient, Training currentTraining)
        {
            InitializeComponent();

            CurrentPatient = currentPatient;
            CurrentTraining = currentTraining;

            TrackingLostTimer = new DispatcherTimer();
            TrackingLostTimer.Interval = new TimeSpan(0, 0, 1);
            TrackingLostTimer.Tick += TrackingLostTimer_Tick;

            this.Loaded += ExerciseView_Loaded;
        }

        private void ExerciseView_Loaded(object sender, RoutedEventArgs e)
        {
            CreatePopUps();

            CurrentExercise = CurrentTraining.GetNextExercise();
            UpdatePlayerAfterExerciseChange();
        }

        private void TrackingLostTimer_Tick(object sender, EventArgs e)
        {
            if (_gestureDetector.IsPaused)
            {
                trackingLostCounter++;

                if (trackingLostCounter == POPUP_SHOW_AFTER_SEC)
                {
                    OpenPausePopUp();
                }
            }
            else
            {
                trackingLostCounter = 0;
                TrackingLostTimer.Stop();
            }
        }

        private void CurrentExercise_UpdateRoundNumber()
        {;
            RoundIndexText.DataContext = CurrentExercise.CurrentRound;
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

        private void CurrentExercise_NextRoundUpdateUIEvet()
        {
            ExerciseStatus.DataContext = CurrentExercise.CurrentRound;
            RoundIndexText.DataContext = CurrentExercise.CurrentRound;
            RoundMotionQualityGrid.DataContext = CurrentExercise.CurrentRound;
        }
        
        private void InitPlayerScreenMode()
        {
            switch (CurrentExercise.Mode)
            {
                case Exercise.ExerciseMode.Demo:
                    SetUIInDemoMode();
                    ExerciseVideo.Play();
                    inPlayMode = true;
                    break;

                case Exercise.ExerciseMode.NonTraceable:
                    OpenNotTrackablePopUp();
                    SetUIInNoTrackableMode();
                    break;

                case Exercise.ExerciseMode.NonTraceableDuplicate:
                    SetUIInNoTrackableMode();
                    ExerciseVideo.Play();
                    inPlayMode = true;

                    break;

                case Exercise.ExerciseMode.Traceable:
                    StartTraceableMode();
                    break;

                case Exercise.ExerciseMode.TraceableDuplicate:
                    StartTraceableMode();
                    break;

            }
        }

        private void _gestureAnalysis_startGestureDeteced()
        {
            WaitForStartLbl.Visibility = Visibility.Hidden;
        }

        private void StartTraceableMode()
        {
            SetUIInTrackingMode();

            ExerciseVideo.Play();

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
                inPlayMode = true;
            }
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
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

                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    detected = true;
                }
            }

            if (detected)
            {
                bool pauseTracking = GetTrackingId(this.bodies);
                
                if (pauseTracking)
                {
                    if (!TrackingLostTimer.IsEnabled && inPlayMode)
                    {
                        _gestureDetector.IsPaused = pauseTracking;
                        TrackingLostTimer.Start();
                    }
                    else
                    {
                        if (!inPlayMode)
                        {
                            _gestureDetector.IsPaused = pauseTracking;
                            TrackingLostTimer.Stop();
                            trackingLostCounter = 0;
                        }
                    }
                }
                else
                {
                    if (trackingLostCounter < POPUP_SHOW_AFTER_SEC) // incase the timer started
                    {
                        TrackingLostTimer.Stop();
                        trackingLostCounter = 0;
                        _gestureDetector.IsPaused = pauseTracking;
                    }
                 
                }
            }
        }

        public bool GetTrackingId(Body[] bodies)
        {
            //int numTrack = this.bodies.Where(t => t.IsTracked).Count();

            //switch (numTrack)
            //{
            //    case 0:

            //        break;

            //    case 1:
            //        _gestureDetector.TrackingId = this.bodies.Where(t => t.IsTracked).FirstOrDefault().TrackingId;
            //        return true;

            //    default:
            //        break;
            //}
            //if (numTrack == 1)
            //{
                
            //}
            //else
            //{

            //}

            int numOfTrackingBodies = 0;
            for (int i = 0; i < bodies.Length; i++)
            {
                Body body = this.bodies[i];
                if (body.IsTracked)
                {
                    if  (body.TrackingId != _gestureDetector.TrackingId)
                    {
                        _gestureDetector.TrackingId = body.TrackingId;
                    }
                    numOfTrackingBodies++;
                }
            }
            Console.WriteLine(numOfTrackingBodies);

            if (numOfTrackingBodies == 1) //valid tracking
                return false;
            else //more then 1 or no tracking at all;
                return true;
        }

        #region ui_modes

        private void UpdatePlayerAfterExerciseChange()
        {
            //clearing the view
            KinectShilloute.Source = null;
            KinectSkeleton.Source = null;

            //clear data
            Dispose();

            InitPlayerScreenMode();

            CurrentExercise.NextRoundUpdateUIEvent += CurrentExercise_NextRoundUpdateUIEvet;
            CurrentExercise.UpdateLastRound += CurrentExercise_UpdateRoundNumber;

            UpdateDataContext();
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
            WaitForStartLbl.Visibility = Visibility.Collapsed;
        }

        private void SetUIInTrackingMode()
        {
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

            WaitForStartLbl.Visibility = Visibility.Visible;

            ExerciseMotionPanel.Visibility = Visibility.Visible;
            NAExerciseMotion.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region popups

        private void CreatePopUps()
        {
            pausePopUp = new PausePopUp();
            int width = (int)(ActualWidth * 0.5);
            int height = (int)(ActualHeight * 0.5);

            pausePopUp.SetSize(height, width);
            pausePopUp.ClosePopup += ClosePausePopUp;

            height = (int)(ActualHeight * 0.55);
            moreThenOnePopUp = new MoreThenOnePopUp();
            moreThenOnePopUp.SetSize(height, width);
            moreThenOnePopUp.ClosePopup += CloseMoreThenOnePopUp;
        }

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

        private void OpenMoreThenOnePopUp()
        {
            if (!ExerciseWindow.Children.Contains(moreThenOnePopUp))
            {
                ExerciseWindow.Children.Add(moreThenOnePopUp);

                ChangeUIToPauseMode();
            }
        }

        private void CloseMoreThenOnePopUp()
        {
            if (ExerciseWindow.Children.Contains(moreThenOnePopUp))
            {
                ExerciseWindow.Children.Remove(moreThenOnePopUp);

                ChangeUIToPlayMode();
            }
        }

        private void OpenPausePopUp()
        {
            if (!ExerciseWindow.Children.Contains(pausePopUp))
            {
                ExerciseWindow.Children.Add(pausePopUp);

                ChangeUIToPauseMode();
            } 
        }

        private void ClosePausePopUp()
        {
            if (ExerciseWindow.Children.Contains(pausePopUp))
            {
                ExerciseWindow.Children.Remove(pausePopUp);

                ChangeUIToPlayMode();

                trackingLostCounter = 0;
                TrackingLostTimer.Stop();
            }
        }
        
        private void OpenNotTrackablePopUp()
        {
            ExerciseVideo.Stop();

            noTrackingPopUp = new NoTrackingPopUp();
            int width = (int)(ActualWidth * 0.45);
            int height = (int)(ActualHeight * 0.65);

            noTrackingPopUp.SetSize(height, width);
            noTrackingPopUp.ExerciseView = this;

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


            ExerciseVideo.Play();
            inPlayMode = true;
        }

        #endregion

        #region Commands
        private void PlayNextSessionVideo()
        {
            CurrentExercise = CurrentTraining.GetNextSession();

            if (CurrentExercise != null)
            {
                //there is another session
                UpdatePlayerAfterExerciseChange();
            }
            else
            {
                //end of playlist
                //show questionere pop-up
                OpenSummaryPopUp();
                CloseAllComponents();
            }
        }

        private void PlayPervSessionVideo()
        {
            CurrentExercise = CurrentTraining.GetPrevSession();

            UpdatePlayerAfterExerciseChange();
        }

        private void PlayNextVideo()
        {
            CurrentExercise.CheckComplianceOfExercise(ExerciseVideo.Position.Seconds);

            CurrentExercise = CurrentTraining.GetNextExercise();

            //if the current exercise is null = meaning the end of playlist
            if (CurrentExercise != null)
            {
                UpdatePlayerAfterExerciseChange();
            }

            //incase of ending the playlist
            else
            {
                //show questionere pop-up
                OpenSummaryPopUp();

                //todo- send to server
                UpdateTrainingCompliance();

                CloseAllComponents();
            }
        }

        private void PlayPrevVideo()
        {
            CurrentExercise = CurrentTraining.GetPrevExercise();

            UpdatePlayerAfterExerciseChange();

        }

        private async void UpdateTrainingCompliance()
        {
            if (CurrentTraining.CheckTrainingCompliance())
            {
                string response = await ApiConnection.ReportTrainingComplianceApiAsync(CurrentTraining);
                
                //todo - check error sending server
            }
        }
        #endregion

        #region events
        private void NextVideoClick(object sender, MouseButtonEventArgs e)
        {
            PlayNextVideo();
            //ChangeUIToPlayMode();
        }

        private void PrevVideoClick(object sender, MouseButtonEventArgs e)
        {
            PlayPrevVideo();
            //ChangeUIToPlayMode();
        }

        private void NextSessionMouseClick(object sender, MouseButtonEventArgs e)
        {
            PlayNextSessionVideo();
            //ChangeUIToPlayMode();
        }

        private void PrevSessionMouseClick(object sender, MouseButtonEventArgs e)
        {
            PlayPervSessionVideo();
            //ChangeUIToPlayMode();
        }

        private void PlayPauseClick(object sender, MouseButtonEventArgs e)
        {
            if (inPlayMode)
            {
                ChangeUIToPauseMode();

                if (_gestureDetector != null)
                {
                    _gestureDetector.IsPaused = true;
                }
                
            }
            else
            {
                ChangeUIToPlayMode();

                if (_gestureDetector != null)
                {
                    _gestureDetector.IsPaused = false;
                }
            }
        }

        private void ChangeUIToPauseMode()
        {
            ExerciseVideo.Pause();
            inPlayMode = false;
            PlayPauseVideo.Source = new BitmapImage(new Uri("Images\\play.png", UriKind.RelativeOrAbsolute));
            PlayPauseVideo.Margin = new Thickness(8, 0, 0, 0);
        }

        private void ChangeUIToPlayMode()
        {
            ExerciseVideo.Play();
            inPlayMode = true;
            PlayPauseVideo.Source = new BitmapImage(new Uri("Images\\pause.png", UriKind.RelativeOrAbsolute));
            PlayPauseVideo.Margin = new Thickness(0, 0, 0, 0);
        }

        private void ExerciseVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayNextVideo();
        }

        private void GoBackTreatmentButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //todo - need to clear all data
            //Returning to the treatement screen
            CurrentTraining.ClearTrainingData();
            GoBackToTreatmentScreen(CurrentPatient);
            
        }
        #endregion

        private void CloseAllComponents()
        {
            ExerciseVideo.Stop();

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
    }
}
