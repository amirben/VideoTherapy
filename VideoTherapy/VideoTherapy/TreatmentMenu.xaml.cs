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
using VideoTherapy;

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for TreatmentMenu.xaml
    /// </summary>
    public partial class TreatmentMenu : UserControl, IDisposable
    {
        /// <summary>
        /// Current patient of the session
        /// </summary>
        public Patient _currentPatient { set; get; }

        /// <summary>
        /// The main window of the application
        /// </summary>
        public MainWindow MainWindow { set; get; }

        /// <summary>
        /// Treatment selection user contorl - use for information of the treatment,
        /// therapist details, calender, compliance and score of the treatment
        /// </summary>
        private UC_TreatmentSelection _treatmentSelection;

        /// <summary>
        /// The user control that show you the recommended training and all the training list
        /// </summary>
        private UC_TrainingProgramSelection _trainingSeleciton;

        /// <summary>
        /// The user control that show you if kinect connected or not, link to user profile
        /// </summary>
        private UC_UserInfo _userInfo;

        /// <summary>
        /// Delegate use for open the selected training
        /// </summary>
        /// <param name="_selectedTraining"></param>
        public delegate void TrainingSelectedDelegate(Training _selectedTraining);

        /// <summary>
        /// Close the application delegate
        /// </summary>
        public event MainWindow.CloseAppDelegate CloseApp;

        /// <summary>
        /// Logout delegate, use for go back to login screen
        /// </summary>
        public event MainWindow.LogOutDelegate LogOut;


        /// <summary>
        /// The constractor of this window
        /// </summary>
        /// <param name="_currentPatient">The current patient of the session</param>
        public TreatmentMenu(Patient _currentPatient)
        {
            InitializeComponent();

            this._currentPatient = _currentPatient;
            this.Loaded += TreatmentMenu_Loaded;
        }


        private void TreatmentMenu_Loaded(object sender, RoutedEventArgs e)
        {
            //create the user controls inside this window.
            _treatmentSelection = new UC_TreatmentSelection(_currentPatient.PatientTreatment);
            _trainingSeleciton = new UC_TrainingProgramSelection(_currentPatient.PatientTreatment);
            _userInfo = new UC_UserInfo(_currentPatient);
            //TrainingSelectedDelegate trainingSelected = new TrainingSelectedDelegate(_trainingSeleciton_trainingSelectedEvent);

            //attach training selection handler
            _trainingSeleciton.trainingSelectedEvent += _trainingSeleciton_trainingSelectedEvent;

            //attach the handlers
            _userInfo.closeApp += CloseApp;
            _userInfo.logOut += LogOut;

            //Add the user control into this window
            TreatmentMenuGrid.Children.Add(_treatmentSelection);
            TreatmentMenuGrid.Children.Add(_trainingSeleciton);
            TreatmentMenuGrid.Children.Add(_userInfo);

            Grid.SetColumn(_treatmentSelection, 0);
            Grid.SetColumn(_trainingSeleciton, 1);
            Grid.SetColumn(_userInfo, 2);
        }

        /// <summary>
        /// Handling the selection of the training and open his window
        /// </summary>
        /// <param name="_selectedTraining">The select training</param>
        private void _trainingSeleciton_trainingSelectedEvent(Training _selectedTraining)
        {
            _currentPatient.PatientTreatment.CurrentTraining = _selectedTraining;
            MainWindow.OpenTrainingWindow(_currentPatient, _selectedTraining);
            
            //todo - start download gdb files
            //
        }

        public void Dispose()
        {
        }
    }
}
