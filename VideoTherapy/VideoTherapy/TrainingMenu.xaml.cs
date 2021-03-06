﻿using System;
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

using VideoTherapy.Views.TrainingMenu;
using VideoTherapy.Views;
using VideoTherapy.Objects;
using System.Threading;
using VideoTherapy.ServerConnections;
using System.ComponentModel;

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for TrainingMenu.xaml
    /// </summary>
    public partial class TrainingMenu : UserControl, IDisposable
    {
        public Patient _currentPatient { set; get; }
        public Training _currentTraining { set; get; }

        public MainWindow MainWindow { set; get; }

        private UC_TrainingSelection _trainingSelection;
        private UC_ExerciseSelection _exerciseSelection;
        private UC_UserInfo _userInfo;

        public delegate void StartPlaylistDelegate(Training currentTraining);
        public delegate void BackToTreatment();
        public delegate void PrevTraining();
        public delegate void NextTraining();

        private SemaphoreSlim finishFirstGDBSemaphore;
        private SemaphoreSlim userInDistanceSemaphore;
        private VT_Splash splash;
        private BackgroundWorker worker;
        private const int FIRST_EXERCISE = 1;
        private bool userInDistance = false;

        //close app
        public event MainWindow.CloseAppDelegate CloseApp;
        public event MainWindow.LogOutDelegate LogOut;

        private Barrier distanceBarrier;

        private System.Diagnostics.Stopwatch watch;

        public TrainingMenu(Patient currentPatient, Training currentTraining)
        {
            InitializeComponent();

            _currentPatient = currentPatient;
            _currentTraining = currentTraining;

            this.Loaded += TrainingMenu_Loaded;

            splash = new VT_Splash();
            //splash.ErrorEvent+=

            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("worker stuck");
            
            //distanceBarrier.SignalAndWait();
            Console.WriteLine("worker release");
            MainWindow.OpenExerciseWindow(_currentPatient, _currentTraining);

            //if (_currentTraining.IsTraceableTraining)
            //{
            //    MainWindow.OpenDistanceChecker(_currentPatient, _currentTraining);
            //}
            //else
            //{
            //    MainWindow.OpenExerciseWindow(_currentPatient, _currentTraining);
            //}

            //MainWindow.OpenExerciseWindow(_currentPatient, _currentTraining);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            finishFirstGDBSemaphore.Wait();
            
            Console.WriteLine("in dowork before sem");
            userInDistanceSemaphore.Wait();
            Console.WriteLine("in dowork after sem");
        }

        public void Dispose()
        {
            
        }

        private void TrainingMenu_Loaded(object sender, RoutedEventArgs e)
        {
            _trainingSelection = new UC_TrainingSelection(_currentPatient, _currentTraining);
            _exerciseSelection = new UC_ExerciseSelection(_currentTraining);
            _userInfo = new UC_UserInfo(_currentPatient);

            watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            Task downloadGDBTask = new Task(DownloadGDBFiles);
            downloadGDBTask.Start();
            
            AttachDelegates();
            AddToUI();
            
        }

        private void DownloadGDBFiles()
        {
            finishFirstGDBSemaphore = new SemaphoreSlim(1);
            //finishFirstGDBSemaphore.Wait();

            Barrier downloadTrainingBarrier = new Barrier(1);
            
            foreach (int key in _currentTraining.Playlist.Keys)
            {
                Exercise exercise = _currentTraining.Playlist[key][1];
                int newKey = key;
               
                if (exercise.IsTraceable && !exercise.Downloaded)
                {
                    downloadTrainingBarrier.AddParticipant();

                    if (newKey == FIRST_EXERCISE)
                    {
                        finishFirstGDBSemaphore.Wait();
                    }
                    Task downloadGDB = new Task(() => DownloadCurrentGDB(exercise, newKey, downloadTrainingBarrier));
                    downloadGDB.Start(); 
                }

            }

            downloadTrainingBarrier.SignalAndWait();
            _currentTraining.Downloaded = true;

            //watch.Stop();
            //var elapsedMs = watch.ElapsedMilliseconds;
            //Console.WriteLine("==> Downloading gdb " + elapsedMs.ToString());
        }

        private async void DownloadCurrentGDB(Exercise exercise, int key, Barrier downloadTrainingBarrier)
        {
            Console.WriteLine("S => Exercise id {0}, key = {1},  start at {2}", exercise.ExerciseId, key, DateTime.Now.ToString("hh:MM:ss.fff"));

            string json = await ApiConnection.GetExerciseGesturesApiAsync(exercise.ExerciseId);
            JSONConvertor.GettingExerciseGesture(exercise, json);

            using (DownloadCache dc = new DownloadCache(_currentPatient))
            {
                dc.DownloadGDBfile(exercise);
                exercise.Downloaded = true;
            }

            if (key == FIRST_EXERCISE) //use for the first gdb - that exercise window can be upload (after splash)
            {
                finishFirstGDBSemaphore.Release();
            }

            foreach (var item in _currentTraining.Playlist[key])
            {
                if (!item.isDemo && !item.Equals(exercise))
                {
                    item.DBPath = exercise.DBPath;
                    item.CopyGesturesFromOrigin(exercise);
                    item.Downloaded = true;
                }

            }

            Console.WriteLine("F => Exercise id {0}, key = {1},  finish at {2}", exercise.ExerciseId, key, DateTime.Now.ToString("hh:MM:ss.fff"));
            downloadTrainingBarrier.SignalAndWait();
        }

        private void AttachDelegates()
        {
            //user info delegates
            _userInfo.closeApp += CloseApp;
            _userInfo.logOut += LogOut;

            //training selection delegates
            StartPlaylistDelegate startPlaylistDelegate = new StartPlaylistDelegate(_trainingSelection_StartPlaylist);
            _trainingSelection.StartPlaylist += _trainingSelection_StartPlaylist;
            _trainingSelection.BackToTreatment += _trainingSelection_BackToTreatment;
            _trainingSelection.PrevTraining += _trainingSelection_PrevTraining;
            _trainingSelection.NextTraining += _trainingSelection_NextTraining;
        }

        private void AddToUI()
        {
            TrainingMenuGrid.Children.Add(_trainingSelection);
            TrainingMenuGrid.Children.Add(_exerciseSelection);
            TrainingMenuGrid.Children.Add(_userInfo);

            Grid.SetColumn(_trainingSelection, 0);
            Grid.SetColumn(_exerciseSelection, 1);
            Grid.SetColumn(_userInfo, 2);
        }

        private void _trainingSelection_NextTraining()
        {
            _currentTraining = _currentPatient.PatientTreatment.NextTraining();

            if (!_currentTraining.Downloaded)
            {
                Task downloadGDBThread = new Task(DownloadGDBFiles);
                downloadGDBThread.Start();
            }

            _trainingSelection.DataContext = _currentTraining;

            _exerciseSelection.CurrentTraining = _currentTraining;
            _exerciseSelection.ShowExerciseList();
        }

        private void _trainingSelection_PrevTraining()
        {
            _currentTraining = _currentPatient.PatientTreatment.PrevTraining();

            if (!_currentTraining.Downloaded)
            {
                Thread downloadGDBTask = new Thread(DownloadGDBFiles);
                downloadGDBTask.Start();
            }

            _trainingSelection.DataContext = _currentTraining;

            _exerciseSelection.CurrentTraining = _currentTraining;
            _exerciseSelection.ShowExerciseList();
        }

        private void _trainingSelection_BackToTreatment()
        {
            MainWindow.OpenTreatmentWindow(_currentPatient);
        }

        private void _trainingSelection_StartPlaylist(Training currentTraining)
        {
            
            if (_currentTraining.IsTraceableTraining)
            {
                userInDistanceSemaphore = new SemaphoreSlim(1);
                userInDistanceSemaphore.Wait();

                worker.RunWorkerAsync();

                using (DistanceWindow distance = new DistanceWindow(_currentPatient, currentTraining, this))
                {
                    this.Content = distance;
                    
                    Console.WriteLine("distance wait finish");
                    //distance.trainingMenu = this;
                }
                //MainWindow.OpenDistanceChecker(_currentPatient, _currentTraining);
            }
            else
            {
                MainWindow.OpenExerciseWindow(_currentPatient, _currentTraining);
            }

            
            //barrier.SignalAndWait();
        }

        public void OpenSplash()
        {
            userInDistance = true;
            
            this.Content = splash;
            splash.MessageTimer.Stop();
            userInDistanceSemaphore.Release();
            Console.WriteLine("distance + splash release");
            //distanceBarrier.SignalAndWait();
        }
    }
}
