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

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : UserControl
    {
        private UIElement _parent;
        private bool _hideRequest = false;
        private MainWindow mainWindow;

        private string apiUri = "https://videotherapy.co/dev/vt/api/dispatcher.php";
        private string appLogin = "app-login";
        private string clientKey = "8e28b8db-6395-4417-9df9-10dd0efb5ef9";

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
            String email = emailTxt.Text;
            String password = passwordTxt.Password;

            Dictionary<string, string> pairs = new Dictionary<string, string>();
            pairs.Add("api", appLogin);
            pairs.Add("clientKey", clientKey);
            pairs.Add("email", email);
            pairs.Add("password", password);

            using (HttpClient client = new HttpClient())
            {
                string json = new JavaScriptSerializer().Serialize((object)pairs);
                HttpContent contentPost = new StringContent(json, Encoding.UTF8);

                var response = await client.PostAsync(apiUri, contentPost);
                var x = response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();

                mainWindow.OpenTreatmentWindow();
            }

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
