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
            // todo check if user is set the check box to "Rememeber me"
            
            if (!authenticated)
            {
                //LoginDialog.ShowHandlerDialog();
                DataContext = new LoginDialog();

            }
            else
            {
                // todo load treatment window
            }

        }
    }
}
