using SyriaNTMP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SyriaNTMP.Dto
{
    public class ReservationsSearchCriteria : PagedAndSortedResultRequestDto
    {
        //public string? CompanyName { get; set; }
        public string? guestNationality { get; set; }
        public string? PropertyName { get; set; }
        public PropertyRatingEnum? PropertyRating { get; set; }
        public string? ReservationNumber { get; set; }
        public ReservationStatus? ReservationStatus { get; set; }
        public ReservationPurpose? ReservationPurpose { get; set; }
        public string? DateFrom { get; set; }
        public string? DateTo { get; set; }
    }
}
