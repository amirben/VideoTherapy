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

using VideoTherapy.Objects;

namespace VideoTherapy.Views.ExerciseScreen
{
    /// <summary>
    /// Interaction logic for SummaryPopUp.xaml
    /// </summary>
    public partial class SummaryPopUp : UserControl
    {
        private bool _hideRequest = false;
        public Patient CurrentPatient;
        public Training CurrentTraining;

        public SummaryPopUp()
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
            SummaryPopUpStackpanel.Width = width;
            SummaryPopUpStackpanel.Height = height;
        }
    }
}
