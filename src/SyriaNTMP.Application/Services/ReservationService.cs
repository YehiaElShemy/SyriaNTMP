using AutoMapper;
using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using SyriaNTMP.Dto;
using SyriaNTMP.Models;
using SyriaNTMP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SyriaNTMP.Services
{
    [Authorize]
    public class ReservationService : ApplicationService
    {
        private readonly IRepository<Reservations> _reservationsRepository;

        public ReservationService(IRepository<Reservations> reservationsRepository)
        {
            _reservationsRepository = reservationsRepository;
        }

        public async Task<PagedResultDto<ReservationsDto>> GetReservations(ReservationsSearchCriteria searchCriteria)
        {
            var queryable = await _reservationsRepository.GetQueryableAsync();

            if (!string.IsNullOrEmpty(searchCriteria.CompanyName))
                queryable = queryable.Where(r => r.CompanyName.Contains(searchCriteria.CompanyName));

            if (!string.IsNullOrEmpty(searchCriteria.PropertyName))
                queryable = queryable.Where(r => r.PropertyName.Contains(searchCriteria.PropertyName));

            if (searchCriteria.PropertyRating.HasValue)
                queryable = queryable.Where(r => r.PropertyRating == searchCriteria.PropertyRating);

            if (!string.IsNullOrEmpty(searchCriteria.ReservationNumber))
                queryable = queryable.Where(r => r.ReservationNumber == searchCriteria.ReservationNumber);

            if (searchCriteria.ReservationStatus.HasValue)
                queryable = queryable.Where(r => r.ReservationStatus == searchCriteria.ReservationStatus);

            if (searchCriteria.ReservationPurpose.HasValue)
                queryable = queryable.Where(r => r.ReservationPurpose == searchCriteria.ReservationPurpose);

            if (!string.IsNullOrEmpty(searchCriteria.DateFrom) && DateTime.TryParse(searchCriteria.DateFrom, out var dateFrom))
                queryable = queryable.Where(x => dateFrom.Date <= x.CreatedDate.Date);

            if (!string.IsNullOrEmpty(searchCriteria.DateTo) && DateTime.TryParse(searchCriteria.DateTo, out var dateTo))
                queryable = queryable.Where(x => dateTo.Date >= x.CreatedDate.Date);

            // Apply sorting and paging
            var sortedQueryable = queryable.OrderBy(x => x.Id);  // or searchCriteria.Sorting
            var data = sortedQueryable
                .Skip(searchCriteria.SkipCount)
                .Take(searchCriteria.MaxResultCount).ToList();


            var count = await AsyncExecuter.CountAsync(queryable);  // Count before paging

            var result = new PagedResultDto<ReservationsDto>
            {
                Items = ObjectMapper.Map<List<Reservations>, List<ReservationsDto>>(data),
                TotalCount = count
            };

            return result;
        }
        public async Task<DashbordResponseDto> GetReservationsDashbord()
        {
            var todayDateStart = DateTime.Today;
            var todayDateEnd = todayDateStart.AddDays(1).AddTicks(-1);
            var weekDateStart = todayDateStart.AddDays(-7);
            // Query reservations from last week: CheckedIn (DateFrom), CheckedOut/Canceled (DateTo)
            var weekReservations = await _reservationsRepository.GetListAsync(r =>
                    (r.ReservationStatus == ReservationStatus.CheckedIn && r.FromDate >= weekDateStart && r.FromDate <= todayDateEnd) ||
                    (r.ReservationStatus == ReservationStatus.CheckedOut && r.ToDate >= weekDateStart && r.ToDate <= todayDateEnd) ||
                    (r.ReservationStatus == ReservationStatus.Canceled && r.ToDate >= weekDateStart && r.ToDate <= todayDateEnd)

            );

            // PIE: Today counts
            var todayReservationStatusCount = new TodayReservationStatusCountDto
            {
                CheckInStatusCount = weekReservations.Count(x =>
                    x.ReservationStatus == ReservationStatus.CheckedIn && x.FromDate.Date == todayDateStart),
                CheckOutStatusCount = weekReservations.Count(x =>
                    x.ReservationStatus == ReservationStatus.CheckedOut && x.ToDate.Date == todayDateStart),
                CanceledStatusCount = weekReservations.Count(x =>
                    x.ReservationStatus == ReservationStatus.Canceled && x.ToDate.Date == todayDateStart)
            };

            // BAR: Weekly group by FromDate
            var weeklyCounts = weekReservations.GroupBy(r => r.FromDate.Date)
                .Select(g => new WeeklyReservationCount
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    DayName = g.Key.ToString("dddd"),
                    Count = g.Count()
                })
                .OrderBy(w => w.Date)
                .ToList();

            return new DashbordResponseDto
            {
                TodayReservationStatusCount = todayReservationStatusCount,
                WeeklyReservationCounts = weeklyCounts,
                TotalCountWeekly = weekReservations.Count
            };
        }
        public async Task<ReservationsDto> GetById(int id)
        {
            var data = await _reservationsRepository.GetAsync(x => x.Id == id);
            return ObjectMapper.Map<Reservations, ReservationsDto>(data);
        }

        public async Task<ReservationsDto> CreateAsync(ReservationsDto dto)
        {
            var entity = ObjectMapper.Map<ReservationsDto, Reservations>(dto);
            await _reservationsRepository.InsertAsync(entity);
            return ObjectMapper.Map<Reservations, ReservationsDto>(entity);
        }

        public async Task<ReservationsDto> UpdateAsync(ReservationsDto dto)
        {
            var entity = await _reservationsRepository.GetAsync(x => x.Id == dto.Id);
            entity = ObjectMapper.Map(dto, entity);
            await _reservationsRepository.UpdateAsync(entity);
            return ObjectMapper.Map<Reservations, ReservationsDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _reservationsRepository.DeleteDirectAsync(x => x.Id == id);
            return true;
        }
    }
}

