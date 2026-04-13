import { mapEnumToOptions } from '@abp/ng.core';

export enum ReservationStatus {
  UnConfirmed = 1,
  Confirmed = 2,
  Expired = 3,
  NoShow = 4,
  CheckedIn = 5,
  Canceled = 6,
  CheckedOut = 7,
  PreviousState = 8,
}

export const reservationStatusOptions = mapEnumToOptions(ReservationStatus);
