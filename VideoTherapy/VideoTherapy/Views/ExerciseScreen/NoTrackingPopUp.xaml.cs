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

namespace VideoTherapy.Views.ExerciseScreen
{
    /// <summary>
    /// Interaction logic for NoTrackingPopUp.xaml
    /// </summary>
    public partial class NoTrackingPopUp : UserControl
    {
        public ExerciseView ExerciseView;
        private DispatcherTimer showPopUpTimer;
        private int timerCounter = 0;
        private const int TIMER_COUNTER = 5;
        private int countDown = TIMER_COUNTER;

        public NoTrackingPopUp()
        {
            InitializeComponent();

            Visibility = Visibility.Visible;

            showPopUpTimer = new DispatcherTimer();
            showPopUpTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            showPopUpTimer.Tick += ShowPopUpTimer_Tick;

            CountdownText.Text = countDown.ToString();
        }

        private void ShowPopUpTimer_Tick(object sender, EventArgs e)
        {
            if (timerCounter == TIMER_COUNTER)
            {
                StopTimer();
                timerCounter = 0;
                countDown = TIMER_COUNTER;
                CountdownText.Text = countDown.ToString();

                ExerciseView.CloseNotTrackablePopUp();
            }

            CountdownText.Text = countDown.ToString();
            countDown--;
            timerCounter++;
        }
        public void StartTimer()
        {
            showPopUpTimer.Start();
        }

        public void StopTimer()
        {
            timerCounter = 0;
            countDown = TIMER_COUNTER;
            showPopUpTimer.Stop();
        }
        public void SetSize(int height, int width)
        {
            NoTrackingPopup.Width = width;
            NoTrackingPopup.Height = height;
        }

        private void okGotItButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ExerciseView.CloseNotTrackablePopUp();
        }
    }
}
