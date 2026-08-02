using MarineWorkshopApp.ViewModels;

namespace MarineWorkshopApp.Views
{
    public partial class ClientsView : System.Windows.Controls.UserControl
    {
        public ClientsView()
        {
            InitializeComponent();
            DataContext = new ClientsViewModel();
        }
    }
}
