import { Component } from '@angular/core';
import { TranslationService } from './shared/services/translation.service';

@Component({
  standalone: false,
  selector: 'app-root',
  template: `
    <abp-loader-bar></abp-loader-bar>
    <abp-dynamic-layout></abp-dynamic-layout>
  `,
  styleUrls: ['./app.component.scss'],
  host: {
    '[attr.dir]': 'translationService.currentLanguage() === "ar" ? "rtl" : "ltr"'
  }


})
export class AppComponent {
  constructor(public translationService: TranslationService) { }

}
