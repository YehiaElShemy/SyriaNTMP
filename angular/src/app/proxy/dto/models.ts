import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { PropertyRatingEnum } from '../models/enums/property-rating-enum.enum';
import type { ReservationPurpose } from '../models/enums/reservation-purpose.enum';
import type { ReservationStatus } from '../models/enums/reservation-status.enum';

export interface AdrByCityDto {
  city?: string;
  adr: number;
  totalNight: number;
}

export interface CityOccupancyDto {
  city?: string;
  occupancyRate: number;
}

export interface CurrencyDTO extends EntityDto<number> {
  currencyCustomizationId?: number;
  nameAr?: string;
  nameEn?: string;
  symbol?: string;
  isActive: boolean;
  color?: string;
}

export interface DashboardDto {
  opertation: OperationDto;
  purposeStats: PurposeDto;
  nationalityStats: NationalityDto[];
  occupancyDto: OccupancyDto;
  weeklyReservations: WeeklyDto[];
  totalWeeklyReservations: number;
  todayStats: TodayStatsDto;
  revenue: RevenueDto;
}

export interface DashboardFilterDto {
  fromDate?: string;
  toDate?: string;
  city?: string;
  hotelName?: string;
  hotelStars?: PropertyRatingEnum;
  nationality?: string;
  purpose?: ReservationPurpose;
  currencyId?: number;
}

export interface LookupDto {
  nameEn?: string;
  nameAr?: string;
  value?: string;
  id?: number;
}

export interface NationalityDto {
  nationality?: string;
  nightCount: number;
  visitorCount: number;
}

export interface OccupancyDto {
  avgOccupancyRate: number;
  totalSoldNights: number;
  totalNightsNotSolds: number;
  cityOccupancyDto: CityOccupancyDto[];
}

export interface OperationDto {
  totalReservations: number;
  occupancyRate: number;
  cancellationRate: number;
  activeProperties: number;
  totalSoldNights: number;
}

export interface PeakCityDto {
  city?: string;
  adr: number;
}

export interface PurposeDetailsDto {
  purpose?: string;
  count: number;
  purposeRate: number;
}

export interface PurposeDto {
  numOfGuests: number;
  totalNight: number;
  mostCommonPurpose?: string;
  purposeDetailsDtos: PurposeDetailsDto[];
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
  city?: string;
  propertyRating?: PropertyRatingEnum;
  reservationNumber?: string;
  reservationStatus?: ReservationStatus;
  numberOfGuests: number;
  numberOfNights: number;
  numberOfRooms: number;
  reservationPurpose?: ReservationPurpose;
  fromDate?: string;
  toDate?: string;
  guestNationality?: string;
  totalPrice: number;
  createdDate?: string;
  totalNumberOfPropertyUnits?: number;
  currencyId?: number;
  currencySymbolEn?: string;
  currencySymbolAr?: string;
}

export interface ReservationsSearchCriteria extends PagedAndSortedResultRequestDto {
  guestNationality?: string;
  propertyName?: string;
  propertyRating?: PropertyRatingEnum;
  reservationNumber?: string;
  reservationStatus?: ReservationStatus;
  reservationPurpose?: ReservationPurpose;
  dateFrom?: string;
  dateTo?: string;
  currencyId?: number;
}

export interface RevenueDto {
  portfolioAdr: number;
  totalRevenue: number;
  currencySymbolEn?: string;
  currencySymbolAr?: string;
  totalNight: number;
  adrAvgPriceDay: number;
  peakCity: PeakCityDto;
  meanAdrByCity: AdrByCityDto[];
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
