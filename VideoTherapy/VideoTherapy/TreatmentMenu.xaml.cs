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
using VideoTherapy.XML;
using VideoTherapy.Objects;
using VideoTherapy.Views;
using VideoTherapy.Views.TreatmentMenu;
using VideoTherapy.ServerConnections;

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for TreatmentMenu.xaml
    /// </summary>
    public partial class TreatmentMenu : UserControl, IDisposable
    {
        public Patient _currentPatient { set; get; }
        public MainWindow MainWindow { set; get; }

        private UC_TreatmentSelection _treatmentSelection;
        private UC_TrainingProgramSelection _trainingSeleciton;
        private UC_UserInfo _userInfo;

        public delegate void TrainingSelectedDelegate(Training _selectedTraining);


        public TreatmentMenu(Patient _currentPatient)
        {
            InitializeComponent();

            this._currentPatient = _currentPatient;
            this.Loaded += TreatmentMenu_Loaded;
        }

        private void TreatmentMenu_Loaded(object sender, RoutedEventArgs e)
        {
            _treatmentSelection = new UC_TreatmentSelection(_currentPatient.PatientTreatment);
            _trainingSeleciton = new UC_TrainingProgramSelection(_currentPatient.PatientTreatment.TrainingList);

            TrainingSelectedDelegate trainingSelected = new TrainingSelectedDelegate(_trainingSeleciton_trainingSelectedEvent);
            _trainingSeleciton.trainingSelectedEvent += _trainingSeleciton_trainingSelectedEvent;

            _userInfo = new UC_UserInfo(_currentPatient);
            _userInfo.ShowRecommended = true;
            _userInfo.CurrentTraining = _currentPatient.PatientTreatment.CurrentTraining;
            _userInfo.trainingSelectedEvent += _trainingSeleciton_trainingSelectedEvent;
                    
           

            TreatmentMenuGrid.Children.Add(_treatmentSelection);
            TreatmentMenuGrid.Children.Add(_trainingSeleciton);
            TreatmentMenuGrid.Children.Add(_userInfo);

            Grid.SetColumn(_treatmentSelection, 0);
            Grid.SetColumn(_trainingSeleciton, 1);
            Grid.SetColumn(_userInfo, 2);
        }

        private async void _trainingSeleciton_trainingSelectedEvent(Training _selectedTraining)
        {
            //Console.WriteLine(_selectedTraining);
            string json = await ApiConnection.GetTrainingApiAsync(_selectedTraining.TrainingId);
            JSONConvertor.GettingPatientTraining(_selectedTraining, json);

            DownloadCache cache = new DownloadCache(_currentPatient);
            cache.DownloadTraining(_currentPatient, _currentPatient.PatientTreatment.TrainingList.IndexOf(_selectedTraining));

            MainWindow.OpenTrainingWindow(_currentPatient, _selectedTraining);
        }

        public void Dispose()
        {
            Console.WriteLine("Treatment window disposed");
        }
    }
}
