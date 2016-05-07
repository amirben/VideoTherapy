using Microsoft.Kinect;
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
using VideoTherapy.Objects;

namespace VideoTherapy.Views
{
    /// <summary>
    /// Interaction logic for DistanceWindow.xaml
    /// </summary>
    public partial class DistanceWindow : UserControl, IDisposable
    {
        private const float DISTANCE = 2f;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        public MainWindow mainWindow;
        public VideoTherapy.TrainingMenu trainingMenu;

        private Training currentTraining;
        private Patient currentPatient;

        private bool stopDetection;

        public DistanceWindow(Patient currentPatient, Training currentTraining)
        {
             InitializeComponent();

            this.currentPatient = currentPatient;
            this.currentTraining = currentTraining;

            this.Loaded += DistanceWindow_Loaded;
        }

        private void DistanceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            if (this.kinectSensor != null)
            {
                // open the sensor
                this.kinectSensor.Open();
            }

            if (kinectSensor.IsAvailable)
            {
                // open the reader for the body frames
                this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

                if (this.bodyFrameReader != null)
                {
                    this.bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;
                }
            }
            else
            {
                //mainWindow.OpenExerciseWindow(currentPatient, currentTraining);
                trainingMenu.OpenSplash();
            }
            
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            if (!stopDetection)
            {
                bool dataReceived = false;

                using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
                {
                    if (bodyFrame != null)
                    {
                        if (this.bodies == null)
                        {
                            this.bodies = new Body[bodyFrame.BodyCount];
                        }

                        // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                        // As long as those body objects are not disposed and not set to null in the array,
                        // those body objects will be re-used.
                        bodyFrame.GetAndRefreshBodyData(this.bodies);
                        dataReceived = true;
                    }
                }

                if (dataReceived)
                { 
                    int numTrack = this.bodies.Where(t => t.IsTracked).Count();

                    if (numTrack == 1)
                    {
                        Body body = this.bodies.Where(b => b.IsTracked).FirstOrDefault();

                        float z = body.Joints[JointType.SpineBase].Position.Z;

                        Console.WriteLine("Range = {0}", z);
                        
                        #if DEBUG
                        meter.Text = "Range = " + z.ToString();
                        meter.Visibility = Visibility.Visible;
                        #endif

                        if (z >= DISTANCE)
                        {
                            //mainWindow.OpenExerciseWindow(currentPatient, currentTraining);
                            trainingMenu.OpenSplash();
                            //this.bodyFrameReader.FrameArrived -= BodyFrameReader_FrameArrived;
                            stopDetection = true;
                        }
                    }
                    else
                    {
                        if (numTrack != 0)
                        {
                            AlertMessege.Text = "We can only detect one person.";
                        }

                    }

                }
            }
            
        }

        public void Dispose()
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }
    }
}
