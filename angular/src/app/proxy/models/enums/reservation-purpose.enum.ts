import { mapEnumToOptions } from '@abp/ng.core';

export enum ReservationPurpose {
  Tourism = 1,
  FamilyOrFriends = 2,
  Religious = 3,
  BusinessOrWork = 4,
  Sports = 5,
  Entertainment = 6,
  Other = 7,
  Work_RoyalCourt = 8,
  Quarantined_guests = 9,
  MinistryOfHealthStaff = 10,
}

export const reservationPurposeOptions = mapEnumToOptions(ReservationPurpose);
