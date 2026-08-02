using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarineWorkshopApp.Core.Models;
using MarineWorkshopApp.Data;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MarineWorkshopApp.ViewModels
{
    public partial class ClientsViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<Client> _clients = new();
        [ObservableProperty] private Client? _selectedClient;

        [ObservableProperty] private string _ownerName = string.Empty;
        [ObservableProperty] private string _companyName = string.Empty;
        [ObservableProperty] private string _phone = string.Empty;
        [ObservableProperty] private string _logoPath = string.Empty;
        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private BitmapImage? _logoPreview;

        public ClientsViewModel()
        {
            LoadClients();
        }

        partial void OnSearchTextChanged(string value) => LoadClients();

        partial void OnSelectedClientChanged(Client? value)
        {
            if (value == null) return;
            OwnerName = value.OwnerName;
            CompanyName = value.CompanyName;
            Phone = value.Phone;
            LogoPath = value.LogoPath;
            LoadLogoPreview(LogoPath);
        }

        private void LoadClients()
        {
            using var db = new AppDbContext();
            var query = db.Clients.AsQueryable();
            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(c => c.CompanyName.Contains(SearchText) || c.OwnerName.Contains(SearchText));

            Clients = new ObservableCollection<Client>(query.OrderBy(c => c.CompanyName).ToList());
        }

        private void LoadLogoPreview(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                LogoPreview = null;
                return;
            }
            LogoPreview = new BitmapImage();
            LogoPreview.BeginInit();
            LogoPreview.UriSource = new Uri(path);
            LogoPreview.CacheOption = BitmapCacheOption.OnLoad;
            LogoPreview.EndInit();
        }

        [RelayCommand]
        private void SaveClient()
        {
            if (string.IsNullOrWhiteSpace(CompanyName) || string.IsNullOrWhiteSpace(OwnerName))
            {
                MessageBox.Show("يرجى إدخال اسم الشركة واسم المالك", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new AppDbContext();

            if (SelectedClient != null)
            {
                var client = db.Clients.Find(SelectedClient.Id);
                if (client == null) return;
                client.OwnerName = OwnerName.Trim();
                client.CompanyName = CompanyName.Trim();
                client.Phone = Phone.Trim();
                client.LogoPath = LogoPath;
            }
            else
            {
                db.Clients.Add(new Client
                {
                    OwnerName = OwnerName.Trim(),
                    CompanyName = CompanyName.Trim(),
                    Phone = Phone.Trim(),
                    LogoPath = LogoPath
                });
            }

            db.SaveChanges();
            ClearForm();
            LoadClients();
            MessageBox.Show("تم حفظ بيانات العميل بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void ClearForm()
        {
            SelectedClient = null;
            OwnerName = string.Empty;
            CompanyName = string.Empty;
            Phone = string.Empty;
            LogoPath = string.Empty;
            LogoPreview = null;
        }

        [RelayCommand]
        private void DeleteClient()
        {
            if (SelectedClient == null)
            {
                MessageBox.Show("يرجى تحديد عميل للحذف", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"هل تريد حذف العميل {SelectedClient.CompanyName}؟", "تأكيد الحذف",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            using var db = new AppDbContext();
            var client = db.Clients.Find(SelectedClient.Id);
            if (client == null) return;

            db.Clients.Remove(client);
            db.SaveChanges();

            ClearForm();
            LoadClients();
        }

        [RelayCommand]
        private void BrowseLogo()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };
            if (dialog.ShowDialog() == true)
            {
                LogoPath = dialog.FileName;
                LoadLogoPreview(LogoPath);
            }
        }
    }
}
