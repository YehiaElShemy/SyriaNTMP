import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CurrencyDTO } from '../dto/models';

@Injectable({
  providedIn: 'root',
})
export class CurrencyService {
  apiName = 'Default';
  

  getAllCurrenies = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, CurrencyDTO[]>({
      method: 'GET',
      url: '/api/app/currency/currenies',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
