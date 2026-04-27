using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyriaNTMP.Dto
{
  public class OperationDto
  {
    public int TotalReservations { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal CancellationRate { get; set; }
    public int ActiveProperties { get; set; }
  }
}
