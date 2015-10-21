﻿using System;
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

using VideoTherapy_Objects;

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool authenticated = false;

        public MainWindow()
        {
            InitializeComponent();
            
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //LoginDialog ld = new LoginDialog();

            //ld.SetMainWindow(this);
            //DataContext = ld;
            //OpenningWindow.Children.Add(ld);

            OpenLoginPopUp();

            //OpenTreatmentWindow();

            //OpenTrainingWindow();


            //OpenExerciseWindow();
        }

        private void OpenLoginPopUp()
        {
            LoginDialog ld = new LoginDialog();

            ld.SetMainWindow(this);
            DataContext = ld;
            OpenningWindow.Children.Add(ld);
        }
        private void OpenExerciseWindow()
        {
            OpenningWindow.Children.Clear();
            OpenningWindow.Background = null;
            OpenningWindow.Children.Add(new ExerciseView());
        }

        private void OpenTrainingWindow()
        {
            OpenningWindow.Children.Clear();
            OpenningWindow.Background = null;
            OpenningWindow.Children.Add(new TrainingMenu());
        }

        public void OpenTreatmentWindow(Patient _currentPatient)
        {
            OpenningWindow.Children.Clear();
            OpenningWindow.Background = null;
            using (TreatmentMenu _treatmentMenu = new TreatmentMenu(_currentPatient))
            {
                OpenningWindow.Children.Add(_treatmentMenu);
            }
            //TreatmentMenu _treatmentMenu = new TreatmentMenu(_currentPatient);
            //OpenningWindow.Children.Add(_treatmentMenu);
        }
    }
}
