import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SyriaStatsService } from '../syria-stats.service';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { RatingModule } from 'primeng/rating';
import { TagModule } from 'primeng/tag';
import { FormsModule } from '@angular/forms';
import { DatePickerModule } from 'primeng/datepicker';
import { ReservationService } from '../../../proxy/services';
import { ReservationsSearchCriteria } from '@proxy/dto';
import { ReservationStatus } from '@proxy/models/enums';
import { PropertyRatingEnum } from '@proxy/models/enums';
import { ReservationPurpose } from '@proxy/models/enums';


@Component({
  selector: 'app-detailed-statistics',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, InputTextModule, SelectModule, RatingModule, TagModule, FormsModule, DatePickerModule],
  templateUrl: './detailed-statistics.component.html',
  styleUrl: './detailed-statistics.component.scss'

})
export class DetailedStatisticsComponent implements OnInit {
  reservationService = inject(ReservationService);
  showFilter: boolean = false;
  reservations: any[] = [];
  ratingsList: any[] = [];
  statusList: any[] = [];
  purposeList: any[] = [];
  isLoading = false;

  // Filter model
  filterCompanyName: string | null = null;
  filterPropertyName: string | null = null;
  filterPropertyRating: number | null = null;
  filterReservationNumber: string | null = null;
  filterReservationStatus: number | null = null;
  filterReservationPurpose: number | null = null;
  filterDateFrom: Date | null = null;
  filterDateTo: Date | null = null;
  
  // Make enum available to template
  ReservationStatus = ReservationStatus;
  PropertyRatingEnum = PropertyRatingEnum;
  ReservationPurpose = ReservationPurpose;

  ngOnInit() {
    this.ratingsList = Object.keys(PropertyRatingEnum)
      .filter(key => isNaN(Number(key)))
      .map(key => ({
        label: key.replace('Star', ' Star').trim(), // Format "OneStar" to "One Star"
        value: PropertyRatingEnum[key as keyof typeof PropertyRatingEnum]
      }));

    // Filter out numeric keys to just get the string names, and map them to label/value pairs
    this.statusList = Object.keys(ReservationStatus)
      .filter(key => isNaN(Number(key)))
      .map(key => ({
        label: key.replace(/([A-Z])/g, ' $1').trim(), // Add space before capitals (e.g. 'CheckedIn' -> 'Checked In')
        value: ReservationStatus[key as keyof typeof ReservationStatus] // value is the numeric value (e.g., 5)
      }));

    this.purposeList = Object.keys(ReservationPurpose)
      .filter(key => isNaN(Number(key)))
      .map(key => ({
        label: key.replace(/([A-Z])/g, ' $1').trim(), // Add space before capitals
        value: ReservationPurpose[key as keyof typeof ReservationPurpose]
      }));

    this.fetchReservations();
  }

  // Pagination state
  totalRecords = 0;
  pageSize = 7;
  pageIndex = 0;

  fetchReservations() {
    this.isLoading = true;

    const payload : ReservationsSearchCriteria = {
      // paging: {
      //   pagingEnabled: true,
      //   pageIndex: this.pageIndex,
      //   pageSize: this.pageSize,
      //   totalCount: 0,
      //   subTotalCount: 0,
      //   skip: this.pageIndex * this.pageSize
      // },
      // orderByDirection: 1,
      // orderByCultureMode: 1,
      maxResultCount: this.pageSize,
      companyName: this.filterCompanyName || null,
      propertyName: this.filterPropertyName || null,
      propertyRating: this.filterPropertyRating || null,
      reservationNumber: this.filterReservationNumber || null,
      reservationStatus: this.filterReservationStatus || null,
      reservationPurpose: this.filterReservationPurpose || null,
      dateFrom: this.filterDateFrom ? this.formatDate(this.filterDateFrom) : null,
      dateTo: this.filterDateTo ? this.formatDate(this.filterDateTo) : null
    };

    this.reservationService.getReservationsBySearchCriteria(payload).subscribe({
      next: (res: any) => {
        console.log('Real API Data:', res);
        this.reservations = res?.items || [];
        // Read totalCount from res.data.paging (the actual API shape)
        this.totalRecords = res?.totalCount ?? this.reservations.length;
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Error fetching reservations:', err);
        this.isLoading = false;
      }
    });
  }

  onPageChange(event: any) {
    this.pageIndex = event.first / event.rows;
    this.pageSize = event.rows;
    this.fetchReservations();
  }

  onSearch() {
    // Reset to first page before searching
    this.pageIndex = 0;
    this.fetchReservations();
  }

  clearFilters() {
    this.filterCompanyName = null;
    this.filterPropertyName = null;
    this.filterPropertyRating = null;
    this.filterReservationNumber = null;
    this.filterReservationStatus = null;
    this.filterReservationPurpose = null;
    this.filterDateFrom = null;
    this.filterDateTo = null;
    this.pageIndex = 0;
    this.fetchReservations();
  }

  toggleFilter() {
    this.showFilter = !this.showFilter;
  }

  formatDate(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  getSeverity(status: ReservationStatus) {
    switch (status) {
      case ReservationStatus.CheckedIn: return 'success';
      case ReservationStatus.CheckedOut: return 'info';
      case ReservationStatus.Canceled: return 'danger';
      case ReservationStatus.UnConfirmed: return 'warning';
      case ReservationStatus.Confirmed: return 'success';
      case ReservationStatus.Expired: return 'danger';
      case ReservationStatus.NoShow: return 'danger';
      case ReservationStatus.PreviousState: return 'secondary';
      default: return 'info';
    }
  }
}
