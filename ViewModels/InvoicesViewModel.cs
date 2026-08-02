using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarineWorkshopApp.Core.Models;
using MarineWorkshopApp.Data;
using MarineWorkshopApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MarineWorkshopApp.ViewModels
{
    public partial class InvoicesViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<Client> _clients = new();
        [ObservableProperty] private Client? _selectedClient;
        [ObservableProperty] private CompanySettings _companySettings = new();

        [ObservableProperty] private string _itemName = string.Empty;
        [ObservableProperty] private string _dimensions = string.Empty;
        [ObservableProperty] private int _quantity = 1;
        [ObservableProperty] private decimal _unitPrice;

        [ObservableProperty] private ObservableCollection<InvoiceItem> _currentInvoiceItems = new();
        [ObservableProperty] private decimal _grandTotal;
        [ObservableProperty] private string _previewInvoiceNumber = "جديد";
        [ObservableProperty] private BitmapImage? _clientLogoPreview;

        public InvoicesViewModel()
        {
            RefreshData();
        }

        public void RefreshData()
        {
            using var db = new AppDbContext();
            Clients = new ObservableCollection<Client>(db.Clients.OrderBy(c => c.CompanyName).ToList());
            CompanySettings = db.Settings.FirstOrDefault() ?? new CompanySettings();
            if (Clients.Any() && SelectedClient == null)
                SelectedClient = Clients.First();
            UpdateClientLogo();
        }

        partial void OnSelectedClientChanged(Client? value) => UpdateClientLogo();

        private void UpdateClientLogo()
        {
            if (SelectedClient == null || string.IsNullOrEmpty(SelectedClient.LogoPath) || !File.Exists(SelectedClient.LogoPath))
            {
                ClientLogoPreview = null;
                return;
            }
            ClientLogoPreview = new BitmapImage();
            ClientLogoPreview.BeginInit();
            ClientLogoPreview.UriSource = new Uri(SelectedClient.LogoPath);
            ClientLogoPreview.CacheOption = BitmapCacheOption.OnLoad;
            ClientLogoPreview.EndInit();
        }

        [RelayCommand]
        public void AddItemToInvoice()
        {
            if (string.IsNullOrWhiteSpace(ItemName) || UnitPrice <= 0 || Quantity <= 0)
            {
                MessageBox.Show("يرجى إدخال اسم البند والعدد وسعر الوحدة", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CurrentInvoiceItems.Add(new InvoiceItem
            {
                ItemName = ItemName.Trim(),
                Dimensions = Dimensions.Trim(),
                Quantity = Quantity,
                UnitPrice = UnitPrice
            });
            RecalculateTotals();

            ItemName = string.Empty;
            Dimensions = string.Empty;
            Quantity = 1;
            UnitPrice = 0;
        }

        [RelayCommand]
        public void RemoveItem(InvoiceItem item)
        {
            CurrentInvoiceItems.Remove(item);
            RecalculateTotals();
        }

        [RelayCommand]
        public void ClearInvoice()
        {
            CurrentInvoiceItems.Clear();
            GrandTotal = 0;
            PreviewInvoiceNumber = "جديد";
        }

        private void RecalculateTotals()
        {
            GrandTotal = CurrentInvoiceItems.Sum(i => i.TotalPrice);
        }

        [RelayCommand]
        public void SaveAndPrintPdf()
        {
            if (SelectedClient == null)
            {
                MessageBox.Show("يرجى اختيار العميل", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!CurrentInvoiceItems.Any())
            {
                MessageBox.Show("يرجى إضافة بنود للبيان", "تنبieh", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var invoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(100, 999)}";
            PreviewInvoiceNumber = invoiceNumber;

            using var db = new AppDbContext();
            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                Date = DateTime.Now,
                ClientId = SelectedClient.Id,
                TotalAmount = GrandTotal,
                TaxRate = 0,
                GrandTotal = GrandTotal
            };
            db.Invoices.Add(invoice);
            db.SaveChanges();

            foreach (var item in CurrentInvoiceItems)
            {
                item.InvoiceId = invoice.Id;
                db.InvoiceItems.Add(item);
            }
            db.SaveChanges();

            var settings = db.Settings.FirstOrDefault() ?? CompanySettings;
            var savedPath = InvoicePdfService.GenerateAndSave(invoice, SelectedClient, settings, CurrentInvoiceItems.ToList());

            var result = MessageBox.Show(
                $"تم حفظ بيان الأعمال رقم {invoiceNumber} بنجاح!\n\nهل تريد فتح ملف PDF؟",
                "نجاح العملية", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
                Process.Start(new ProcessStartInfo(savedPath) { UseShellExecute = true });

            ClearInvoice();
        }

        [RelayCommand]
        public void DownloadPdf()
        {
            if (SelectedClient == null || !CurrentInvoiceItems.Any())
            {
                MessageBox.Show("يرجى اختيار العميل وإضافة بنود أولاً", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"بيان_أعمال_{DateTime.Now:yyyyMMdd}.pdf"
            };
            if (dialog.ShowDialog() != true) return;

            var tempInvoice = new Invoice
            {
                InvoiceNumber = PreviewInvoiceNumber == "جديد" ? $"DRAFT-{DateTime.Now:yyyyMMdd}" : PreviewInvoiceNumber,
                Date = DateTime.Now,
                GrandTotal = GrandTotal,
                TotalAmount = GrandTotal
            };

            InvoicePdfService.GenerateAndSave(tempInvoice, SelectedClient, CompanySettings, CurrentInvoiceItems.ToList(), dialog.FileName);
            MessageBox.Show("تم تحميل PDF بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
