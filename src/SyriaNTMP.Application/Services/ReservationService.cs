using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
using SyriaNTMP.Dto;
using SyriaNTMP.Localization;
using SyriaNTMP.Models;
using SyriaNTMP.Models.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
namespace SyriaNTMP.Services
{
    [Authorize]
    public partial class ReservationService : ApplicationService
    {
        private readonly IRepository<Reservations> _reservationsWriteRepository;
        private readonly IReadOnlyRepository<Reservations> _reservationsRepository;
        private readonly IStringLocalizer<SyriaNTMPResource> L;
        public ReservationService(IReadOnlyRepository<Reservations> reservationsRepository, IRepository<Reservations> reservationsWriteRepository, IStringLocalizer<SyriaNTMPResource> l)
        {
            _reservationsRepository = reservationsRepository;
            _reservationsWriteRepository = reservationsWriteRepository;
            L = l;
        }
        public async Task<FileExportDto> GetReservationsGridToExcelAsync(ReservationsSearchCriteria searchCriteria)
        {
            var rawData = await GetReservations(searchCriteria);
            return ExportDataTOExcel(rawData.Items.ToList());
        }
        private FileExportDto ExportDataTOExcel(List<ReservationsDto> rawData)
        {
            using var stream = new MemoryStream();

            var isArabic = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar";

            var exportData = rawData.Select(r => new
            {
                ReservationNumber = r.ReservationNumber,
                CompanyName = r.CompanyName,
                PropertyName = r.PropertyName,
                City = r.City,
                PropertyRating = r.PropertyRating?.ToString(),
                ReservationStatus = r.ReservationStatus.ToString(),
                NumberOfGuests = r.NumberOfGuests,
                NumberOfNights = r.NumberOfNights,
                NumberOfRooms = r.NumberOfRooms,
                ReservationPurpose = r.ReservationPurpose.ToString(),
                FromDate = r.FromDate.ToString("yyyy-MM-dd"),
                ToDate = r.ToDate.ToString("yyyy-MM-dd"),
                GuestNationality = r.GuestNationality,
                TotalPrice = r.TotalPrice,
                Currency = isArabic ? r.CurrencySymbolAr : r.CurrencySymbolEn,
                CreatedDate = r.CreatedDate.ToString("yyyy-MM-dd HH:mm")
            });
            var config = new OpenXmlConfiguration
            {
                DynamicColumns = new[]
                {
            new DynamicExcelColumn("ReservationNumber") { Name = L["Reservation:ReservationNumber"] },
            new DynamicExcelColumn("CompanyName") { Name = L["Reservation:CompanyName"] },
            new DynamicExcelColumn("PropertyName") { Name = L["Reservation:PropertyName"] },
            new DynamicExcelColumn("City") { Name = L["Reservation:City"] },
            new DynamicExcelColumn("PropertyRating") { Name = L["Reservation:PropertyRating"] },
            new DynamicExcelColumn("ReservationStatus") { Name = L["Reservation:ReservationStatus"] },
            new DynamicExcelColumn("NumberOfGuests") { Name = L["Reservation:NumberOfGuests"] },
            new DynamicExcelColumn("NumberOfNights") { Name = L["Reservation:NumberOfNights"] },
            new DynamicExcelColumn("NumberOfRooms") { Name = L["Reservation:NumberOfRooms"] },
            new DynamicExcelColumn("ReservationPurpose") { Name = L["Reservation:ReservationPurpose"] },
            new DynamicExcelColumn("FromDate") { Name = L["Reservation:FromDate"] },
            new DynamicExcelColumn("ToDate") { Name = L["Reservation:ToDate"] },
            new DynamicExcelColumn("GuestNationality") { Name = L["Reservation:GuestNationality"] },
            new DynamicExcelColumn("TotalPrice") { Name = L["Reservation:TotalPrice"] },
            new DynamicExcelColumn("Currency") { Name = L["Reservation:Currency"] },
            new DynamicExcelColumn("CreatedDate") { Name = L["Reservation:CreatedDate"] }
        }
            };

            MiniExcel.SaveAs(stream, exportData, configuration: config);

            var bytes = stream.ToArray();
            var fileName = $"Reservations_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return new FileExportDto(fileName, bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
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
        #region Excel
        public async Task<FileExportDto> GetExportOccupancyToExcelAsync(DashboardFilterDto filter)
        {
            var query = await _reservationsRepository.GetQueryableAsync();
            #region filter
            query = ApplayFilter(filter, query);

            #endregion
            var rawData = await AsyncExecuter.ToListAsync(query);

            var startPeriod = filter.FromDate ?? rawData.Min(a => a.FromDate);
            var endPeriod = filter.ToDate ?? rawData.Max(a => a.ToDate);
            // Assign to your DTO/ViewModel
            filter.FromDate = startPeriod;

            var activeData = rawData.Where(x =>
                x.ReservationStatus != ReservationStatus.Canceled &&
                x.ReservationStatus != ReservationStatus.NoShow).ToList();

            var totalSoldNights = CalculateTotalSoldNights(activeData, startPeriod, endPeriod);
            // sum of solid nights
            var totalOccupiedNights = activeData.Sum(r => r.NumberOfRooms * r.NumberOfNights);
            // sum of available nights
            int totalDays = ((endPeriod - startPeriod).Days) + 1;

            var totalAvailableNights = activeData.GroupBy(x => x.PropertyName).Sum(g => (g.First().TotalNumberOfPropertyUnits ?? 0) * totalDays);

            var avgOccupancyRate = Math.Round(totalAvailableNights > 0 ? (totalSoldNights / (double)totalAvailableNights) * 100 : 0, 3);
            var totalNightsNotSolds = totalAvailableNights - totalSoldNights;

            var dashborddto = new DashboardDto();
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
            return ExportDataTOExcel(dashborddto, filter.DashboardTabs);
        }
        public async Task<FileExportDto> GetExportADRToExcelAsync(DashboardFilterDto filter)
        {
            var query = await _reservationsRepository.GetQueryableAsync();
            #region filter
            query = ApplayFilter(filter, query);

            #endregion
            var rawData = await AsyncExecuter.ToListAsync(query);

            var startPeriod = filter.FromDate ?? rawData.Min(a => a.FromDate);
            var endPeriod = filter.ToDate ?? rawData.Max(a => a.ToDate);
            // Assign to your DTO/ViewModel
            filter.FromDate = startPeriod;

            var activeData = rawData.Where(x =>
                x.ReservationStatus != ReservationStatus.Canceled &&
                x.ReservationStatus != ReservationStatus.NoShow).ToList();



            var totalSoldNights = CalculateTotalSoldNights(activeData, startPeriod, endPeriod);
            decimal totalRevenue = Math.Round(CalacTotalRevenue(startPeriod, endPeriod, activeData), 2);

            var portfolioAdr = Math.Round(totalSoldNights == 0 ? 0 : (totalRevenue / totalSoldNights));

            var distinctDays = activeData.Select(s => s.CreatedDate.Date).Distinct().Count();
            var AdrAvgPriceDay = Math.Round(distinctDays > 0 ? totalRevenue / distinctDays : 0, 2);
            var dashborddto = new DashboardDto();
            dashborddto.Revenue = new RevenueDto();
            dashborddto.Revenue.PortfolioAdr = portfolioAdr;
            dashborddto.Revenue.TotalRevenue = totalRevenue;
            dashborddto.Revenue.TotalNight = totalSoldNights;
            dashborddto.Revenue.MeanAdrByCity = activeData.GroupBy(x => x.City).Select(g => new AdrByCityDto
            {
                City = g.Key,
                Adr = CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod) > 0 ?
                CalacTotalRevenue(startPeriod, endPeriod, g.ToList()) / CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod) : 0, //CalculateAdrPercentage(g.ToList(), startPeriod, endPeriod),
                TotalNight = CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod),

            }).ToList();
            return ExportDataTOExcel(dashborddto, filter.DashboardTabs);

        }
        public async Task<FileExportDto> GetExportNationaltyToExcelAsync(DashboardFilterDto filter)
        {
            var query = await _reservationsRepository.GetQueryableAsync();
            #region filter
            query = ApplayFilter(filter, query);

            #endregion
            var rawData = await AsyncExecuter.ToListAsync(query);

            var startPeriod = filter.FromDate ?? rawData.Min(a => a.FromDate);
            var endPeriod = filter.ToDate ?? rawData.Max(a => a.ToDate);
            // Assign to your DTO/ViewModel
            filter.FromDate = startPeriod;

            var activeData = rawData.Where(x =>
                x.ReservationStatus != ReservationStatus.Canceled &&
                x.ReservationStatus != ReservationStatus.NoShow).ToList();

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
            return ExportDataTOExcel(new DashboardDto(), filter.DashboardTabs, nationalityDtos: nationality);

        }
        public async Task<FileExportDto> GetExportPurposeToExcel(DashboardFilterDto filter)
        {
            var query = await _reservationsRepository.GetQueryableAsync();
            #region filter
            query = ApplayFilter(filter, query);

            #endregion
            var rawData = await AsyncExecuter.ToListAsync(query);

            var startPeriod = filter.FromDate ?? rawData.Min(a => a.FromDate);
            var endPeriod = filter.ToDate ?? rawData.Max(a => a.ToDate);
            var activeData = rawData.Where(x =>
         x.ReservationStatus != ReservationStatus.Canceled &&
         x.ReservationStatus != ReservationStatus.NoShow).ToList();
            var totalNights = CalculateTotalSoldNights(activeData, startPeriod, endPeriod);
            var dashborddto = new DashboardDto();

            dashborddto.PurposeStats = new PurposeDto()
            {
                TotalNight = totalNights,
                NumOfGuests = activeData.Sum(a => a.NumberOfGuests),
                MostCommonPurpose = activeData.Count() > 0 ? activeData.GroupBy(x => x.ReservationPurpose)
               .Select(g => new PurposeDetailsDto
               {
                   Purpose = g.Key.ToString(),
                   Count = CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod)
               }).MaxBy(a => a.Count).Purpose : "",

                PurposeDetailsDtos = activeData.GroupBy(x => x.ReservationPurpose)
               .Select(g => new PurposeDetailsDto
               {
                   Purpose = g.Key.ToString(),
                   NumCityOfGuests = g.Sum(x => x.NumberOfGuests),
                   Count = CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod),
                   PurposeRate = totalNights == 0 ? 0 : (CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod) * 100) / totalNights
               }).ToList(),
            };
            return ExportDataTOExcel(dashborddto, filter.DashboardTabs);

        }
        public async Task<FileExportDto> GetExportOperationToExcel(DashboardFilterDto filter)
        {
            var query = await _reservationsRepository.GetQueryableAsync();
            #region filter
            query = ApplayFilter(filter, query);

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
            (List<WeeklyDto> weekly, TodayStatsDto todayStats) = await GetReservationWeeklyAndToday(rawData);
            #endregion
            var totalSoldNights = CalculateTotalSoldNights(activeData, startPeriod, endPeriod);
            decimal totalRevenue = Math.Round(CalacTotalRevenue(startPeriod, endPeriod, activeData), 2);
            var portfolioAdr = Math.Round(totalSoldNights == 0 ? 0 : (totalRevenue / totalSoldNights));


            var dashborddto = new DashboardDto();
            dashborddto.Opertation = new OperationDto
            {
                TotalReservations = activeData.Count,
                CancellationRate = rawData.Count == 0 ? 0 : (rawData.Count(x => x.ReservationStatus == ReservationStatus.Canceled) * 100 /
                                                             rawData.Count),
                OccupancyRate = CalculateOccupancyPercentage(totalSoldNights, activeData, startPeriod, endPeriod),
                ActiveProperties = activeProperties,
                TotalSoldNights = totalSoldNights
            };

            dashborddto.WeeklyReservations = weekly;
            dashborddto.TotalWeeklyReservations = weekly.Sum(a => a.Count);
            dashborddto.TodayStats = todayStats;

            return ExportDataTOExcel(dashborddto, filter.DashboardTabs);


        }
        #endregion



        public async Task<DashboardDto> GetDashboardAsync(DashboardFilterDto filter)
        {
            var query = await _reservationsRepository.GetQueryableAsync();
            #region filter
            query = ApplayFilter(filter, query);

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
            (List<WeeklyDto> weekly, TodayStatsDto todayStats) = await GetReservationWeeklyAndToday(rawData);
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
            decimal totalRevenue = Math.Round(CalacTotalRevenue(startPeriod, endPeriod, activeData), 2);

            var portfolioAdr = Math.Round(totalSoldNights == 0 ? 0 : (totalRevenue / totalSoldNights));
            var periodData = activeData.Where(x => x.FromDate >= startPeriod && x.ToDate <= endPeriod).ToList();

            var distinctDays = activeData.Select(s => s.CreatedDate.Date).Distinct().Count();
            var AdrAvgPriceDay = Math.Round(distinctDays > 0 ? totalRevenue / distinctDays : 0, 2);


            // sum of solid nights
            var totalOccupiedNights = activeData.Sum(r => r.NumberOfRooms * r.NumberOfNights);
            // sum of available nights
            int totalDays = ((endPeriod - startPeriod).Days) + 1;

            var totalAvailableNights = activeData.GroupBy(x => x.PropertyName).Sum(g => (g.First().TotalNumberOfPropertyUnits ?? 0) * totalDays);

            var avgOccupancyRate = Math.Round(totalAvailableNights > 0 ? (totalSoldNights / (double)totalAvailableNights) * 100 : 0, 3);
            var totalNightsNotSolds = totalAvailableNights - totalSoldNights;

            var dashborddto = new DashboardDto();
            dashborddto.Opertation = new OperationDto
            {
                TotalReservations = activeData.Count,
                CancellationRate = rawData.Count == 0 ? 0 : (rawData.Count(x => x.ReservationStatus == ReservationStatus.Canceled) * 100 /
                                                             rawData.Count),
                OccupancyRate = CalculateOccupancyPercentage(totalSoldNights, activeData, startPeriod, endPeriod),
                ActiveProperties = activeProperties,
                TotalSoldNights = totalSoldNights
            };
            var totalNights = CalculateTotalSoldNights(activeData, startPeriod, endPeriod);

            dashborddto.PurposeStats = new PurposeDto()
            {
                TotalNight = totalNights,
                NumOfGuests = activeData.Sum(a => a.NumberOfGuests),
                MostCommonPurpose = activeData.Count() > 0 ? activeData.GroupBy(x => x.ReservationPurpose)
                .Select(g => new PurposeDetailsDto
                {
                    Purpose = g.Key.ToString(),
                    Count = CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod)
                }).MaxBy(a => a.Count).Purpose : "",

                PurposeDetailsDtos = activeData.GroupBy(x => x.ReservationPurpose)
                .Select(g => new PurposeDetailsDto
                {
                    Purpose = g.Key.ToString(),
                    Count = CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod),
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
                Adr = CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod) > 0 ?
                CalacTotalRevenue(startPeriod, endPeriod, g.ToList()) / CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod) : 0, //CalculateAdrPercentage(g.ToList(), startPeriod, endPeriod),
                TotalNight = CalculateTotalSoldNights(g.ToList(), startPeriod, endPeriod),

            }).ToList();
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
        private FileExportDto ExportDataTOExcel(DashboardDto rawData, DashboardTabsEnum dashboardTabs, List<NationalityDto> nationalityDtos = null)
        {
            using var stream = new MemoryStream();

            var isArabic = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "ar";
            var exportData = new List<object>();
            var fileName = "";
            var config = new OpenXmlConfiguration();
            if (dashboardTabs == DashboardTabsEnum.VisitPurpose)
            {
                config = new OpenXmlConfiguration
                {
                    DynamicColumns = new[]
                   {
                        new DynamicExcelColumn("Purpose") { Name = L["Purpose"] },
                        new DynamicExcelColumn("NightsCount") { Name = L["NightsCount"] },
                        new DynamicExcelColumn("GuestsCount") { Name = L["GuestsCount"] },
                        new DynamicExcelColumn("PurposeRate") { Name = L["PurposeRate"] }
                    }
                };
                exportData = rawData.PurposeStats.PurposeDetailsDtos.Select(r =>
                {
                    return (object)new
                    {
                        Purpose = r.Purpose,
                        NightsCount = r.Count,
                        NightCountPrecentage = r.PurposeRate.ToString("F2") + " %",
                        GuestsCount = r.NumCityOfGuests
                    };
                }).ToList();
                fileName = $"Visit Purpose_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            }
            else if (dashboardTabs == DashboardTabsEnum.GuestMix)
            {
                config = new OpenXmlConfiguration
                {
                    DynamicColumns = new[]
                    {
                        new DynamicExcelColumn("Nationality") { Name = L["Nationality"] },
                        new DynamicExcelColumn("NightCount") { Name = L["NightCount"] },
                        new DynamicExcelColumn("VisitorCount") { Name = L["VisitorCount"] }
                    }
                };
                exportData = nationalityDtos.Select(r =>
                   {
                       return (object)new
                       {
                           r.Nationality,
                           r.NightCount,
                           r.VisitorCount
                       };
                   }).ToList();
                fileName = $"Nationalties Guests_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";


            }
            else if (dashboardTabs == DashboardTabsEnum.RevenueAndADR)
            {
                config = new OpenXmlConfiguration
                {
                    DynamicColumns = new[]
                    {
                        new DynamicExcelColumn("City") { Name = L["City"] },
                        new DynamicExcelColumn("TotalNightByCity") { Name = L["TotalNightByCity"] },
                        new DynamicExcelColumn("TotalRevenueBYCity") { Name = L["TotalRevenueBYCity"] },
                        new DynamicExcelColumn("TotalRevenue") { Name = L["TotalRevenue"] },
                        new DynamicExcelColumn("TotalNight") { Name = L["TotalNight"] }
                    }
                };
                exportData = rawData.Revenue.MeanAdrByCity.Select(r =>
                {
    
                return (object)new
                {
                    r.City,
                    TotalNightByCity = r.TotalNight,
                    TotalRevenueBYCity = r.Adr,
                    TotalRevenue = rawData.Revenue.TotalRevenue,
                    TotalNight = rawData.Revenue.TotalNight

                };

                }).ToList();
                    fileName = $"Revenue and ADR_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                }
            else if (dashboardTabs == DashboardTabsEnum.DemandAndOccupancy)
            {
                config = new OpenXmlConfiguration
                {                 
                    DynamicColumns = new[]
                        {
                            new DynamicExcelColumn("City") { Name = L["City"] },
                            new DynamicExcelColumn("OccupancyRate") { Name = L["OccupancyRate"] }
                        }
                };
                exportData = rawData.OccupancyDto.CityOccupancyDto.Select(r =>
                {

                    return (object)new
                    {
                        r.City,
                        OccupancyRate = r.OccupancyRate.ToString("F2") + " %",
                    };

                }).ToList();
                fileName = $"Demand And Occupancy_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            }
            else if (dashboardTabs == DashboardTabsEnum.Operations)
            {
                config = new OpenXmlConfiguration
                {
                    DynamicColumns = new[]
                    {
                        new DynamicExcelColumn("dayName") { Name = L["dayName"] },
                        new DynamicExcelColumn("date") { Name = L["date"] },
                        new DynamicExcelColumn("SoldNights") { Name = L["SoldNights"] },
                        new DynamicExcelColumn("totalSoldNights") { Name = L["totalSoldNights"] },
                        new DynamicExcelColumn("ActiveProperties") { Name = L["ActiveProperties"] },
                        new DynamicExcelColumn("CancellationRate") { Name = L["CancellationRate"] },
                        new DynamicExcelColumn("TotalWeeklyReservations") { Name = L["TotalWeeklyReservations"] },
                        new DynamicExcelColumn("CheckedInPercentage") { Name = L["CheckedInPercentage"] },
                        new DynamicExcelColumn("CheckedOutPercentage") { Name = L["CheckedOutPercentage"] },
                        new DynamicExcelColumn("CancelledPercentage") { Name = L["CancelledPercentage"] }
                    }
                };
                exportData = rawData.WeeklyReservations.Select(r =>
                {

                    return (object)new
                    {
                        dayName = DateTime.Parse(r.Date).DayOfWeek.ToString(),
                        date = r.Date,
                        SoldNights = r.Count,
                        totalSoldNights = rawData.Opertation.TotalSoldNights,
                        ActiveProperties = rawData.Opertation.ActiveProperties,
                        CancellationRate = rawData.Opertation.CancellationRate.ToString("F2") + " %",
                        TotalWeeklyReservations = rawData.TotalWeeklyReservations,
                        CheckedInPercentage = rawData.TodayStats.CheckedIn.ToString("F2") + " %",
                        CheckedOutPercentage = rawData.TodayStats.CheckedOut.ToString("F2") + " %",
                        CancelledPercentage = rawData.TodayStats.Cancelled.ToString("F2") + " %",


                    };

                }).ToList();
                fileName = $"Operation_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            }


            MiniExcel.SaveAs(stream, exportData,configuration:config);

            var bytes = stream.ToArray();
            return new FileExportDto(fileName, bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        private IQueryable<Reservations> ApplayFilter(DashboardFilterDto filter, IQueryable<Reservations> query)
        {
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

            return query;
        }

        private decimal CalacTotalRevenue(DateTime startPeriod, DateTime endPeriod, List<Reservations> activeData)
        {
            return activeData.Sum(a =>
            {
                int totalNights = (a.ToDate.Date - a.FromDate.Date).Days;
                int overlapNights = GetOverlappingNights(a.FromDate, a.ToDate, startPeriod, endPeriod);
                decimal pricePerNight = totalNights > 0 ? a.TotalPrice / totalNights : 0;
                return pricePerNight * overlapNights;
            });
        }

        private async Task<(List<WeeklyDto> weekly, TodayStatsDto todayStats)> GetReservationWeeklyAndToday(List<Reservations> activeData)
        {
            int diff = (7 + (DateTime.Today.DayOfWeek - DayOfWeek.Sunday)) % 7;
            var startPeriodOperation = DateTime.Today.AddDays(-diff).Date;
            var endPeriodOperation = startPeriodOperation.AddDays(6);
            var queryOperation = await _reservationsRepository.GetQueryableAsync();
            queryOperation = queryOperation.Where(x => x.FromDate <= endPeriodOperation && x.ToDate >= startPeriodOperation
            && x.ReservationStatus != ReservationStatus.Canceled &&
                x.ReservationStatus != ReservationStatus.NoShow && x.ReservationStatus != ReservationStatus.Expired
            );
            var weekData = await AsyncExecuter.ToListAsync(queryOperation);
            var weekly = new List<WeeklyDto>();
            for (var date = startPeriodOperation.Date; date <= endPeriodOperation.Date; date = date.AddDays(1))
            {
                var count = weekData.Count(x => x.FromDate.Date <= date && x.ToDate.Date > date);

                weekly.Add(new WeeklyDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Count = count
                });
            }
            var today = DateTime.Today;
            var todayStats = new TodayStatsDto();

            var totalCheckedIn =
                 (activeData.Count(x => x.ReservationStatus == ReservationStatus.CheckedIn && x.FromDate.Date == today));
            var totalCheckedOut = (activeData.Count(x => x.ReservationStatus == ReservationStatus.CheckedOut && x.ToDate.Date == today));
            var totalCancelled = (activeData.Count(x => (x.ReservationStatus == ReservationStatus.Canceled
            || x.ReservationStatus == ReservationStatus.NoShow)
            && x.CancelledDate.HasValue && x.CancelledDate.Value.Date == today));
            var totalCreatedToday = totalCheckedIn + totalCheckedOut + totalCancelled;
            todayStats.CheckedIn = totalCreatedToday == 0 ? 0 : totalCheckedIn * 100 / totalCreatedToday;
            todayStats.CheckedOut = totalCreatedToday == 0 ? 0 : totalCheckedOut * 100 / totalCreatedToday;
            todayStats.Cancelled = totalCreatedToday == 0 ? 0 : totalCancelled * 100 / totalCreatedToday;

            return (weekly, todayStats);
        }

        public async Task<ReservationsDto> GetById(int id)
        {
            var data = await _reservationsRepository.FirstOrDefaultAsync(x => x.Id == id);
            return ObjectMapper.Map<Reservations, ReservationsDto>(data ?? new Reservations());
        }

        public async Task<ReservationResponseDto> CreateAsync(ReservationsDto dto)
        {
            Logger.LogInformation($"Creating reservation with ID: {dto.ReservationId} , Data: {JsonSerializer.Serialize(dto)}");
            var entity = ObjectMapper.Map<ReservationsDto, Reservations>(dto);
            // check if item already exists
            var existingItem =
                await _reservationsWriteRepository.FirstOrDefaultAsync(x => x.ReservationId == dto.ReservationId);
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
                existingItem.CurrencyId = entity.CurrencyId;
                existingItem.CurrencySymbolAr = entity.CurrencySymbolAr;
                existingItem.CurrencySymbolEn = entity.CurrencySymbolEn;

                await _reservationsWriteRepository.UpdateAsync(existingItem);
            }
            else
            {
                entity.CreatedDate = DateTime.Now;
                await _reservationsWriteRepository.InsertAsync(entity);
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
            var entity = await _reservationsWriteRepository.FirstOrDefaultAsync(x => x.ReservationId == id);
            if (entity != null)
            {
                entity.CancelledDate = DateTime.Now;
                entity.ReservationStatus = ReservationStatus.Canceled;
                await _reservationsWriteRepository.UpdateAsync(entity);
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
                    var noOfNights = (overlapEnd.Date - overlapStart.Date).Days + 1;
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