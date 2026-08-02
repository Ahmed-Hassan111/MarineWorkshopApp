using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarineWorkshopApp.Core.Models;
using MarineWorkshopApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace MarineWorkshopApp.ViewModels
{
    public class WorkLogDisplay
    {
        public DateTime Date { get; set; }
        public string LaborerName { get; set; } = string.Empty;
        public string Days { get; set; } = string.Empty;
        public double Overtime { get; set; }
        public decimal DayTotal { get; set; }
    }

    public class WeeklySettlementDisplay
    {
        public int LaborerId { get; set; }
        public string LaborerName { get; set; } = string.Empty;
        public int TotalDays { get; set; }
        public double TotalOvertimeHours { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal TotalAdvances { get; set; }
        public decimal NetPayable { get; set; }
    }

    public partial class LaborersViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<Laborer> _laborers = new();
        [ObservableProperty] private Laborer? _selectedLaborer;
        [ObservableProperty] private Laborer? _workLogSelectedLaborer;
        [ObservableProperty] private Laborer? _advanceSelectedLaborer;

        [ObservableProperty] private string _newName = string.Empty;
        [ObservableProperty] private string _newGroup = string.Empty;
        [ObservableProperty] private decimal _newDailyRate;
        [ObservableProperty] private decimal _newHourlyOvertimeRate;
        [ObservableProperty] private string _searchText = string.Empty;

        [ObservableProperty] private DateTime? _logDate = DateTime.Today;
        [ObservableProperty] private double _workDaysCount = 1;
        [ObservableProperty] private double _overtimeHours;

        [ObservableProperty] private decimal _advanceAmount;
        [ObservableProperty] private string _advanceNotes = string.Empty;

        [ObservableProperty] private ObservableCollection<WorkLogDisplay> _workLogs = new();
        [ObservableProperty] private ObservableCollection<WeeklySettlementDisplay> _weeklySettlements = new();

        public LaborersViewModel()
        {
            LoadLaborers();
            LoadWorkLogs();
            CalculateWeeklySettlements();
        }

        partial void OnSearchTextChanged(string value) => LoadLaborers();

        partial void OnSelectedLaborerChanged(Laborer? value)
        {
            if (value == null) return;
            NewName = value.Name;
            NewGroup = value.GroupName;
            NewDailyRate = value.DailyRate;
            NewHourlyOvertimeRate = value.HourlyOvertimeRate;
        }

        private (DateTime Start, DateTime End) GetCurrentWeekRange()
        {
            var today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Saturday)) % 7;
            var start = today.AddDays(-diff);
            return (start, start.AddDays(6));
        }

        private void LoadLaborers()
        {
            using var db = new AppDbContext();
            var query = db.Laborers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(l => l.Name.Contains(SearchText) || l.Code.Contains(SearchText));

            Laborers = new ObservableCollection<Laborer>(query.OrderBy(l => l.Name).ToList());
        }

        private void LoadWorkLogs()
        {
            var (weekStart, weekEnd) = GetCurrentWeekRange();
            using var db = new AppDbContext();
            var records = db.AttendanceRecords
                .Include(a => a.Laborer)
                .Where(a => !a.IsClosedInWeeklySettlement && a.Date >= weekStart && a.Date <= weekEnd)
                .OrderByDescending(a => a.Date)
                .ToList();

            WorkLogs = new ObservableCollection<WorkLogDisplay>(records.Select(r => new WorkLogDisplay
            {
                Date = r.Date,
                LaborerName = r.Laborer?.Name ?? "",
                Days = r.IsPresent ? "1 يوم" : "غائب",
                Overtime = r.OvertimeHours,
                DayTotal = (r.IsPresent ? (r.Laborer?.DailyRate ?? 0) : 0)
                           + (decimal)r.OvertimeHours * (r.Laborer?.HourlyOvertimeRate ?? 0)
            }));
        }

        private void CalculateWeeklySettlements()
        {
            var (weekStart, weekEnd) = GetCurrentWeekRange();
            using var db = new AppDbContext();
            var laborers = db.Laborers
                .Include(l => l.AttendanceRecords)
                .Include(l => l.AdvanceRecords)
                .ToList();

            var settlements = new List<WeeklySettlementDisplay>();
            foreach (var laborer in laborers)
            {
                var weekAttendance = laborer.AttendanceRecords
                    .Where(a => !a.IsClosedInWeeklySettlement && a.Date >= weekStart && a.Date <= weekEnd && a.IsPresent)
                    .ToList();
                var weekAdvances = laborer.AdvanceRecords
                    .Where(a => !a.IsDeducted && a.Date >= weekStart && a.Date <= weekEnd)
                    .ToList();

                if (!weekAttendance.Any() && !weekAdvances.Any()) continue;

                int totalDays = weekAttendance.Count;
                double totalOvertime = weekAttendance.Sum(a => a.OvertimeHours);
                decimal gross = totalDays * laborer.DailyRate + (decimal)totalOvertime * laborer.HourlyOvertimeRate;
                decimal advances = weekAdvances.Sum(a => a.Amount);

                settlements.Add(new WeeklySettlementDisplay
                {
                    LaborerId = laborer.Id,
                    LaborerName = laborer.Name,
                    TotalDays = totalDays,
                    TotalOvertimeHours = totalOvertime,
                    GrossAmount = gross,
                    TotalAdvances = advances,
                    NetPayable = gross - advances
                });
            }

            WeeklySettlements = new ObservableCollection<WeeklySettlementDisplay>(settlements);
        }

        [RelayCommand]
        private void SaveLaborer()
        {
            if (string.IsNullOrWhiteSpace(NewName))
            {
                MessageBox.Show("يرجى إدخال اسم العامل", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new AppDbContext();

            if (SelectedLaborer != null)
            {
                var laborer = db.Laborers.Find(SelectedLaborer.Id);
                if (laborer == null) return;
                laborer.Name = NewName.Trim();
                laborer.GroupName = NewGroup.Trim();
                laborer.DailyRate = NewDailyRate;
                laborer.HourlyOvertimeRate = NewHourlyOvertimeRate;
            }
            else
            {
                int nextCode = db.Laborers.Count() + 1;
                db.Laborers.Add(new Laborer
                {
                    Code = $"{nextCode}#",
                    Name = NewName.Trim(),
                    GroupName = NewGroup.Trim(),
                    DailyRate = NewDailyRate,
                    HourlyOvertimeRate = NewHourlyOvertimeRate
                });
            }

            db.SaveChanges();
            ClearForm();
            LoadLaborers();
            MessageBox.Show("تم حفظ بيانات العامل بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void ClearForm()
        {
            SelectedLaborer = null;
            NewName = string.Empty;
            NewGroup = string.Empty;
            NewDailyRate = 0;
            NewHourlyOvertimeRate = 0;
        }

        [RelayCommand]
        private void DeleteLaborer()
        {
            if (SelectedLaborer == null)
            {
                MessageBox.Show("يرجى تحديد عامل للحذف", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"هل تريد حذف العامل {SelectedLaborer.Name}؟", "تأكيد الحذف",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            using var db = new AppDbContext();
            var laborer = db.Laborers
                .Include(l => l.AttendanceRecords)
                .Include(l => l.AdvanceRecords)
                .FirstOrDefault(l => l.Id == SelectedLaborer.Id);
            if (laborer == null) return;

            db.AttendanceRecords.RemoveRange(laborer.AttendanceRecords);
            db.AdvanceRecords.RemoveRange(laborer.AdvanceRecords);
            db.Laborers.Remove(laborer);
            db.SaveChanges();

            ClearForm();
            LoadLaborers();
            LoadWorkLogs();
            CalculateWeeklySettlements();
        }

        [RelayCommand]
        private void AddWorkLog()
        {
            if (WorkLogSelectedLaborer == null)
            {
                MessageBox.Show("يرجى اختيار العامل", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (LogDate == null)
            {
                MessageBox.Show("يرجى اختيار التاريخ", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new AppDbContext();
            var existing = db.AttendanceRecords.FirstOrDefault(a =>
                a.LaborerId == WorkLogSelectedLaborer.Id &&
                a.Date.Date == LogDate.Value.Date &&
                !a.IsClosedInWeeklySettlement);

            if (existing != null)
            {
                existing.IsPresent = WorkDaysCount > 0;
                existing.OvertimeHours = OvertimeHours;
            }
            else
            {
                db.AttendanceRecords.Add(new AttendanceRecord
                {
                    LaborerId = WorkLogSelectedLaborer.Id,
                    Date = LogDate.Value.Date,
                    IsPresent = WorkDaysCount > 0,
                    OvertimeHours = OvertimeHours
                });
            }

            db.SaveChanges();
            WorkDaysCount = 1;
            OvertimeHours = 0;
            LoadWorkLogs();
            CalculateWeeklySettlements();
            MessageBox.Show("تم تسجيل اليومية بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void AddAdvance()
        {
            if (AdvanceSelectedLaborer == null)
            {
                MessageBox.Show("يرجى اختيار العامل", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (AdvanceAmount <= 0)
            {
                MessageBox.Show("يرجى إدخال مبلغ السلفة", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new AppDbContext();
            db.AdvanceRecords.Add(new AdvanceRecord
            {
                LaborerId = AdvanceSelectedLaborer.Id,
                Date = DateTime.Today,
                Amount = AdvanceAmount,
                Notes = AdvanceNotes.Trim()
            });
            db.SaveChanges();

            AdvanceAmount = 0;
            AdvanceNotes = string.Empty;
            CalculateWeeklySettlements();
            MessageBox.Show("تم تسجيل السلفة بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void CloseWeeklySettlement()
        {
            if (!WeeklySettlements.Any())
            {
                MessageBox.Show("لا توجد حسابات للتقفيل هذا الأسبوع", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("هل تريد تقفيل الأسبوع الحالي؟ سيتم أرشفة جميع السجلات المفتوحة.", "تأكيد التقفيل",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var (weekStart, weekEnd) = GetCurrentWeekRange();
            using var db = new AppDbContext();

            var attendance = db.AttendanceRecords
                .Where(a => !a.IsClosedInWeeklySettlement && a.Date >= weekStart && a.Date <= weekEnd);
            foreach (var record in attendance)
                record.IsClosedInWeeklySettlement = true;

            var advances = db.AdvanceRecords
                .Where(a => !a.IsDeducted && a.Date >= weekStart && a.Date <= weekEnd);
            foreach (var advance in advances)
                advance.IsDeducted = true;

            db.SaveChanges();
            LoadWorkLogs();
            CalculateWeeklySettlements();
            MessageBox.Show("تم تقفيل الأسبوع بنجاح ✅", "تقفيل الأسبوع", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
