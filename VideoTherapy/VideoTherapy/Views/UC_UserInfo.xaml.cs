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
using System.Windows.Threading;

using Microsoft.Kinect;
using VideoTherapy.Objects;

namespace VideoTherapy.Views
{
    /// <summary>
    /// Interaction logic for UC_UserInfo.xaml
    /// </summary>
    public partial class UC_UserInfo : UserControl
    {
        private Patient _currentPatient;
        private DispatcherTimer timer;

        public event VideoTherapy.TreatmentMenu.TrainingSelectedDelegate trainingSelectedEvent;
        public event VideoTherapy.MainWindow.CloseAppDelegate closeApp;
        public event VideoTherapy.MainWindow.LogOutDelegate logOut;

        private Training _RecomendedTraining;
        public Training RecomendedTraining
        {
            set
            {
                _RecomendedTraining = value;
                RecomendedTrainingPanel.DataContext = value;
            }

            get
            {
                return _RecomendedTraining;
            }
        }

        private KinectSensor sensor;

        //to show or not the recommended panel
        public Boolean ShowRecommended
        {
            set
            {
                if (value)
                {
                    this.ShowRecommendedBorder.Visibility = Visibility.Visible;
                    this.ShowKinectStatus.Visibility = Visibility.Hidden;
                    this.UserInfoStack.Children.Remove(ShowKinectStatus);
                    
                }
                else
                {
                    this.ShowRecommendedBorder.Visibility = Visibility.Hidden;
                    this.ShowKinectStatus.Visibility = Visibility.Visible;
                    this.UserInfoStack.Children.Remove(ShowRecommendedBorder);
                    KinectConnectionStatus();
                }
            }
        }

        public UC_UserInfo(Patient _currentPatient)
        {
            InitializeComponent();

            this._currentPatient = _currentPatient;
            this.DataContext = this._currentPatient;

            CurrentDate.Text = DateTime.Now.ToString("ddd, dd.mm.yy");
            CurrentTime.Text = DateTime.Now.ToString("HH:mm");

            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 1, 0), DispatcherPriority.Normal, delegate
            {
                CurrentTime.Text = DateTime.Now.ToString("HH:mm");
                CurrentDate.Text = DateTime.Now.ToString("ddd, dd.mm.yy");

            }, this.Dispatcher);

            timer.Start();
            
            LinktToProfile.RequestNavigate += LinktToProfile_RequestNavigate;
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            if (sensor.IsAvailable)
            {
                KinectStatusLbl.Text = "KINECT 2.0 connected";
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("./../images/connected.png", UriKind.Relative);
                bitmap.EndInit();

                KinectStatusImg.Source = bitmap;
            }
            else
            {
                KinectStatusLbl.Text = "KINECT 2.0 not connected";

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("./../images/disconnected.png", UriKind.Relative);
                bitmap.EndInit();

                KinectStatusImg.Source = bitmap;
            }

        }

        private void SetTimerAndDate()
        {
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 1, 0);
        }

        private void LinktToProfile_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("chrome.exe", e.Uri.ToString());
            }
            // for default case that there is no chrome
            catch(Exception e1)
            {
                System.Diagnostics.Process.Start(e.Uri.ToString());
            }
           
        }

        private void OpenRecommendedTraining_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            trainingSelectedEvent(RecomendedTraining);
        }

        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            closeApp();
        }

        private void LogoutBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            logOut();
        }
        
        private void KinectConnectionStatus()
        {
            try
            {
                sensor = KinectSensor.GetDefault();
                sensor.IsAvailableChanged += Sensor_IsAvailableChanged;
                sensor.Open();
                
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
