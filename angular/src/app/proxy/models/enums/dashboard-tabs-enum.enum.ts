import { mapEnumToOptions } from '@abp/ng.core';

export enum DashboardTabsEnum {
  Operations = 1,
  VisitPurpose = 2,
  GuestMix = 3,
  RevenueAndADR = 4,
  DemandAndOccupancy = 5,
}

export const dashboardTabsEnumOptions = mapEnumToOptions(DashboardTabsEnum);
