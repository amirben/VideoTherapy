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
        private List<Training> _treatmentList; //for future porpuse

        /// <summary>
        /// training list of the treatment
        /// </summary>
        private List<Training> _currentTrainingList;

        /// <summary>
        /// Current treatement that is showing
        /// </summary>
        private Treatment CurrentTreatment;

        /// <summary>
        /// event handler that called when training has been selected
        /// </summary>
        public event VideoTherapy.TreatmentMenu.TrainingSelectedDelegate trainingSelectedEvent;


        /// <summary>
        /// Training program selection constractor
        /// </summary>
        /// <param name="currentTreatemet">The current treatement that the user have</param>
        public UC_TrainingProgramSelection(Treatment currentTreatemet)
        {
            InitializeComponent();

            CurrentTreatment = currentTreatemet;
            SetTrainingList(currentTreatemet.TrainingList);
            
            //if there is a recommended training, then will a panel will appear otherwise will not
            if (CurrentTreatment.RecommendedTraining != null)
            {
                ShowRecommendedBorder.DataContext = CurrentTreatment.RecommendedTraining;
            }
            else
            {
                ShowRecommendedBorder.Visibility = Visibility.Collapsed;
            }
           
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


        private void ShowRecommendedBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var selectedTraining = sender as Border;
            var training = (Training)selectedTraining.DataContext;

            trainingSelectedEvent(training);
        }
    }
}
