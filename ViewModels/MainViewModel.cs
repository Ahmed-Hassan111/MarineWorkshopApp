using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarineWorkshopApp.Views;

namespace MarineWorkshopApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private object _currentView;

        [ObservableProperty]
        private string _currentPageTitle = "إدارة العمالة واليوميات";

        private readonly LaborersView _laborersView = new();
        private readonly ClientsView _clientsView = new();
        private readonly InvoicesView _invoicesView = new();
        private readonly SettingsView _settingsView = new();

        public MainViewModel()
        {
            CurrentView = _laborersView;
        }

        [RelayCommand]
        private void Navigate(string destination)
        {
            switch (destination)
            {
                case "Laborers":
                    CurrentView = _laborersView;
                    CurrentPageTitle = "إدارة العمالة واليوميات";
                    break;
                case "Clients":
                    CurrentView = _clientsView;
                    CurrentPageTitle = "حسابات العملاء وبيانات الأعمال";
                    break;
                case "Invoices":
                    if (_invoicesView.DataContext is InvoicesViewModel invoicesVm)
                        invoicesVm.RefreshData();
                    CurrentView = _invoicesView;
                    CurrentPageTitle = "الفواتير وبيان الأعمال";
                    break;
                case "Settings":
                    CurrentView = _settingsView;
                    CurrentPageTitle = "الإعدادات";
                    break;
            }
        }
    }
}
