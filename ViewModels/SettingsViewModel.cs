using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarineWorkshopApp.Core.Models;
using MarineWorkshopApp.Data;
using Microsoft.Win32;

namespace MarineWorkshopApp.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private CompanySettings _currentSettings = new();

        public SettingsViewModel()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            using var db = new AppDbContext();
            var settings = db.Settings.FirstOrDefault();
            if (settings != null)
            {
                CurrentSettings = settings;
            }
        }

        [RelayCommand]
        public void BrowseLogo()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (dialog.ShowDialog() == true)
            {
                CurrentSettings.LogoPath = dialog.FileName;
                OnPropertyChanged(nameof(CurrentSettings));
            }
        }

        [RelayCommand]
        public void SaveSettings()
        {
            using var db = new AppDbContext();
            db.Settings.Update(CurrentSettings);
            db.SaveChanges();

            MessageBox.Show("تم حفظ إعدادات الشركة بنجاح!", "حفظ البيانات", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}