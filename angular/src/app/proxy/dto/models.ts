import type { PropertyRatingEnum } from '../models/enums/property-rating-enum.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ReservationStatus } from '../models/enums/reservation-status.enum';
import type { ReservationPurpose } from '../models/enums/reservation-purpose.enum';

export interface AdrByCityDto {
  city?: string;
  adr: number;
}

export interface CityOccupancyDto {
  city?: string;
  occupancyRate: number;
}

export interface DashboardDto {
  summary: SummaryDto;
  purposeStats: PurposeDto[];
  nationalityStats: NationalityDto[];
  occupancyByCity: CityOccupancyDto[];
  weeklyReservations: WeeklyDto[];
  todayStats: TodayStatsDto;
  revenue: RevenueDto;
}

export interface DashboardFilterDto {
  fromDate?: string;
  toDate?: string;
  city?: string;
  hotelName?: string;
  hotelStars?: PropertyRatingEnum;
}

export interface LookupDto {
  nameEn?: string;
  nameAr?: string;
  value?: string;
}

export interface NationalityDto {
  nationality?: string;
  count: number;
}

export interface PeakCityDto {
  city?: string;
  adr: number;
}

export interface PurposeDto {
  purpose?: string;
  count: number;
}

export interface ReservationResponseDto {
  success: boolean;
  transactionId?: string;
  bookingNumber?: string;
}

export interface ReservationsDto extends EntityDto<number> {
  reservationId: number;
  companyId?: string;
  companyName?: string;
  propertyId?: string;
  propertyName?: string;
  propertyRating?: PropertyRatingEnum;
  reservationNumber?: string;
  reservationStatus?: ReservationStatus;
  numberOfGuests: number;
  numberOfNights: number;
  reservationPurpose?: ReservationPurpose;
  fromDate?: string;
  toDate?: string;
  guestNationality?: string;
  totalPrice: number;
}

export interface ReservationsSearchCriteria extends PagedAndSortedResultRequestDto {
  companyName?: string;
  propertyName?: string;
  propertyRating?: PropertyRatingEnum;
  reservationNumber?: string;
  reservationStatus?: ReservationStatus;
  reservationPurpose?: ReservationPurpose;
  dateFrom?: string;
  dateTo?: string;
}

export interface RevenueDto {
  portfolioAdr: number;
  peakCity: PeakCityDto;
  meanAdrByCity: AdrByCityDto[];
}

export interface SummaryDto {
  totalReservations: number;
  occupancyRate: number;
  cancellationRate: number;
}

export interface TodayStatsDto {
  checkedIn: number;
  checkedOut: number;
  cancelled: number;
}

export interface WeeklyDto {
  date?: string;
  count: number;
}
