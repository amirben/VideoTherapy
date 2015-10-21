using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using System.Web.Script.Serialization;

using VideoTherapy.ServerConnections;
using VideoTherapy_Objects;

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : UserControl
    {
        private bool _hideRequest = false;
        private MainWindow mainWindow;


        public bool IsLogin { set; get;}

        public LoginDialog()
        {
            InitializeComponent();

            Visibility = Visibility.Visible;
        }


        public void SetMainWindow(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

        }
        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            //String email = emailTxt.Text;
            //String password = passwordTxt.Password;

            string email = "amir.ben@gmail.com";
            string password = "123456789";

            string loginData = await ApiConnection.AppLoginApiAsync(email, password);
            Patient _currentPatient = JSONConvertor.CreatePatient(loginData);

            string userData = await ApiConnection.GetUserDataApiAsync(_currentPatient.AccountId);
            JSONConvertor.GettingPatientData(_currentPatient, userData);

            string treatmentData = await ApiConnection.GetUserTrainingApiAsync(_currentPatient.AccountId);
            JSONConvertor.GettingPatientTreatment(_currentPatient, treatmentData);
            //todo get patient current therapist

            mainWindow.OpenTreatmentWindow(_currentPatient);
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
        }
    }
}
