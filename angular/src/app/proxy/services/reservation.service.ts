import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { DashbordResponseDto, ReservationsDto, ReservationsSearchCriteria } from '../dto/models';

@Injectable({
  providedIn: 'root',
})
export class ReservationService {
  apiName = 'Default';
  

  create = (dto: ReservationsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReservationsDto>({
      method: 'POST',
      url: '/api/app/reservation',
      body: dto,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, boolean>({
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
  

  getReservationsBySearchCriteria = (searchCriteria: ReservationsSearchCriteria, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ReservationsDto>>({
      method: 'GET',
      url: '/api/app/reservation/reservations',
      params: { companyName: searchCriteria.companyName, propertyName: searchCriteria.propertyName, propertyRating: searchCriteria.propertyRating, reservationNumber: searchCriteria.reservationNumber, reservationStatus: searchCriteria.reservationStatus, reservationPurpose: searchCriteria.reservationPurpose, dateFrom: searchCriteria.dateFrom, dateTo: searchCriteria.dateTo, sorting: searchCriteria.sorting, skipCount: searchCriteria.skipCount, maxResultCount: searchCriteria.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getReservationsDashbord = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashbordResponseDto>({
      method: 'GET',
      url: '/api/app/reservation/reservations-dashbord',
    },
    { apiName: this.apiName,...config });
  

  update = (dto: ReservationsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ReservationsDto>({
      method: 'PUT',
      url: '/api/app/reservation',
      body: dto,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
