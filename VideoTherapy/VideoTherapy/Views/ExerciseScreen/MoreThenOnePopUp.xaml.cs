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
    /// Interaction logic for MoreThenOnePopUp.xaml
    /// </summary>
    public partial class MoreThenOnePopUp : UserControl
    {
        public event ExerciseView.ClosePausePopupDelegate ClosePopup;

        private DispatcherTimer countDownTimer;
        private const int TIMER_COUNT = 3;
        private int counterTimer = TIMER_COUNT;


        public MoreThenOnePopUp()
        {
            InitializeComponent();

            countDownTimer = new DispatcherTimer();
            countDownTimer.Interval = new TimeSpan(0, 0, 1);
            countDownTimer.Tick += CountDownTimer_Tick;
        }

        private void CountDownTimer_Tick(object sender, EventArgs e)
        {
            if (counterTimer > 0)
            {
                counterTimer--;
            }
            else
            {
                countDownTimer.Stop();

                ClosePopup();

                counterTimer = TIMER_COUNT;
                countDownTextBorder.Visibility = Visibility.Collapsed;
                pauseImg.Visibility = Visibility.Visible;
            }
        }

        public void SetSize(int height, int width)
        {
            MoreThenOnePopUpStackpanel.Width = width;
            MoreThenOnePopUpStackpanel.Height = height;
        }

        private void ResumeTraining_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pauseImg.Visibility = Visibility.Collapsed;
            CountdownText.Text = TIMER_COUNT.ToString();
            countDownTextBorder.Visibility = Visibility.Visible;

            countDownTimer.Start();
        }
    }
}
