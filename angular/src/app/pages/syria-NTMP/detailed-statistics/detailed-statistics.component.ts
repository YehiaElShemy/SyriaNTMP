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
import { LookupDto, ReservationsSearchCriteria } from '@proxy/dto';
import { ReservationStatus } from '@proxy/models/enums';
import { PropertyRatingEnum } from '@proxy/models/enums';
import { ReservationPurpose } from '@proxy/models/enums';
import { TranslateModule, TranslateService } from '@ngx-translate/core';


@Component({
  selector: 'app-detailed-statistics',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, InputTextModule, SelectModule, RatingModule, TagModule, FormsModule, DatePickerModule, TranslateModule],
  templateUrl: './detailed-statistics.component.html',
  styleUrl: './detailed-statistics.component.scss'

})
export class DetailedStatisticsComponent implements OnInit {
  reservationService = inject(ReservationService);
  translateService = inject(TranslateService);
  showFilter: boolean = false;
  reservations: any[] = [];
  ratingsList: any[] = [];
  statusList: any[] = [];
  purposeList: any[] = [];
  isLoading = false;

  // Filter model
  filterNationality: string | null = null;
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
  nationalityOptions: any[] = [];
  ngOnInit() {
    this.initializeDateRange();
    this.translateService.onLangChange.subscribe(() => {
      this.buildLists();
    });
    this.buildLists();
  }
  private initializeDateRange(): void {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth();
    const day = now.getDate();

    this.filterDateFrom = new Date(year, 0, 1);           // beginning of year
    this.filterDateTo = new Date(year, month, day, 23, 59, 59); // end of today
  }
  get isAr(): boolean {
    return this.translateService.getCurrentLang() === 'ar';
  }
  buildLists() {
    this.translateService.get('enums').subscribe(enumsTranslations => {
      this.ratingsList = Object.keys(PropertyRatingEnum)
        .filter(key => isNaN(Number(key)))
        .map(key => ({
          label: enumsTranslations?.PropertyRatingEnum?.[key] || key.replace('Star', ' Star').trim(),
          value: PropertyRatingEnum[key as keyof typeof PropertyRatingEnum]
        }));

      this.statusList = Object.keys(ReservationStatus)
        .filter(key => isNaN(Number(key)))
        .map(key => ({
          label: enumsTranslations?.ReservationStatus?.[key] || key.replace(/([A-Z])/g, ' $1').trim(),
          value: ReservationStatus[key as keyof typeof ReservationStatus]
        }));

      this.purposeList = Object.keys(ReservationPurpose)
        .filter(key => isNaN(Number(key)))
        .map(key => ({
          label: enumsTranslations?.ReservationPurpose?.[key] || key.replace(/([A-Z])/g, ' $1').trim(),
          value: ReservationPurpose[key as keyof typeof ReservationPurpose]
        }));


    });
    this.getNationalities();
    this.fetchReservations();
  }
  getNationalities() {
    this.reservationService.getNationalities({}).subscribe({
      next: (res: LookupDto[]) => {
        this.nationalityOptions = res.map(x => ({
          label: this.isAr ? x.nameAr : x.nameEn,
          value: x.nameEn
        }));
        console.log(this.nationalityOptions, "nationalityOptions");
      },
      error: (err) => {
        console.log(err, "err");
      }
    })
  }

  // Pagination state
  totalRecords = 0;
  pageSize = 7;
  pageIndex = 0;

  fetchReservations() {
    this.isLoading = true;

    const payload: ReservationsSearchCriteria = {
      skipCount: this.pageIndex * this.pageSize,
      maxResultCount: this.pageSize,
      guestNationality: this.filterNationality || null,
      propertyName: this.filterPropertyName || null,
      propertyRating: this.filterPropertyRating == 0 ? 0 : this.filterPropertyRating,
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
    this.filterNationality = null;
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
