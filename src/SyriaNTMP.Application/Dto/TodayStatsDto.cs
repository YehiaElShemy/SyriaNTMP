using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyriaNTMP.Dto
{
  public class TodayStatsDto
  {
    public int CheckedIn { get; set; }
    public int CheckedOut { get; set; }
    public int Cancelled { get; set; }
  }
}
