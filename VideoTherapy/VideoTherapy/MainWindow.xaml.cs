using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;


namespace VideoTherapy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool authenticated = false;

        public MainWindow()
        {
            InitializeComponent();
            
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //LoginDialog ld = new LoginDialog();

            //ld.SetMainWindow(this);
            //DataContext = ld;
            //OpenningWindow.Children.Add(ld);

            OpenTreatmentWindow();

            //OpenTrainingWindow();


            //OpenExerciseWindow();
        }

        private void OpenExerciseWindow()
        {
            OpenningWindow.Children.Clear();
            OpenningWindow.Background = null;
            OpenningWindow.Children.Add(new ExerciseView());
        }

        private void OpenTrainingWindow()
        {
            OpenningWindow.Children.Clear();
            OpenningWindow.Background = null;
            OpenningWindow.Children.Add(new TrainingMenu());
        }

        public void OpenTreatmentWindow()
        {
            OpenningWindow.Children.Clear();
            OpenningWindow.Background = null;
            OpenningWindow.Children.Add(new TreatmentMenu());
        }
    }
}
