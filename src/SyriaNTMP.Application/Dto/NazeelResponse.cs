using SyriaNTMP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SyriaNTMP.Dto
{
    public class CurrencyDTO : EntityDto<int>
    {
        public int? CurrencyCustomizationId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Symbol { get; set; }
        public bool IsActive { get; set; }
        public string Color { get; set; }
    }
}
