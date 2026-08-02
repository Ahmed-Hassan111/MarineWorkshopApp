using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MarineWorkshopApp.Core.Models;

namespace MarineWorkshopApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Laborer> Laborers { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<AdvanceRecord> AdvanceRecords { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<CompanySettings> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // حفظ قاعدة البيانات في مسار AppData الثابت حتى لا تضيع مع الـ Rebuild
            string folderPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MarineWorkshopApp");

            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            string dbPath = System.IO.Path.Combine(folderPath, "WorkshopData.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // إضافة بيانات إفتراضية لاسم الورشة "أعالي البحار"
            modelBuilder.Entity<CompanySettings>().HasData(new CompanySettings
            {
                Id = 1,
                WorkshopName = "أعالي البحار",
                Subtitle = "ورشة صيانة وتصليح السفن",
                CurrencySymbol = "ج.م"
            });
        }
    }
}
