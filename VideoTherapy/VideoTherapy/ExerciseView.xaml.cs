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

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;

        BackgroundRemovalTool _backgroundRemovalTool;
        DrawSkeleton _drawSkeleton;

        //Temp - just for videos
        //DispatcherTimer _timer;
        //int counter = 0;

        public ExerciseView(Patient currentPatient, Training currentTraining)
        {
            InitializeComponent();

            CurrentPatient = currentPatient;
            CurrentTraining = currentTraining;

            Playlist = currentTraining.Playlist;
            CurrentExercise = Playlist[0];

            DataContext = CurrentExercise;
            UserProfile.DataContext = CurrentPatient;
            CurrentTrainingLbl.DataContext = CurrentTraining;

            this.Loaded += ExerciseView_Loaded;

            //_timer = new DispatcherTimer();
            //_timer.Interval = new TimeSpan(0, 0, 0, 1);
            //_timer.Tick += _timer_Tick;
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
                }
            }
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
    }
}
