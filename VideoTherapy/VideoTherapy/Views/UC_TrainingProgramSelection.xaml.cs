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
using VideoTherapy_Objects;


using VideoTherapy.XML;


namespace VideoTherapy.Views
{
    /// <summary>
    /// Interaction logic for UC_TrainingProgramSelection.xaml
    /// </summary>
    public partial class UC_TrainingProgramSelection : UserControl
    {
        private List<Treatment> _treatmentList;

        public UC_TrainingProgramSelection()
        {
            InitializeComponent();

            this.Loaded += UC_TrainingProgramSelection_Loaded;
        }

        private void UC_TrainingProgramSelection_Loaded(object sender, RoutedEventArgs e)
        {
            _treatmentList = ObjectReader.GetAllTreatments();
            TreatmentTrainingList.DataContext = _treatmentList[0].TrainingList;
        }
    }
}
