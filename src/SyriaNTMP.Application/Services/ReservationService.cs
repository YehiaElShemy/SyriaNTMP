using Microsoft.AspNetCore.Authorization;
using SyriaNTMP.Dto;
using SyriaNTMP.Models;
using SyriaNTMP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
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


        public async Task<DashboardDto> GetDashboardAsync(DashboardFilterDto filter)
        {
          var query = await _reservationsRepository.GetQueryableAsync();

          if (filter.FromDate.HasValue)
          {
            query = query.Where(x => x.FromDate >= filter.FromDate.Value);
          }

          if (filter.ToDate.HasValue)
          {
            query = query.Where(x => x.ToDate <= filter.ToDate.Value);
          }

          if (!string.IsNullOrWhiteSpace(filter.City))
          {
            query = query.Where(x => x.City == filter.City);
          }

          if (!string.IsNullOrWhiteSpace(filter.HotelName))
          {
            query = query.Where(x => x.PropertyName.Contains(filter.HotelName));
          }

          if (filter.HotelStars.HasValue)
          {
            query = query.Where(x => x.PropertyRating == filter.HotelStars);
          }

          var rawData = await AsyncExecuter.ToListAsync(query);

          var startPeriod = filter.FromDate ?? DateTime.Today.AddDays(-7);
          var endPeriod = filter.ToDate ?? DateTime.Today;

          var activeData = rawData.Where(x =>
              x.ReservationStatus != ReservationStatus.Canceled &&
              x.ReservationStatus != ReservationStatus.NoShow).ToList();

          var weekly = new List<WeeklyDto>();
          for (var date = startPeriod.Date; date <= endPeriod.Date; date = date.AddDays(1))
          {
            var occupiedCount = activeData.Count(x => date >= x.FromDate.Date && date < x.ToDate.Date);
            weekly.Add(new WeeklyDto
            {
              Date = date.ToString("yyyy-MM-dd"),
              Count = occupiedCount
            });
          }

          var nationality = activeData
              .GroupBy(x => x.GuestNationality)
              .Select(g => new NationalityDto
              {
                Nationality = g.Key,
                Count = CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod)
              })
              .OrderByDescending(x => x.Count)
              .ToList();

          var today = DateTime.Today;
          var handledToday = rawData.Count(x => x.CreatedDate.Date == today || x.FromDate.Date == today);
          var todayStats = new TodayStatsDto
          {
            CheckedIn = handledToday == 0 ? 0 : (rawData.Count(x => x.ReservationStatus == ReservationStatus.CheckedIn && x.FromDate.Date == today) * 100 / handledToday),
            CheckedOut = handledToday == 0 ? 0 : (rawData.Count(x => x.ReservationStatus == ReservationStatus.CheckedOut && x.ToDate.Date == today) * 100 / handledToday),
            Cancelled = handledToday == 0 ? 0 : (rawData.Count(x => x.ReservationStatus == ReservationStatus.Canceled && x.CreatedDate.Date == today) * 100 / handledToday)
          };

 
          var totalSoldNights = CalculateTotalSoldNights(activeData, startPeriod, endPeriod);
          var totalRevenue = activeData.Sum(x => x.TotalPrice);
          var portfolioAdr = totalSoldNights == 0 ? 0 : (totalRevenue / totalSoldNights);

          return new DashboardDto
          {
            Summary = new SummaryDto
            {
              TotalReservations = rawData.Count,
              CancellationRate = rawData.Count == 0 ? 0 : (rawData.Count(x => x.ReservationStatus == ReservationStatus.Canceled) * 100 / rawData.Count),
              OccupancyRate = CalculateOccupancyPercentage(totalSoldNights, activeData, startPeriod, endPeriod)
            },
            PurposeStats = activeData.GroupBy(x => x.ReservationPurpose).Select(g => new PurposeDto { Purpose = g.Key.ToString(), Count = g.Count() }).ToList(),
            NationalityStats = nationality,
            WeeklyReservations = weekly,
            TodayStats = todayStats,
            Revenue = new RevenueDto { PortfolioAdr = portfolioAdr },
            OccupancyByCity = activeData.GroupBy(x => x.City).Select(g => new CityOccupancyDto { City = g.Key, OccupancyRate = CalculateOccupancyPercentage(CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod), g.ToList(), startPeriod, endPeriod) }).ToList()
          };
        }

    
        public async Task<ReservationsDto> GetById(int id)
        {
            var data = await _reservationsRepository.GetAsync(x => x.Id == id);
            return ObjectMapper.Map<Reservations, ReservationsDto>(data);
        }

        public async Task<ReservationResponseDto> CreateAsync(ReservationsDto dto)
        {
            var entity = ObjectMapper.Map<ReservationsDto, Reservations>(dto);
            // check if item already exists
            var existingItem = await _reservationsRepository.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (existingItem != null)
            {
                await _reservationsRepository.UpdateAsync(entity);
            }
            await _reservationsRepository.InsertAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();
            return new ReservationResponseDto()
            {
                BookingNumber = entity.ReservationNumber,
                Success = true,
                TransactionId = entity.Id.ToString()
            };
        }
        
        public async Task<ReservationResponseDto> DeleteAsync(int id)
        {
            var entity = await _reservationsRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                entity.ReservationStatus = ReservationStatus.Canceled;
                await _reservationsRepository.UpdateAsync(entity);
                return new ReservationResponseDto()
                {
                    BookingNumber = entity.ReservationNumber,
                    Success = true,
                    TransactionId = entity.Id.ToString()
                };
            }
            return new ReservationResponseDto()
            {
                BookingNumber = "",
                Success = false,
                TransactionId = ""
            };
        }

        private int CalculateTotalSoldNights(List<Reservations> reservations, DateTime start, DateTime end)
        {
          int nights = 0;
          foreach (var res in reservations)
          {
            var overlapStart = res.FromDate > start ? res.FromDate : start;
            var overlapEnd = res.ToDate < end ? res.ToDate : end;
            if (overlapStart < overlapEnd)
            {
              nights += (overlapEnd - overlapStart).Days;
            }
          }
          return nights;
        }

        private decimal CalculateOccupancyPercentage(int soldNights, List<Reservations> data, DateTime start, DateTime end)
        {
          var propertyCount = data.Select(x => x.PropertyName).Distinct().Count();
          if (propertyCount == 0) return 0;
          int totalDays = (end - start).Days;
          if (totalDays <= 0) totalDays = 1;
          return ((decimal)soldNights / (propertyCount * totalDays)) * 100;
        }

  }
}

