using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyriaNTMP.Dto
{
  public class DashboardDto
  {
    public SummaryDto Summary { get; set; }
    public PurposeDto PurposeStats { get; set; }
    public List<NationalityDto> NationalityStats { get; set; }
    public OccupancyDto OccupancyDto { get; set; }
    public List<WeeklyDto> WeeklyReservations { get; set; }
    public TodayStatsDto TodayStats { get; set; }

    public RevenueDto Revenue { get; set; }
  }
}
