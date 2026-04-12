using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyriaNTMP.Mappers
{
    public class ReservationsMapper: AutoMapper.Profile
    {
        public ReservationsMapper()
        {
            CreateMap<SyriaNTMP.Models.Reservations, SyriaNTMP.Dto.ReservationsDto>().ReverseMap();
        }
    }
}
