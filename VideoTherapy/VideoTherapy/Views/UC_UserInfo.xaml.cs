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
namespace VideoTherapy.Views
{
    /// <summary>
    /// Interaction logic for UC_UserInfo.xaml
    /// </summary>
    public partial class UC_UserInfo : UserControl
    {
        private Patient _currentPatient;

        public UC_UserInfo(Patient _currentPatient)
        {
            InitializeComponent();
            this._currentPatient = _currentPatient;
            this.DataContext = this._currentPatient;
        }

        
    }
}
