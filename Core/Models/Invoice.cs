using System;
using System.Collections.Generic;
using System.Text;

namespace MarineWorkshopApp.Core.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty; // مثل: INV-2026-001
        public DateTime Date { get; set; } = DateTime.Now;
        public int ClientId { get; set; }

        public List<InvoiceItem> Items { get; set; } = new();

        public decimal TotalAmount { get; set; }
        public decimal TaxRate { get; set; } = 0.15m; // نسبة الضريبة إن وجدت
        public decimal GrandTotal { get; set; }

        public Client? Client { get; set; }
    }

    public class InvoiceItem
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public string ItemName { get; set; } = string.Empty; // تفاصيل الصنعة (مثلاً: تصنيع يافطة / صيانة محرك)
        public string Dimensions { get; set; } = string.Empty; // المقاسات (مثال: 200سم × 100سم)
        public int Quantity { get; set; } // العدد
        public decimal UnitPrice { get; set; } // سعر الوحدة
        public decimal TotalPrice => Quantity * UnitPrice; // الإجمالي الأوتوماتيكي للبند
    }
}
