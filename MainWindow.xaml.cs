using MarineWorkshopApp.ViewModels;
using MarineWorkshopApp.Views;
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

namespace MarineWorkshopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
        //private void NavLaborers_Click(object sender, RoutedEventArgs e)
        //{
        //    MainContentFrame.Content = new Views.LaborersView();
        //}

        //private void NavInvoices_Click(object sender, RoutedEventArgs e)
        //{
        //    MainContentFrame.Content = new Views.InvoicesView();
        //}

        //private void NavSettings_Click(object sender, RoutedEventArgs e)
        //{
        //    MainContentFrame.Content = new Views.SettingsView();
        //}

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // العودة لشاشة الدخول
            Views.LoginWindow login = new Views.LoginWindow();
            login.Show();
            this.Close();
        }
    }
}