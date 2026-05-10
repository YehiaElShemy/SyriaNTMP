import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { DashboardDto, DashboardFilterDto, FileExportDto, LookupDto, ReservationResponseDto, ReservationsDto, ReservationsSearchCriteria } from '../dto/models';

@Injectable({
  providedIn: 'root',
})
export class ReservationService {
  apiName = 'Default';
  

  create = (dto: ReservationsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReservationResponseDto>({
      method: 'POST',
      url: '/api/app/reservation',
      body: dto,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReservationResponseDto>({
      method: 'DELETE',
      url: `/api/app/reservation/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getByIdById = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReservationsDto>({
      method: 'GET',
      url: `/api/app/reservation/${id}/by-id`,
    },
    { apiName: this.apiName,...config });
  

  getCities = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, LookupDto[]>({
      method: 'GET',
      url: '/api/app/reservation/cities',
    },
    { apiName: this.apiName,...config });
  

  getCurrencies = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, LookupDto[]>({
      method: 'GET',
      url: '/api/app/reservation/currencies',
    },
    { apiName: this.apiName,...config });
  

  getDashboard = (filter: DashboardFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardDto>({
      method: 'GET',
      url: '/api/app/reservation/dashboard',
      params: { fromDate: filter.fromDate, toDate: filter.toDate, city: filter.city, hotelName: filter.hotelName, hotelStars: filter.hotelStars, nationality: filter.nationality, purpose: filter.purpose, currencyId: filter.currencyId, dashboardTabs: filter.dashboardTabs },
    },
    { apiName: this.apiName,...config });
  

  getExportADRToExcel = (filter: DashboardFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FileExportDto>({
      method: 'GET',
      url: '/api/app/reservation/export-aDRTo-excel',
      params: { fromDate: filter.fromDate, toDate: filter.toDate, city: filter.city, hotelName: filter.hotelName, hotelStars: filter.hotelStars, nationality: filter.nationality, purpose: filter.purpose, currencyId: filter.currencyId, dashboardTabs: filter.dashboardTabs },
    },
    { apiName: this.apiName,...config });
  

  getExportNationaltyToExcel = (filter: DashboardFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FileExportDto>({
      method: 'GET',
      url: '/api/app/reservation/export-nationalty-to-excel',
      params: { fromDate: filter.fromDate, toDate: filter.toDate, city: filter.city, hotelName: filter.hotelName, hotelStars: filter.hotelStars, nationality: filter.nationality, purpose: filter.purpose, currencyId: filter.currencyId, dashboardTabs: filter.dashboardTabs },
    },
    { apiName: this.apiName,...config });
  

  getExportOccupancyToExcel = (filter: DashboardFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FileExportDto>({
      method: 'GET',
      url: '/api/app/reservation/export-occupancy-to-excel',
      params: { fromDate: filter.fromDate, toDate: filter.toDate, city: filter.city, hotelName: filter.hotelName, hotelStars: filter.hotelStars, nationality: filter.nationality, purpose: filter.purpose, currencyId: filter.currencyId, dashboardTabs: filter.dashboardTabs },
    },
    { apiName: this.apiName,...config });
  

  getExportOperationToExcelByFilter = (filter: DashboardFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FileExportDto>({
      method: 'GET',
      url: '/api/app/reservation/export-operation-to-excel',
      params: { fromDate: filter.fromDate, toDate: filter.toDate, city: filter.city, hotelName: filter.hotelName, hotelStars: filter.hotelStars, nationality: filter.nationality, purpose: filter.purpose, currencyId: filter.currencyId, dashboardTabs: filter.dashboardTabs },
    },
    { apiName: this.apiName,...config });
  

  getExportPurposeToExcelByFilter = (filter: DashboardFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FileExportDto>({
      method: 'GET',
      url: '/api/app/reservation/export-purpose-to-excel',
      params: { fromDate: filter.fromDate, toDate: filter.toDate, city: filter.city, hotelName: filter.hotelName, hotelStars: filter.hotelStars, nationality: filter.nationality, purpose: filter.purpose, currencyId: filter.currencyId, dashboardTabs: filter.dashboardTabs },
    },
    { apiName: this.apiName,...config });
  

  getExportRevenueADRToExcelByFilter = (filter: DashboardFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FileExportDto>({
      method: 'GET',
      url: '/api/app/reservation/export-revenue-aDRTo-excel',
      params: { fromDate: filter.fromDate, toDate: filter.toDate, city: filter.city, hotelName: filter.hotelName, hotelStars: filter.hotelStars, nationality: filter.nationality, purpose: filter.purpose, currencyId: filter.currencyId, dashboardTabs: filter.dashboardTabs },
    },
    { apiName: this.apiName,...config });
  

  getNationalities = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, LookupDto[]>({
      method: 'GET',
      url: '/api/app/reservation/nationalities',
    },
    { apiName: this.apiName,...config });
  

  getProperties = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, LookupDto[]>({
      method: 'GET',
      url: '/api/app/reservation/properties',
    },
    { apiName: this.apiName,...config });
  

  getPurposes = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, LookupDto[]>({
      method: 'GET',
      url: '/api/app/reservation/purposes',
    },
    { apiName: this.apiName,...config });
  

  getReservationsBySearchCriteria = (searchCriteria: ReservationsSearchCriteria, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ReservationsDto>>({
      method: 'GET',
      url: '/api/app/reservation/reservations',
      params: { guestNationality: searchCriteria.guestNationality, propertyName: searchCriteria.propertyName, propertyRating: searchCriteria.propertyRating, reservationNumber: searchCriteria.reservationNumber, reservationStatus: searchCriteria.reservationStatus, reservationPurpose: searchCriteria.reservationPurpose, dateFrom: searchCriteria.dateFrom, dateTo: searchCriteria.dateTo, currencyId: searchCriteria.currencyId, sorting: searchCriteria.sorting, skipCount: searchCriteria.skipCount, maxResultCount: searchCriteria.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getReservationsGridToExcel = (searchCriteria: ReservationsSearchCriteria, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FileExportDto>({
      method: 'GET',
      url: '/api/app/reservation/reservations-grid-to-excel',
      params: { guestNationality: searchCriteria.guestNationality, propertyName: searchCriteria.propertyName, propertyRating: searchCriteria.propertyRating, reservationNumber: searchCriteria.reservationNumber, reservationStatus: searchCriteria.reservationStatus, reservationPurpose: searchCriteria.reservationPurpose, dateFrom: searchCriteria.dateFrom, dateTo: searchCriteria.dateTo, currencyId: searchCriteria.currencyId, sorting: searchCriteria.sorting, skipCount: searchCriteria.skipCount, maxResultCount: searchCriteria.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
