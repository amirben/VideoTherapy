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

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for TreatmentMenu.xaml
    /// </summary>
    public partial class TreatmentMenu : UserControl
    {
        
        public TreatmentMenu()
        {
            InitializeComponent();

            this.Loaded += TreatmentMenu_Loaded;
        }

        private void TreatmentMenu_Loaded(object sender, RoutedEventArgs e)
        {
            UC_TreatmentSelection treatmentSelection = new UC_TreatmentSelection();
            UC_TrainingProgramSelection trainingSeleciton = new UC_TrainingProgramSelection();
            UC_UserInfo userInfo = new UC_UserInfo();
            
            TreatmentMenuGrid.Children.Add(treatmentSelection);
            TreatmentMenuGrid.Children.Add(trainingSeleciton);
            TreatmentMenuGrid.Children.Add(userInfo);

            Grid.SetColumn(treatmentSelection, 0);
            Grid.SetColumn(trainingSeleciton, 1);
            Grid.SetColumn(userInfo, 2);
        }
    }
}
