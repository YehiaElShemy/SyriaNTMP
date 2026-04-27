using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyriaNTMP.Dto
{
    public class OccupancyDto
    {
        public double AvgOccupancyRate { get; set; }
        public int TotalSoldNights { get; set; }
        public int TotalNightsNotSolds { get; set; }
        public List<CityOccupancyDto> CityOccupancyDto { get; set; }
    }
    public class CityOccupancyDto
    {
        public string City { get; set; }
        public decimal OccupancyRate { get; set; }
    }

}
