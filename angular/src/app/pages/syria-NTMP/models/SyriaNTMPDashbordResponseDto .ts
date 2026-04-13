export interface SyriaNTMPDashbordResponseDto {
  todayReservationStatusCount: TodayReservationStatusCountDto;
  weeklyReservationCounts: WeeklyReservationCount[];
  totalCountWeekly: number;
}

export interface TodayReservationStatusCountDto {
  checkInStatusCount: number;
  checkOutStatusCount: number;
  canceledStatusCount: number;
}

export interface WeeklyReservationCount {
  date: string;
  dayName: string;
  count: number;
}