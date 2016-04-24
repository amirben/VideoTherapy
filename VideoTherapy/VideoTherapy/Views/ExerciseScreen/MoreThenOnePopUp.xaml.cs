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
    /// Interaction logic for MoreThenOnePopUp.xaml
    /// </summary>
    public partial class MoreThenOnePopUp : UserControl
    {
        public event ExerciseView.ClosePausePopupDelegate ClosePopup;

        public MoreThenOnePopUp()
        {
            InitializeComponent();
        }

        public void SetSize(int height, int width)
        {
            MoreThenOnePopUpStackpanel.Width = width;
            MoreThenOnePopUpStackpanel.Height = height;
        }

        private void ResumeTraining_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ClosePopup();
        }
    }
}
