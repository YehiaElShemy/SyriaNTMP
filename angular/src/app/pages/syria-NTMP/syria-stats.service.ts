import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SyriaStatsService {
  private http = inject(HttpClient);
  
  private baseUrl = 'https://staging.nazeel.net/api/syria-demo-integration';

  getReservationsStatistics(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/get-syria-ntmp-statistics-reservations`);
  }

  getSyriaNtmpReservations(payload: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/get-syria-ntmp-reservations`, payload);
  }

  getSyriaShomoosReservations(payload: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/get-syria-shomoos-reservations`, payload);
  }

  getSyriaShomoosStatisticsReservations(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/get-syria-ntmp-statistics-reservations`);
  }
}
