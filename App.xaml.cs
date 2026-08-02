using System.Windows;
using MarineWorkshopApp.Data;
using QuestPDF.Infrastructure;

namespace MarineWorkshopApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            base.OnStartup(e);

            using var db = new AppDbContext();
            db.Database.EnsureCreated();
        }
    }
}
