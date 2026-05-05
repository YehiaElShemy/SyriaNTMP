using SyriaNTMP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace SyriaNTMP.Models
{
    public class Currency : Entity<int>
    {
        public int? CurrencyCustomizationId { get; set; }
        public int CurrencyId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Symbol { get; set; }
        public bool IsActive { get; set; }
        public string Color { get; set; }
    }
}
