using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for PausePopUp.xaml
    /// </summary>
    public partial class PausePopUp : UserControl
    {
        private bool _hideRequest = false;
        public event ExerciseView.ClosePausePopupDelegate ClosePopup;

        public PausePopUp()
        {
            InitializeComponent();
            Visibility = Visibility.Visible;

        }

        public void ShowHandlerDialog()
        {
            Visibility = Visibility.Visible;

            //_parent.IsEnabled = false;

            _hideRequest = false;
            while (!_hideRequest)
            {
                // HACK: Stop the thread if the application is about to close
                if (this.Dispatcher.HasShutdownStarted ||
                    this.Dispatcher.HasShutdownFinished)
                {
                    break;
                }

                // HACK: Simulate "DoEvents"
                this.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new ThreadStart(delegate { }));
                Thread.Sleep(20);
            }

        }

        private void HideHandlerDialog()
        {
            _hideRequest = true;
            Visibility = Visibility.Hidden;
        }

        public void SetSize(int height, int width)
        {
            PausePopUpStackpanel.Width = width;
            PausePopUpStackpanel.Height = height;
        }

        private void ResumeTraining_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ClosePopup();
        }
    }
}
