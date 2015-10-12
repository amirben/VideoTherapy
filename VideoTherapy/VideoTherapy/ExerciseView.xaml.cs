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

namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for ExerciseView.xaml
    /// </summary>
    public partial class ExerciseView : UserControl
    {
        public Exercise CurrentExercise { set; get; }

        public List<Exercise> ExerciseList { set; get; }

        public ExerciseView()
        {
            InitializeComponent();
        }
    }
}
