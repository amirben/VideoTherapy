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
        private string VT_Intro = @"http://mil01.objectstorage.softlayer.net/v1/AUTH_2bc94b1c-c83f-4247-b43c-c1dfaf29db00/vtexercisevideos2/vtTrainingIntro.mp4";
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


            this.Loaded += ExerciseView_Loaded;
        }

        private void ExerciseView_Loaded(object sender, RoutedEventArgs e)
        {
            CreatePopUps();

            CreatePauseTimer();

            UserProfile.DataContext = CurrentPatient;

            RemoveUpperLayer();
            
            ExerciseVideo.Source = new Uri(VT_Intro);
            ExerciseVideo.Play();
            
            //CurrentExercise = CurrentTraining.GetNextExercise();
            //UpdatePlayerAfterExerciseChange();
        }

        private void RemoveUpperLayer()
        {
            ExerciseDetails.Visibility = Visibility.Collapsed;
            ExerciseStatus.Visibility = Visibility.Collapsed;
            DemoStatus.Visibility = Visibility.Collapsed;
            PlayerControl.Visibility = Visibility.Collapsed;
        }

        private void TrackingLostTimer_Tick(object sender, EventArgs e)
        {
            if (_gestureDetector != null)
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
        }

        private void CurrentExercise_UpdateRoundNumber()
        {;
            RoundIndexText.DataContext = CurrentExercise.CurrentRound;
        }

        private void UpdateDataContext()
        {
            ExerciseDetails.Visibility = Visibility.Visible;

            //Data context update
            DataContext = CurrentExercise;
            
            CurrentTrainingLbl.DataContext = CurrentTraining;

            ExerciseMotionQualityGrid.DataContext = CurrentExercise;

            UpdateExerciseUI();
        }

        private void CurrentExercise_NextRoundUpdateUIEvet()
        {
            UpdateExerciseUI();
        }
      
        private void UpdateExerciseUI()
        {
            PlayerControl.Visibility = Visibility.Visible;
            ExerciseStatus.DataContext = CurrentExercise.CurrentRound;
            RoundIndexText.DataContext = CurrentExercise.CurrentRound;
            RoundMotionQualityGrid.DataContext = CurrentExercise.CurrentRound;

            Binding videoBinding = new Binding("VideoPath");
            ExerciseVideo.SetBinding(MediaElement.SourceProperty, videoBinding);

        }

        private void CreatePauseTimer()
        {
            TrackingLostTimer = new DispatcherTimer();
            TrackingLostTimer.Interval = new TimeSpan(0, 0, 1);
            TrackingLostTimer.Tick += TrackingLostTimer_Tick;
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
                    StartNonTraceableMode();
                    //SetUIInNoTrackableMode();
                    break;

                case Exercise.ExerciseMode.NonTraceableDuplicate:
                    //SetUIInNoTrackableMode();
                    StartNonTraceableMode();
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
            //WaitForStartLbl.Visibility = Visibility.Hidden;
        }

        private void StartNonTraceableMode()
        {
            SetUIInNoTrackableMode();

            _sensor = KinectSensor.GetDefault();

            //todo - check, not working
            if (_sensor != null)
            {
                _sensor.Open();

                // 2) Initialize the background removal tool.
                _backgroundRemovalTool = new BackgroundRemovalTool(_sensor.CoordinateMapper);
                _drawSkeleton = new DrawSkeleton(_sensor, (int)(KinectSkeleton.Width), (int)(KinectSkeleton.Height));

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived_NonTraceable;

            }
        }

        private void Reader_MultiSourceFrameArrived_NonTraceable(object sender, MultiSourceFrameArrivedEventArgs e)
        {
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
                }
            }
        }

        private void StartTraceableMode()
        {
            SetUIInTrackingMode();

            //ExerciseVideo.Play();

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
            }

            ExerciseVideo.Play();
            inPlayMode = true;
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
             }
        }

        private void LostTracking()
        {
            if (!TrackingLostTimer.IsEnabled && inPlayMode)
            {
                _gestureDetector.IsPaused = true; 
                TrackingLostTimer.Start();
            }
            else
            {
                if (!inPlayMode)
                {
                    _gestureDetector.IsPaused = true;
                    TrackingLostTimer.Stop();
                    trackingLostCounter = 0;
                }
            }
        }

        private void CheckIfPausePopupCounterStart()
        {
            
            if (!ExerciseWindow.Children.Contains(pausePopUp))
            {
                _gestureDetector.IsPaused = false;
            }

            if (TrackingLostTimer.IsEnabled && trackingLostCounter < POPUP_SHOW_AFTER_SEC) // incase the timer started
            {
                TrackingLostTimer.Stop();
                _gestureDetector.IsPaused = false;
            }
            
            trackingLostCounter = 0;
        }
     
        public bool GetTrackingId(Body[] bodies)
        {
            int numTrack = this.bodies.Where(t => t.IsTracked).Count();

            switch (numTrack)
            {
                case 0:
                    LostTracking();
                    return true;

                case 1:
                    CloseMoreThenOnePopUp();

                    _gestureDetector.TrackingId = this.bodies.Where(t => t.IsTracked).FirstOrDefault().TrackingId;

                    CheckIfPausePopupCounterStart();

                    return false;

                default:
                    OpenMoreThenOnePopUp();
                    return false;
            }
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

            trackingLostCounter = 0;
            TrackingLostTimer.Stop();
            CreatePauseTimer();

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
            //WaitForStartLbl.Visibility = Visibility.Collapsed;

            
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

            //WaitForStartLbl.Visibility = Visibility.Visible;

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
            //ExerciseVideo.Visibility = Visibility.Hidden;
            //Background = new SolidColorBrush(Color.FromArgb(0x80, 0xc6, 0x83, 0));

            summary = new SummaryPopUp();
            int width = (int)(ActualWidth / 2.5);
            int height = (int)(ActualHeight * 0.75);

            summary.SetSize(height, width);

            summary.CurrentTraining = CurrentTraining;
            summary.CurrentPatient = CurrentPatient;
            summary.ExerciseView = this;
            summary.UpdateScore();

            TrackingLostTimer.Stop();

            ExerciseWindow.Children.Add(summary);
        }

        public void ClosedSummaryPopUp(Boolean goToQuestions)
        {
            ExerciseWindow.Children.Remove(summary);
            summary = null;

            CurrentTraining.ClearTrainingData();

            //if the user decided to answer the questions
            if (goToQuestions)
            {
                OpenQuestionnairePopUp();
            }
            else
            {
                CloseExeciseView();
                //CurrentTraining.ClearTrainingData();
                //MainWindow.OpenTreatmentWindow(this.CurrentPatient);
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
                CloseExeciseView();
                //CurrentTraining.ClearTrainingData();
                //MainWindow.OpenTreatmentWindow(this.CurrentPatient);
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
            if (ExerciseWindow.Children.Contains(questionnaireFinishedPopUp))
            {
                ExerciseWindow.Children.Remove(questionnaireFinishedPopUp);
            }
            CloseExeciseView();
            //MainWindow.OpenTreatmentWindow(this.CurrentPatient);
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
                
                if (_gestureDetector != null)
                {
                    _gestureDetector.IsPaused = false;
                }

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

        private void PlayNextVideo()
        {
            if (CurrentExercise != null)
            {
                CurrentExercise.CheckComplianceOfExercise(ExerciseVideo.Position.Seconds);
            }

            CurrentExercise = CurrentTraining.GetNextExercise();

            //if the current exercise is null = meaning the end of playlist
            if (CurrentExercise != null)
            {
                UpdatePlayerAfterExerciseChange();
            }

            //incase of ending the playlist
            else
            {
                CloseAllComponents();

                //show questionere pop-up
                OpenSummaryPopUp();

                //send compliance to server
                UpdateTrainingCompliance();
            }
        }

        private void PlayPervSessionVideo()
        {

            CurrentExercise = CurrentTraining.GetPrevSession();

            UpdatePlayerAfterExerciseChange();
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

                if  (_gestureDetector != null && 
                        (CurrentExercise.Mode == Exercise.ExerciseMode.Traceable || CurrentExercise.Mode == Exercise.ExerciseMode.TraceableDuplicate))
                {
                    _gestureDetector.IsPaused = true;
                }
                
            }
            else
            {
                ChangeUIToPlayMode();

                if (_gestureDetector != null &&
                        (CurrentExercise.Mode == Exercise.ExerciseMode.Traceable || CurrentExercise.Mode == Exercise.ExerciseMode.TraceableDuplicate))
                {
                    _gestureDetector.IsPaused = true;
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
            //CloseExeciseView();
            OpenQuestionnairePopUp();
        }
        #endregion

        private async void CloseExeciseView()
        {
            Task clearData = new Task(async() =>
            {
                CurrentTraining.ClearTrainingData();
                string response = await ApiConnection.GetTrainingApiAsync(CurrentTraining.TrainingId);
                JSONConvertor.GettingPatientTraining2(CurrentTraining, response);
            });
            clearData.Start();

            GoBackToTreatmentScreen(CurrentPatient);
        }

        private void CloseAllComponents()
        {
            ExerciseVideo.Stop();

            //clearing the view
            KinectShilloute.Source = null;
            KinectSkeleton.Source = null;

            Dispose();
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
