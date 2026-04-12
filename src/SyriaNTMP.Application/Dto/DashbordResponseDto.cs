using SyriaNTMP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SyriaNTMP.Dto
{

    public class DashbordResponseDto
    {
        public TodayReservationStatusCountDto TodayReservationStatusCount { get; set; } = new();
        public List<WeeklyReservationCount> WeeklyReservationCounts { get; set; } = new();
        public int TotalCountWeekly { get; set; }
    }
    public class TodayReservationStatusCountDto
    {
        public long CheckInStatusCount { get; set; }
        public long CheckOutStatusCount { get; set; }
        public long CanceledStatusCount { get; set; }
    }
    public class WeeklyReservationCount
    {
        public string Date { get; set; } = string.Empty;
        public string DayName { get; set; } = string.Empty;
        public long Count { get; set; }
    }

}
