using SyriaNTMP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyriaNTMP.Dto
{
    public class DashboardFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? City { get; set; }
        public string? HotelName { get; set; }
        public PropertyRatingEnum? HotelStars { get; set; }
        public string? Nationality { get; set; }
        public ReservationPurpose? Purpose { get; set; }
        public int? CurrencyId { get; set; }
    }
}
