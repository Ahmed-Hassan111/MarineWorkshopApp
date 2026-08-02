using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MarineWorkshopApp.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty] private decimal _totalWeeklyWages = 45280;
        [ObservableProperty] private decimal _totalAdvances = 8450;
        [ObservableProperty] private decimal _netRemainingPayable = 36830;
        [ObservableProperty] private int _activeWorkersCount = 86;

        public ObservableCollection<RecentActivityModel> RecentActivities { get; set; }
        public ObservableCollection<WorkerSummaryModel> WeeklySummaryList { get; set; }

        public DashboardViewModel()
        {
            LoadDummyData();
        }

        private void LoadDummyData()
        {
            RecentActivities = new ObservableCollection<RecentActivityModel>
            {
                new RecentActivityModel { Icon = "🧾", Title = "إصدار فاتورة #8842", Subtitle = "للعميل: شركة المقاولات العربية", TimeAgo = "منذ 2 س" },
                new RecentActivityModel { Icon = "👷", Title = "تسجيل عامل جديد", Subtitle = "محمد أحمد - فئة أولى", TimeAgo = "منذ 4 س" },
                new RecentActivityModel { Icon = "💵", Title = "صرف سلفة نقدية", Subtitle = "أحمد حسن - مبلغ 500 ج.م", TimeAgo = "منذ 6 س" },
                new RecentActivityModel { Icon = "📝", Title = "تسجيل غياب جماعي", Subtitle = "موقع ورشة البحرية - 4 عمال", TimeAgo = "أمس" }
            };

            WeeklySummaryList = new ObservableCollection<WorkerSummaryModel>
            {
                new WorkerSummaryModel { Code = "101", Name = "أحمد حسن", Group = "حداد", DaysPresent = 6, Advances = 500, NetAmount = 2500 },
                new WorkerSummaryModel { Code = "102", Name = "محمد علي", Group = "لحام", DaysPresent = 5.5, Advances = 200, NetAmount = 2300 },
                new WorkerSummaryModel { Code = "103", Name = "محمود السيد", Group = "ميكانيكي", DaysPresent = 6, Advances = 0, NetAmount = 3000 },
            };
        }
    }

    public class RecentActivityModel
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string TimeAgo { get; set; }
    }

    public class WorkerSummaryModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public double DaysPresent { get; set; }
        public decimal Advances { get; set; }
        public decimal NetAmount { get; set; }
    }
}