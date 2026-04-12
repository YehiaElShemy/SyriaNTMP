using SyriaNTMP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SyriaNTMP.Dto
{
    public class ReservationsDto: EntityDto<long>
    {
        public long ReservationId { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string PropertyId { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public PropertyRatingEnum? PropertyRating { get; set; }
        public string ReservationNumber { get; set; } = string.Empty;
        public ReservationStatus ReservationStatus { get; set; }
        public int NumberOfGuests { get; set; }
        public int NumberOfNights { get; set; }
        public ReservationPurpose ReservationPurpose { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string GuestNationality { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
