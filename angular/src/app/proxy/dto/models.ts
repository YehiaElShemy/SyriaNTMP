import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { PropertyRatingEnum } from '../models/enums/property-rating-enum.enum';
import type { ReservationStatus } from '../models/enums/reservation-status.enum';
import type { ReservationPurpose } from '../models/enums/reservation-purpose.enum';

export interface DashbordResponseDto {
  todayReservationStatusCount: TodayReservationStatusCountDto;
  weeklyReservationCounts: WeeklyReservationCount[];
  totalCountWeekly: number;
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

export interface TodayReservationStatusCountDto {
  checkInStatusCount: number;
  checkOutStatusCount: number;
  canceledStatusCount: number;
}

export interface WeeklyReservationCount {
  date?: string;
  dayName?: string;
  count: number;
}
