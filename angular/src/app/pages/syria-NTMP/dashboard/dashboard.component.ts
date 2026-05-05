import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChartModule } from 'primeng/chart';
import { ButtonModule } from 'primeng/button';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { DatePickerModule } from 'primeng/datepicker';
import { CurrencyService, ReservationService } from '../../../proxy/services';
import { DashboardDto, WeeklyDto, PurposeDto, NationalityDto, CityOccupancyDto, AdrByCityDto, LookupDto, DashboardFilterDto, CurrencyDTO } from '@proxy/dto';
import { PropertyRatingEnum } from '../../../proxy/models/enums/property-rating-enum.enum';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TranslationService } from 'src/app/shared/services/translation.service';
import { ReservationPurpose } from '@proxy/models/enums';
import { Chart } from 'chart.js';
import ChartDataLabels from 'chartjs-plugin-datalabels';
import { PageAlertService, ToasterService } from '@abp/ng.theme.shared';
import { DashboardTabs } from 'src/app/shared/models/enums/dashbord-tabs.enum';

Chart.register(ChartDataLabels);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ChartModule, ButtonModule, DialogModule, DropdownModule, DatePickerModule, FormsModule, TranslateModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  dashboardTabs = DashboardTabs;
  statsData: DashboardDto | null = null;
  isLoading = false;
  error: string | null = null;
  hasTodayData = false;
  activeTab = DashboardTabs.guestMix;

  percentages = { checkedIn: 0, checkedOut: 0, cancelled: 0 };
  fromDate: Date | null = null;
  toDate: Date | null = null;

  // Filter states
  displayFilterDialog: boolean = false;
  currentFilters: DashboardFilterDto = {};

  citiesOptions: LookupDto[] = [];
  currenciesOptions: LookupDto[] = [];
  propertiesOptions: LookupDto[] = [];
  starOptions: any[] = [];

  lineData: any;
  opsLineOptions: any;

  // Guest Mix tab charts
  // Guest Mix tab charts
  mixPurposeData: any;
  mixPurposeOptions: any;
  mixAdrData: any;
  mixAdrOptions: any;

  // Demand tab charts
  demandPurposeData: any;
  demandPurposeOptions: any;
  demandOccupancyData: any;
  demandOccupancyOptions: any;

  // Visit Purpose donut chart
  purposeDonutData: any;
  purposeDonutOptions: any;
  readonly purposeColors = ['#A78BFA', '#3B82F6', '#4ADE80', '#FBBF24', '#F97316', '#FB7185', '#6D28D9'];

  get purposeTotalNights(): number {
    if (!this.statsData?.purposeStats?.purposeDetailsDtos) return 0;
    return this.statsData.purposeStats.purposeDetailsDtos.reduce((sum, item) => sum + (item.count || 0), 0);
  }

  formatNights(n: number): string {
    if (n >= 1_000_000) return (n / 1_000_000).toFixed(0) + 'M';
    if (n >= 1_000) return (n / 1_000).toFixed(0) + 'K';
    return n.toString();
  }



  get nationalityMaxCount(): number {
    if (!this.statsData?.nationalityStats?.length) return 1;
    const maxVisitor = Math.max(...this.statsData.nationalityStats.map(item => item.visitorCount || 0));
    const maxNight = Math.max(...this.statsData.nationalityStats.map(item => item.nightCount || 0));
    return Math.max(maxVisitor, maxNight) + 2;
  }

  getNationalityPercent(count: number, max: number): number {
    if (!max) return 0;
    return Math.round((count / max) * 100);
  }

  getNationalityColor(index: number): string {
    const colors = ['#1d4ed8', '#0ea5e9', '#14b8a6', '#f59e0b', '#facc15', '#ec4899', '#8b5cf6'];
    return colors[index % colors.length];
  }

  // Revenue tab charts
  revAdrData: any;
  revAdrOptions: any;
  nationalityOptions: LookupDto[];
  purposeOptions: any[] = [];
  constructor(
    private translationService: TranslationService,
    private reservationService: ReservationService,
    private currencyService: CurrencyService,
    private translateService: TranslateService,
    private alertService: ToasterService
  ) { }

  ngOnInit() {
    this.initializeDateRange();
    this.initChart();
    this.Search();
    this.getFilters();

    this.translateService.onLangChange.subscribe(() => {
      this.buildLists();
    });
    this.buildLists();
  }
  get isAr(): boolean {
    return this.translationService.currentLanguage() === 'ar';
  }

  buildLists() {
    this.translateService.get(['enums.PropertyRatingEnum']).subscribe(translations => {
      const starTranslations = translations['enums.PropertyRatingEnum'];

      this.starOptions = [
        { label: starTranslations?.None || 'None', value: PropertyRatingEnum.None },
        { label: starTranslations?.OneStar || '1 Star', value: PropertyRatingEnum.OneStar },
        { label: starTranslations?.TwoStar || '2 Stars', value: PropertyRatingEnum.TwoStar },
        { label: starTranslations?.ThreeStar || '3 Stars', value: PropertyRatingEnum.ThreeStar },
        { label: starTranslations?.FourStar || '4 Stars', value: PropertyRatingEnum.FourStar },
        { label: starTranslations?.FiveStar || '5 Stars', value: PropertyRatingEnum.FiveStar },
      ];
    });
  }
  private initializeDateRange(): void {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth();
    const day = now.getDate();

    this.fromDate = new Date(year, 0, 1);           // beginning of year
    this.toDate = new Date(year, month, day, 23, 59, 59); // end of today
  }
  getFilters() {
    this.getFiltersCities()
    this.getFiltersProperties()
    this.getNationalities()
    this.getPurposes()
    this.getCurrencies()
  }
  applyFilters() {
    this.displayFilterDialog = false;
    this.Search();
  }
  Search() {
    this.isLoading = true;
    this.error = null;
    this.currentFilters.currencyId = this.currentFilters.currencyId != null ? Number(this.currentFilters.currencyId) || undefined : undefined;
    this.currentFilters.fromDate = this.fromDate ? this.formatDate(this.fromDate) : null;

    this.currentFilters.toDate = this.toDate ? this.formatDate(this.toDate) : null;
    if (this.activeTab == DashboardTabs.revenueAndADR && (this.currentFilters.currencyId == null || this.currentFilters.currencyId == 0)) {
      this.alertService.error('dashboard.pleaseSelectACurrency');
      return;
    }
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

  getNationalities() {
    this.reservationService.getNationalities({

    }).subscribe({
      next: (res: LookupDto[]) => {
        this.nationalityOptions = res;
        // //console.log('res, "getNationalities");
      },
      error: (err) => {
        //console.log('err, "err");
      }
    })
  }
  getPurposes() {
    this.translateService.get('enums').subscribe(enumsTranslations => {
      this.purposeOptions = Object.keys(ReservationPurpose)
        .filter(key => isNaN(Number(key)))
        .map(key => ({
          label: enumsTranslations?.ReservationPurpose?.[key] || key.replace(/([A-Z])/g, ' $1').trim(),
          value: ReservationPurpose[key as keyof typeof ReservationPurpose]
        }));
    });
  }

  getFiltersCities() {
    this.reservationService.getCities({}).subscribe({
      next: (res: LookupDto[]) => {
        this.citiesOptions = res;
      },
      error: (err) => {
        //console.log('err, "err");
      }
    })
  }

  getCurrencies() {
    this.currencyService.getAllCurrenies({}).subscribe(
      (res: any) => {
        if (res) {
          this.currenciesOptions = res.map(x => {
            return {
              id: x.id,
              nameAr: x.nameAr,
              nameEn: x.nameEn
            }
          });
        }
      }
    )
  }
  choseTab(tab: DashboardTabs) {
    this.activeTab = tab;
    this.currentFilters.currencyId = null;
    if (tab == this.dashboardTabs.revenueAndADR && this.currenciesOptions && this.currenciesOptions.length > 0) {
      this.currentFilters.currencyId = this.currenciesOptions[0].id;
      this.Search();
    } else {
      this.Search();
    }
  }


  getFiltersProperties() {
    this.reservationService.getProperties({

    }).subscribe({
      next: (res: LookupDto[]) => {
        this.propertiesOptions = res;
        // //console.log('res, "getProperties");
      },
      error: (err) => {
        //console.log('err, "err");
      }
    })
  }

  showFilterDialog() {
    this.displayFilterDialog = true;
  }


  formatDate(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }



  updateCharts(res: DashboardDto) {
    if (!res) return;

    // Operations Line Chart (Weekly)
    if (res.weeklyReservations && Array.isArray(res.weeklyReservations)) {
      const locale = this.isAr ? 'ar-EG' : 'en-US';
      const labels = res.weeklyReservations.map((item: WeeklyDto) => {
        if (!item.date) return '';
        try {
          const date = new Date(item.date);
          return date.toLocaleDateString(locale, { weekday: 'short' });
        } catch (e) {
          return item.date;
        }
      });
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

    // Guest Mix - Nationality (Progress bars in HTML)
    // Legacy mixNationalityData logic removed as it's handled via manual progress bars in template

    // Guest Mix & Demand - Purpose
    if (res.purposeStats?.purposeDetailsDtos && Array.isArray(res.purposeStats.purposeDetailsDtos)) {
      const colors = ['#9f6af0', '#7bb7d5', '#e2ba71', '#c87f82', '#644B96'];
      const purposeDatasets = res.purposeStats.purposeDetailsDtos.map((item, index: number) => ({
        label: item.purpose || 'Other',
        backgroundColor: colors[index % colors.length],
        data: [item.count]
      }));

      this.mixPurposeData = { labels: ['PURPOSE'], datasets: purposeDatasets };
      this.demandPurposeData = { labels: ['PURPOSE'], datasets: purposeDatasets };

      // Donut chart for Visit Purpose tab
      this.purposeDonutData = {
        labels: res.purposeStats.purposeDetailsDtos.map((item) => item.purpose || 'Other'),
        datasets: [{
          data: res.purposeStats.purposeDetailsDtos.map((item) => item.count),
          backgroundColor: res.purposeStats.purposeDetailsDtos.map((_, i: number) => this.purposeColors[i % this.purposeColors.length]),
          borderWidth: 0,
          borderColor: '#ffffff',
          hoverBorderColor: '#ffffff'
        }]
      };
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
          data: chartData,
          maxBarThickness: 50
        }]
      };

      const maxAdr = chartData.length > 0 ? Math.max(...chartData) : 0;
      if (this.revAdrOptions?.scales?.y) {
        this.revAdrOptions.scales.y.max = maxAdr + 2;
        this.revAdrOptions = { ...this.revAdrOptions };
      }
    }

    // Demand - Occupancy By City
    if (res.occupancyDto?.cityOccupancyDto && Array.isArray(res.occupancyDto.cityOccupancyDto)) {
      const labels = res.occupancyDto.cityOccupancyDto.map((item: CityOccupancyDto) => item.city || 'Unknown');
      const chartData = res.occupancyDto.cityOccupancyDto.map((item: CityOccupancyDto) => item.occupancyRate);
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
    Chart.defaults.font.size = 16; // Increase default chart font size by 2px (from 12 to 14)
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
      layout: {
        padding: {
          left: 25,
          right: 25,
          top: 25,
          bottom: 25
        }
      },
      plugins: {
        legend: { display: false },
        datalabels: {
          display: true,
          align: 'top',
          color: '#334155', // Hardcoded dark grey to avoid invisible text
          font: {
            size: 16,
            weight: 'bold'
          }
        }
      },
      scales: {
        x: {
          display: true,
          grid: { display: false, drawBorder: false },
          ticks: { color: textColorSecondary, font: { weight: 'bold' } }
        },
        y: {
          display: false,
          grid: { display: false },
          min: 0,
          suggestedMax: function (context: any) {
            const max = Math.max(...(context.chart.data.datasets[0]?.data || [0]));
            return max * 1.5; // Gives padding at top so data labels are not cut off
          }
        }
      },
      elements: {
        point: { radius: 0 }
      }
    };


    this.demandPurposeData = {
      labels: ['PURPOSE'],
      datasets: []
    };

    this.purposeDonutData = { labels: [], datasets: [] };
    this.purposeDonutOptions = {
      cutout: '45%',
      maintainAspectRatio: true,
      plugins: {
        legend: { display: false },
        tooltip: {
          enabled: true
        },
        datalabels: {
          display: false
        }
      }
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
        legend: { display: false },
        datalabels: {
          display: false
        }
      },
      scales: {
        x: {
          ticks: { color: textColorSecondary, font: { size: 16 } },
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
        legend: { display: false },
        datalabels: {
          display: false
        }

      },
      scales: {
        x: {
          ticks: { color: textColorSecondary, font: { size: 16 } },
          grid: { display: false, drawBorder: false }
        },
        y: {
          min: 0,
          ticks: {
            color: textColorSecondary,
            stepSize: 25
          },
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
      plugins: {
        legend: { display: false },
        datalabels: {
          display: false
        }
      },
      scales: {
        x: {
          ticks: { color: textColorSecondary, font: { size: 16 }, maxRotation: 45, minRotation: 45 },
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
