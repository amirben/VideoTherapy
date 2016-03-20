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
        /// <summary>
        /// Current treatment of the user
        /// </summary>
        private Treatment _currentTreatment;

        /// <summary>
        /// Treatment selection constractor
        /// </summary>
        /// <param name="_currentTreatment">the current treatment of the user</param>
        public UC_TreatmentSelection(Treatment _currentTreatment)
        {
            InitializeComponent();

            SetCurrentTreatment(_currentTreatment);

            TreatmentSelection.DataContext = _currentTreatment;
            TherapistInfo.DataContext = _currentTreatment.TreatmentTherapist;

        }

        public void SetCurrentTreatment(Treatment newTreatment)
        {
            _currentTreatment = newTreatment;
        }
        
    }
}
