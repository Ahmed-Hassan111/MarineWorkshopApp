using System;
using System.Collections.Generic;
using System.Text;

namespace MarineWorkshopApp.Core.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty; // اسم شركة العميل
        public string OwnerName { get; set; } = string.Empty; // اسم المالك
        public string Phone { get; set; } = string.Empty;
        public string LogoPath { get; set; } = string.Empty; // مسار لوجو شركة العميل

        public List<Invoice> Invoices { get; set; } = new();
    }
}
