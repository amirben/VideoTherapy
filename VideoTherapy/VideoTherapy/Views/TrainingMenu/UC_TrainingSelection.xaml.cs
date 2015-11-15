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

        public event VideoTherapy.TrainingMenu.StartPlaylistDelegate _startPlaylist;

        public UC_TrainingSelection(Patient currentPatient, Training currentTraining)
        {
            InitializeComponent();

            _currentPatient = currentPatient;
            _currentTraining = currentTraining;

            DataContext = _currentTraining;
        }

        private void StartButton(object sender, RoutedEventArgs e)
        {
            _startPlaylist(_currentTraining);
        }
    }
}
