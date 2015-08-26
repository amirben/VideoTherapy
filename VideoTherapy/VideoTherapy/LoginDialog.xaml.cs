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

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : UserControl
    {
        private UIElement _parent;
        private bool _hideRequest = false;

        public LoginDialog()
        {
            InitializeComponent();
            Visibility = Visibility.Visible;
        }


        public void SetParent(UIElement parent)
        {
            _parent = parent;

        }
        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            HideHandlerDialog();
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
            //_parent.IsEnabled = true;
        }
    }
}
