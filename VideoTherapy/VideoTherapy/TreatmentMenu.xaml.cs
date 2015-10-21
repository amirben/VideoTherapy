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
using VideoTherapy_Objects;
using VideoTherapy.Views;
using VideoTherapy.Views.TreatmentMenu;


namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for TreatmentMenu.xaml
    /// </summary>
    public partial class TreatmentMenu : UserControl, IDisposable
    {
        public Patient _currentPatient { set; get; }
        private UC_TreatmentSelection treatmentSelection;
        private UC_TrainingProgramSelection trainingSeleciton;
        private UC_UserInfo userInfo;

        public TreatmentMenu(Patient _currentPatient)
        {
            InitializeComponent();

            this._currentPatient = _currentPatient;
            this.Loaded += TreatmentMenu_Loaded;
        }

        private void TreatmentMenu_Loaded(object sender, RoutedEventArgs e)
        {
            treatmentSelection = new UC_TreatmentSelection(_currentPatient.PatientTreatment);
            trainingSeleciton = new UC_TrainingProgramSelection(_currentPatient.PatientTreatment.TrainingList);
            userInfo = new UC_UserInfo(_currentPatient);
            
            TreatmentMenuGrid.Children.Add(treatmentSelection);
            TreatmentMenuGrid.Children.Add(trainingSeleciton);
            TreatmentMenuGrid.Children.Add(userInfo);

            Grid.SetColumn(treatmentSelection, 0);
            Grid.SetColumn(trainingSeleciton, 1);
            Grid.SetColumn(userInfo, 2);
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
            
        }
    }
}
