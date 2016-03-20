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
using VideoTherapy.Objects;


namespace VideoTherapy.Views.TrainingMenu
{
    /// <summary>
    /// Interaction logic for UC_TrainingSelection.xaml
    /// </summary>
    public partial class UC_TrainingSelection : UserControl
    {
        private Patient _currentPatient;
        private Training _currentTraining;

        public event VideoTherapy.TrainingMenu.StartPlaylistDelegate StartPlaylist;
        public event VideoTherapy.TrainingMenu.BackToTreatment BackToTreatment;
        public event VideoTherapy.TrainingMenu.NextTraining NextTraining;
        public event VideoTherapy.TrainingMenu.PrevTraining PrevTraining;

        public UC_TrainingSelection(Patient currentPatient, Training currentTraining)
        {
            InitializeComponent();

            _currentPatient = currentPatient;
            _currentTraining = currentTraining;

            DataContext = _currentTraining;
        }

        private void StartButton(object sender, RoutedEventArgs e)
        {
            
            StartPlaylist(_currentTraining);
        }

        private void NextTrainingBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NextTraining();

        }

        private void BackTrainingBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PrevTraining();
        }

        private void BackToTreatmentBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BackToTreatment();
        }

        private void SkipDemoCheckBox_Checked(object sender, RoutedEventArgs e)
        {

            HandlerSkipDemo(sender as CheckBox);
        }

        private void HandlerSkipDemo(CheckBox checkBox)
        {
            _currentTraining.SkipDemo = checkBox.IsChecked.Value;
        }
    }
}
