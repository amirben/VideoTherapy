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

using VideoTherapy.Views;
using System.ComponentModel;

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

        /// <summary>
        /// saving the check-box value due to ui-thread problems
        /// </summary> 
        private bool isSavingConfig = false;

        /// <summary>
        /// Worker for download all user data from server
        /// </summary> 
        private BackgroundWorker worker;

        /// <summary>
        /// If error accure somewhere, it will stop all connection to server
        /// </summary> 
        private bool isErrorAccure = false;

        /// <summary>
        /// Saving the login screen in case that there is an error and need to show it again
        /// </summary> 
        private object loginScreen;

        /// <summary>
        /// Saving the last error messege that accure
        /// </summary>
        private ErrorMessege lastErrorMessege;

        /// <summary>
        /// Splash screen when loading
        /// </summary>
        private VT_Splash splash;

        //*************
        //todo - remove
        private Stopwatch watch;
        //*************

        #endregion

        #region constractor
        /// <summary>
        /// Constractor
        /// </summary> 
        public LoginDialog()
        {
            InitializeComponent();

            //Adding event to the static class
            JSONConvertor.ErrorEvent += JSONConvertor_ErrorEvent;

            splash = new VT_Splash();
            splash.ErrorEvent += JSONConvertor_ErrorEvent;
            splash.CloseApp += Close;
           

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
#if DEBUG
                passwordTxt.Password = user.Element("pass").Value;
#endif

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
            //create worker for background job
            CreateWorker();
            isErrorAccure = false;

            string email = emailTxt.Text;
            string password = MD5Hash.CalculateMD5Hash(passwordTxt.Password);
            isSavingConfig = SaveConfig.IsChecked.Value;

            //************
            //todo - remove
            watch = new Stopwatch();
            watch.Start();
            //************

            //retrive the user id
            int userIdExist = await LoginUserForId(email, password);

            switch (userIdExist)
            {

                case 0://in case that email or password are wrong
                    //print error to error label
                    errorMessegeFromServer.Content = "Error in username or password";
                    errorMessegeFromServer.Visibility = Visibility.Visible;

                    break;

                case 1: //if there is a user id
                    //Save login screen and change to splash
                    loginScreen = this.Content;
                    this.Content = splash;
                    splash.MessageTimer.Start();

                    //start download data
                    worker.RunWorkerAsync();
                    break;

                case 2:
                    //error connection
                    errorMessegeFromServer.Content = "Check your connection";
                    errorMessegeFromServer.Visibility = Visibility.Visible;
                    break;
            }

        }

        /// <summary>
        /// Login by email and password
        /// </summary>
        /// <param name="emil">User email</param>
        /// <param name="password">User password</param>
        private async Task<int> LoginUserForId(string email, string password)
        {
            bool regexCheck = Regex.IsMatch(email,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

            if (regexCheck)
            {
                wrongInputLbl.Visibility = Visibility.Hidden;

                email = Uri.EscapeDataString(email);
                try
                {
                    string loginData = await ApiConnection.AppLoginApiAsync(email, password);

                    _currentPatient = JSONConvertor.CreatePatient(loginData);

                    return _currentPatient != null ? 1 : 0;
                }
                catch (HttpRequestException httpE)
                {
                    
                    return 2;
                }
                
            }

            wrongInputLbl.Visibility = Visibility.Visible;
            return 0;   
        }

        /// <summary>
        /// Retrive patient details from server (such as name, age, gender, image and etc.)
        /// </summary>
        private async void RetrivePatientDetails()
        {
            try
            {
                await _semaphoreSlime.WaitAsync();
                if (!isErrorAccure)
                {
                    string userData = await ApiConnection.GetUserDataApiAsync(_currentPatient.AccountId, ApiConnection.PATIENT_LEVEL);
                    JSONConvertor.GettingPatientData(_currentPatient, userData);
                }
                
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
                if (!isErrorAccure)
                {
                    //string treatmentData = await ApiConnection.GetTreatmentApiAsync(_currentPatient.PatientTreatment.TreatmentId);
                    string userTreatmentsData = await ApiConnection.GetTreatmentByUserApiAsync(_currentPatient.AccountId);
                    JSONConvertor.GettingPatientTreatment(_currentPatient, userTreatmentsData);
                }
                
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

                if (!isErrorAccure)
                {
                    Barrier barrier = new Barrier(_currentPatient.PatientTreatment.TrainingList.Count + 1);
                    
                    //downloading all the trainings data in the current treatment
                    foreach (Training training in _currentPatient.PatientTreatment.TrainingList)
                    {
                        Thread t = new Thread(async () =>
                        {
                            string trainingData = await ApiConnection.GetTrainingApiAsync(training.TrainingId);
                            JSONConvertor.GettingPatientTraining2(training, trainingData);

                            barrier.SignalAndWait();
                        });

                        t.Start();
                    }

                    barrier.SignalAndWait();
                    
                    //downloading current images for treatment screen
                    using (DownloadCache _downloadCache = new DownloadCache(_currentPatient))
                    {
                        _downloadCache.DownloadAllTreatmentImages();
                    }
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

                if (!isErrorAccure)
                {
                    string therapistData = await ApiConnection.GetUserDataApiAsync(_currentPatient.PatientTreatment.TreatmentTherapist.AccountId, ApiConnection.THERAPIST_LEVEL);
                    JSONConvertor.GettingTherapistData(_currentPatient.PatientTreatment.TreatmentTherapist, therapistData);

                    _currentPatient.PatientTreatment.TreatmentTherapist.StartDate = _currentPatient.PatientTreatment.StartDate;
                }
                
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
                if (!isErrorAccure)
                {
                    //configuration handler
                    if (isSavingConfig)
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

#if DEBUG
            writer.WriteStartElement("pass");
            writer.WriteString(passwordTxt.Password);
            writer.WriteEndElement();
#endif

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

        /// <summary>
        /// Create new worker
        /// </summary>
        private void CreateWorker()
        {
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }
        #endregion

        #region events
        /// <summary>
        /// Login btn click event - not used
        /// </summary>
        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

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
            if (worker != null)
            {
                worker.Dispose();
            }
        }

        /// <summary>
        /// Close button event - close the application
        /// </summary>
        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseApp(null);
        }

        private void Close(Patient patient)
        {
            CloseApp(null);
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

        /// <summary>
        /// worker completed method - open treatment screen
        /// </summary>
        private async void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            await _semaphoreSlime.WaitAsync();

            //**************
            //TODO - remove
            watch.Stop();
            TimeSpan time = watch.Elapsed;
            Console.WriteLine(time.ToString());
            //**************

            if (isErrorAccure)
            {
                //change back to login;
                this.Content = loginScreen;

                //print error to error label
                errorMessegeFromServer.Content = "Error: " + lastErrorMessege.Code + " " + lastErrorMessege.Messege.ToString();
                errorMessegeFromServer.Visibility = Visibility.Visible;
            }
            else
            {
                MainWindow.OpenTreatmentWindow(_currentPatient);
            }

        }

        /// <summary>
        /// worker do work - download all user data
        /// </summary>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            RetrivePatientDetails();

            RetriveTreatmentDetails();

            //RetriveTherapistDetails();

            RetriveTrainingDetails();

            IsNeedToSaveConfig();

        }

        public void LoginErrorAccur()
        {
            worker.CancelAsync();
                
            //print error to error label
            errorMessegeFromServer.Content = "We had problem to login, try again";
            errorMessegeFromServer.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// handle the error messege and returning to the login screen
        /// </summary>
        private void JSONConvertor_ErrorEvent(object sender, EventArgs e)
        {
            //stop worker
            if (worker.IsBusy)
            {
                worker.CancelAsync();
            }
                
            worker.Dispose();

            isErrorAccure = true;
            lastErrorMessege = e as ErrorMessege;
        }
        #endregion


    }
}
