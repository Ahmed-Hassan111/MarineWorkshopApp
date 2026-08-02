using System;
using System.Collections.Generic;
using System.Text;

namespace MarineWorkshopApp.Core.Models
{
    public class AdvanceRecord
    {
        public int Id { get; set; }
        public int LaborerId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; } // مبلغ السلفة (400، 500، 1100...)
        public string Notes { get; set; } = string.Empty;
        public bool IsDeducted { get; set; } // هل تم خصمها في التقفيل الأسبوعي؟

        public Laborer? Laborer { get; set; }
    }
}
