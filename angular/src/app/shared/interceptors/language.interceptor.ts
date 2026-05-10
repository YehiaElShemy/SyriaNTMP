import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';

@Injectable()
export class LanguageInterceptor implements HttpInterceptor {
  private translateService = inject(TranslateService);

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const lang = this.translateService.currentLang || this.translateService.getDefaultLang() || 'en';
    const clonedReq = req.clone({
      headers: req.headers.set('Accept-Language', lang)
    });
    return next.handle(clonedReq);
  }
}
