using System;
using System.Collections.Generic;
using System.Text;

namespace MarineWorkshopApp.Core.Models
{
    public class Laborer
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // مثل: 2045#
        public string Name { get; set; } = string.Empty; // اسم العامل
        public string GroupName { get; set; } = string.Empty; // المجموعة/الصنعة (نجارة، حدادة، الخ)
        public decimal DailyRate { get; set; } // أجر اليومية
        public decimal HourlyOvertimeRate { get; set; } // أجر ساعة السهرة (الوقت الإضافي)

        // العلاقات
        public List<AttendanceRecord> AttendanceRecords { get; set; } = new();
        public List<AdvanceRecord> AdvanceRecords { get; set; } = new();
    }
}
