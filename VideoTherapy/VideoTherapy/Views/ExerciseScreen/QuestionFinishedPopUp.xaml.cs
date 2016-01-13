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

namespace VideoTherapy.Views.ExerciseScreen
{
    /// <summary>
    /// Interaction logic for QuestionFinishedPopUp.xaml
    /// </summary>
    public partial class QuestionFinishedPopUp : UserControl
    {
        public ExerciseView ExerciseView;

        public QuestionFinishedPopUp()
        {
            InitializeComponent();
        }

        public void SetSize(int height, int width)
        {
            QuestionFinishedPopUpStackpanel.Width = width;
            QuestionFinishedPopUpStackpanel.Height = height;
        }

        private void GoToTreatment_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ExerciseView.CloseQuestionnaireFinishedPopUp();
        }
    }
}
