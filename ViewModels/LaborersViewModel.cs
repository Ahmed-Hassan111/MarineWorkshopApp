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
        public int AttendanceRecordId { get; set; }  // Id السجل في قاعدة البيانات
        public int LaborerId { get; set; }
        public DateTime Date { get; set; }
        public string LaborerName { get; set; } = string.Empty;
        public string Days { get; set; } = string.Empty;
        public double Overtime { get; set; }
        public decimal DayTotal { get; set; }
    }

    public class AdvanceDisplay
    {
        public int AdvanceRecordId { get; set; }     // Id السجل في قاعدة البيانات
        public int LaborerId { get; set; }
        public DateTime Date { get; set; }
        public string LaborerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Notes { get; set; } = string.Empty;
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

        // حقول تعديل اليومية
        [ObservableProperty] private int _editingWorkLogId;  // 0 = إضافة جديدة

        [ObservableProperty] private decimal _advanceAmount;
        [ObservableProperty] private string _advanceNotes = string.Empty;

        // حقول تعديل السلفة
        [ObservableProperty] private int _editingAdvanceId;  // 0 = إضافة جديدة

        [ObservableProperty] private ObservableCollection<WorkLogDisplay> _workLogs = new();
        [ObservableProperty] private ObservableCollection<AdvanceDisplay> _advances = new();
        [ObservableProperty] private ObservableCollection<WeeklySettlementDisplay> _weeklySettlements = new();

        // ======= إجماليات السلف =======
        [ObservableProperty] private string _totalAllAdvancesText = string.Empty;
        [ObservableProperty] private Laborer? _selectedAdvanceSummaryLaborer;
        [ObservableProperty] private string _laborerAdvancesTotalText = string.Empty;

        public LaborersViewModel()
        {
            LoadLaborers();
            LoadWorkLogs();
            LoadAdvances();
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
                AttendanceRecordId = r.Id,
                LaborerId = r.LaborerId,
                Date = r.Date,
                LaborerName = r.Laborer?.Name ?? "",
                Days = r.IsPresent ? "1 يوم" : "غائب",
                Overtime = r.OvertimeHours,
                DayTotal = (r.IsPresent ? (r.Laborer?.DailyRate ?? 0) : 0)
                           + (decimal)r.OvertimeHours * (r.Laborer?.HourlyOvertimeRate ?? 0)
            }));
        }

        private void LoadAdvances()
        {
            var (weekStart, weekEnd) = GetCurrentWeekRange();
            using var db = new AppDbContext();
            var records = db.AdvanceRecords
                .Include(a => a.Laborer)
                .Where(a => !a.IsDeducted && a.Date >= weekStart && a.Date <= weekEnd)
                .OrderByDescending(a => a.Date)
                .ToList();

            Advances = new ObservableCollection<AdvanceDisplay>(records.Select(r => new AdvanceDisplay
            {
                AdvanceRecordId = r.Id,
                LaborerId = r.LaborerId,
                Date = r.Date,
                LaborerName = r.Laborer?.Name ?? "",
                Amount = r.Amount,
                Notes = r.Notes
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
        private void SelectLaborerForEdit(Laborer? laborer)
        {
            if (laborer == null) return;
            SelectedLaborer = laborer;
            NewName = laborer.Name;
            NewGroup = laborer.GroupName;
            NewDailyRate = laborer.DailyRate;
            NewHourlyOvertimeRate = laborer.HourlyOvertimeRate;
        }

        [RelayCommand]
        private void DeleteLaborerByItem(Laborer? laborer)
        {
            if (laborer == null) return;

            if (MessageBox.Show($"هل تريد حذف العامل {laborer.Name}؟ سيتم حذف جميع سجلاته نهائيا.",
                "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using var db = new AppDbContext();
            var entity = db.Laborers
                .Include(l => l.AttendanceRecords)
                .Include(l => l.AdvanceRecords)
                .FirstOrDefault(l => l.Id == laborer.Id);
            if (entity == null) return;

            db.AttendanceRecords.RemoveRange(entity.AttendanceRecords);
            db.AdvanceRecords.RemoveRange(entity.AdvanceRecords);
            db.Laborers.Remove(entity);
            db.SaveChanges();

            ClearForm();
            LoadLaborers();
            LoadWorkLogs();
            CalculateWeeklySettlements();
            MessageBox.Show($"تم حذف العامل {laborer.Name} بنجاح", "تم الحذف", MessageBoxButton.OK, MessageBoxImage.Information);
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

            if (EditingWorkLogId > 0)
            {
                // تعديل سجل موجود
                var record = db.AttendanceRecords.Find(EditingWorkLogId);
                if (record != null)
                {
                    record.LaborerId = WorkLogSelectedLaborer.Id;
                    record.Date = LogDate.Value.Date;
                    record.IsPresent = WorkDaysCount > 0;
                    record.OvertimeHours = OvertimeHours;
                    db.SaveChanges();
                    MessageBox.Show("تم تعديل اليومية بنجاح", "تعديل", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                EditingWorkLogId = 0;
            }
            else
            {
                // إضافة جديدة
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
                MessageBox.Show("تم تسجيل اليومية بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            WorkLogSelectedLaborer = null;
            WorkDaysCount = 1;
            OvertimeHours = 0;
            LogDate = DateTime.Today;
            LoadWorkLogs();
            CalculateWeeklySettlements();
        }

        [RelayCommand]
        private void EditWorkLog(WorkLogDisplay? log)
        {
            if (log == null) return;
            // تحميل بيانات السجل في حقول الإدخال
            EditingWorkLogId = log.AttendanceRecordId;
            WorkLogSelectedLaborer = Laborers.FirstOrDefault(l => l.Id == log.LaborerId);
            LogDate = log.Date;
            WorkDaysCount = log.Days.Contains("1") ? 1 : (log.Days.Contains("0.5") ? 0.5 : 0);
            OvertimeHours = log.Overtime;
        }

        [RelayCommand]
        private void DeleteWorkLog(WorkLogDisplay? log)
        {
            if (log == null) return;

            if (MessageBox.Show(
                $"هل تريد حذف يومية {log.LaborerName} بتاريخ {log.Date:yyyy-MM-dd}؟",
                "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using var db = new AppDbContext();
            var record = db.AttendanceRecords.Find(log.AttendanceRecordId);
            if (record == null) return;

            db.AttendanceRecords.Remove(record);
            db.SaveChanges();

            LoadWorkLogs();
            CalculateWeeklySettlements();
            MessageBox.Show("تم حذف اليومية بنجاح", "تم الحذف", MessageBoxButton.OK, MessageBoxImage.Information);
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

            if (EditingAdvanceId > 0)
            {
                // تعديل سلفة موجودة
                var record = db.AdvanceRecords.Find(EditingAdvanceId);
                if (record != null)
                {
                    record.LaborerId = AdvanceSelectedLaborer.Id;
                    record.Amount = AdvanceAmount;
                    record.Notes = AdvanceNotes.Trim();
                    db.SaveChanges();
                    MessageBox.Show("تم تعديل السلفة بنجاح", "تعديل", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                EditingAdvanceId = 0;
            }
            else
            {
                db.AdvanceRecords.Add(new AdvanceRecord
                {
                    LaborerId = AdvanceSelectedLaborer.Id,
                    Date = DateTime.Today,
                    Amount = AdvanceAmount,
                    Notes = AdvanceNotes.Trim()
                });
                db.SaveChanges();
                MessageBox.Show("تم تسجيل السلفة بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            AdvanceSelectedLaborer = null;
            AdvanceAmount = 0;
            AdvanceNotes = string.Empty;
            LoadAdvances();
            CalculateWeeklySettlements();
        }

        [RelayCommand]
        private void EditAdvance(AdvanceDisplay? adv)
        {
            if (adv == null) return;
            // تحميل بيانات السلفة في حقول الإدخال
            EditingAdvanceId = adv.AdvanceRecordId;
            AdvanceSelectedLaborer = Laborers.FirstOrDefault(l => l.Id == adv.LaborerId);
            AdvanceAmount = adv.Amount;
            AdvanceNotes = adv.Notes;
        }

        [RelayCommand]
        private void DeleteAdvance(AdvanceDisplay? adv)
        {
            if (adv == null) return;

            if (MessageBox.Show(
                $"هل تريد حذف سلفة {adv.LaborerName} بمبلغ {adv.Amount:N0} ج.م؟",
                "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using var db = new AppDbContext();
            var record = db.AdvanceRecords.Find(adv.AdvanceRecordId);
            if (record == null) return;

            db.AdvanceRecords.Remove(record);
            db.SaveChanges();

            LoadAdvances();
            CalculateWeeklySettlements();
            MessageBox.Show("تم حذف السلفة بنجاح", "تم الحذف", MessageBoxButton.OK, MessageBoxImage.Information);
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

        // ======= إجمالي كل السلف لجميع العمال (كل التاريخ) =======
        [RelayCommand]
        private void ShowTotalAllAdvances()
        {
            using var db = new AppDbContext();
            var total = db.AdvanceRecords.Sum(a => (decimal?)a.Amount) ?? 0m;
            var count = db.AdvanceRecords.Count();
            TotalAllAdvancesText = $"إجمالي السلف (كل العمال): {total:N0} ج.م  |عدد السلف: {count}";
        }

        // ======= إجمالي سلف عامل مختار =======
        [RelayCommand]
        private void ShowLaborerTotalAdvances()
        {
            if (SelectedAdvanceSummaryLaborer == null)
            {
                MessageBox.Show("يرجى اختيار عامل أولاً", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new AppDbContext();
            var id = SelectedAdvanceSummaryLaborer.Id;
            var total = db.AdvanceRecords
                .Where(a => a.LaborerId == id)
                .Sum(a => (decimal?)a.Amount) ?? 0m;
            var count = db.AdvanceRecords.Count(a => a.LaborerId == id);
            var pending = db.AdvanceRecords
                .Where(a => a.LaborerId == id && !a.IsDeducted)
                .Sum(a => (decimal?)a.Amount) ?? 0m;

            LaborerAdvancesTotalText =
                $"إجمالي سلف {SelectedAdvanceSummaryLaborer.Name}: {total:N0} ج.م" +
                $"  |  عدد: {count}" +
                $"  |  غير مخصوم بعد: {pending:N0} ج.م";
        }
    }
}
