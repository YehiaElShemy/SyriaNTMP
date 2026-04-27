using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyriaNTMP.Dto
{
    public class PurposeDto
    {
        public int NumOfGuests { get; set; }
        public int TotalNight { get; set; }
        public string MostCommonPurpose { get; set; }
        public List<PurposeDetailsDto> PurposeDetailsDtos { get; set; }


    }
    public class PurposeDetailsDto
    {
        public string Purpose { get; set; }
        public int Count { get; set; }
        public int PurposeRate { get; set; }
    }
}
