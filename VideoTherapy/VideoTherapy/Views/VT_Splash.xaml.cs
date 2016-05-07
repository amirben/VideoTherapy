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
using VideoTherapy.ServerConnections;

namespace VideoTherapy.Views
{
    /// <summary>
    /// Interaction logic for VT_Splash.xaml
    /// </summary>
    public partial class VT_Splash : UserControl, IDisposable
    {
        public DispatcherTimer MessageTimer;
        private int timeCounter = 0;

        public event EventHandler ErrorEvent;
        public event MainWindow.CloseAppDelegate CloseApp;

        public VT_Splash()
        {
            InitializeComponent();

            MessageTimer = new DispatcherTimer();
            MessageTimer.Interval = new TimeSpan(0, 0, 1);
            MessageTimer.Tick += MessageTimer_Tick;
        }

        private void MessageTimer_Tick(object sender, EventArgs e)
        {
            timeCounter++;

            switch (timeCounter)
            {
                case 5:
                    LoadingMessage.Content = "On our way!";
                    break;
                case 10:
                    LoadingMessage.Content = "We are almost there!";
                    break;
                case 25:
                    LoadingMessage.Content = "Takes more than usual...";
                    break;
                case 30:
                    StopLoadingMessages();
                    ErrorEvent(null, new ErrorMessege("We had problem to login, try again", 0));
                    break;
            }
        }

        public void StopLoadingMessages()
        {
            MessageTimer.Stop();
            timeCounter = 0;

            LoadingMessage.Content = "Loading your training";

        }

        public void Dispose()
        {
            MessageTimer.Stop();
            
        }

        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseApp(null);

        }
    }
}
