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
        //console.log('this.nationalityOptions, "nationalityOptions");
      },
      error: (err) => {
        //console.log('err, "err");
      }
    })
  }

  // Pagination state
  totalRecords = 0;
  pageSize = 7;
  pageIndex = 0;

  fetchReservations() {
    this.isLoading = true;

    const payload: ReservationsSearchCriteria = this.getFilterPayload();
    this.reservationService.getReservationsBySearchCriteria(payload).subscribe({
      next: (res: any) => {
        //console.log(''Real API Data:', res);
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

  private getFilterPayload(isForExport: boolean = false): ReservationsSearchCriteria {
    return {
      skipCount: isForExport ? 0 : this.pageIndex * this.pageSize,
      maxResultCount: isForExport ? 999 : this.pageSize,
      guestNationality: this.filterNationality || null,
      propertyName: this.filterPropertyName || null,
      propertyRating: this.filterPropertyRating == 0 ? 0 : this.filterPropertyRating,
      reservationNumber: this.filterReservationNumber || null,
      reservationStatus: this.filterReservationStatus || null,
      reservationPurpose: this.filterReservationPurpose || null,
      dateFrom: this.filterDateFrom ? this.formatDate(this.filterDateFrom) : null,
      dateTo: this.filterDateTo ? this.formatDate(this.filterDateTo) : null
    };
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
  export() {
    const searchCriteria: ReservationsSearchCriteria = this.getFilterPayload(true);
    const isArabic = this.translateService.currentLang === 'ar';
    const fileName = isArabic
      ? `حجوزات_${this.formatDate(new Date())}`
      : `Reservations_${this.formatDate(new Date())}`;

    this.reservationService.getReservationsGridToExcel(searchCriteria).subscribe(dto => {
      // 2. Convert bytes → Blob (your code works)
      let blob: Blob;
      const bytesData = dto.bytes as any;

      if (typeof bytesData === 'string') {
        const byteCharacters = atob(bytesData);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
          byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        blob = new Blob([byteArray], { type: dto.contentType });
      } else {
        const bytes = dto.bytes || [];
        const uint8Array = new Uint8Array(bytes.length);
        for (let i = 0; i < bytes.length; i++) {
          uint8Array[i] = bytes[i];
        }
        blob = new Blob([uint8Array], { type: dto.contentType });
      }

      // 3. RTL Arabic filename + download
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName;
      document.body.appendChild(a);  // Fix for some browsers
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    });
  }

  formatLanDate(date: string | Date): string {
  if (!date) return '';

  const d = new Date(date);

  const lang = this.translateService.currentLang;

  return new Intl.DateTimeFormat(
    lang === 'ar' ? 'ar-EG' : 'en-GB',
    {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }
  ).format(d);
}
}
