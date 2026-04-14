using SyriaNTMP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace SyriaNTMP.Models
{
    public class Reservations:Entity<long>
    {
        public long ReservationId { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string PropertyId { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string City { get; set; }
        public PropertyRatingEnum? PropertyRating { get; set; }
        public string ReservationNumber { get; set; } = string.Empty;
        public ReservationStatus ReservationStatus { get; set; }
        public int NumberOfGuests { get; set; }
        public int NumberOfNights { get; set; }
        public int NumberOfRooms { get; set; }
        public ReservationPurpose ReservationPurpose { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string GuestNationality { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; }
      }
}
