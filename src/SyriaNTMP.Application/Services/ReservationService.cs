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

            if (!string.IsNullOrEmpty(searchCriteria.DateFrom) &&
                DateTime.TryParse(searchCriteria.DateFrom, out var dateFrom))
                queryable = queryable.Where(x => dateFrom.Date <= x.FromDate.Date);

            if (!string.IsNullOrEmpty(searchCriteria.DateTo) &&
                DateTime.TryParse(searchCriteria.DateTo, out var dateTo))
                queryable = queryable.Where(x => dateTo.Date >= x.ToDate.Date);

            // Apply sorting and paging
            var sortedQueryable = queryable.OrderByDescending(x => x.Id); // or searchCriteria.Sorting
            var data = sortedQueryable
                .Skip(searchCriteria.SkipCount)
                .Take(searchCriteria.MaxResultCount).ToList();


            var count = await AsyncExecuter.CountAsync(queryable); // Count before paging

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
                query = query.Where(x => x.PropertyName.ToLower().Contains(filter.HotelName.ToLower()));
            }

            if (filter.HotelStars.HasValue)
            {
                query = query.Where(x => x.PropertyRating == filter.HotelStars);
            }

            if (!string.IsNullOrWhiteSpace(filter.Nationality))
            {
                query = query.Where(x => x.GuestNationality == filter.Nationality);
            }

            if (filter.Purpose.HasValue)
            {
                query = query.Where(x => x.ReservationPurpose == filter.Purpose.Value);
            }

            var rawData = await AsyncExecuter.ToListAsync(query);

            int diff = (7 + (DateTime.Today.DayOfWeek - DayOfWeek.Sunday)) % 7;

            var startPeriod = DateTime.Today.AddDays(-diff).Date;
            var endPeriod = startPeriod.AddDays(6);

            var activeData = rawData.Where(x =>
                x.ReservationStatus != ReservationStatus.Canceled &&
                x.ReservationStatus != ReservationStatus.NoShow).ToList();

            var activeProperties = activeData
                .Where(x => x.FromDate.Date <= endPeriod && x.ToDate.Date >= startPeriod)
                .Select(x => x.PropertyName).Distinct().Count();

            var weekly = new List<WeeklyDto>();
            for (var date = startPeriod.Date; date <= endPeriod.Date; date = date.AddDays(1))
            {
                var count = activeData.Count(x => x.FromDate.Date <= date && x.ToDate.Date >= date);

                weekly.Add(new WeeklyDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Count = count
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
            var totalCreatedToday = rawData.Count(x => x.CreatedDate.Date == today);
            var todayStats = new TodayStatsDto
            {
                CheckedIn = totalCreatedToday == 0
                    ? 0
                    : (rawData.Count(x =>
                           x.ReservationStatus == ReservationStatus.CheckedIn && x.FromDate.Date == today) * 100 /
                       totalCreatedToday),
                CheckedOut = totalCreatedToday == 0
                    ? 0
                    : (rawData.Count(x =>
                           x.ReservationStatus == ReservationStatus.CheckedOut && x.ToDate.Date == today) * 100 /
                       totalCreatedToday),
                Cancelled = totalCreatedToday == 0
                    ? 0
                    : (rawData.Count(x =>
                           x.ReservationStatus == ReservationStatus.Canceled && x.CreatedDate.Date == today) * 100 /
                       totalCreatedToday)
            };


            var totalSoldNights = CalculateTotalSoldNights(activeData, startPeriod, endPeriod);
            var totalRevenue = activeData.Sum(x => x.TotalPrice);
            var portfolioAdr = totalSoldNights == 0 ? 0 : (totalRevenue / totalSoldNights);
            var periodData = rawData.Where(x => x.FromDate >= startPeriod && x.ToDate <= endPeriod).ToList();

            return new DashboardDto
            {
                Summary = new SummaryDto
                {
                    TotalReservations = rawData.Count,
                    CancellationRate = periodData.Count == 0
                        ? 0
                        : (periodData.Count(x => x.ReservationStatus == ReservationStatus.Canceled) * 100 /
                           periodData.Count),
                    OccupancyRate = CalculateOccupancyPercentage(totalSoldNights, activeData, startPeriod, endPeriod),
                    ActiveProperties = activeProperties
                },
                PurposeStats = activeData.GroupBy(x => x.ReservationPurpose)
                    .Select(g => new PurposeDto { Purpose = g.Key.ToString(), Count = g.Count() }).ToList(),
                NationalityStats = nationality,
                WeeklyReservations = weekly,
                TodayStats = todayStats,
                Revenue = new RevenueDto
                {
                    PortfolioAdr = portfolioAdr,
                    MeanAdrByCity = activeData.GroupBy(x => x.City).Select(g => new AdrByCityDto
                    {
                        City = g.Key,
                        Adr = CalculateAdrPercentage(g.ToList(), startPeriod, endPeriod)
                    }).ToList(),
                },
                OccupancyByCity = activeData.GroupBy(x => x.City).Select(g => new CityOccupancyDto
                {
                    City = g.Key,
                    OccupancyRate = CalculateOccupancyPercentage(
                        CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod), g.ToList(), startPeriod,
                        endPeriod)
                }).ToList(),
                
                
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
            var existingItem =
                await _reservationsRepository.FirstOrDefaultAsync(x => x.ReservationId == dto.ReservationId);
            if (existingItem != null)
            {
                existingItem.ReservationStatus = entity.ReservationStatus;
                existingItem.ReservationPurpose = entity.ReservationPurpose;
                existingItem.FromDate = entity.FromDate;
                existingItem.ToDate = entity.ToDate;
                existingItem.NumberOfRooms = entity.NumberOfRooms;
                existingItem.NumberOfGuests = entity.NumberOfGuests;
                existingItem.NumberOfNights = entity.NumberOfNights;
                existingItem.GuestNationality = entity.GuestNationality;
                existingItem.TotalPrice = entity.TotalPrice;
                existingItem.ReservationStatus = entity.ReservationStatus;
                existingItem.ReservationPurpose = entity.ReservationPurpose;
                existingItem.ToDate = entity.ToDate;
                existingItem.TotalNumberOfPropertyUnits = entity.TotalNumberOfPropertyUnits;
                existingItem.City = entity.City;

                await _reservationsRepository.UpdateAsync(existingItem);
            }
            else
            {
                entity.CreatedDate = DateTime.Now;
                await _reservationsRepository.InsertAsync(entity);
            }
            
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

        public async Task<List<LookupDto>> GetCitiesAsync()
        {
            var query = await _reservationsRepository.GetQueryableAsync();

            var cities = await AsyncExecuter.ToListAsync(
                query
                    .Where(x => x.City != null)
                    .Select(x => new
                    {
                        x.City
                    })
                    .Distinct()
            );

            return cities.Select(x => new LookupDto
            {
                Value = x.City,
                NameEn = x.City,
                NameAr = x.City
            }).ToList();
        }

        public async Task<List<LookupDto>> GetPropertiesAsync()
        {
            var query = await _reservationsRepository.GetQueryableAsync();

            var properties = await AsyncExecuter.ToListAsync(
                query
                    .Where(x => x.PropertyName != null)
                    .Select(x => new
                    {
                        x.PropertyName
                    })
                    .Distinct()
            );

            return properties.Select(x => new LookupDto
            {
                Value = x.PropertyName,
                NameEn = x.PropertyName,
                NameAr = x.PropertyName
            }).ToList();
        }

        public async Task<List<LookupDto>> GetNationalitiesAsync()
        {
            var query = await _reservationsRepository.GetQueryableAsync();

            var list = await AsyncExecuter.ToListAsync(
                query
                    .Where(x => x.GuestNationality != null)
                    .Select(x => x.GuestNationality)
                    .Distinct()
            );

            return list.Select(nationality => new LookupDto
            {
                Value = nationality,
                NameEn = nationality,
                NameAr = nationality
            }).ToList();
        }

        public async Task<List<LookupDto>> GetPurposesAsync()
        {
            var values = Enum.GetValues(typeof(ReservationPurpose))
                .Cast<ReservationPurpose>()
                .ToList();

            return values.Select(x => new LookupDto
            {
                Value = ((int)x).ToString(),
                NameEn = x.ToString(),
                NameAr = x.ToString()
            }).ToList();
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

        private decimal CalculateOccupancyPercentage(int soldNights, List<Reservations> data, DateTime start,
            DateTime end)
        {
            int totalDays = (end - start).Days;
            if (totalDays <= 0) totalDays = 1;
            var totalAvailableUnits = data.GroupBy(x => x.PropertyName).Sum(g => g.First().NumberOfRooms * totalDays);

            return totalAvailableUnits == 0 ? 0 : ((decimal)soldNights / totalAvailableUnits) * 100;
        }
        
        private decimal CalculateAdrPercentage(List<Reservations> data, DateTime start,
            DateTime end)
        {
            int totalDays = (end - start).Days;
            if (totalDays <= 0) totalDays = 1;
            var totalPrice = data.Sum(g => g.TotalPrice);
            var totalUnits = data.GroupBy(x => x.PropertyName).Sum(g => g.First()?.TotalNumberOfPropertyUnits??0);
            
            return totalPrice == 0 || totalUnits == 0 ? 0 : totalPrice / totalDays * totalUnits;
        }
    }
}