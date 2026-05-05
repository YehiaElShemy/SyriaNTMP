using Microsoft.AspNetCore.Authorization;
using SyriaNTMP.Dto;
using SyriaNTMP.Models;
using SyriaNTMP.Models.Enums;
using System;
using System.Collections;
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
            DateTime.TryParse(searchCriteria.DateFrom, out var dateFrom);
            DateTime.TryParse(searchCriteria.DateTo, out var dateTo);
            if (!string.IsNullOrEmpty(searchCriteria.guestNationality))
                queryable = queryable.Where(r => r.GuestNationality.Contains(searchCriteria.guestNationality));

            if (!string.IsNullOrEmpty(searchCriteria.PropertyName))
                queryable = queryable.Where(r => r.PropertyName.Contains(searchCriteria.PropertyName));

            if (searchCriteria.PropertyRating.HasValue)
            {
                queryable = searchCriteria.PropertyRating == PropertyRatingEnum.None
                         ? queryable.Where(r => r.PropertyRating == null || r.PropertyRating == PropertyRatingEnum.None)
                         : queryable.Where(r => r.PropertyRating == searchCriteria.PropertyRating);

            }
            if (!string.IsNullOrEmpty(searchCriteria.ReservationNumber))
                queryable = queryable.Where(r => r.ReservationNumber == searchCriteria.ReservationNumber);

            if (searchCriteria.ReservationStatus.HasValue)
                queryable = queryable.Where(r => r.ReservationStatus == searchCriteria.ReservationStatus);

            if (searchCriteria.ReservationPurpose.HasValue)
                queryable = queryable.Where(r => r.ReservationPurpose == searchCriteria.ReservationPurpose);
            if (!string.IsNullOrEmpty(searchCriteria.DateFrom) && !string.IsNullOrEmpty(searchCriteria.DateTo))
            {
                queryable = queryable.Where(x => x.FromDate <= dateTo && x.ToDate >= dateFrom);
            }
            else if (!string.IsNullOrEmpty(searchCriteria.DateFrom))
            {
                queryable = queryable.Where(x => x.FromDate >= dateFrom);
            }
            else if (!string.IsNullOrEmpty(searchCriteria.DateTo))
            {
                dateTo = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day, 23, 59, 59);
                queryable = queryable.Where(x => x.ToDate <= dateTo);
            }
            if (searchCriteria.CurrencyId.HasValue)
                queryable = queryable.Where(r => r.CurrencyId == searchCriteria.CurrencyId);


            // Apply sorting and paging
            var sortedQueryable = queryable.OrderByDescending(x => x.Id); // or searchCriteria.Sorting
            var data = sortedQueryable
                .Skip(searchCriteria.SkipCount)
                .Take(searchCriteria.MaxResultCount).ToList();


            var count = await AsyncExecuter.CountAsync(queryable); // NightCount before paging

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
            #region filter
            if (filter.FromDate.HasValue && filter.ToDate.HasValue)
            {
                filter.ToDate = new DateTime(filter.ToDate.Value.Year, filter.ToDate.Value.Month, filter.ToDate.Value.Day, 23, 59, 59);
                query = query.Where(x => x.FromDate <= filter.ToDate.Value && x.ToDate >= filter.FromDate.Value);
            }
            else if (filter.FromDate.HasValue)
            {
                query = query.Where(x => x.FromDate >= filter.FromDate.Value);
            }
            else if (filter.ToDate.HasValue)
            {
                filter.ToDate = new DateTime(filter.ToDate.Value.Year, filter.ToDate.Value.Month, filter.ToDate.Value.Day, 23, 59, 59);
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
                query = filter.HotelStars == PropertyRatingEnum.None
                ? query.Where(r => r.PropertyRating == null || r.PropertyRating == PropertyRatingEnum.None)
                : query.Where(r => r.PropertyRating == filter.HotelStars);
            }

            if (!string.IsNullOrWhiteSpace(filter.Nationality))
            {
                query = query.Where(x => x.GuestNationality == filter.Nationality);
            }

            if (filter.Purpose.HasValue)
            {
                query = query.Where(x => x.ReservationPurpose == filter.Purpose.Value);
            }
            if (filter.CurrencyId.HasValue)
            {
                query = query.Where(x => x.CurrencyId == filter.CurrencyId.Value);
            }
            #endregion
            var rawData = await AsyncExecuter.ToListAsync(query);

            var startPeriod = filter.FromDate ?? rawData.Min(a => a.FromDate);
            var endPeriod = filter.ToDate ?? rawData.Max(a => a.ToDate);
            // Assign to your DTO/ViewModel
            filter.FromDate = startPeriod;

            var activeData = rawData.Where(x =>
                x.ReservationStatus != ReservationStatus.Canceled &&
                x.ReservationStatus != ReservationStatus.NoShow).ToList();

            var activeProperties = activeData
                .Where(x => x.FromDate.Date <= endPeriod && x.ToDate.Date >= startPeriod)
                .Select(x => x.PropertyName).Distinct().Count();
            // only for operation reservation for last week start from sunday without look today is be
            #region weekly reservations
            (List<WeeklyDto> weekly, TodayStatsDto todayStats) = await GetReservationWeeklyAndToday();
            #endregion
            var nationality = activeData
                .GroupBy(x => x.GuestNationality)
                .Select(g => new NationalityDto
                {
                    Nationality = g.Key,
                    NightCount = CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod),
                    VisitorCount = g.Sum(x => x.NumberOfGuests)

                })
                .OrderByDescending(x => x.NightCount)
                .ToList();

            var totalSoldNights = CalculateTotalSoldNights(activeData, startPeriod, endPeriod);
            decimal totalRevenue = activeData.Sum(a =>
            {
                int totalNights = (a.ToDate.Date - a.FromDate.Date).Days;
                int overlapNights = GetOverlappingNights(a.FromDate, a.ToDate, startPeriod, endPeriod);
                decimal pricePerNight = totalNights>0? a.TotalPrice / totalNights:0;
                return pricePerNight * overlapNights;
            });
            //var totalRevenue =  activeData.Sum(x => x.TotalPrice);
            var portfolioAdr = totalSoldNights == 0 ? 0 : (totalRevenue / totalSoldNights);
            var periodData = rawData.Where(x => x.FromDate >= startPeriod && x.ToDate <= endPeriod).ToList();

            var distinctDays = activeData.Select(s => s.CreatedDate.Date).Distinct().Count();
            var AdrAvgPriceDay = distinctDays > 0 ? totalRevenue / distinctDays : 0;


            // sum of solid nights
            var totalOccupiedNights = activeData.Sum(r => r.NumberOfRooms * r.NumberOfNights);
            // sum of available nights
            int totalDays = ((endPeriod - startPeriod).Days) + 1;

            var totalAvailableNights = activeData.GroupBy(x => x.PropertyName).Sum(g => (g.First().TotalNumberOfPropertyUnits ?? 0) * totalDays);

            var avgOccupancyRate = Math.Round(totalAvailableNights > 0 ? (totalOccupiedNights / (double)totalAvailableNights) * 100 : 0, 3);
            var totalNightsNotSolds = totalAvailableNights - totalOccupiedNights;

            var dashborddto = new DashboardDto();
            dashborddto.Opertation = new OperationDto
            {
                TotalReservations = rawData.Count,
                CancellationRate = rawData.Count == 0 ? 0 : (rawData.Count(x => x.ReservationStatus == ReservationStatus.Canceled) * 100 /
                       rawData.Count),
                OccupancyRate = CalculateOccupancyPercentage(totalSoldNights, activeData, startPeriod, endPeriod),
                ActiveProperties = activeProperties,
                TotalSoldNights = totalSoldNights
            };
            var totalNights = CalculateTotalSoldNights(rawData, startPeriod, endPeriod);

            dashborddto.PurposeStats = new PurposeDto()
            {
                TotalNight = totalNights,
                NumOfGuests = rawData.Sum(a => a.NumberOfGuests),
                MostCommonPurpose = rawData.Count() > 0 ? rawData.GroupBy(x => x.ReservationPurpose)
                .Select(g => new PurposeDetailsDto
                {
                    Purpose = g.Key.ToString(),
                    Count = g.Sum(a => a.NumberOfNights)
                }).MaxBy(a => a.Count).Purpose : "",

                PurposeDetailsDtos = rawData.GroupBy(x => x.ReservationPurpose)
                .Select(g => new PurposeDetailsDto
                {
                    Purpose = g.Key.ToString(),
                    Count = g.Sum(a => a.NumberOfNights),
                    PurposeRate = totalNights == 0 ? 0 : (CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod) * 100) / totalNights
                }).ToList(),
            };

            dashborddto.NationalityStats = nationality;
            dashborddto.WeeklyReservations = weekly;
            dashborddto.TotalWeeklyReservations = weekly.Sum(a => a.Count);
            dashborddto.TodayStats = todayStats;
            dashborddto.Revenue = new RevenueDto();

            dashborddto.Revenue.PortfolioAdr = portfolioAdr;
            dashborddto.Revenue.TotalRevenue = totalRevenue;
            // expected currency all reservation same currency
            dashborddto.Revenue.CurrencySymbolAr = activeData.FirstOrDefault()?.CurrencySymbolAr ?? "";
            dashborddto.Revenue.CurrencySymbolEn = activeData.FirstOrDefault()?.CurrencySymbolEn ?? "";

            dashborddto.Revenue.TotalNight = totalSoldNights;
            // dashborddto.Revenue.AdrAvgPriceDay = AdrAvgPriceDay;
            dashborddto.Revenue.MeanAdrByCity = activeData.GroupBy(x => x.City).Select(g => new AdrByCityDto
            {
                City = g.Key,
                Adr = CalculateAdrPercentage(g.ToList(), startPeriod, endPeriod),
                TotalNight = CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod),

            }).ToList();

            //dashborddto.Revenue.PeakCity = new PeakCityDto();

            //dashborddto.Revenue.PeakCity.Adr = activeData.Any() ? Math.Round(activeData.GroupBy(x => x.City)
            //        .Select(g => new AdrByCityDto
            //        {
            //            City = g.Key,
            //            Adr = CalculateAdrPercentage(g.ToList(), startPeriod, endPeriod)
            //        }).Max(a => a.Adr), 3) : 0;
            //dashborddto.Revenue.PeakCity.City = activeData.Any() ? activeData.GroupBy(x => x.City)
            //    .Select(g => new AdrByCityDto
            //    {
            //        City = g.Key,
            //        Adr = CalculateAdrPercentage(g.ToList(), startPeriod, endPeriod),
            //        TotalNight = CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod)

            //    }).MaxBy(a => a.Adr).City : "";



            dashborddto.OccupancyDto = new OccupancyDto()
            {
                AvgOccupancyRate = avgOccupancyRate,
                TotalSoldNights = totalSoldNights,
                TotalNightsNotSolds = totalNightsNotSolds,
                CityOccupancyDto = activeData.GroupBy(x => x.City).Select(g => new CityOccupancyDto
                {
                    City = g.Key,
                    OccupancyRate = CalculateOccupancyPercentage(
                     CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod), g.ToList(), startPeriod,
                     endPeriod)
                }).ToList(),

            };
            return dashborddto;
        }

        private async Task<(List<WeeklyDto> weekly, TodayStatsDto todayStats)> GetReservationWeeklyAndToday()
        {
            int diff = (7 + (DateTime.Today.DayOfWeek - DayOfWeek.Sunday)) % 7;
            var startPeriodOperation = DateTime.Today.AddDays(-diff).Date;
            var endPeriodOperation = startPeriodOperation.AddDays(6);
            var queryOperation = await _reservationsRepository.GetQueryableAsync();
            queryOperation = queryOperation.Where(x => x.FromDate >= startPeriodOperation && x.ToDate <= endPeriodOperation &&
                x.ReservationStatus != ReservationStatus.Canceled &&
                x.ReservationStatus != ReservationStatus.Expired &&
                x.ReservationStatus != ReservationStatus.NoShow);
            var rawDataOperation = await AsyncExecuter.ToListAsync(queryOperation);
            var weekly = new List<WeeklyDto>();
            for (var date = startPeriodOperation.Date; date <= endPeriodOperation.Date; date = date.AddDays(1))
            {
                var count = rawDataOperation.Count(x => x.FromDate.Date <= date && x.ToDate.Date >= date);

                weekly.Add(new WeeklyDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Count = count
                });
            }
            var today = DateTime.Today;
            var totalCreatedToday = rawDataOperation.Count(x => x.CreatedDate.Date == today);
            var todayStats = new TodayStatsDto
            {
                CheckedIn = totalCreatedToday == 0
                    ? 0
                    : (rawDataOperation.Count(x =>
                           x.ReservationStatus == ReservationStatus.CheckedIn && x.FromDate.Date == today) * 100 /
                       totalCreatedToday),
                CheckedOut = totalCreatedToday == 0
                    ? 0
                    : (rawDataOperation.Count(x =>
                           x.ReservationStatus == ReservationStatus.CheckedOut && x.ToDate.Date == today) * 100 /
                       totalCreatedToday),
                Cancelled = totalCreatedToday == 0
                    ? 0
                    : (rawDataOperation.Count(x =>
                           x.ReservationStatus == ReservationStatus.Canceled && x.CreatedDate.Date == today) * 100 /
                       totalCreatedToday)
            };
            return (weekly, todayStats);
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
            var entity = await _reservationsRepository.FirstOrDefaultAsync(x => x.ReservationId == id);
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
        public async Task<List<LookupDto>> GetCurrenciesAsync()
        {
            var query = await _reservationsRepository.GetQueryableAsync();

            var list = await AsyncExecuter.ToListAsync(
                query
                    .Where(x => x.CurrencyId != null)
                    .Select(x => new LookupDto()
                    {
                        NameAr = x.CurrencySymbolAr,
                        NameEn = x.CurrencySymbolEn,
                        Id = x.CurrencyId

                    }).Distinct()
            );

            return list ?? new List<LookupDto>();
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
                var overlapEnd = res.ToDate < end ? res.ToDate.AddDays(-1) : end;
                if (overlapStart < overlapEnd)
                {
                    var noOfNights = (overlapEnd.Date - overlapStart.Date).Days+1;
                    nights += noOfNights;
                    res.NumberOfNights = noOfNights;
                }
            }

            return nights;
        }

        private decimal CalculateOccupancyPercentage(int soldNights, List<Reservations> data, DateTime start,
            DateTime end)
        {
            int totalDays = ((end - start).Days) + 1;
            if (totalDays <= 0) totalDays = 1;
            var totalAvailableUnits = data.GroupBy(x => x.PropertyName).Sum(g => (g.First().TotalNumberOfPropertyUnits ?? 0) * totalDays);

            return totalAvailableUnits == 0 ? 0 : ((decimal)soldNights / totalAvailableUnits) * 100;
        }

        private decimal CalculateAdrPercentage(List<Reservations> data, DateTime start, DateTime end)
        {
            var totalPrice = data.Sum(g => g.TotalPrice);
            if (totalPrice == 0) return 0;
            int totalRoomNights = 0;
            decimal totalNightsPrice = 0;
            foreach (var r in data)
            {
                var nightPrice = r.NumberOfNights > 0 ? (r.TotalPrice / r.NumberOfNights) : 0;
                var arrival = r.FromDate;
                var departure = r.ToDate;
                var nightsInPeriod = 0;
                var current = arrival.Date;
                while (current < departure.Date && current >= start.Date && current < end.Date)
                {
                    nightsInPeriod++;
                    current = current.AddDays(1);
                }
                totalRoomNights += nightsInPeriod;
                totalNightsPrice += nightPrice * nightsInPeriod;
            }
            if (totalRoomNights == 0) return 0;
            return totalNightsPrice / totalRoomNights;
        }

        int GetOverlappingNights(DateTime resStart, DateTime resEnd, DateTime start, DateTime end)
        {
            DateTime overlapStart = resStart > start ? resStart : start;
            DateTime overlapEnd = resEnd < end ? resEnd.AddDays(-1) : end;
            return (overlapEnd.Date - overlapStart.Date).Days + 1;  // Inclusive nights
        }
    }
}