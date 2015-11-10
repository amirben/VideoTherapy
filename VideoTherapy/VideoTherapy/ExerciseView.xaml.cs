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
using VideoTherapy_Objects;
using VideoTherapy.Kinect;
using VideoTherapy.Kinect_Detection;
using System.ComponentModel;
using System.Windows.Threading;

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for ExerciseView.xaml
    /// </summary>
    /// 
    public partial class ExerciseView : UserControl, IDisposable
    {
        public Exercise CurrentExercise { set; get; }

        public List<Exercise> Playlist { set; get; }

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
        private bool bodyTracked = false;

        //Temp - just for videos
        DispatcherTimer _timer;
        int counter = 0;

        public ExerciseView(Patient currentPatient, Training currentTraining)
        {
            InitializeComponent();

            CurrentPatient = currentPatient;
            CurrentTraining = currentTraining;

            Playlist = currentTraining.Playlist;
            CurrentExercise = Playlist[0];
            //CurrentExercise.CreateRounds();


            //TEMP!!
            //Todo
            ForDemo();

            //Data context update
            DataContext = CurrentExercise;
            UserProfile.DataContext = CurrentPatient;
            CurrentTrainingLbl.DataContext = CurrentTraining;

            ExerciseStatus.DataContext = CurrentExercise.CurrentRound;
            RoundIndexText.DataContext = CurrentExercise.CurrentRound;

            //this.Loaded += ExerciseView_Loaded;

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 1);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            //counter++;
            //if (counter == 4)
            //{
            //    ExerciseVideo.Pause();
            //}
            //if (counter == 9)
            //{
            //    _timer.Stop();

            //    WaitForStartLbl.Visibility = Visibility.Hidden ;
            //    KinectSkeleton.Source = null;
            //    ExerciseVideo.Play();
            //}

            CurrentExercise.CurrentRound.RoundProgress++;
            CurrentExercise.CurrentRound.NotifyPropertyChanged("RoundProgress");
            //ExerciseRepetitionsProgressBar.Value++;

            counter++;
            if (counter == 10)
            {
                CurrentExercise.CurrentRound.RoundNumber++;
                CurrentExercise.CurrentRound.NotifyPropertyChanged("RoundNumber");
                //RoundIndexText.DataContext = CurrentExercise.CurrentRound;
            }

        }

        private void ExerciseView_Loaded(object sender, RoutedEventArgs e)
        {
            
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                // 2) Initialize the background removal tool.
                _backgroundRemovalTool = new BackgroundRemovalTool(_sensor.CoordinateMapper);
                _drawSkeleton = new DrawSkeleton(_sensor,(int) (KinectSkeleton.Width), (int)(KinectSkeleton.Height));

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

                //Gesture detection
                _gestureAnalysis = new GestureAnalysis(CurrentExercise);
                _gestureDetector = new GestureDetector(_sensor, _gestureAnalysis, CurrentExercise);
            }


            //_timer.Start();
            ExerciseVideo.Play();
            _playPause = true;
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
                    //_gestureDetector.IsPaused = !bodyTracked;
                    //_gestureDetector.TrackingId = bodyIndex;
                    //Console.WriteLine("Pause: {0}, ID: {1}", _gestureDetector.IsPaused, _gestureDetector.TrackingId);
                    //_gestureDetector.GestureDetectionToAnalyze();



                }
            }

            if (detected)
            {
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

            if (bodyTracked)
            {
                if (bodies[bodyIndex].IsTracked)
                {
                    body = bodies[bodyIndex];
                }
                else
                {
                    bodyTracked = false;
                }
            }
            if (!bodyTracked)
            {
                for (int i = 0; i < bodies.Length; ++i)
                {
                    if (bodies[i].IsTracked)
                    {
                        bodyIndex = (ulong) i;
                        bodyTracked = true;
                        break;
                    }
                    else
                    {
                        bodyIndex = 0;
                    }
                }
            }

            return bodyTracked;

            //Console.WriteLine("Current Tracking id {0}", bodyIndex);
        }

        public void Dispose()
        {
        }


        private void NextVideoClick(object sender, MouseButtonEventArgs e)
        {
            if (!(_currentExerciseIndex + 1 >= Playlist.Count))
            {
                _currentExerciseIndex++;
                CurrentExercise = Playlist[_currentExerciseIndex];

                DataContext = CurrentExercise;

                //FOR DEMO!!!
                ForDemo();


                ExerciseVideo.Play();
            }

            Console.WriteLine(_currentExerciseIndex);
        }

        private void PrevVideoClick(object sender, MouseButtonEventArgs e)
        {
            if (!(_currentExerciseIndex <= 0))
            {
                _currentExerciseIndex--;

                CurrentExercise = Playlist[_currentExerciseIndex];

                DataContext = CurrentExercise;

                //FOR DEMO!!!
                ForDemo();


                ExerciseVideo.Play();
            }

            Console.WriteLine(_currentExerciseIndex);
        }

        private void PlayPauseClick(object sender, MouseButtonEventArgs e)
        {
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

        //PLEASE DELETE AFTER DEMO
        private void ForDemo()
        {
            CurrentExercise.Repetitions = 2;
            CurrentExercise.createGestures();
            CurrentExercise.CreateRounds();


        }
    }
}
