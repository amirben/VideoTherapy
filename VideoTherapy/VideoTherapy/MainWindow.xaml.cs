using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;

using VideoTherapy.Views;
using VideoTherapy.Objects;
using VideoTherapy.ServerConnections;
using System.IO;

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool authenticated = false;

        public delegate void CloseAppDelegate(Patient _currentPatient);
        public delegate void LogOutDelegate();

        public MainWindow()
        {
            InitializeComponent();
            
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            OpenLoginPopUp();

            //CloseAppDelegate closeApp = new CloseAppDelegate(CloseApp);
            //LogOutDelegate logOut = new LogOutDelegate(LogOut);
        }

        private void CloseApp(Patient _currentPatient)
        {
            //if (_currentPatient != null)
            //{
            //    using (DownloadCache downloadCache = new DownloadCache(_currentPatient))
            //    {
            //        downloadCache.DeleteTempDir();
            //    }

            //}
            
            Close();
        }

        private void LogOut()
        {
            OpenLoginPopUp(true);
        }

        public void OpenLoginPopUp()
        {
            OpenLoginPopUp(false);
        }

        public void OpenLoginPopUp(bool deleteConfig)
        {
            using (LoginDialog ld = new LoginDialog())
            {
                if (deleteConfig)
                {
                    ld.DeleteConfigFile();
                }

                int width = (int)(ActualWidth / 4);
                int height = (int)(ActualHeight * 0.65);

                ld.SetSize(width, height);
                ld.CloseApp += CloseApp;
                ld.MainWindow = this;
                this.Content = ld;
            }               
        }

        private void OpenExerciseWindow()
        {
            //OpenningWindow.Children.Clear();
            //OpenningWindow.Background = null;
            //OpenningWindow.Children.Add(new ExerciseView());
        }

        public void OpenTrainingWindow(Patient _currentPatient, Training _selectedTraining)
        {
            //OpenningWindow.Children.Clear();
            //OpenningWindow.Background = null;
            using (TrainingMenu _trainingMenu = new TrainingMenu(_currentPatient, _selectedTraining))
            {
                _trainingMenu.MainWindow = this;
                _trainingMenu.CloseApp += CloseApp;
                _trainingMenu.LogOut += LogOut;
                this.Content = _trainingMenu;
                //OpenningWindow.Children.Add(_trainingMenu);
            }
        }

        public void OpenTreatmentWindow(Patient _currentPatient)
        {
            //OpenningWindow.Background = null;
            using (TreatmentMenu _treatmentMenu = new TreatmentMenu(_currentPatient))
            {
                _treatmentMenu.CloseApp += CloseApp;
                _treatmentMenu.LogOut += LogOut;
                _treatmentMenu.MainWindow = this;
                this.Content = _treatmentMenu;
            }
        }

        public async void OpenExerciseWindow(Patient _currentPatient, Training _currentTraining)
        {
            foreach (int key in _currentTraining.Playlist2.Keys)
            {
                Exercise exercise = _currentTraining.Playlist2[key][1];
                if (exercise.isTrackable)
                {
                    string json = await ApiConnection.GetExerciseGesturesApiAsync(exercise.ExerciseId);
                    JSONConvertor.GettingExerciseGesture(exercise, json);

                    using (DownloadCache dc = new DownloadCache(_currentPatient))
                    {
                        dc.DownloadGDBfile(exercise);
                    }

                    foreach (var item in _currentTraining.Playlist2[key])
                    {
                        if (!item.isDemo && !item.Equals(exercise))
                        {
                            item.DBPath = exercise.DBPath;
                            item.CopyGesturesFromOrigin(exercise);
                        }

                    }
                }
                
            }

            //OpenningWindow.Background = null;
            using (ExerciseView _exerciseView = new ExerciseView(_currentPatient, _currentTraining))
            {
                _exerciseView.MainWindow = this;
                _exerciseView.CloseApp += CloseApp;
                this.Content = _exerciseView;
            }
        }

        public void DownloadGDBOld()
        {
            ////download the gestures and .gdb files for the current training
            //foreach (var exercise in _currentTraining.Playlist)
            //{
            //    if (!exercise.isDemo && !exercise.isDuplicate)
            //    {
            //        string json = await ApiConnection.GetExerciseGesturesApiAsync(exercise.ExerciseId);
            //        JSONConvertor.GettingExerciseGesture(exercise, json);

            //        using (DownloadCache dc = new DownloadCache(_currentPatient))
            //        {
            //            dc.DownloadGDBfile(exercise);
            //        }
            //    }

            //}
        }
    }
}
