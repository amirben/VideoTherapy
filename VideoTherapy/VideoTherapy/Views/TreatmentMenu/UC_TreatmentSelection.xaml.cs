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
namespace VideoTherapy.Views.TreatmentMenu
{
    /// <summary>
    /// Interaction logic for UC_TreatmentSelection.xaml
    /// </summary>
    public partial class UC_TreatmentSelection : UserControl
    {
        private Treatment _currentTreatment;

        public UC_TreatmentSelection(Treatment _currentTreatment)
        {
            InitializeComponent();

            SetCurrentTreatment(_currentTreatment);
            TreatmentSelection.DataContext = _currentTreatment;
            TherapistInfo.DataContext = _currentTreatment.TreatmentTherapist;
            CalenderTimeline.DataContext = _currentTreatment;
        }

        public void SetCurrentTreatment(Treatment newTreatment)
        {
            _currentTreatment = newTreatment;
        }
        
    }
}
