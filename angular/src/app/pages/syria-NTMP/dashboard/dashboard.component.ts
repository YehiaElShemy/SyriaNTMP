import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChartModule } from 'primeng/chart';
import { ButtonModule } from 'primeng/button';
import { ReservationService } from '../../../proxy/services';
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ChartModule, ButtonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  reservationService = inject(ReservationService);
  statsData: any = null;
  isLoading = false;
  error: string | null = null;
  hasTodayData = false;
  
  percentages = { checkedIn: 0, checkedOut: 0, cancelled: 0 };
  todayDate: Date = new Date();

  lineData: any;
  lineOptions: any;

  doughnutData: any;
  doughnutOptions: any;

  occupancyRateData: any;
  occupancyRateOptions: any;

  visitPurposeData: any;
  visitPurposeOptions: any;

  adrData: any;
  adrOptions: any;

  nationalityData: any;
  nationalityOptions: any;

  ngOnInit() {
    this.initChart();
    this.fetchStats();
  }

  fetchStats() {
    this.isLoading = true;
    this.error = null;
    this.reservationService.getReservationsDashbord().subscribe({
      next: (res) => {
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

  updateCharts(res: any) {
    if (!res || !res.data) return;

    const data = res.data;

    // Line Chart Update
    if (data.weeklyReservationCounts && Array.isArray(data.weeklyReservationCounts)) {
      const labels = data.weeklyReservationCounts.map((item: any) => item.dayName);
      const chartData = data.weeklyReservationCounts.map((item: any) => item.count);

      this.lineData = {
        ...this.lineData,
        labels: labels,
        datasets: [
          {
            ...this.lineData.datasets[0],
            data: chartData
          }
        ]
      };
    }

    // Doughnut Chart Update
    if (data.todayReservationStatusCount) {
      const checkedIn = data.todayReservationStatusCount.checkInStatusCount || 0;
      const checkedOut = data.todayReservationStatusCount.checkOutStatusCount || 0;
      const cancelled = data.todayReservationStatusCount.canceledStatusCount || 0;
      
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

      this.doughnutData = {
        ...this.doughnutData,
        datasets: [
          {
            ...this.doughnutData.datasets[0],
            data: [checkedIn, checkedOut, cancelled]
          }
        ]
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

    this.lineOptions = {
      plugins: {
        legend: { display: false }
      },
      scales: {
        x: {
          ticks: { color: textColorSecondary },
          grid: { color: surfaceBorder, drawBorder: false }
        },
        y: {
          min: 0,
          ticks: { color: textColorSecondary },
          grid: { color: surfaceBorder, drawBorder: false }
        }
      }
    };

    // Doughnut Chart Data
    this.doughnutData = {
      labels: ['Checked In', 'Checked Out', 'Cancelled'],
      datasets: [
        {
          data: [],
          backgroundColor: ['#1e293b', '#64748b', '#cbd5e1'],
          hoverBackgroundColor: ['#0f172a', '#475569', '#94a3b8']
        }
      ]
    };

    this.doughnutOptions = {
      plugins: {
        legend: { display: false }
      },
      cutout: '70%'
    };

    // Occupancy Rate Chart Data
    this.occupancyRateData = {
      labels: ['Aleppo', 'Damascus', 'Homs', 'Latakia', 'Hama'],
      datasets: [
        {
          label: 'Occupancy Rate',
          backgroundColor: '#bcbcbc',
          data: [48, 33, 30, 0, 0]
        }
      ]
    };

    this.occupancyRateOptions = {
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false }
      },
      scales: {
        x: {
          ticks: { color: textColorSecondary },
          grid: { color: surfaceBorder, drawBorder: false }
        },
        y: {
          min: 0,
          max: 50,
          ticks: {
            color: textColorSecondary,
            stepSize: 5,
            callback: function(value: any) {
                return value + '%';
            }
          },
          grid: { color: surfaceBorder, drawBorder: false }
        }
      }
    };

    // Visit Purpose Chart Data
    this.visitPurposeData = {
      labels: ['Purpose'],
      datasets: [
        {
          label: 'Tourism',
          backgroundColor: '#9f6af0', // Purple
          data: [120]
        },
        {
          label: 'Religious',
          backgroundColor: '#7bb7d5', // Blue
          data: [30]
        },
        {
          label: 'Medical',
          backgroundColor: '#e2ba71', // Yellow
          data: [60]
        },
        {
          label: 'Other',
          backgroundColor: '#d66e74', // Red
          data: [80]
        }
      ]
    };

    this.visitPurposeOptions = {
      indexAxis: 'y',
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'bottom',
          labels: {
            usePointStyle: true,
            color: textColorSecondary,
            boxWidth: 10
          }
        }
      },
      scales: {
        x: {
          stacked: true,
          ticks: { color: textColorSecondary },
          grid: { color: surfaceBorder, drawBorder: false }
        },
        y: {
          stacked: true,
          ticks: { color: textColorSecondary },
          grid: { display: false, drawBorder: false }
        }
      }
    };

    // ADR Chart Data
    this.adrData = {
      labels: ['Aleppo', 'Damascus', 'Homs', 'Latakia', 'Hama'],
      datasets: [
        {
          label: 'ADR',
          backgroundColor: '#bcbcbc',
          data: [41000, 30000, 26000, 0, 0]
        }
      ]
    };

    this.adrOptions = {
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false }
      },
      scales: {
        x: {
          ticks: { color: textColorSecondary },
          grid: { color: surfaceBorder, drawBorder: false }
        },
        y: {
          min: 0,
          max: 45000,
          ticks: {
            color: textColorSecondary,
            stepSize: 5000,
            callback: function(value: any) {
                return (value === 0 ? '0K' : (value / 1000) + 'K');
            }
          },
          grid: { color: surfaceBorder, drawBorder: false }
        }
      }
    };

    // Nationality Statistics Chart Data
    this.nationalityData = {
      labels: ['Syrian', 'Russian', 'Iranian', 'Lebanese', 'Iraqi'],
      datasets: [
        {
          label: 'Nationality',
          backgroundColor: ['#60a5fa', '#f472b6', '#fbbf24', '#34d399', '#a78bfa'],
          data: [15000, 8000, 6000, 4500, 3000]
        }
      ]
    };

    this.nationalityOptions = {
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false }
      },
      scales: {
        x: {
          ticks: { color: textColorSecondary },
          grid: { color: surfaceBorder, drawBorder: false }
        },
        y: {
          min: 0,
          ticks: {
            color: textColorSecondary,
            callback: function(value: any) {
                return (value === 0 ? '0' : (value / 1000) + 'K');
            }
          },
          grid: { color: surfaceBorder, drawBorder: false }
        }
      }
    };
  }
}
