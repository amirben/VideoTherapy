﻿using System;
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
using VideoTherapy.Utils;

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

        public ExerciseView ExerciseView;

        public SummaryPopUp()
        {
            InitializeComponent();

            Visibility = Visibility.Visible;
        }


        //public void ShowHandlerDialog()
        //{
        //    Visibility = Visibility.Visible;

        //    //_parent.IsEnabled = false;

        //    _hideRequest = false;
        //    while (!_hideRequest)
        //    {
        //        // HACK: Stop the thread if the application is about to close
        //        if (this.Dispatcher.HasShutdownStarted ||
        //            this.Dispatcher.HasShutdownFinished)
        //        {
        //            break;
        //        }

        //        // HACK: Simulate "DoEvents"
        //        this.Dispatcher.Invoke(
        //            DispatcherPriority.Background,
        //            new ThreadStart(delegate { }));
        //        Thread.Sleep(20);
        //    }

        //}

        //private void HideHandlerDialog()
        //{
        //    _hideRequest = true;
        //    Visibility = Visibility.Hidden;
        //}

        public void SetSize(int height, int width)
        {
            SummaryPopUpStackpanel.Width = width;
            SummaryPopUpStackpanel.Height = height;
        }

        public void UpdateScore()
        {
            int trainingQuality = Scoring.GetTrainingQuailty(CurrentTraining);
            if (trainingQuality != 0)
            {
                MotionQualityValue.Text = trainingQuality.ToString();
                MotionQualityProgressBar.Value = trainingQuality;
            }
            else
            {
                MotionQualityPrecentLbl.Visibility = Visibility.Hidden;
                MotionQualityProgressBar.Value = 0;
                MotionQualityValue.Text = "n/a";
            }


            int trainingScore = Scoring.GetTrainingScore(CurrentTraining);
            if (trainingScore != 0 )
            {
                RepetitionsProgressBar.Value = trainingScore;
                RepetitionScore.Text = trainingScore.ToString();
            }
            else
            {
                RepetitionScorePrecentLbl.Visibility = Visibility.Hidden;
                RepetitionsProgressBar.Value = 0;
                RepetitionScore.Text = "n/a";

            }
        }

        private void OpenQuestionsButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ExerciseView.ClosedSummaryPopUp(true);
        }

        private void GoBackToTreatment_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ExerciseView.ClosedSummaryPopUp(false);
        }
    }
}
