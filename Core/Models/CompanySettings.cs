using System;
using System.Collections.Generic;
using System.Text;

namespace MarineWorkshopApp.Core.Models
{
    public class CompanySettings
    {
        public int Id { get; set; }
        public string WorkshopName { get; set; } = "شركة أعالي البحار للخدمات البحرية";
        public string Subtitle { get; set; } = "إدارة صيانة وتصليح السفن";
        public string LogoPath { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = "ج.م";
    }
}
