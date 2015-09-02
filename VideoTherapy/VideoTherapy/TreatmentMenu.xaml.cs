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

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for TreatmentMenu.xaml
    /// </summary>
    public partial class TreatmentMenu : UserControl
    {
        private List<Treatment> _treatmentList;
        private int _currentTreatment = 0;
        private int _numOfTreatments = 0;

        public TreatmentMenu()
        {
            InitializeComponent();
            this.Loaded += TreatmentMenu_Loaded;
        }

        private void TreatmentMenu_Loaded(object sender, RoutedEventArgs e)
        {
            _treatmentList = ObjectReader.GetAllTreatments();
            if (_treatmentList != null && _treatmentList.Count > 0)
            {
                _numOfTreatments = _treatmentList.Count;

                CurrentTreatmentLbl.DataContext = _treatmentList[0];
                TreatmentExercisesList.DataContext = _treatmentList[0].ExerciseList;
            }
        }

        private void ForwardBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentTreatment++;
            _currentTreatment = _currentTreatment % _numOfTreatments;

            TreatmentExercisesList.DataContext = _treatmentList[_currentTreatment].ExerciseList;
            CurrentTreatmentLbl.DataContext = _treatmentList[_currentTreatment];
        }

        private void BackwardBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentTreatment--;
            if (_currentTreatment < 0)
            {
                _currentTreatment = _numOfTreatments - 1;
            }

            TreatmentExercisesList.DataContext = _treatmentList[_currentTreatment].ExerciseList;
            CurrentTreatmentLbl.DataContext = _treatmentList[_currentTreatment];
        }
    }
}
