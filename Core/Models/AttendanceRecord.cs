using System;
using System.Collections.Generic;
using System.Text;

namespace MarineWorkshopApp.Core.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public int LaborerId { get; set; }
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; } // حضر أم غائب
        public double OvertimeHours { get; set; } // عدد ساعات السهرة
        public bool IsClosedInWeeklySettlement { get; set; } // هل تم تقفيل هذا اليوم في حساب أسبوعي؟

        public Laborer? Laborer { get; set; }
    }
}
