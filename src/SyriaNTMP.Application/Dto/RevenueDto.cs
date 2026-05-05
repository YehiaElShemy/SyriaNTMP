using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyriaNTMP.Dto
{
    public class RevenueDto
    {
        public decimal PortfolioAdr { get; set; }
        public decimal TotalRevenue { get; set; }
        public string CurrencySymbolEn { get; set; }
        public string CurrencySymbolAr { get; set; }
        public int TotalNight { get; set; }
        public decimal AdrAvgPriceDay { get; set; }
        public PeakCityDto PeakCity { get; set; }
        public List<AdrByCityDto> MeanAdrByCity { get; set; }
    }

    public class PeakCityDto
    {
        public string City { get; set; }
        public decimal Adr { get; set; }
    }

    public class AdrByCityDto
    {
        public string City { get; set; }
        public decimal Adr { get; set; }
        public int TotalNight { get; set; }
    }

  
}
