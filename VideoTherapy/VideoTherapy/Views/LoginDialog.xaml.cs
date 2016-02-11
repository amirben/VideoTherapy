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
using System.Diagnostics;
using System.Text.RegularExpressions;

using VideoTherapy.ServerConnections;
using VideoTherapy.Objects;
using VideoTherapy.Utils;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : UserControl, IDisposable
    {
        #region members

        /// <summary>
        /// User patient variable, use fo retrive all data on user
        /// </summary>       
        private Patient _currentPatient;

        /// <summary>
        /// Semaphore used to syncronize the call to api
        /// </summary> 
        private SemaphoreSlim _semaphoreSlime = new SemaphoreSlim(1);

        /// <summary>
        /// Parent of corrent user control
        /// </summary> 
        public MainWindow MainWindow;

        /// <summary>
        /// Close the application event
        /// </summary> 
        public event VideoTherapy.MainWindow.CloseAppDelegate CloseApp;

        /// <summary>
        /// User configuration file name
        /// </summary> 
        private string config_path = @"user_config.xml";

        #endregion 

        #region constractor
        /// <summary>
        /// Constractor
        /// </summary> 
        public LoginDialog()
        {
            InitializeComponent();

            CheckConfigFile();
        }

        #endregion

        #region methods
        /// <summary>
        /// Check if there is a configuration file, if so read the data from him to show in the login popup
        /// </summary>
        private bool CheckConfigFile()
        {
            try
            {
                XDocument xmlReader = XDocument.Load(config_path);

                var user = xmlReader.Element("user");

                welcomeMessege.Content = "Welcome back";

                string s = user.Element("user_img").Value;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(s, UriKind.Absolute);
                bitmap.EndInit();
                userImg.Source = bitmap;

                userFullName.Content = user.Element("user_fullname").Value;
                emailTxt.Text = user.Element("user_name").Value;
                passwordTxt.Password = user.Element("pass").Value;

                SaveConfig.IsChecked = true;

                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Login method, calling the methonds that read from server
        /// </summary>
        private async void Login()
        {
            //todo - check input text
            string email = emailTxt.Text;
            string password = MD5Hash.CalculateMD5Hash(passwordTxt.Password);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //retrive the user id
            //todo - check for errors from server
            bool userIdExist = await LoginUserForId(email, password);

            //if there is a user id
            if (userIdExist)
            {
                RetrivePatientDetails();

                RetriveTreatmentDetails();

                RetriveTherapistDetails();

                RetriveTrainingDetails();

                IsNeedToSaveConfig();

                await _semaphoreSlime.WaitAsync();

                //TODO - remove
                watch.Stop();
                TimeSpan time = watch.Elapsed;
                Console.WriteLine(time.ToString());

                MainWindow.OpenTreatmentWindow(_currentPatient);
            }

            //in case that email or password are wrong
            else
            {

            }
            
            //TODO - REMOVE
            //string gestures = await ApiConnection.GetExerciseGesturesApiAsync(1);
            //JSONConvertor.GettingExerciseGesture(_currentPatient.PatientTreatment.TrainingList[0].Playlist[0], gestures);
        }

        /// <summary>
        /// Login by email and password
        /// </summary>
        /// <param name="emil">User email</param>
        /// <param name="password">User password</param>
        private async Task<bool> LoginUserForId(string email, string password)
        {
            bool regexCheck = Regex.IsMatch(email,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

            if (regexCheck)
            {
                wrongInputLbl.Visibility = Visibility.Hidden;

                string loginData = await ApiConnection.AppLoginApiAsync(email, password);
                //todo - check errors
                _currentPatient = JSONConvertor.CreatePatient(loginData);

                //todo - check error code for login

                return _currentPatient != null ? true : false;
            }

            wrongInputLbl.Visibility = Visibility.Visible;
            return false;   
        }

        /// <summary>
        /// Retrive patient details from server (such as name, age, gender, image and etc.)
        /// </summary>
        private async void RetrivePatientDetails()
        {
            try
            {
                await _semaphoreSlime.WaitAsync();
                string userData = await ApiConnection.GetUserDataApiAsync(_currentPatient.AccountId, ApiConnection.PATIENT_LEVEL);
                //todo - check errors
                JSONConvertor.GettingPatientData(_currentPatient, userData);
            }
            finally
            {
                _semaphoreSlime.Release();
            }
                       
        }

        /// <summary>
        /// Retrive treatment details from server (such id, start date, therapsit id, list of training and etc.)
        /// </summary>
        private async void RetriveTreatmentDetails()
        {
            try
            {
                await _semaphoreSlime.WaitAsync();
                string treatmentData = await ApiConnection.GetTreatmentApiAsync(_currentPatient.PatientTreatment.TreatmentId);
                JSONConvertor.GettingPatientTreatment(_currentPatient, treatmentData);
            }
            finally
            {
                _semaphoreSlime.Release();
            }
            
        }

        /// <summary>
        /// Retrive trainings detail from server (such id, all exercises list and etc.)
        /// </summary>
        private async void RetriveTrainingDetails()
        {
            try
            {
                await _semaphoreSlime.WaitAsync();

                //downloading all the trainings data in the current treatment
                foreach (Training training in _currentPatient.PatientTreatment.TrainingList)
                {
                    string trainingData = await ApiConnection.GetTrainingApiAsync(training.TrainingId);
                    JSONConvertor.GettingPatientTraining(training, trainingData);
                }


                //downloading current images for treatment screen
                using (DownloadCache _downloadCache = new DownloadCache(_currentPatient))
                {
                    _downloadCache.DownloadAllTreatmentImages();
                    //_downloadCache.DownloadTreatment();
                }
            }
            finally
            {
                _semaphoreSlime.Release();
            }
        }

        /// <summary>
        /// Retrive therapist detail from server (such id, name, image and etc.)
        /// </summary>
        private async void RetriveTherapistDetails()
        {
            try
            {
                await _semaphoreSlime.WaitAsync();
                //todo get patient current therapist
                string therapistData = await ApiConnection.GetUserDataApiAsync(_currentPatient.PatientTreatment.TreatmentTherapist.AccountId, ApiConnection.THERAPIST_LEVEL);
                JSONConvertor.GettingTherapistData(_currentPatient.PatientTreatment.TreatmentTherapist, therapistData);

                _currentPatient.PatientTreatment.TreatmentTherapist.StartDate = _currentPatient.PatientTreatment.StartDate;
            }
            finally
            {
                _semaphoreSlime.Release();
            }
        }

        /// <summary>
        /// Check if user ask to rememeber his details.
        /// </summary>
        private async void IsNeedToSaveConfig()
        {
            try
            {
                await _semaphoreSlime.WaitAsync();

                //configuration handler
                if (SaveConfig.IsChecked.Value)
                {
                    //saving config file
                    SaveUserConfig(_currentPatient);
                }
                else
                {
                    //clear the config file
                    DeleteConfigFile();
                }
            }
            finally
            {
                _semaphoreSlime.Release();
            }
            
        }

        /// <summary>
        /// Saving user configuration details to file
        /// </summary>
        private void SaveUserConfig(Patient _currentPatient)
        {
            XmlTextWriter writer = new XmlTextWriter(config_path, System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;

            writer.WriteStartElement("user");

            writer.WriteStartElement("user_fullname");
            writer.WriteString(_currentPatient.FullName);
            writer.WriteEndElement();

            writer.WriteStartElement("user_name");
            writer.WriteString(_currentPatient.Email);
            writer.WriteEndElement();

            writer.WriteStartElement("pass");
            writer.WriteString(passwordTxt.Password);
            writer.WriteEndElement();

            writer.WriteStartElement("user_img");
            writer.WriteString(_currentPatient.ImageThumb);
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        /// <summary>
        /// Adjust popup window to screen.
        /// </summary>
        public void SetSize(int width, int height)
        {
            LoginPopup.Width = width;
            LoginPopup.Height = height;
        }

        /// <summary>
        /// Delete configuration file if user didn't ask to keep his details.
        /// </summary>
        public void DeleteConfigFile()
        {
            try
            {
                File.Delete(config_path);
            }

            catch (Exception e)
            {

            }

        }
        #endregion

        #region events
        /// <summary>
        /// Login btn click event - not used
        /// </summary>
        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        /*public void ShowHandlerDialog()
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
        }*/

        /// <summary>
        /// Login button click event
        /// </summary>
        private void LoginBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Login();
        }

        /// <summary>
        /// Used for show or not the "password" hint
        /// </summary>
        private void passwordTxt_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (passwordTxt.Password.Length > 0)
            {
                passwordHint.Visibility = Visibility.Hidden;
            }
            else
            {
                if (!passwordHint.IsKeyboardFocused)
                    passwordHint.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// IDisposable interface method
        /// </summary>
        public void Dispose()
        {
            
        }

        /// <summary>
        /// Close button event - close the application
        /// </summary>
        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseApp();
        }

        /// <summary>
        /// Check email regex, show messege if the email isn't like pattern
        /// </summary>
        private void emailTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool regexCheck = Regex.IsMatch(emailTxt.Text,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

            if (!regexCheck)
            {
                wrongInputLbl.Visibility = Visibility.Visible;
                LoginBtn.IsEnabled = false;
                LoginBtn.Opacity = 0.5;
            }
            else
            {
                wrongInputLbl.Visibility = Visibility.Hidden;
                LoginBtn.IsEnabled = true;
                LoginBtn.Opacity = 1;
            }
        }
        #endregion


    }
}
