using System;
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

        public TrainingMenu(Patient currentPatient, Training currentTraining)
        {
            InitializeComponent();

            _currentPatient = currentPatient;
            _currentTraining = currentTraining;

            this.Loaded += TrainingMenu_Loaded;
        }

        public void Dispose()
        {
            
        }

        private void TrainingMenu_Loaded(object sender, RoutedEventArgs e)
        {
            _trainingSelection = new UC_TrainingSelection(_currentPatient, _currentTraining);
            _exerciseSelection = new UC_ExerciseSelection(_currentTraining);

            _userInfo = new UC_UserInfo(_currentPatient);
            _userInfo.ShowRecommended = false;
            //_userInfo.CurrentTraining = _currentTraining;

            StartPlaylistDelegate startPlaylistDelegate = new StartPlaylistDelegate(_trainingSelection_StartPlaylist);
            _trainingSelection.StartPlaylist += _trainingSelection_StartPlaylist;
            _trainingSelection.BackToTreatment += _trainingSelection_BackToTreatment;
            _trainingSelection.PrevTraining += _trainingSelection_PrevTraining;
            _trainingSelection.NextTraining += _trainingSelection_NextTraining;

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
            _trainingSelection.DataContext = _currentTraining;
            _exerciseSelection.DataContext = _currentTraining;

        }

        private void _trainingSelection_PrevTraining()
        {
            _currentTraining = _currentPatient.PatientTreatment.PrevTraining();
            _trainingSelection.DataContext = _currentTraining;
            _exerciseSelection.DataContext = _currentTraining;
        }

        private void _trainingSelection_BackToTreatment()
        {
            MainWindow.OpenTreatmentWindow(_currentPatient);
        }

        private void _trainingSelection_StartPlaylist(Training currentTraining)
        {
            MainWindow.OpenExerciseWindow(_currentPatient, currentTraining);
        }
    }
}
