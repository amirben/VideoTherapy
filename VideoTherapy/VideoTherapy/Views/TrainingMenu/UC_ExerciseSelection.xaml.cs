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

namespace VideoTherapy.Views.TrainingMenu
{
    /// <summary>
    /// Interaction logic for UC_ExerciseSelection.xaml
    /// </summary>
    public partial class UC_ExerciseSelection : UserControl
    {
        public UC_ExerciseSelection()
        {
            InitializeComponent();

            this.Loaded += UC_ExerciseSelection_Loaded;
        }

        private void UC_ExerciseSelection_Loaded(object sender, RoutedEventArgs e)
        {
            ExercisesListUI.DataContext = createDemoExercises();
        }

        private List<Exercise> createDemoExercises()
        {
            List<Exercise> list = new List<Exercise>();

            for (int i = 1; i < 8; i++)
            {
                Exercise e = new Exercise();
                e.ExerciseName = "Body Posture " + i;
                e.ExerciseNum = i;
                e.Repetitions = 5;
                e.ExerciseThumbs = "../../Images/video1.jpg";

                list.Add(e);
            }

            return list;
        }
    }
}
