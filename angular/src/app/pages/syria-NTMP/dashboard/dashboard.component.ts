import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChartModule } from 'primeng/chart';
import { ButtonModule } from 'primeng/button';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { DatePickerModule } from 'primeng/datepicker';
import { ReservationService } from '../../../proxy/services';
import { DashboardDto, WeeklyDto, PurposeDto, NationalityDto, CityOccupancyDto, AdrByCityDto, LookupDto, DashboardFilterDto } from '@proxy/dto';
import { PropertyRatingEnum } from '../../../proxy/models/enums/property-rating-enum.enum';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ChartModule, ButtonModule, DialogModule, DropdownModule, DatePickerModule, FormsModule, TranslateModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  reservationService = inject(ReservationService);
  translateService = inject(TranslateService);
  statsData: DashboardDto | null = null;
  isLoading = false;
  error: string | null = null;
  hasTodayData = false;
  activeTab: string = 'guest-mix';

  percentages = { checkedIn: 0, checkedOut: 0, cancelled: 0 };
  todayDate: Date = new Date();

  // Filter states
  displayFilterDialog: boolean = false;
  currentFilters: DashboardFilterDto = {};

  citiesOptions: LookupDto[] = [];
  propertiesOptions: LookupDto[] = [];
  starOptions: any[] = [];

  lineData: any;
  opsLineOptions: any;

  // Guest Mix tab charts
  mixNationalityData: any;
  mixNationalityOptions: any;
  mixPurposeData: any;
  mixPurposeOptions: any;
  mixAdrData: any;
  mixAdrOptions: any;

  // Demand tab charts
  demandPurposeData: any;
  demandPurposeOptions: any;
  demandOccupancyData: any;
  demandOccupancyOptions: any;

  // Revenue tab charts
  revAdrData: any;
  revAdrOptions: any;
  nationalityOptions: LookupDto[];
  purposeOptions: LookupDto[];

  ngOnInit() {
    this.initChart();
    this.fetchStats();
    this.getFilters();

    this.translateService.onLangChange.subscribe(() => {
      this.buildLists();
    });
    this.buildLists();
  }

  buildLists() {
    this.translateService.get(['dashboard.all', 'enums.PropertyRatingEnum']).subscribe(translations => {
      const allLabel = translations['dashboard.all'];
      const starTranslations = translations['enums.PropertyRatingEnum'];

      this.starOptions = [
        { label: allLabel, value: null },
        { label: starTranslations?.OneStar || '1 Star', value: PropertyRatingEnum.OneStar },
        { label: starTranslations?.TwoStar || '2 Stars', value: PropertyRatingEnum.TwoStar },
        { label: starTranslations?.ThreeStar || '3 Stars', value: PropertyRatingEnum.ThreeStar },
        { label: starTranslations?.FourStar || '4 Stars', value: PropertyRatingEnum.FourStar },
        { label: starTranslations?.FiveStar || '5 Stars', value: PropertyRatingEnum.FiveStar },
      ];
    });
  }

  getFilters() {
    this.getFiltersCities()
    this.getFiltersProperties()
    this.getNationalities()
    this.getPurposes()
  }

  getNationalities() {
    this.reservationService.getNationalities({

    }).subscribe({
      next: (res: LookupDto[]) => {
        this.nationalityOptions = res;
        // console.log(res, "getNationalities");
      },
      error: (err) => {
        console.log(err, "err");
      }
    })
  }
  getPurposes() {
    this.reservationService.getPurposes({

    }).subscribe({
      next: (res: LookupDto[]) => {
        this.purposeOptions = res;
        // console.log(res, "getPurposes");
      },
      error: (err) => {
        console.log(err, "err");
      }
    })
  }

  getFiltersCities() {
    this.reservationService.getCities({

    }).subscribe({
      next: (res: LookupDto[]) => {
        this.citiesOptions = res;
        // console.log(res, "getCities");
      },
      error: (err) => {
        console.log(err, "err");
      }
    })
  }

  getFiltersProperties() {
    this.reservationService.getProperties({

    }).subscribe({
      next: (res: LookupDto[]) => {
        this.propertiesOptions = res;
        // console.log(res, "getProperties");
      },
      error: (err) => {
        console.log(err, "err");
      }
    })
  }

  showFilterDialog() {
    this.displayFilterDialog = true;
  }

  applyFilters() {
    this.displayFilterDialog = false;
    this.fetchStats();
  }

  fetchStats() {
    this.isLoading = true;
    this.error = null;

    this.reservationService.getDashboard(this.currentFilters).subscribe({
      next: (res: DashboardDto) => {
        this.isLoading = false;
        this.statsData = res;
        this.updateCharts(res);
      },
      error: (err) => {
        this.isLoading = false;
        this.error = 'Failed to load statistics.';
        console.error('Error fetching statistics:', err);
      }
    });
  }

  updateCharts(res: DashboardDto) {
    if (!res) return;

    // Operations Line Chart (Weekly)
    if (res.weeklyReservations && Array.isArray(res.weeklyReservations)) {
      const labels = res.weeklyReservations.map((item: WeeklyDto) => item.date || '');
      const chartData = res.weeklyReservations.map((item: WeeklyDto) => item.count);

      this.lineData = {
        ...this.lineData,
        labels: labels,
        datasets: [{ ...this.lineData.datasets[0], data: chartData }]
      };
    }

    // Today's Activity Percentages
    if (res.todayStats) {
      const checkedIn = res.todayStats.checkedIn || 0;
      const checkedOut = res.todayStats.checkedOut || 0;
      const cancelled = res.todayStats.cancelled || 0;
      const totalToday = checkedIn + checkedOut + cancelled;

      if (totalToday > 0) {
        this.percentages.checkedIn = Math.round((checkedIn / totalToday) * 100);
        this.percentages.checkedOut = Math.round((checkedOut / totalToday) * 100);
        this.percentages.cancelled = Math.round((cancelled / totalToday) * 100);
        this.hasTodayData = true;
      } else {
        this.percentages = { checkedIn: 0, checkedOut: 0, cancelled: 0 };
        this.hasTodayData = false;
      }
    }

    // Guest Mix - Nationality
    if (res.nationalityStats && Array.isArray(res.nationalityStats)) {
      const labels = res.nationalityStats.map((item: NationalityDto) => item.nationality || 'Unknown');
      const chartData = res.nationalityStats.map((item: NationalityDto) => item.count);
      this.mixNationalityData = {
        labels: labels,
        datasets: [{
          label: 'Nationality',
          backgroundColor: '#3b82f6',
          data: chartData
        }]
      };
    }

    // Guest Mix & Demand - Purpose
    if (res.purposeStats && Array.isArray(res.purposeStats)) {
      const colors = ['#9f6af0', '#7bb7d5', '#e2ba71', '#c87f82', '#644B96'];
      const purposeDatasets = res.purposeStats.map((item: PurposeDto, index: number) => ({
        label: item.purpose || 'Other',
        backgroundColor: colors[index % colors.length],
        data: [item.count]
      }));

      this.mixPurposeData = { labels: ['PURPOSE'], datasets: purposeDatasets };
      this.demandPurposeData = { labels: ['PURPOSE'], datasets: purposeDatasets };
    }

    // Guest Mix & Revenue - ADR By City
    if (res.revenue && res.revenue.meanAdrByCity && Array.isArray(res.revenue.meanAdrByCity)) {
      const labels = res.revenue.meanAdrByCity.map((item: AdrByCityDto) => item.city || 'Unknown');
      const chartData = res.revenue.meanAdrByCity.map((item: AdrByCityDto) => item.adr);
      const colors = ['#4412c1', '#638fd6', '#71c5a7', '#d09f19', '#e8df1c', '#9f6af0', '#7bb7d5', '#c87f82'];
      const bgColors = labels.map((_, i) => colors[i % colors.length]);

      this.mixAdrData = {
        labels: labels,
        datasets: [{
          label: 'ADR',
          backgroundColor: bgColors,
          data: chartData
        }]
      };

      this.revAdrData = {
        labels: labels,
        datasets: [{
          label: 'ADR',
          backgroundColor: bgColors,
          data: chartData
        }]
      };
    }

    // Demand - Occupancy By City
    if (res.occupancyByCity && Array.isArray(res.occupancyByCity)) {
      const labels = res.occupancyByCity.map((item: CityOccupancyDto) => item.city || 'Unknown');
      const chartData = res.occupancyByCity.map((item: CityOccupancyDto) => item.occupancyRate);
      const colors = ['#644B96', '#644B96', '#7bb7d5', '#7bb7d5', '#d4a843', '#d4a843', '#22c55e', '#22c55e', '#4412c1', '#638fd6'];
      const bgColors = labels.map((_, i) => colors[i % colors.length]);

      this.demandOccupancyData = {
        labels: labels,
        datasets: [{
          label: 'Occupancy %',
          backgroundColor: bgColors,
          data: chartData,
          borderRadius: 4
        }]
      };
    }
  }

  initChart() {
    const documentStyle = getComputedStyle(document.documentElement);
    const textColor = documentStyle.getPropertyValue('--text-color');
    const textColorSecondary = documentStyle.getPropertyValue('--text-color-secondary');
    const surfaceBorder = documentStyle.getPropertyValue('--surface-border');

    // Line Chart Data
    this.lineData = {
      labels: [],
      datasets: [
        {
          label: 'Reservations',
          data: [],
          fill: true,
          borderColor: '#94a3b8',
          tension: 0.4,
          backgroundColor: 'rgba(148, 163, 184, 0.1)',
          pointBackgroundColor: '#fff',
          pointBorderColor: '#94a3b8',
          pointHoverBackgroundColor: '#94a3b8',
          pointHoverBorderColor: '#fff'
        }
      ]
    };

    // Ops small line chart (sparkline style)
    this.opsLineOptions = {
      plugins: {
        legend: { display: false }
      },
      scales: {
        x: {
          display: false,
          grid: { display: false }
        },
        y: {
          display: false,
          grid: { display: false }
        }
      },
      elements: {
        point: { radius: 0 }
      }
    };

    // ========================================
    // Demand Tab Charts
    // ========================================

    // Visit Purpose - Horizontal Stacked Bar
    this.demandPurposeData = {
      labels: ['PURPOSE'],
      datasets: []
    };

    this.demandPurposeOptions = {
      indexAxis: 'y',
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'bottom',
          labels: {
            usePointStyle: true,
            color: textColorSecondary,
            boxWidth: 10,
            padding: 16
          }
        }
      },
      scales: {
        x: {
          stacked: true,
          ticks: { color: textColorSecondary, stepSize: 25 },
          grid: { color: surfaceBorder, drawBorder: false },
          max: 150
        },
        y: {
          stacked: true,
          ticks: { color: textColorSecondary },
          grid: { display: false, drawBorder: false }
        }
      }
    };

    // Occupancy Rate by City - Vertical Bar
    this.demandOccupancyData = {
      labels: [],
      datasets: []
    };

    this.demandOccupancyOptions = {
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false }
      },
      scales: {
        x: {
          ticks: { color: textColorSecondary, font: { size: 10 } },
          grid: { display: false, drawBorder: false }
        },
        y: {
          display: false,
          grid: { display: false }
        }
      }
    };

    // ========================================
    // Revenue Tab Charts
    // ========================================



    this.revAdrOptions = {
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false }
      },
      scales: {
        x: {
          ticks: { color: textColorSecondary, font: { size: 10 } },
          grid: { display: false, drawBorder: false }
        },
        y: {
          min: 0,
          max: 250,
          ticks: {
            color: textColorSecondary,
            stepSize: 25
          },
          grid: { color: surfaceBorder, drawBorder: false }
        }
      }
    };




    this.mixNationalityOptions = {
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        x: {
          ticks: { color: textColorSecondary, font: { size: 10 }, maxRotation: 45, minRotation: 45 },
          grid: { display: false, drawBorder: false }
        },
        y: {
          min: 0,
          max: 70,
          ticks: { color: textColorSecondary, stepSize: 3 },
          grid: { color: surfaceBorder, drawBorder: false }
        }
      }
    };


    this.mixPurposeOptions = {
      indexAxis: 'y',
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'bottom',
          labels: { usePointStyle: true, color: textColorSecondary, boxWidth: 10, padding: 16 }
        }
      },
      scales: {
        x: {
          stacked: true,
          ticks: { color: textColorSecondary, stepSize: 10 },
          grid: { color: surfaceBorder, drawBorder: false },
          max: 90
        },
        y: {
          stacked: true,
          ticks: { color: textColorSecondary },
          grid: { display: false, drawBorder: false }
        }
      }
    };

    // 3. ADR (Average Daily Rate by City)
    this.mixAdrData = {
      labels: [],
      datasets: []
    };

    this.mixAdrOptions = {
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        x: {
          ticks: { color: textColorSecondary, font: { size: 10 }, maxRotation: 45, minRotation: 45 },
          grid: { display: false, drawBorder: false }
        },
        y: {
          min: 0,
          max: 250,
          ticks: { color: textColorSecondary, stepSize: 25 },
          grid: { color: surfaceBorder, drawBorder: false }
        }
      }
    };
  }
}
