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
    public partial class ExerciseView : UserControl
    {
        public Exercise CurrentExercise { set; get; }

        public List<Exercise> ExerciseList { set; get; }

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;

        
        BackgroundRemovalTool _backgroundRemovalTool;
        DrawSkeleton _drawSkeleton;


        //Temp - just for videos
        DispatcherTimer _timer;
        int counter = 0;

        public ExerciseView()
        {
            InitializeComponent();

            DataContext = this;
            this.Loaded += ExerciseView_Loaded;

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 1);
            _timer.Tick += _timer_Tick;

            
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            counter++;
            if (counter == 4)
            {
                ExerciseVideo.Pause();
            }
            if (counter == 9)
            {
                _timer.Stop();

                WaitForStartLbl.Visibility = Visibility.Hidden ;
                KinectSkeleton.Source = null;
                ExerciseVideo.Play();
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
            }


            _timer.Start();
            ExerciseVideo.Play();
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
        
    }
}
