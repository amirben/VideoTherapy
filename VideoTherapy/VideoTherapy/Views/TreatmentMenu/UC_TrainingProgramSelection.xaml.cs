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

using VideoTherapy.Views.TreatmentMenu;

using VideoTherapy.XML;


namespace VideoTherapy.Views.TreatmentMenu
{
    /// <summary>
    /// Interaction logic for UC_TrainingProgramSelection.xaml
    /// </summary>
    public partial class UC_TrainingProgramSelection : UserControl
    {
        private List<Training> _treatmentList;
        private List<Training> _currentTrainingList;

        public event VideoTherapy.TreatmentMenu.TrainingSelectedDelegate trainingSelectedEvent;

        public UC_TrainingProgramSelection(List<Training> _trainingList)
        {
            InitializeComponent();

            SetTrainingList(_trainingList);
            this.Loaded += UC_TrainingProgramSelection_Loaded;
        }

        private void UC_TrainingProgramSelection_Loaded(object sender, RoutedEventArgs e)
        {
            //_treatmentList = ObjectReader.GetAllTreatments();
            //TreatmentTrainingList.DataContext = _treatmentList[0].TrainingList;
        }

        public void SetTrainingList(List<Training> _trainingList)
        {
            _currentTrainingList = _trainingList;
            TreatmentTrainingList.DataContext = _trainingList;
        }

        public void DoubleClickHandler(object sender, MouseEventArgs e)
        {
            var selectedTraining = sender as ListBoxItem;
            var training = (Training)selectedTraining.DataContext;

            trainingSelectedEvent(training);
        }
    }
}
