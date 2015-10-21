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

using VideoTherapy.Views.TrainingMenu;
using VideoTherapy.Views;

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for TrainingMenu.xaml
    /// </summary>
    public partial class TrainingMenu : UserControl
    {
        public TrainingMenu()
        {
            InitializeComponent();

            this.Loaded += TrainingMenu_Loaded;
        }

        private void TrainingMenu_Loaded(object sender, RoutedEventArgs e)
        {
            UC_TrainingSelection trainingSelection = new UC_TrainingSelection();
            UC_ExerciseSelection exerciseSelection = new UC_ExerciseSelection();
            UC_UserInfo userInfo = new UC_UserInfo(null);


            TrainingMenuGrid.Children.Add(trainingSelection);
            TrainingMenuGrid.Children.Add(exerciseSelection);
            TrainingMenuGrid.Children.Add(userInfo);

            Grid.SetColumn(trainingSelection, 0);
            Grid.SetColumn(exerciseSelection, 1);
            Grid.SetColumn(userInfo, 2);
        }
    }
}
